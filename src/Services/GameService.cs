using DotFeather;
using Silk.NET.SDL;
using static Sukiteto.Global;

namespace Sukiteto;

public class GameService
{
    public bool[,] CurrentShape => Shapes[CurrentBlockColor][BlockRotation];

    /// <summary>
    /// フィールド
    /// </summary>
    public BlockColor[,] Field { get; private set; }

    /// <summary>
    /// フィールドの幅
    /// </summary>
    public int Width { get; set; } = 10;
        
    /// <summary>
    /// フィールドの高さ
    /// </summary>
    public int Height { get; set; } = 20;

    // 窒息高度より上にどれくらい積めるか
    // これを超えようとするか、窒息高度付近でブロックを召喚できなくなったらゲームオーバーとなる
    public int HeightOffset { get; set; } = 6;

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
    /// ホールドできるかどうか
    /// </summary>
    public bool CanHold { get; set; } = true;

    /// <summary>
    /// 次に出てくるブロックのキュー
    /// </summary>
    public Queue<BlockColor> NextQueue { get; } = new Queue<BlockColor>();

    /// <summary>
    /// ブロックの速度
    /// </summary>
    public float FallSpeed { get; set; } = 2;

    /// <summary>
    /// 固定までの猶予時間（単位は時間）
    /// </summary>
    public float graceTimeForFix { get; set; } = 0.5f;

    /// <summary>
    /// 自由落下のタイマー
    /// </summary>
    private float freefallDistance;

    /// <summary>
    /// 固定のタイマー
    /// </summary>
    private float fixTimer;

    /// <summary>
    /// 固定猶予リセットカウンター
    /// </summary>
    private float fixResetCounter;

    /// <summary>
    /// ソフトドロップ中かどうか
    /// </summary>
    private bool isDroppingSoftly;

    /// <summary>
    /// Tspinされたかどうか
    /// </summary>
    private bool isTspin;
    
    /// <summary>
    /// ラインクリア時に消えたラインのインデックスを格納するバッファ
    /// </summary>
    private readonly int[] clearedLineIndicesBuffer;

    /// <summary>
    /// TspinMiniかどうか
    /// </summary>
    private bool isTspinMini;

    private static readonly BlockColor[] allBlocks =
    {
        BlockColor.I,
        BlockColor.J,
        BlockColor.L,
        BlockColor.O,
        BlockColor.S,
        BlockColor.T,
        BlockColor.Z
    };

    /// <summary>
    /// 固定猶予リセットの最大数（置くかホールドでリセットする）
    /// </summary>
    private static readonly float fixResetMax = 8;
    
    /// <summary>
    /// キックテーブル
    /// </summary>
    private static readonly Dictionary<(int fromRot, int toRot), VectorInt[]> kickTable = new()
    {
        [(0, 1)] = new VectorInt[]{ (0, 0), (-1, 0), (-1, -1), (0, +2), (-1, +2) },
        [(1, 0)] = new VectorInt[]{ (0, 0), (+1, 0), (+1, +1), (0, -2), (+1, -2) },
        [(1, 2)] = new VectorInt[]{ (0, 0), (+1, 0), (+1, +1), (0, -2), (+1, -2) },
        [(2, 1)] = new VectorInt[]{ (0, 0), (-1, 0), (-1, -1), (0, +2), (-1, +2) },
        [(2, 3)] = new VectorInt[]{ (0, 0), (+1, 0), (+1, -1), (0, +2), (+1, +2) },
        [(3, 2)] = new VectorInt[]{ (0, 0), (-1, 0), (-1, +1), (0, -2), (-1, -2) },
        [(3, 0)] = new VectorInt[]{ (0, 0), (-1, 0), (-1, +1), (0, -2), (-1, -2) },
        [(0, 3)] = new VectorInt[]{ (0, 0), (+1, 0), (+1, -1), (0, +2), (+1, +2) },
    };

    /// <summary>
    /// キックテーブル（Iブロック用）
    /// </summary>
    private static readonly Dictionary<(int fromRot, int toRot), VectorInt[]> kickTableI = new()
    {
        [(0, 1)] = new VectorInt[]{ (0, 0), (-2, 0), (+1, 0), (-2, +1), (+1, -2) },
        [(1, 0)] = new VectorInt[]{ (0, 0), (+2, 0), (-1, 0), (+2, -1), (-1, +2) },
        [(1, 2)] = new VectorInt[]{ (0, 0), (-1, 0), (+2, 0), (-1, -2), (+2, +1) },
        [(2, 1)] = new VectorInt[]{ (0, 0), (+1, 0), (-2, 0), (+1, +2), (-2, -1) },
        [(2, 3)] = new VectorInt[]{ (0, 0), (+2, 0), (-1, 0), (+2, -1), (-1, +2) },
        [(3, 2)] = new VectorInt[]{ (0, 0), (-2, 0), (+1, 0), (-2, +1), (+1, -2) },
        [(3, 0)] = new VectorInt[]{ (0, 0), (+1, 0), (-2, 0), (+1, +2), (-2, -1) },
        [(0, 3)] = new VectorInt[]{ (0, 0), (-1, 0), (+2, 0), (-1, -2), (+2, +1) },
    };
    
    private static readonly Random random = new();

    public GameService()
    {
        clearedLineIndicesBuffer = new int[Height + HeightOffset];
    }

    public void Start()
    {
        Field = new BlockColor[Width, Height + HeightOffset];

        EnqueueNexts();
        SpawnNextBlock();
    }

    public void Tick(float deltaTime)
    {
        ProcessFreefall();
        ProcessFix();
        isDroppingSoftly = false;
    }

    public void TriggerLeft()
    {
        if (!CanPlaceBlock(BlockPosition.X - 1, BlockPosition.Y, CurrentShape)) return;
        BlockPosition += VectorInt.Left;;
    }
    
    public void TriggerRight()
    {
        if (!CanPlaceBlock(BlockPosition.X + 1, BlockPosition.Y, CurrentShape)) return;
        BlockPosition += VectorInt.Right;
    }
    
    public void TriggerDown()
    {
        if (!CanPlaceBlock(BlockPosition.X, BlockPosition.Y + 1, CurrentShape)) return;
        BlockPosition += VectorInt.Down;
        isDroppingSoftly = true;
    }
    
    public void TriggerHardDrop()
    {
        var y = RayToDown();
        BlockPosition = (BlockPosition.X, (int)y);
        fixTimer = graceTimeForFix;
    }
    
    public void TriggerRotateLeft()
    {
        var nextRotation = BlockRotation - 1;
        if (nextRotation < 0) nextRotation = 3;
        var kickValue = TryKick(nextRotation);
        if (!kickValue.HasValue) return;
        BlockPosition += kickValue.Value;
        BlockRotation = nextRotation;
        ResetFix();
        isTspin = CheckTspin(kickValue);
    }
    
    public void TriggerRotateRight()
    {
        var nextRotation = BlockRotation + 1;
        if (nextRotation > 3) nextRotation = 0;
        var kickValue = TryKick(nextRotation);
        if (!kickValue.HasValue) return;
        BlockPosition += kickValue.Value;
        BlockRotation = nextRotation;
        ResetFix();
        isTspin = CheckTspin(kickValue);
    }
    
    public void TriggerHold()
    {
        if (!CanHold) return;
        if (CurrentHold == BlockColor.None)
        {
            CurrentHold = CurrentBlockColor;
            SpawnNextBlock();
            Hold?.Invoke();
            CanHold = false;
            return;
        }
        (CurrentHold, CurrentBlockColor) = (CurrentBlockColor, CurrentHold);
        CanHold = false;
        Hold?.Invoke();
        ResetStateForSpawning();
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
    /// 自由落下の制御
    /// </summary>
    private void ProcessFreefall()
    {
        if (fixTimer > 0) return;

        freefallDistance += MathF.Min(FallSpeed * Time.DeltaTime, 20);
        if (freefallDistance < 1) return;

        var distanceInt = (int)MathF.Floor(freefallDistance);
        BlockPosition += (0, distanceInt);
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
    private void ProcessFix()
    {
        if (!CanPlaceBlock(BlockPosition.X, BlockPosition.Y + 1, CurrentShape))
        {
            if (fixTimer == 0) BlockHit?.Invoke();
            fixTimer += Time.DeltaTime;
        }
        else if (fixTimer > 0)
        {
            ResetFix();
        }
        
        if (fixTimer < graceTimeForFix) return;
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
        var bottom = Height + HeightOffset - 1;
        var y2 = bottom + 1;
        for (var y = bottom; y >= 0; y--)
        {
            y2--;
            var isLineFilled = true;
            for (var x = 0; x < Width; x++)
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
        // Tを持っているかどうかチェック
        if (CurrentBlockColor != BlockColor.T)
        {
            // 持っていなかったら抜ける
            return false;
        }

        // TspinMiniフラグの初期化
        isTspinMini = false;

        // Tspinの判定カウンタ
        int counter = 0;

        // 4隅の判定箇所のうちTspinMiniの条件を満たす場所が空いているかどうか
        bool isMini = false;

        VectorInt offsetPosition = BlockPosition + (1, 1);

        // 左上
        {
            int checkX = offsetPosition.X - 1;
            int checkY = offsetPosition.Y - 1;
            if (checkY >= 0)
            {
                if (checkX < 0 || Field[checkX, checkY] != BlockColor.None) 
                {
                    counter++;
                }
                else if (BlockRotation == 0 || BlockRotation == 3)
                {
                    //TspinMini判定
                    isMini = true;
                }
            }
        }

        // 右上
        {
            int checkX = offsetPosition.X + 1;
            int checkY = offsetPosition.Y - 1;
            if (checkY >= 0)
            {
                if (checkX >= Width || Field[checkX, checkY] != BlockColor.None)
                {
                    counter++;

                }
                else if (BlockRotation == 0 || BlockRotation == 1)
                {
                    //TspinMini判定
                    isMini = true;
                }
            }
        }

        // 右下
        {
            int checkX = offsetPosition.X + 1;
            int checkY = offsetPosition.Y + 1;
            if (checkX >= Width || checkY >= Height+HeightOffset || Field[checkX, checkY] != BlockColor.None)
            {
                counter++;
            }
            else if (BlockRotation == 1 || BlockRotation == 2)
            {
                //TspinMini判定
                isMini = true;
            }

        }

        // 左下
        {
            int checkX = offsetPosition.X - 1;
            int checkY = offsetPosition.Y + 1;
            if (checkX < 0 || checkY >= Height+HeightOffset || Field[checkX, checkY] != BlockColor.None)
            {
                counter++;
            }
            else if (BlockRotation == 2 || BlockRotation == 3)
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

            TspinRotate.Invoke();
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
        foreach (var type in allBlocks.OrderBy(_ => random.Next()))
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
        if (NextQueue.Count < 7)
        {
            EnqueueNexts();
        }

        ResetStateForSpawning();
        CanHold = true;

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
            for (var j = 0; j < Width; j++)
            {
                Field[j, i] = i > 0 ? Field[j, i - 1] : BlockColor.None;
            }
        }
        
        for (var i = 0; i < Width; i++)
        {
            Field[i, 0] = BlockColor.None;
        }
    }

    /// <summary>
    /// 固定猶予タイマーをリセットする
    /// </summary>
    private void ResetFix()
    {
        if (fixResetCounter >= fixResetMax) return;
        if (!(fixTimer > 0)) return;
        fixTimer = 0;
        fixResetCounter++;
    }

    /// <summary>
    /// キックテーブルを元にブロックのキックを試みます。
    /// </summary>
    /// <param name="nextRotation">試行する回転</param>
    /// <returns>キックが成功した場合はその相対座標、失敗した場合は<c>null</c>。</returns>
    private VectorInt? TryKick(int nextRotation)
    {
        var table = CurrentBlockColor == BlockColor.I ? kickTableI : kickTable;
        var testCases = table[(BlockRotation, nextRotation)];
        foreach (var testCase in testCases)
        {
            if (CanPlaceBlock(BlockPosition.X + testCase.X, BlockPosition.Y + testCase.Y, Shapes[CurrentBlockColor][nextRotation]))
            {
                return testCase;
            }
        }

        return null;
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
    /// ブロックをその位置に配置できるかどうかを算出します。
    /// </summary>
    /// <param name="x">フィールド X</param>
    /// <param name="y">フィールド Y</param>
    /// <param name="blockShape">ブロック形状</param>
    /// <returns>配置できる（衝突しない）場合は<c>true</c>を、衝突してしまう場合は<c>false</c>を返します。</returns>
    private bool CanPlaceBlock(int x, int y, bool[,] blockShape)
    {
        for (var i = 0; i < blockShape.GetLength(0); i++)
        {
            for (var j = 0; j < blockShape.GetLength(1); j++)
            {
                if (!blockShape[i, j]) continue;
                if (x + i < 0 || x + i >= Width || y + j < 0 || y + j >= Height + HeightOffset) return false;
                if (Field[x + i, y + j] != BlockColor.None) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ブロックのスポーンおよびホールドから引っ張ってきたときに行うリセット処理
    /// </summary>
    private void ResetStateForSpawning()
    {
        fixResetCounter = 0;
        fixTimer = 0;
        BlockPosition = (Width / 2 - 2, HeightOffset - 3);
        BlockRotation = 0;
    }

    public event Action<LineClearEventArgs> LineClear;
    public event Action Hold;
    public event Action SpawnNext;
    public event Action GameOver;
    public event Action BlockPlace;
    public event Action BlockHit;
    public event Action TspinRotate;
}