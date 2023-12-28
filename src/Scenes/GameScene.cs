using System.Collections.Generic;
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
    /// ソフトドロップタイマー
    /// </summary>
    private float softDropTimer;

    /// <summary>
    /// ホールドできるかどうか
    /// </summary>
    private bool canHold = true;

    private bool isGameOver;

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
        BuildWalls();
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
        DF.Console.Print($"FPS: {Time.Fps}");

        if (isGameOver)
        {
            if (DFKeyboard.Z.IsKeyUp)
            {
                DF.Router.ChangeScene<TitleScene>();
            }

            return;
        }

        ProcessFreefall();
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

    private void ProcessInput()
    {
        if (DFKeyboard.Left.IsKeyDown && CanPlaceMino(minoPosition.X - 1, minoPosition.Y, MinoMatrix))
        {
            minoPosition.X--;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        if (DFKeyboard.Right.IsKeyDown && CanPlaceMino(minoPosition.X + 1, minoPosition.Y, MinoMatrix))
        {
            minoPosition.X++;
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        if (DFKeyboard.Down.IsPressed && CanPlaceMino(minoPosition.X, minoPosition.Y + 1, MinoMatrix))
        {
            softDropTimer += Time.DeltaTime;
            if (softDropTimer > 0.01f)
            {
                minoPosition.Y++;
                Audio.PlayOneShotAsync(Resources.SfxMove);
                softDropTimer = 0;
            }
        }
        else
        {
            softDropTimer = 0;
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
            RotateLeft();
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        
        // 右回転
        if (DFKeyboard.X.IsKeyDown)
        {
            RotateRight();
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

    private void ProcessHold()
    {
        if (currentHold == MinoType.None)
        {
            currentHold = currentMino;
            SpawnMino();
            return;
        }
        (currentHold, currentMino) = (currentMino, currentHold);
        canHold = false;
        RenderHoldNext();
        minoPosition = (width / 2 - 2, heightOffset - 2);
        minoRotation = 0;
    }

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

    private float RayToDown()
    {
        var y = minoPosition.Y;
        while (CanPlaceMino(minoPosition.X, y + 1, MinoMatrix))
        {
            y++;
        }

        return y;
    }

    private void ProcessFix()
    {
        if (!CanPlaceMino(minoPosition.X, minoPosition.Y + 1, MinoMatrix))
        {
            fixTimer += Time.DeltaTime;
        }
        else
        {
            fixTimer = 0;
        }
        
        if (fixTimer < graceTimeForFix) return;
        fixTimer = 0;
        PlaceMino(minoPosition.X, minoPosition.Y, MinoMatrix, currentMino);
        ProcessLineClear();
        SpawnMino();
    }
    
    private void ProcessGameOver()
    {
        Audio.Stop();
        isGameOver = true;
        var gameoverText = new TextElement("GAME OVER", 32, DFFontStyle.Normal, Color.Red);
        gameoverText.Location = (320 / 2 - gameoverText.Width / 2, 240 / 2 - gameoverText.Height / 2);
        Audio.Play(Resources.SfxGameOver);
        Root.Add(gameoverText);
    }

    private void RotateLeft()
    {
        var nextRotation = minoRotation - 1;
        if (nextRotation < 0) nextRotation = 3;
        if (!CanPlaceMino(minoPosition.X, minoPosition.Y, Minos[currentMino][nextRotation])) return;
        minoRotation = nextRotation;
    }
    
    private void RotateRight()
    {
        var nextRotation = minoRotation + 1;
        if (nextRotation > 3) nextRotation = 0;
        if (!CanPlaceMino(minoPosition.X, minoPosition.Y, Minos[currentMino][nextRotation])) return;
        minoRotation = nextRotation;
    }

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

    private void BuildWalls()
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

    private void InitializeTiles()
    {
        foreach (var (type, texture) in Resources.Mino)
        {
            minoTiles[type] = new Tile(texture);
        }

        wallTile = new Tile(Resources.Wall);
    }

    private void EnqueueNexts()
    {
        foreach (var type in allMinos.OrderBy(_ => random.Next()))
        {
            nextQueue.Enqueue(type);
        }
    }

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

    private void SpawnMino()
    {
        currentMino = nextQueue.Dequeue();
        if (nextQueue.Count < 7)
        {
            EnqueueNexts();
        }

        canHold = true;
        RenderHoldNext();
        minoPosition = (width / 2 - 2, heightOffset - 2);
        minoRotation = 0;
        if (!CanPlaceMino(minoPosition.X, minoPosition.Y, MinoMatrix))
        {
            ProcessGameOver();
        }
    }
}