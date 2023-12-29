using System.Drawing;
using DotFeather;
using Sukiteto;

using static Sukiteto.Global;

namespace SukiTeto;

public class GameScene : Scene
{
    private bool[,] CurrentShape => Shapes[currentColor][blockRotation];
    
    /// <summary>
    /// フィールドの表示に使うタイルマップ
    /// </summary>
    private Tilemap fieldTileMap;

    /// <summary>
    /// 現在のブロックの表示に使うタイルマップ
    /// </summary>
    private Tilemap currentBlockTileMap;

    /// <summary>
    /// NEXT, HOLD などの表示に使うタイルマップ
    /// </summary>
    private Tilemap uiTileMap;

    /// <summary>
    /// ブロックテクスチャのタイルデータ
    /// </summary>
    private Dictionary<BlockColor, ITile> blockTiles;
    
    /// <summary>
    /// 壁のタイルデータ
    /// </summary>
    private ITile wallTile;

    /// <summary>
    /// フィールド
    /// </summary>
    private BlockColor[,] field;

    /// <summary>
    /// フィールドの幅
    /// </summary>
    private int width = 10;
        
    /// <summary>
    /// フィールドの高さ
    /// </summary>
    private int height = 20;

    // 窒息高度より上にどれくらい積めるか
    // これを超えようとするか、窒息高度付近でブロックを召喚できなくなったらゲームオーバーとなる
    private int heightOffset = 6;
    
    /// <summary>
    /// 現在ホールドしているブロック
    /// </summary>
    private BlockColor currentHold = BlockColor.None;
    
    /// <summary>
    /// 次に出てくるブロックのキュー
    /// </summary>
    private Queue<BlockColor> nextQueue = new Queue<BlockColor>();

    private BlockColor currentColor = BlockColor.None;

    /// <summary>
    /// ブロックの現在位置
    /// </summary>
    private VectorInt blockPosition = (0, 0);

    /// <summary>
    /// ブロックの現在の回転値
    /// </summary>
    private int blockRotation = 0;

    /// <summary>
    /// フィールドが更新されたかどうか
    /// </summary>
    private bool isFieldUpdated;

    /// <summary>
    /// ブロックの速度
    /// </summary>
    private float fallSpeed = 2;
    
    /// <summary>
    /// 自由落下のタイマー
    /// </summary>
    private float freefallDistance;

    /// <summary>
    /// 固定までの猶予時間（単位は時間）
    /// </summary>
    private float graceTimeForFix = 0.5f;
    
    /// <summary>
    /// 固定のタイマー
    /// </summary>
    private float fixTimer;

    /// <summary>
    /// 長押ししてからブロックが動き始めるまでの時間
    /// </summary>
    private float das = 1f / 60 * 10;
    
    /// <summary>
    /// 長押し中のブロックの移動速度
    /// </summary>
    private float arr = 1f / 60 * 2;

    /// <summary>
    /// DAS用のタイマー
    /// </summary>
    private float dasTimer;

    /// <summary>
    /// 固定猶予リセットカウンター
    /// </summary>
    private float fixResetCounter;

    /// <summary>
    /// ホールドできるかどうか
    /// </summary>
    private bool canHold = true;

    private bool isGameOver;
    
    // DAS 考慮用
    private bool isLeftPressed;
    private bool isRightPressed;
    private bool isDownPressed;

    private readonly VectorInt holdPosition = (9, 5);
    private readonly VectorInt nextPosition = (27, 5);

    private readonly BlockColor[] allBlocks =
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
    /// キックテーブル
    /// </summary>
    private readonly Dictionary<(int fromRot, int toRot), VectorInt[]> kickTable = new()
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
    private readonly Dictionary<(int fromRot, int toRot), VectorInt[]> kickTableI = new()
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

    /// <summary>
    /// 固定猶予リセットの最大数（置くかホールドでリセットする）
    /// </summary>
    private readonly float fixResetMax = 8;
    
    private readonly Random random = new();

    public override void OnStart(Dictionary<string, object> args)
    {
        fieldTileMap = new Tilemap((8, 8));
        currentBlockTileMap = new Tilemap((8, 8));
        uiTileMap = new Tilemap((8, 8));
        Root.AddRange(fieldTileMap, currentBlockTileMap, uiTileMap);
        blockTiles = new Dictionary<BlockColor, ITile>();
        field = new BlockColor[width, height + heightOffset];
        
        InitializeTiles();
        RenderWalls();
        EnqueueNexts();
        RenderHoldNext();
        SpawnNextBlock();

        currentBlockTileMap.Location = fieldTileMap.Location = (
            320 / 2 - width * 8 / 2,
            240 / 2 - height * 8 / 2
            );

        Audio.Play(Resources.BgmTypeA, 0);
    }

    public override void OnUpdate()
    {
        DF.Console.Cls();
        DF.Console.Print($"{Time.Fps}fps\nDAS:{dasTimer:0.000}\nFFD:{freefallDistance:0.000}\nFT:{fixTimer:0.000}\n");

        if (isGameOver)
        {
            if (DFKeyboard.Z.IsKeyUp)
            {
                DF.Router.ChangeScene<TitleScene>();
            }

            return;
        }

        ProcessFreefall();
        ProcessDas();
        ProcessInput();
        ProcessFix();
        
        currentBlockTileMap.Clear();
        var ghostY = RayToDown();
        RenderBlockToTilemap(blockPosition.X, (int)ghostY - heightOffset, CurrentShape, BlockColor.Ghost, currentBlockTileMap);
        RenderBlockToTilemap(blockPosition.X, blockPosition.Y - heightOffset, CurrentShape, currentColor, currentBlockTileMap);

        if (isFieldUpdated)
        {
            RenderField();
            isFieldUpdated = false;
        }
    }

    /// <summary>
    /// 自由落下の制御
    /// </summary>
    private void ProcessFreefall()
    {
        // Note: DASでの落下よりも落下速度が低いときに、DASでの落下中であれば自由落下しない
        if (DFKeyboard.Down.IsPressed && (fallSpeed < (1 / das))) return;
        if (fixTimer > 0) return;

        freefallDistance += MathF.Min(fallSpeed * Time.DeltaTime, 20);
        if (freefallDistance < 1) return;

        var distanceInt = (int)MathF.Floor(freefallDistance);
        blockPosition.Y += distanceInt;
        freefallDistance -= distanceInt;
        
        // 床判定
        while (!CanPlaceBlock(blockPosition.X, blockPosition.Y, CurrentShape))
        {
            blockPosition.Y--;
        }
    }

    /// <summary>
    /// DAS（遅延付き連射入力）の制御
    /// </summary>
    private void ProcessDas()
    {
        isLeftPressed = isRightPressed = isDownPressed = false;
        if (DFKeyboard.Left.IsKeyDown) isLeftPressed = true;
        if (DFKeyboard.Right.IsKeyDown) isRightPressed = true;

        if (DFKeyboard.Left.ElapsedTime >= das)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                isLeftPressed = true;
                dasTimer = 0;
            }
        }
        else if (DFKeyboard.Right.ElapsedTime >= das)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                isRightPressed = true;
                dasTimer = 0;
            }
        }

        if (DFKeyboard.Down)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                isDownPressed = true;
                dasTimer = 0;
            }
        }
        
        if (!DFKeyboard.Left && !DFKeyboard.Right && !DFKeyboard.Down)
        {
            dasTimer = 0;
        }
    }

    /// <summary>
    /// ユーザー入力の処理
    /// </summary>
    private void ProcessInput()
    {
        if (isLeftPressed && CanPlaceBlock(blockPosition.X - 1, blockPosition.Y, CurrentShape))
        {
            blockPosition.X--;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        if (isRightPressed && CanPlaceBlock(blockPosition.X + 1, blockPosition.Y, CurrentShape))
        {
            blockPosition.X++;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        if (isDownPressed && CanPlaceBlock(blockPosition.X, blockPosition.Y + 1, CurrentShape))
        {
            blockPosition.Y++;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }

        if (DFKeyboard.Up.IsKeyDown)
        {
            while (CanPlaceBlock(blockPosition.X, blockPosition.Y + 1, CurrentShape))
            {
                blockPosition.Y++;
            }
            Audio.PlayOneShotAsync(Resources.SfxHardDrop);

            fixTimer = graceTimeForFix;
        }

        // 左回転
        if (DFKeyboard.Z.IsKeyDown)
        {
            ProcessRotateLeft();
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        
        // 右回転
        if (DFKeyboard.X.IsKeyDown)
        {
            ProcessRotateRight();
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        
        // リロード
        if (DFKeyboard.R.IsKeyDown)
        {
            DF.Router.ChangeScene<GameScene>();
        }
        
        // ホールド
        if (DFKeyboard.C.IsKeyDown && canHold)
        {
            ProcessHold();
            Audio.PlayOneShotAsync(Resources.SfxHold);
        }
    }

    /// <summary>
    /// 左回転の処理
    /// </summary>
    private void ProcessRotateLeft()
    {
        var nextRotation = blockRotation - 1;
        if (nextRotation < 0) nextRotation = 3;
        var kickValue = TryKick(nextRotation);
        if (!kickValue.HasValue) return;
        blockPosition += kickValue.Value;
        blockRotation = nextRotation;
        ResetFix();
    }
    
    /// <summary>
    /// 右回転
    /// </summary>
    private void ProcessRotateRight()
    {
        var nextRotation = blockRotation + 1;
        if (nextRotation > 3) nextRotation = 0;
        var kickValue = TryKick(nextRotation);
        if (!kickValue.HasValue) return;
        blockPosition += kickValue.Value;
        blockRotation = nextRotation;
        ResetFix();
    }

    /// <summary>
    ///  ホールドの処理
    /// </summary>
    private void ProcessHold()
    {
        if (currentHold == BlockColor.None)
        {
            currentHold = currentColor;
            SpawnNextBlock();
            canHold = false;
            return;
        }
        (currentHold, currentColor) = (currentColor, currentHold);
        canHold = false;
        RenderHoldNext();
        ResetStateForSpawning();
    }

    /// <summary>
    /// ブロックが床に固定するまでの猶予時間の処理
    /// </summary>
    private void ProcessFix()
    {
        if (!CanPlaceBlock(blockPosition.X, blockPosition.Y + 1, CurrentShape))
        {
            fixTimer += Time.DeltaTime;
        }
        else if (fixTimer > 0)
        {
            ResetFix();
        }
        
        if (fixTimer < graceTimeForFix) return;
        PlaceBlock(blockPosition.X, blockPosition.Y, CurrentShape, currentColor);
        ProcessLineClear();
        SpawnNextBlock();
    }

    /// <summary>
    /// ラインクリア処理
    /// </summary>
    private void ProcessLineClear()
    {
        var cleared = 0;
        for (var y = height + heightOffset - 1; y >= 0; y--)
        {
            var isLineFilled = true;
            for (var x = 0; x < width; x++)
            {
                if (field[x, y] != BlockColor.None) continue;
                isLineFilled = false;
                break;
            }

            if (!isLineFilled) continue;
            cleared++;
            ShiftDownField(y);
            y++;
        }
        
        if (cleared <= 0) return;

        // TODO: ラインクリアの音とかスコアとか
        Audio.PlayOneShotAsync(Resources.SfxLineClear);
    }
    
    /// <summary>
    /// ゲームオーバーの処理
    /// </summary>
    private void ProcessGameOver()
    {
        Audio.Stop();
        isGameOver = true;
        var gameoverText = new TextElement("GAME OVER", 32, DFFontStyle.Normal, Color.Red);
        gameoverText.Location = (320 / 2 - gameoverText.Width / 2, 240 / 2 - gameoverText.Height / 2);
        Audio.Play(Resources.SfxGameOver);
        Root.Add(gameoverText);
    }

    /// <summary>
    /// ネクストのブロックを召喚し、必要ならネクストを追加します。
    /// </summary>
    private void SpawnNextBlock()
    {
        currentColor = nextQueue.Dequeue();
        // NEXTが減ってきたら補充する
        if (nextQueue.Count < 7)
        {
            EnqueueNexts();
        }

        RenderHoldNext();
        ResetStateForSpawning();
        canHold = true;

        if (!CanPlaceBlock(blockPosition.X, blockPosition.Y, CurrentShape))
        {
            ProcessGameOver();
        }
    }

    /// <summary>
    /// フィールドの y 行を消して、それより上の行を下に1ずつずらします。
    /// </summary>
    /// <param name="y"></param>
    private void ShiftDownField(int y)
    {   
        for (var i = y; i >= 0; i--)
        {
            for (var j = 0; j < width; j++)
            {
                field[j, i] = i > 0 ? field[j, i - 1] : BlockColor.None;
            }
        }
        
        for (var i = 0; i < width; i++)
        {
            field[i, 0] = BlockColor.None;
        }

        isFieldUpdated = true;
    }

    /// <summary>
    /// 現在のブロックがそのまま降下したときに到達するY座標を算出します。
    /// </summary>
    private float RayToDown()
    {
        var y = blockPosition.Y;
        while (CanPlaceBlock(blockPosition.X, y + 1, CurrentShape))
        {
            y++;
        }

        return y;
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
        var table = currentColor == BlockColor.I ? kickTableI : kickTable;
        var testCases = table[(blockRotation, nextRotation)];
        foreach (var testCase in testCases)
        {
            if (CanPlaceBlock(blockPosition.X + testCase.X, blockPosition.Y + testCase.Y, Shapes[currentColor][nextRotation]))
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
                field[x + i, y + j] = blockColor;
            }
        }

        isFieldUpdated = true;
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
                if (x + i < 0 || x + i >= width || y + j < 0 || y + j >= height + heightOffset) return false;
                if (field[x + i, y + j] != BlockColor.None) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ネクストに新たなブロックを挿入します。
    /// </summary>
    private void EnqueueNexts()
    {
        foreach (var type in allBlocks.OrderBy(_ => random.Next()))
        {
            nextQueue.Enqueue(type);
        }
    }

    /// <summary>
    /// ブロックのスポーンおよびホールドから引っ張ってきたときに行うリセット処理
    /// </summary>
    private void ResetStateForSpawning()
    {
        fixResetCounter = 0;
        fixTimer = 0;
        blockPosition = (width / 2 - 2, heightOffset - 2);
        blockRotation = 0;
    }

    /// <summary>
    /// 現在のフィールドをタイルマップにレンダリングします。
    /// </summary>
    private void RenderField()
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height + heightOffset; y++)
            {
                fieldTileMap[x, y - heightOffset] = field[x, y] == BlockColor.None ? null : blockTiles[field[x, y]];
            }
        }
    }

    /// <summary>
    /// 指定したブロックをタイルマップにレンダリングします。
    /// </summary>
    /// <param name="x">タイルマップのX</param>
    /// <param name="y">タイルマップのY</param>
    /// <param name="blockShape">ブロック形状</param>
    /// <param name="blockColor">ブロックの色</param>
    /// <param name="map">タイルマップ</param>
    private void RenderBlockToTilemap(int x, int y, bool[,] blockShape, BlockColor blockColor, Tilemap map)
    {
        var tile = blockTiles[blockColor];
        for (var i = 0; i < blockShape.GetLength(0); i++)
        {
            for (var j = 0; j < blockShape.GetLength(1); j++)
            {
                if (!blockShape[i, j]) continue;
                map[x + i, y + j] = tile;
            }
        }
    }

    /// <summary>
    /// 壁をタイルマップに描画します。
    /// </summary>
    private void RenderWalls()
    {
        // 縦
        for (var y = 0; y <= height; y++)
        {
            fieldTileMap[-1, y] = wallTile;
            fieldTileMap[width, y] = wallTile;
        }
        
        // 横
        for (var x = 0; x < width; x++)
        {
            fieldTileMap[x, height] = wallTile;
        }
    }

    /// <summary>
    /// ブロックや壁のテクスチャから、<see cref="Tile"/> を生成します。
    /// </summary>
    private void InitializeTiles()
    {
        foreach (var (type, texture) in Resources.Block)
        {
            blockTiles[type] = new Tile(texture);
        }

        wallTile = new Tile(Resources.Wall);
    }

    /// <summary>
    /// ホールド、ネクストを画面上にレンダリングします。
    /// </summary>
    private void RenderHoldNext()
    {
        uiTileMap.Clear();
        if (currentHold != BlockColor.None)
        {
            RenderBlockToTilemap(holdPosition.X, holdPosition.Y, Shapes[currentHold][0], canHold ? currentHold : BlockColor.Ghost, uiTileMap);
        }

        var i = 0;
        foreach (var type in nextQueue.Take(4))
        {
            RenderBlockToTilemap(nextPosition.X, nextPosition.Y + i * 4, Shapes[type][0], type, uiTileMap);
            i++;
        }
    }
}