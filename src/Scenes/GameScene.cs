using System.Collections;
using System.Drawing;
using System.Text;
using DotFeather;

using static Sukiteto.Global;

namespace Sukiteto;

public class GameScene : Scene
{
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
    
    private bool isGameOver;
    
    private float dasTimer;

    private readonly VectorInt holdPosition = (9, 5);

    private readonly VectorInt nextPosition = (27, 5);

    private readonly GameService game = new();

    private bool isPausingGame = false;

    /// <summary>
    /// 長押ししてからブロックが動き始めるまでの時間
    /// </summary>
    private static readonly float das = 1f / 60 * 10;
    
    /// <summary>
    /// 長押し中のブロックの移動速度
    /// </summary>
    private static readonly float arr = 1f / 60 * 2;

    public override void OnStart(Dictionary<string, object> args)
    {
        fieldTileMap = new Tilemap((16, 16));
        currentBlockTileMap = new Tilemap((16, 16));
        uiTileMap = new Tilemap((16, 16));
        Root.AddRange(fieldTileMap, currentBlockTileMap, uiTileMap);
        blockTiles = new Dictionary<BlockColor, ITile>();

        game.Hold += OnHold;
        game.SpawnNext += OnSpawnNext;
        game.LineClear += OnLineClear;
        game.BlockHit += OnBlockHit;
        game.BlockPlace += OnBlockPlace;
        game.GameOver += OnGameOver;
        game.TspinRotate += OnTspinRotate;

        InitializeTiles();
        RenderWalls();

        game.Start();

        currentBlockTileMap.Location = fieldTileMap.Location = (
            640 / 2 - game.Width * 16 / 2f,
            480 / 2 - game.Height * 16 / 2f - game.HeightOffset * 16
            );

        Audio.Play(Resources.BgmTypeA, 0);
    }

    public override void OnUpdate()
    {
        DF.Console.Cls();
        DF.Console.Print($"{Time.Fps}fps");

        if (isGameOver)
        {
            if (Keys.KeyOk.IsKeyUp)
            {
                DF.Router.ChangeScene<TitleScene>();
            }

            return;
        }

        if (isPausingGame) return;
        ProcessDas();
        ProcessInput();
        game.Tick(Time.DeltaTime);

        RenderCurrentBlock();
    }

    public override void OnDestroy()
    {
        Audio.Stop();
    }

    private void OnHold()
    {
        RenderHoldNext();
    }

    private void OnSpawnNext()
    {
        RenderHoldNext();
    }

    private void OnTspinRotate()
    {
        Audio.PlayOneShotAsync(Resources.SfxTspinRotate);
    }

    private void OnGameOver()
    {
        isGameOver = true;
        ProcessGameOver();
    }

    private void OnBlockPlace()
    {
        RenderField();
    }

    private void OnBlockHit()
    {
        Audio.PlayOneShotAsync(Resources.SfxHit);
    }

    private void OnLineClear(LineClearEventArgs e)
    {
        Audio.PlayOneShotAsync(Resources.GetLineClearSound(e));
        isPausingGame = true;
        CoroutineRunner.Start(AnimateLineClear(e));

        var builder = new StringBuilder();

        if (e.IsTSpin) builder.AppendLine(e.IsTSpinMini ? "T-Spin Mini" : "T-Spin");

        switch (e.ClearedLines)
        {
            case 4:
                builder.AppendLine("QUAD");
                break;
            case 3:
                builder.AppendLine("TRIPLE");
                break;
            case 2:
                builder.AppendLine("DOUBLE");
                break;
            case 1 when e.IsTSpin:
                builder.AppendLine("SINGLE");
                break;
        }

        var text = builder.ToString();

        if (string.IsNullOrWhiteSpace(text)) return;
        
        var effect = new EffectedTextElement(text, 24, DFFontStyle.Normal, Color.White)
        {
            Effect = EffectedTextElement.EffectType.SlideUp,
            EffectTime = 1,
            Location = holdPosition * 16 + (0, 96),
        };
        
        Root.Add(effect);
    }

    /// <summary>
    /// DAS（遅延付き連射入力）の制御
    /// </summary>
    private void ProcessDas()
    {
        var moved = false;
        if (Keys.KeyMoveLeft.IsKeyDown) moved = game.TriggerLeft();
        if (Keys.KeyMoveRight.IsKeyDown) moved = game.TriggerRight();

        if (Keys.KeyMoveLeft.ElapsedTime >= das)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                moved = game.TriggerLeft();
                dasTimer = 0;
            }
        }
        else if (Keys.KeyMoveRight.ElapsedTime >= das)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                moved = game.TriggerRight();
                dasTimer = 0;
            }
        }

        if (Keys.KeySoftDrop)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                moved = game.TriggerDown();
                dasTimer = 0;
            }
        }
        
        if (!Keys.KeyMoveLeft && !Keys.KeyMoveRight && !Keys.KeySoftDrop)
        {
            dasTimer = 0;
        }
        
        if (moved) Audio.PlayOneShotAsync(Resources.SfxMove);
    }

    /// <summary>
    /// ユーザー入力の処理
    /// </summary>
    private void ProcessInput()
    {
        if (Keys.KeyHardDrop.IsKeyDown)
        {
            game.TriggerHardDrop();
            Audio.PlayOneShotAsync(Resources.SfxHardDrop);
        }

        // 左回転
        if (Keys.KeyRotateLeft.IsKeyDown && game.TriggerRotateLeft())
        {
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        
        // 右回転
        if (Keys.KeyRotateRight.IsKeyDown && game.TriggerRotateRight())
        {
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        
        // リロード
        if (Keys.KeyQuit.IsKeyDown)
        {
            DF.Router.ChangeScene<TitleScene>();
        }
        
        // ホールド
        if (Keys.KeyHold.IsKeyDown && game.TriggerHold())
        {
            Audio.PlayOneShotAsync(Resources.SfxHold);
        }
    }
    
    /// <summary>
    /// ゲームオーバーの処理
    /// </summary>
    private void ProcessGameOver()
    {
        Audio.Stop();
        var gameoverText = new TextElement("GAME OVER", 64, DFFontStyle.Normal, Color.Red);
        gameoverText.Location = (640 / 2 - gameoverText.Width / 2, 480 / 2 - gameoverText.Height / 2);
        Audio.Play(Resources.SfxGameOver);
        Root.Add(gameoverText);
    }

    /// <summary>
    /// 現在のフィールドをタイルマップにレンダリングします。
    /// </summary>
    private void RenderField()
    {
        for (var x = 0; x < game.Width; x++)
        {
            for (var y = 0; y < game.Height + game.HeightOffset; y++)
            {
                fieldTileMap[x, y] = game.Field[x, y] == BlockColor.None ? null : blockTiles[game.Field[x, y]];
            }
        }
    }
    
    private void RenderCurrentBlock()
    {
        currentBlockTileMap.Clear();
        if (isPausingGame) return;
        var ghostY = game.RayToDown();
        var pos = game.BlockPosition;
        RenderBlockToTilemap(pos.X, ghostY, game.CurrentShape, BlockColor.Ghost, currentBlockTileMap);
        RenderBlockToTilemap(pos.X, pos.Y, game.CurrentShape, game.CurrentBlockColor, currentBlockTileMap);
    }
    
    private IEnumerator AnimateLineClear(LineClearEventArgs e)
    {
        isPausingGame = true;
        var span = e.ClearedLineIndices.Span;
        for (var i = 0; i < span.Length; i++)
        {
            var y = span[i];
            for (var x = 0; x < game.Width; x++)
            {
                fieldTileMap[x, y] = null;
            }
        }
        yield return new WaitForSeconds(0.5f);
        Audio.PlayOneShotAsync(Resources.SfxLineClearFix);
        RenderField();
        isPausingGame = false;
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
        var wallTile = blockTiles[BlockColor.Wall];
        // 縦
        for (var y = 0; y <= game.Height; y++)
        {
            fieldTileMap[-1, y + game.HeightOffset] = wallTile;
            fieldTileMap[game.Width, y + game.HeightOffset] = wallTile;
        }
        
        // 横
        for (var x = 0; x < game.Width; x++)
        {
            fieldTileMap[x, game.Height + game.HeightOffset] = wallTile;
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
    }

    /// <summary>
    /// ホールド、ネクストを画面上にレンダリングします。
    /// </summary>
    private void RenderHoldNext()
    {
        uiTileMap.Clear();
        if (game.CurrentHold != BlockColor.None)
        {
            RenderBlockToTilemap(holdPosition.X, holdPosition.Y, Shapes[game.CurrentHold][0], game.CanHold ? game.CurrentHold : BlockColor.Ghost, uiTileMap);
        }

        var i = 0;
        foreach (var type in game.NextQueue.Take(4))
        {
            RenderBlockToTilemap(nextPosition.X, nextPosition.Y + i * 4, Shapes[type][0], type, uiTileMap);
            i++;
        }
    }
}