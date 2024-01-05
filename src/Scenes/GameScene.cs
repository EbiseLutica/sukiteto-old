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
    
    /// <summary>
    /// 壁のタイルデータ
    /// </summary>
    private ITile wallTile;
    
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
        fieldTileMap = new Tilemap((8, 8));
        currentBlockTileMap = new Tilemap((8, 8));
        uiTileMap = new Tilemap((8, 8));
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
            320 / 2 - game.Width * 8 / 2f,
            240 / 2 - game.Height * 8 / 2f - game.HeightOffset * 8
            );

        Audio.Play(Resources.BgmTypeA, 0);
    }

    public override void OnUpdate()
    {
        DF.Console.Cls();
        DF.Console.Print($"{Time.Fps}fps");

        if (isGameOver)
        {
            if (DFKeyboard.Z.IsKeyUp)
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

    private void OnHold()
    {
        RenderHoldNext();
        Audio.PlayOneShotAsync(Resources.SfxHold);
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
        
        var effect = new EffectedTextElement(text, 18, DFFontStyle.Normal, Color.White)
        {
            Effect = EffectedTextElement.EffectType.SlideUp,
            EffectTime = 1,
            Location = (48, 160)
        };
        
        Root.Add(effect);
    }

    /// <summary>
    /// DAS（遅延付き連射入力）の制御
    /// </summary>
    private void ProcessDas()
    {
        if (DFKeyboard.Left.IsKeyDown) game.TriggerLeft();
        if (DFKeyboard.Right.IsKeyDown) game.TriggerRight();

        if (DFKeyboard.Left.ElapsedTime >= das)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                game.TriggerLeft();
                Audio.PlayOneShotAsync(Resources.SfxMove);
                dasTimer = 0;
            }
        }
        else if (DFKeyboard.Right.ElapsedTime >= das)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                game.TriggerRight();
                Audio.PlayOneShotAsync(Resources.SfxMove);
                dasTimer = 0;
            }
        }

        if (DFKeyboard.Down)
        {
            dasTimer += Time.DeltaTime;
            if (dasTimer > arr)
            {
                game.TriggerDown();
                Audio.PlayOneShotAsync(Resources.SfxMove);
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
        if (DFKeyboard.Up.IsKeyDown)
        {
            game.TriggerHardDrop();
            Audio.PlayOneShotAsync(Resources.SfxHardDrop);
        }

        // 左回転
        if (DFKeyboard.Z.IsKeyDown)
        {
            game.TriggerRotateLeft();
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        
        // 右回転
        if (DFKeyboard.X.IsKeyDown)
        {
            game.TriggerRotateRight();
            Audio.PlayOneShotAsync(Resources.SfxMove);
        }
        
        // リロード
        if (DFKeyboard.R.IsKeyDown)
        {
            DF.Router.ChangeScene<GameScene>();
        }
        
        // ホールド
        if (DFKeyboard.C.IsKeyDown)
        {
            game.TriggerHold();
        }
    }
    
    /// <summary>
    /// ゲームオーバーの処理
    /// </summary>
    private void ProcessGameOver()
    {
        Audio.Stop();
        var gameoverText = new TextElement("GAME OVER", 32, DFFontStyle.Normal, Color.Red);
        gameoverText.Location = (320 / 2 - gameoverText.Width / 2, 240 / 2 - gameoverText.Height / 2);
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

        wallTile = new Tile(Resources.Wall);
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