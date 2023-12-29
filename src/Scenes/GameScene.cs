using System.Drawing;
using DotFeather;
using Sukiteto;

using static Sukiteto.Global;

namespace SukiTeto;

public class GameScene : Scene
{
    private bool[,] MinoMatrix => Minos[currentMino][minoRotation];
    
    /// <summary>
    /// フィールドの表示に使うタイルマップ
    /// </summary>
    private Tilemap fieldTileMap;

    /// <summary>
    /// 現在のミノの表示に使うタイルマップ
    /// </summary>
    private Tilemap currentMinoTileMap;

    /// <summary>
    /// NEXT, HOLD などの表示に使うタイルマップ
    /// </summary>
    private Tilemap uiTileMap;

    /// <summary>
    /// ミノテクスチャのタイルデータ
    /// </summary>
    private Dictionary<MinoType, ITile> minoTiles;
    
    /// <summary>
    /// 壁のタイルデータ
    /// </summary>
    private ITile wallTile;

    /// <summary>
    /// フィールド
    /// </summary>
    private MinoType[,] field;

    /// <summary>
    /// フィールドの幅
    /// </summary>
    private int width = 10;
        
    /// <summary>
    /// フィールドの高さ
    /// </summary>
    private int height = 20;

    // 窒息高度より上にどれくらい積めるか
    // これを超えようとするか、窒息高度付近でミノを召喚できなくなったらゲームオーバーとなる
    private int heightOffset = 6;
    
    /// <summary>
    /// 現在ホールドしているミノ
    /// </summary>
    private MinoType currentHold = MinoType.None;
    
    /// <summary>
    /// 次に出てくるミノのキュー
    /// </summary>
    private Queue<MinoType> nextQueue = new Queue<MinoType>();

    private MinoType currentMino = MinoType.None;

    /// <summary>
    /// ミノの現在位置
    /// </summary>
    private VectorInt minoPosition = (0, 0);

    /// <summary>
    /// ミノの現在の回転値
    /// </summary>
    private int minoRotation = 0;

    /// <summary>
    /// フィールドが更新されたかどうか
    /// </summary>
    private bool isFieldUpdated;

    /// <summary>
    /// ミノの速度
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
    /// 長押ししてからミノが動き始めるまでの時間
    /// </summary>
    private float das = 1f / 60 * 10;
    
    /// <summary>
    /// 長押し中のミノの移動速度
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

    private readonly MinoType[] allMinos =
    {
        MinoType.I,
        MinoType.J,
        MinoType.L,
        MinoType.O,
        MinoType.S,
        MinoType.T,
        MinoType.Z
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
    /// キックテーブル（Iミノ用）
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
    
    private readonly Random random = new Random();

    public override void OnStart(Dictionary<string, object> args)
    {
        fieldTileMap = new Tilemap((8, 8));
        currentMinoTileMap = new Tilemap((8, 8));
        uiTileMap = new Tilemap((8, 8));
        Root.AddRange(fieldTileMap, currentMinoTileMap, uiTileMap);
        minoTiles = new Dictionary<MinoType, ITile>();
        field = new MinoType[width, height + heightOffset];
        
        InitializeTiles();
        RenderWalls();
        EnqueueNexts();
        RenderHoldNext();
        SpawnMino();

        currentMinoTileMap.Location = fieldTileMap.Location = (
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
        
        currentMinoTileMap.Clear();
        var ghostY = RayToDown();
        RenderMinoToTilemap(minoPosition.X, (int)ghostY - heightOffset, MinoMatrix, MinoType.Ghost, currentMinoTileMap);
        RenderMinoToTilemap(minoPosition.X, minoPosition.Y - heightOffset, MinoMatrix, currentMino, currentMinoTileMap);

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
        if (DFKeyboard.Down.IsPressed) return;
        if (fixTimer > 0) return;

        freefallDistance += MathF.Min(fallSpeed * Time.DeltaTime, 20);
        if (freefallDistance < 1) return;

        var distanceInt = (int)MathF.Floor(freefallDistance);
        minoPosition.Y += distanceInt;
        freefallDistance -= distanceInt;
        
        // 床判定
        while (!CanPlaceMino(minoPosition.X, minoPosition.Y, MinoMatrix))
        {
            minoPosition.Y--;
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
        if (isLeftPressed && CanPlaceMino(minoPosition.X - 1, minoPosition.Y, MinoMatrix))
        {
            minoPosition.X--;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        if (isRightPressed && CanPlaceMino(minoPosition.X + 1, minoPosition.Y, MinoMatrix))
        {
            minoPosition.X++;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        if (isDownPressed && CanPlaceMino(minoPosition.X, minoPosition.Y + 1, MinoMatrix))
        {
            minoPosition.Y++;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }

        if (DFKeyboard.Up.IsKeyDown)
        {
            while (CanPlaceMino(minoPosition.X, minoPosition.Y + 1, MinoMatrix))
            {
                minoPosition.Y++;
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
        var nextRotation = minoRotation - 1;
        if (nextRotation < 0) nextRotation = 3;
        var kickValue = TryKick(nextRotation);
        if (!kickValue.HasValue) return;
        minoPosition += kickValue.Value;
        minoRotation = nextRotation;
        ResetFix();
    }
    
    /// <summary>
    /// 右回転
    /// </summary>
    private void ProcessRotateRight()
    {
        var nextRotation = minoRotation + 1;
        if (nextRotation > 3) nextRotation = 0;
        var kickValue = TryKick(nextRotation);
        if (!kickValue.HasValue) return;
        minoPosition += kickValue.Value;
        minoRotation = nextRotation;
        ResetFix();
    }

    /// <summary>
    ///  ホールドの処理
    /// </summary>
    private void ProcessHold()
    {
        if (currentHold == MinoType.None)
        {
            currentHold = currentMino;
            SpawnMino();
            canHold = false;
            return;
        }
        (currentHold, currentMino) = (currentMino, currentHold);
        canHold = false;
        RenderHoldNext();
        ResetStateForSpawning();
    }

    /// <summary>
    /// ミノが床に固定するまでの猶予時間の処理
    /// </summary>
    private void ProcessFix()
    {
        if (!CanPlaceMino(minoPosition.X, minoPosition.Y + 1, MinoMatrix))
        {
            fixTimer += Time.DeltaTime;
        }
        else if (fixTimer > 0)
        {
            ResetFix();
        }
        
        if (fixTimer < graceTimeForFix) return;
        PlaceMino(minoPosition.X, minoPosition.Y, MinoMatrix, currentMino);
        ProcessLineClear();
        SpawnMino();
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
                if (field[x, y] != MinoType.None) continue;
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
    /// ネクストのミノを召喚し、必要ならネクストを追加します。
    /// </summary>
    private void SpawnMino()
    {
        currentMino = nextQueue.Dequeue();
        // NEXTが減ってきたら補充する
        if (nextQueue.Count < 7)
        {
            EnqueueNexts();
        }

        RenderHoldNext();
        ResetStateForSpawning();
        canHold = true;

        if (!CanPlaceMino(minoPosition.X, minoPosition.Y, MinoMatrix))
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
                field[j, i] = i > 0 ? field[j, i - 1] : MinoType.None;
            }
        }
        
        for (var i = 0; i < width; i++)
        {
            field[i, 0] = MinoType.None;
        }

        isFieldUpdated = true;
    }

    /// <summary>
    /// 現在のミノがそのまま降下したときに到達するY座標を算出します。
    /// </summary>
    private float RayToDown()
    {
        var y = minoPosition.Y;
        while (CanPlaceMino(minoPosition.X, y + 1, MinoMatrix))
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
    /// キックテーブルを元にミノのキックを試みます。
    /// </summary>
    /// <param name="nextRotation">試行する回転</param>
    /// <returns>キックが成功した場合はその相対座標、失敗した場合は<c>null</c>。</returns>
    private VectorInt? TryKick(int nextRotation)
    {
        var table = currentMino == MinoType.I ? kickTableI : kickTable;
        var testCases = table[(minoRotation, nextRotation)];
        foreach (var testCase in testCases)
        {
            if (CanPlaceMino(minoPosition.X + testCase.X, minoPosition.Y + testCase.Y, Minos[currentMino][nextRotation]))
            {
                return testCase;
            }
        }

        return null;
    }

    /// <summary>
    /// ミノを指定した位置に配置します。
    /// </summary>
    /// <param name="x">フィールドのX</param>
    /// <param name="y">フィールドのY</param>
    /// <param name="mino">ミノ マトリックス</param>
    /// <param name="minoType">ミノの色</param>
    private void PlaceMino(int x, int y, bool[,] mino, MinoType minoType)
    {
        for (var i = 0; i < mino.GetLength(0); i++)
        {
            for (var j = 0; j < mino.GetLength(1); j++)
            {
                if (!mino[i, j]) continue;
                field[x + i, y + j] = minoType;
            }
        }

        isFieldUpdated = true;
    }
    
    /// <summary>
    /// ミノをその位置に配置できるかどうかを算出します。
    /// </summary>
    /// <param name="x">フィールド X</param>
    /// <param name="y">フィールド Y</param>
    /// <param name="mino">ミノ マトリックス</param>
    /// <returns>配置できる（衝突しない）場合は<c>true</c>を、衝突してしまう場合は<c>false</c>を返します。</returns>
    private bool CanPlaceMino(int x, int y, bool[,] mino)
    {
        for (var i = 0; i < mino.GetLength(0); i++)
        {
            for (var j = 0; j < mino.GetLength(1); j++)
            {
                if (!mino[i, j]) continue;
                if (x + i < 0 || x + i >= width || y + j < 0 || y + j >= height + heightOffset) return false;
                if (field[x + i, y + j] != MinoType.None) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ネクストに新たなミノを挿入します。
    /// </summary>
    private void EnqueueNexts()
    {
        foreach (var type in allMinos.OrderBy(_ => random.Next()))
        {
            nextQueue.Enqueue(type);
        }
    }

    /// <summary>
    /// ミノのスポーンおよびホールドから引っ張ってきたときに行うリセット処理
    /// </summary>
    private void ResetStateForSpawning()
    {
        fixResetCounter = 0;
        fixTimer = 0;
        minoPosition = (width / 2 - 2, heightOffset - 2);
        minoRotation = 0;
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
                fieldTileMap[x, y - heightOffset] = field[x, y] == MinoType.None ? null : minoTiles[field[x, y]];
            }
        }
    }

    /// <summary>
    /// 指定したミノをタイルマップにレンダリングします。
    /// </summary>
    /// <param name="x">タイルマップのX</param>
    /// <param name="y">タイルマップのY</param>
    /// <param name="mino">ミノ マトリックス</param>
    /// <param name="minoType">ミノのタイプ</param>
    /// <param name="map">タイルマップ</param>
    private void RenderMinoToTilemap(int x, int y, bool[,] mino, MinoType minoType, Tilemap map)
    {
        var tile = minoTiles[minoType];
        for (var i = 0; i < mino.GetLength(0); i++)
        {
            for (var j = 0; j < mino.GetLength(1); j++)
            {
                if (!mino[i, j]) continue;
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
    /// ミノや壁のテクスチャから、<see cref="Tile"/> を生成します。
    /// </summary>
    private void InitializeTiles()
    {
        foreach (var (type, texture) in Resources.Mino)
        {
            minoTiles[type] = new Tile(texture);
        }

        wallTile = new Tile(Resources.Wall);
    }

    /// <summary>
    /// ホールド、ネクストを画面上にレンダリングします。
    /// </summary>
    private void RenderHoldNext()
    {
        uiTileMap.Clear();
        if (currentHold != MinoType.None)
        {
            RenderMinoToTilemap(holdPosition.X, holdPosition.Y, Minos[currentHold][0], canHold ? currentHold : MinoType.Ghost, uiTileMap);
        }

        var i = 0;
        foreach (var type in nextQueue.Take(4))
        {
            RenderMinoToTilemap(nextPosition.X, nextPosition.Y + i * 4, Minos[type][0], type, uiTileMap);
            i++;
        }
    }
}