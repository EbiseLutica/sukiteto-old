using System.Collections;
using System.Drawing;
using System.Text;
using Promete;
using Promete.Audio;
using Promete.Elements;
using Promete.Graphics;
using Promete.Input;
using Promete.Windowing;

namespace Sukiteto;

public class GameScene(
    PrometeApp app,
    IWindow window,
    AudioPlayer audio,
    ConsoleLayer console,
    GlyphRenderer glyphRenderer,
    Resources resources,
    InputService input,
    GameService game,
    ShapeLoader shapes
    ) : Scene
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

    private bool isPausingGame = false;

    /// <summary>
    /// 長押ししてからブロックが動き始めるまでの時間
    /// </summary>
    private static readonly float das = 1f / 60 * 10;
    
    /// <summary>
    /// 長押し中のブロックの移動速度
    /// </summary>
    private static readonly float arr = 1f / 60 * 2;

    public override void OnStart()
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

        audio.Gain = 0.4f;
        audio.Play(resources.BgmTypeA, 0);
    }

    public override void OnUpdate()
    {
        console.Clear();
        console.Print($"{window.FramePerSeconds}fps");

        if (isGameOver)
        {
            if (input[InputType.Ok].IsButtonUp)
            {
                app.LoadScene<TitleScene>();
            }

            return;
        }

        if (isPausingGame) return;
        ProcessDas();
        ProcessInput();
        game.Tick(window.DeltaTime);

        RenderCurrentBlock();
    }

    public override void OnDestroy()
    {
        audio.Stop();
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
        audio.PlayOneShotAsync(resources.SfxTspinRotate);
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
        audio.PlayOneShotAsync(resources.SfxHit);
    }

    private void OnLineClear(LineClearEventArgs e)
    {
        audio.PlayOneShotAsync(resources.GetLineClearSound(e));
        isPausingGame = true;
        _ = AnimateLineClear(e);

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
        
        var effect = new EffectedText(text, 24, FontStyle.Normal, Color.White)
        {
            Effect = EffectedText.EffectType.SlideUp,
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
        if (input[InputType.MoveLeft].IsButtonDown) moved = game.TriggerLeft();
        if (input[InputType.MoveRight].IsButtonDown) moved = game.TriggerRight();

        if (input[InputType.MoveLeft].ElapsedTime >= das)
        {
            dasTimer += window.DeltaTime;
            if (dasTimer > arr)
            {
                moved = game.TriggerLeft();
                dasTimer = 0;
            }
        }
        else if (input[InputType.MoveRight].ElapsedTime >= das)
        {
            dasTimer += window.DeltaTime;
            if (dasTimer > arr)
            {
                moved = game.TriggerRight();
                dasTimer = 0;
            }
        }

        if (input[InputType.SoftDrop])
        {
            dasTimer += window.DeltaTime;
            if (dasTimer > arr)
            {
                moved = game.TriggerDown();
                dasTimer = 0;
            }
        }
        
        if (!input[InputType.MoveLeft] && !input[InputType.MoveRight] && !input[InputType.SoftDrop])
        {
            dasTimer = 0;
        }
        
        if (moved) audio.PlayOneShotAsync(resources.SfxMove);
    }

    /// <summary>
    /// ユーザー入力の処理
    /// </summary>
    private void ProcessInput()
    {
        if (input[InputType.HardDrop].IsButtonDown)
        {
            game.TriggerHardDrop();
            audio.PlayOneShotAsync(resources.SfxHardDrop);
        }

        // 左回転
        if (input[InputType.RotateLeft].IsButtonDown && game.TriggerRotateLeft())
        {
            audio.PlayOneShotAsync(resources.SfxMove);
        }
        
        // 右回転
        if (input[InputType.RotateRight].IsButtonDown && game.TriggerRotateRight())
        {
            audio.PlayOneShotAsync(resources.SfxMove);
        }
        
        // リロード
        if (input[InputType.Quit].IsButtonDown)
        {
            app.LoadScene<TitleScene>();
        }
        
        // ホールド
        if (input[InputType.Hold].IsButtonDown && game.TriggerHold())
        {
            audio.PlayOneShotAsync(resources.SfxHold);
        }
    }
    
    /// <summary>
    /// ゲームオーバーの処理
    /// </summary>
    private void ProcessGameOver()
    {
        audio.Stop();
        var gameoverText = new Text("GAME OVER", Font.GetDefault(64, FontStyle.Normal), Color.Red);
        gameoverText.Location = (640 / 2 - gameoverText.Width / 2, 480 / 2 - gameoverText.Height / 2);
        audio.Play(resources.SfxGameOver);
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
    
    private async Task AnimateLineClear(LineClearEventArgs e)
    {
        isPausingGame = true;
        ProcessLine();
        await Task.Delay(500);
        audio.PlayOneShotAsync(resources.SfxLineClearFix);
        RenderField();
        isPausingGame = false;
        return;

        void ProcessLine()
        {
            var span = e.ClearedLineIndices.Span;
            foreach (var y in span)
            {
                for (var x = 0; x < game.Width; x++)
                {
                    fieldTileMap[x, y] = null;
                }
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
        foreach (var (type, texture) in resources.Block)
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
            RenderBlockToTilemap(holdPosition.X, holdPosition.Y, shapes[game.CurrentHold][0], game.CanHold ? game.CurrentHold : BlockColor.Ghost, uiTileMap);
        }

        var i = 0;
        foreach (var type in game.NextQueue.Take(4))
        {
            RenderBlockToTilemap(nextPosition.X, nextPosition.Y + i * 4, shapes[type][0], type, uiTileMap);
            i++;
        }
    }
}