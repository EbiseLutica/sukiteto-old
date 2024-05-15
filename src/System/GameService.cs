using Promete;

namespace Sukiteto;

public class GameService
{
    public BlockColor[,] Field { get; private set; }
    public ShapeLoader Shapes => Config.RotationSystem.Shapes;
    
    public bool[,] CurrentShape => Shapes[CurrentBlockColor][BlockRotation];

    /// <summary>
    /// 現在降下中のブロックの現在位置。
    /// </summary>
    public VectorInt BlockPosition { get; set; } = (0, 0);

    /// <summary>
    /// 現在降下中のブロックの色。
    /// </summary>
    public BlockColor CurrentBlockColor { get; set; } = BlockColor.None;

    /// <summary>
    ///  現在降下中のブロックの回転値。
    /// </summary>
    public int BlockRotation { get; set; } = 0;

    /// <summary>
    /// 現在ホールドしているブロック
    /// </summary>
    public BlockColor CurrentHold { get; set; } = BlockColor.None;

    /// <summary>
    /// 次に出てくるブロックのキュー
    /// </summary>
    public Queue<BlockColor> NextQueue { get; } = [];

    public GameConfig Config { get; }
    
    public bool UsedHold { get; set; }

    /// <summary>
    /// 自由落下のタイマー
    /// </summary>
    private float freefallDistance;

    /// <summary>
    /// 固定のタイマー
    /// </summary>
    private float lockTimer;

    /// <summary>
    /// 固定猶予リセットカウンター
    /// </summary>
    private float lockResetCounter;

    /// <summary>
    /// Tspinされたかどうか
    /// </summary>
    private bool isTspin;

    /// <summary>
    /// TspinMiniかどうか
    /// </summary>
    private bool isTspinMini;

    private static readonly Random Random = new();

    public GameService(GameConfig config)
    {
        this.Config = config;
        if (config.CustomMapString == null)
        {
            Field = new BlockColor[config.FieldSize.X, config.FieldSize.Y + config.TopMargin];
        }
        else
        {
            var mapLines = config.CustomMapString.Split('\n');
            var height = mapLines.Length;
            var width = mapLines.Max(line => line.Length);
            Field = new BlockColor[width, height + config.TopMargin];

            for (var y = 0; y < mapLines.Length; y++)
            {
                for (var x = 0; x < mapLines[y].Length; x++)
                {
                    Field[x, y + config.TopMargin] = mapLines[y][x] switch
                    {
                        'I' => BlockColor.I,
                        'J' => BlockColor.J,
                        'L' => BlockColor.L,
                        'O' => BlockColor.O,
                        'S' => BlockColor.S,
                        'T' => BlockColor.T,
                        'Z' => BlockColor.Z,
                        'W' => BlockColor.Wall,
                        'G' => BlockColor.Ghost,
                        _ => BlockColor.None
                    };
                }
            }
        }
    }

    public void Start()
    {

        EnqueueNexts();
        SpawnNextBlock();
    }

    public void Tick(float deltaTime)
    {
        ProcessFreefall(deltaTime);
        ProcessFix(deltaTime);
    }

    public bool TriggerLeft()
    {
        if (!CanPlaceBlock(BlockPosition.X - 1, BlockPosition.Y, CurrentShape)) return false;
        BlockPosition += VectorInt.Left;
        ResetFix();
        
        return true;
    }
    
    public bool TriggerRight()
    {
        if (!CanPlaceBlock(BlockPosition.X + 1, BlockPosition.Y, CurrentShape)) return false;
        BlockPosition += VectorInt.Right;
        ResetFix();
        
        return true;
    }
    
    public bool TriggerDown()
    {
        if (!CanPlaceBlock(BlockPosition.X, BlockPosition.Y + 1, CurrentShape)) return false;
        BlockPosition += VectorInt.Down;

        return true;
    }
    
    public void TriggerHardDrop()
    {
        BlockPosition = (BlockPosition.X, RayToDown());
        lockTimer = Config.LockDelay;
        ProcessFix(0);
    }
    
    public bool TriggerRotateLeft()
    {
        var nextRotation = BlockRotation - 1;
        if (nextRotation < 0) nextRotation = 3;
        var kickValue = Config.RotationSystem.TryKick(this, CurrentBlockColor, BlockPosition, BlockRotation, nextRotation);
        if (!kickValue.HasValue) return false;
        BlockPosition += kickValue.Value;
        BlockRotation = nextRotation;
        ResetFix();
        isTspin = CheckTspin(kickValue);

        return true;
    }
    
    public bool TriggerRotateRight()
    {
        var nextRotation = BlockRotation + 1;
        if (nextRotation > 3) nextRotation = 0;
        var kickValue = Config.RotationSystem.TryKick(this, CurrentBlockColor, BlockPosition, BlockRotation, nextRotation);
        if (!kickValue.HasValue) return false;
        BlockPosition += kickValue.Value;
        BlockRotation = nextRotation;
        ResetFix();
        isTspin = CheckTspin(kickValue);

        return true;
    }
    
    public bool TriggerHold()
    {
        if (Config.HoldMode == GameConfig.HoldModeFlag.None || UsedHold) return false;
        if (CurrentHold == BlockColor.None)
        {
            CurrentHold = CurrentBlockColor;
            SpawnNextBlock();
            if (Config.HoldMode != GameConfig.HoldModeFlag.Infinite)
            {
                UsedHold = true;
            }
            Hold?.Invoke();
            return true;
        }
        (CurrentHold, CurrentBlockColor) = (CurrentBlockColor, CurrentHold);
        if (Config.HoldMode != GameConfig.HoldModeFlag.Infinite)
        {
            UsedHold = true;
        }
        Hold?.Invoke();
        ResetStateForSpawning();

        return true;
    }

    /// <summary>
    /// 現在のブロックがそのまま降下したときに到達するY座標を算出します。
    /// </summary>
    public int RayToDown()
    {
        var y = BlockPosition.Y;
        while (CanPlaceBlock(BlockPosition.X, y + 1, CurrentShape))
        {
            y++;
        }

        return y;
    }
    
    /// <summary>
    /// ブロックをその位置に配置できるかどうかを算出します。
    /// </summary>
    /// <param name="x">フィールド X</param>
    /// <param name="y">フィールド Y</param>
    /// <param name="blockShape">ブロック形状</param>
    /// <returns>配置できる（衝突しない）場合は<c>true</c>を、衝突してしまう場合は<c>false</c>を返します。</returns>
    public bool CanPlaceBlock(int x, int y, bool[,] blockShape)
    {
        var (w, h) = Config.FieldSize + (0, Config.TopMargin);
        for (var i = 0; i < blockShape.GetLength(0); i++)
        {
            for (var j = 0; j < blockShape.GetLength(1); j++)
            {
                if (!blockShape[i, j]) continue;
                if (x + i < 0 || x + i >= w || y + j < 0 || y + j >= h) return false;
                if (Field[x + i, y + j] != BlockColor.None) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 自由落下の制御
    /// </summary>
    private void ProcessFreefall(float deltaTime = 0)
    {
        if (lockTimer > 0) return;

        var currentTop = RayToDown();

        freefallDistance += MathF.Min(Config.FallSpeed * deltaTime, 20);
        if (freefallDistance < 1) return;

        var distanceInt = (int)freefallDistance;
        BlockPosition += (0, distanceInt);
        if (BlockPosition.Y > currentTop && !Config.IsCrazyMode)
        {
            BlockPosition = (BlockPosition.X, currentTop);
        }

        freefallDistance -= distanceInt;
        
        // 床判定
        while (!CanPlaceBlock(BlockPosition.X, BlockPosition.Y, CurrentShape))
        {
            BlockPosition -= (0, 1);
        }
    }

    /// <summary>
    /// ブロックが床に固定するまでの猶予時間の処理
    /// </summary>
    private void ProcessFix(float deltaTime)
    {
        if (!CanPlaceBlock(BlockPosition.X, BlockPosition.Y + 1, CurrentShape))
        {
            if (lockTimer == 0) BlockHit?.Invoke();
            lockTimer += deltaTime;
        }
        else if (lockTimer > 0)
        {
            ResetFix();
        }
        
        if (lockTimer < Config.LockDelay && lockResetCounter < Config.LockDelayResetMaxCount) return;
        BlockPosition = (BlockPosition.X, RayToDown());
        PlaceBlock(BlockPosition.X, BlockPosition.Y, CurrentShape, CurrentBlockColor);
        ProcessLineClear();
        SpawnNextBlock();
    }

    /// <summary>
    /// ラインクリア処理
    /// </summary>
    private void ProcessLineClear()
    {
        var cleared = 0;
        var (width, height) = Config.FieldSize + (0, Config.TopMargin);
        var bottom = height - 1;
        var y2 = bottom + 1;
        var clearedLineIndicesBuffer = new int[height];
        for (var y = bottom; y >= 0; y--)
        {
            y2--;
            var isLineFilled = true;
            for (var x = 0; x < width; x++)
            {
                if (Field[x, y] != BlockColor.None) continue;
                isLineFilled = false;
                break;
            }

            if (!isLineFilled) continue;
            clearedLineIndicesBuffer[cleared] = y2;
            cleared++;
            ShiftDownField(y);
            y++;
        }

        if (cleared > 0)
        {
            // TODO: 追加判定をスコアに加味する
            LineClear?.Invoke(new LineClearEventArgs()
            {
                ClearedLineIndices = clearedLineIndicesBuffer.AsMemory(0, cleared),
                IsTSpin = isTspin,
                IsTSpinMini = isTspinMini,
            });
        }
    }

    /// <summary>
    /// Tspinが取れているかどうかを判定する
    /// </summary>
    private bool CheckTspin(VectorInt? kickValue)
    {
        var (width, height) = Config.FieldSize + (0, Config.TopMargin);

        // Tを持っているかどうかチェック
        if (CurrentBlockColor != BlockColor.T)
        {
            // 持っていなかったら抜ける
            return false;
        }

        // TspinMiniフラグの初期化
        isTspinMini = false;

        // Tspinの判定カウンタ
        var counter = 0;

        // 4隅の判定箇所のうちTspinMiniの条件を満たす場所が空いているかどうか
        var isMini = false;

        var offsetPosition = BlockPosition + (1, 1);

        // 左上
        {
            var checkX = offsetPosition.X - 1;
            var checkY = offsetPosition.Y - 1;
            if (checkY >= 0)
            {
                if (checkX < 0 || Field[checkX, checkY] != BlockColor.None)
                {
                    counter++;
                }
                else if (BlockRotation is 0 or 3)
                {
                    //TspinMini判定
                    isMini = true;
                }
            }
        }

        // 右上
        {
            var checkX = offsetPosition.X + 1;
            var checkY = offsetPosition.Y - 1;
            if (checkY >= 0)
            {
                if (checkX >= Config.FieldSize.X || Field[checkX, checkY] != BlockColor.None)
                {
                    counter++;

                }
                else if (BlockRotation is 0 or 1)
                {
                    //TspinMini判定
                    isMini = true;
                }
            }
        }

        // 右下
        {
            var checkX = offsetPosition.X + 1;
            var checkY = offsetPosition.Y + 1;
            if (checkX >= width || checkY >= height || Field[checkX, checkY] != BlockColor.None)
            {
                counter++;
            }
            else if (BlockRotation is 1 or 2)
            {
                //TspinMini判定
                isMini = true;
            }

        }

        // 左下
        {
            var checkX = offsetPosition.X - 1;
            var checkY = offsetPosition.Y + 1;
            if (checkX < 0 || checkY >= height || Field[checkX, checkY] != BlockColor.None)
            {
                counter++;
            }
            else if (BlockRotation is 2 or 3)
            {
                //TspinMini判定
                isMini = true;
            }
        }

        // TspinMiniの条件判定のためキックの値を絶対値にする
        VectorInt checkKick = (0, 0);
        checkKick.X = Math.Abs(kickValue.Value.X);
        checkKick.Y = Math.Abs(kickValue.Value.Y);

        // Tspinが成立していたらtrueとEventを返す
        if (counter >= 3)
        {
            // TspinMiniチェック
            if (isMini && checkKick != (1, 2))
            {
                isTspinMini = true;
            }

            TspinRotate?.Invoke();
            return true;
        }

        // Tspinが成立しなかった
        return false;
    }

    /// <summary>
    /// ネクストに新たなブロックを挿入します。
    /// </summary>
    private void EnqueueNexts()
    {
        foreach (var type in Config.UsingBlocks.OrderBy(_ => Random.Next()))
        {
            NextQueue.Enqueue(type);
        }
    }

    /// <summary>
    /// ネクストのブロックを召喚し、必要ならネクストを追加します。
    /// </summary>
    private void SpawnNextBlock()
    {
        CurrentBlockColor = NextQueue.Dequeue();
        // NEXTが減ってきたら補充する
        while (NextQueue.Count < 7)
        {
            EnqueueNexts();
        }

        ResetStateForSpawning();
        UsedHold = false;

        if (!CanPlaceBlock(BlockPosition.X, BlockPosition.Y, CurrentShape))
        {
            GameOver?.Invoke();
            return;
        }

        ProcessFreefall();
        
        SpawnNext?.Invoke();
    }

    /// <summary>
    /// フィールドの y 行を消して、それより上の行を下に1ずつずらします。
    /// </summary>
    /// <param name="y"></param>
    private void ShiftDownField(int y)
    {   
        for (var i = y; i >= 0; i--)
        {
            for (var j = 0; j < Config.FieldSize.X; j++)
            {
                Field[j, i] = i > 0 ? Field[j, i - 1] : BlockColor.None;
            }
        }
        
        for (var i = 0; i < Config.FieldSize.X; i++)
        {
            Field[i, 0] = BlockColor.None;
        }
    }

    /// <summary>
    /// 固定猶予タイマーをリセットする
    /// </summary>
    private void ResetFix()
    {
        if (lockTimer == 0) return;
        lockTimer = 0;
        lockResetCounter++;
    }

    /// <summary>
    /// ブロックを指定した位置に配置します。
    /// </summary>
    /// <param name="x">フィールドのX</param>
    /// <param name="y">フィールドのY</param>
    /// <param name="blockShape">ブロック形状</param>
    /// <param name="blockColor">ブロックの色</param>
    private void PlaceBlock(int x, int y, bool[,] blockShape, BlockColor blockColor)
    {
        for (var i = 0; i < blockShape.GetLength(0); i++)
        {
            for (var j = 0; j < blockShape.GetLength(1); j++)
            {
                if (!blockShape[i, j]) continue;
                Field[x + i, y + j] = blockColor;
            }
        }

        BlockPlace?.Invoke();
    }

    /// <summary>
    /// ブロックのスポーンおよびホールドから引っ張ってきたときに行うリセット処理
    /// </summary>
    private void ResetStateForSpawning()
    {
        lockResetCounter = 0;
        lockTimer = 0;
        BlockPosition = (Config.FieldSize.X / 2 - 2, Math.Max(0, Config.TopMargin - 3));
        BlockRotation = 0;
        isTspin = false;
        isTspinMini = false;
    }

    public event Action<LineClearEventArgs>? LineClear;
    public event Action? Hold;
    public event Action? SpawnNext;
    public event Action? GameOver;
    public event Action? BlockPlace;
    public event Action? BlockHit;
    public event Action? TspinRotate;
}