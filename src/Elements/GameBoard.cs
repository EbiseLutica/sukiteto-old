using System.Collections.ObjectModel;
using System.Drawing;
using Promete;
using Promete.Elements;
using Promete.Graphics;
using Promete.Windowing;

namespace Sukiteto;

public class GameBoard : ContainableElementBase
{
    private readonly GameService _game;
    private readonly Dictionary<BlockColor, ITile> _blockTiles;
    private readonly Tilemap _fieldMap;
    private readonly Tilemap _currentBlockMap;

    private readonly BlockView _holdView;
    private readonly BlockView[] _nextViews;

    public Dictionary<string, string> ScoreboardLeft { get; } = [];
    public Dictionary<string, string> ScoreboardRight { get; } = [];

    private readonly Text _scoreboardLeftText = new Text("");
    private readonly Text _scoreboardRightText = new Text("");
    
    private readonly GlyphRenderer glyphRenderer = PrometeApp.Current?.GetPlugin<GlyphRenderer>() ?? throw new InvalidOperationException("Promete is not initialized");

    public GameBoard(GameService game)
    {
        _game = game;
        _blockTiles = PrometeApp.Current?.GetPlugin<Resources>()?.BlockTiles ?? throw new InvalidOperationException("Promete is not initialized");
        
        var holdText = new Text("HOLD", Font.GetDefault(16), Color.White)
            .Location(0, 24);
        _holdView = new BlockView(game.Shapes)
            .Location(0, 48);
        _fieldMap = new Tilemap((16, 16))
            .Location(11 * 8, 32 - _game.Config.TopMargin * 16);
        _currentBlockMap = new Tilemap((16, 16))
            .Location(11 * 8, 32 - _game.Config.TopMargin * 16);
        _scoreboardLeftText.Location = (0, 160);

        var boardRight = (11 * 8) + 32 + _game.Config.FieldSize.X * 16;
        _scoreboardRightText.Location = (boardRight + 64, 160);
        
        _nextViews = new BlockView[5];
        var nextViewX = (int)_fieldMap.Location.X + (_game.Config.FieldSize.X + 2) * 16 - 8;
        for (var i = 0; i < _nextViews.Length; i++)
        {
            var scale = 4f / (i + 4);
            _nextViews[i] = new BlockView(game.Shapes)
                .Location(nextViewX, 48 + i * 48)
                .Scale(scale, scale);
        }
        var nextText = new Text("NEXT", Font.GetDefault(16), Color.White)
            .Location(nextViewX, 24);
        
        RenderWalls();
        RenderField();
        
        children.Add(_holdView);
        children.Add(_fieldMap);
        children.Add(_currentBlockMap);
        children.Add(_scoreboardLeftText);
        children.Add(_scoreboardRightText);
        foreach (var blockView in _nextViews)
        {
            children.Add(blockView);
        }
        children.Add(holdText);
        children.Add(nextText);

        _game.BlockPlace += OnBlockPlace;
        _game.Hold += OnHold;
        _game.SpawnNext += OnSpawnNext;
        _game.LineClear += OnLineClear;

        Width = nextViewX + 16 * 4;
        // Fieldの最下部のY座標
        Height = 32 + _game.Config.FieldSize.Y * 16;
    }

    protected override void OnUpdate()
    {
        RenderCurrentBlock();
        RenderScoreBoard();
    }

    protected override void OnDestroy()
    {
        _game.BlockPlace -= OnBlockPlace;
        _game.Hold -= OnHold;
        _game.SpawnNext -= OnSpawnNext;
        _game.LineClear -= OnLineClear;
    }

    private void OnBlockPlace()
    {
        RenderField();
    }

    private void OnHold()
    {
        RenderHoldNext();
    }
    
    private void OnLineClear(LineClearEventArgs e)
    {
        RenderField();
    }

    private void OnSpawnNext()
    {
        RenderHoldNext();
    }

    /// <summary>
    /// 壁をタイルマップに描画します。
    /// </summary>
    private void RenderWalls()
    {
        var wallTile = _blockTiles[BlockColor.Wall];
        var (width, height) = _game.Config.FieldSize;
        // 縦
        for (var y = 0; y <= height; y++)
        {
            _fieldMap[-1, y + _game.Config.TopMargin] = wallTile;
            _fieldMap[width, y + _game.Config.TopMargin] = wallTile;
        }
        
        // 横
        for (var x = 0; x < width; x++)
        {
            _fieldMap[x, height + _game.Config.TopMargin] = wallTile;
        }
    }

    /// <summary>
    /// 現在のフィールドをタイルマップにレンダリングします。
    /// </summary>
    private void RenderField()
    {
        var config = _game.Config;
        for (var x = 0; x < config.FieldSize.X; x++)
        {
            for (var y = 0; y < config.FieldSize.Y + config.TopMargin; y++)
            {
                _fieldMap[x, y] = _game.Field[x, y] == BlockColor.None ? null : _blockTiles[_game.Field[x, y]];
            }
        }
    }
    
    private void RenderCurrentBlock()
    {
        _currentBlockMap.Clear();
        var ghostY = _game.RayToDown();
        var pos = _game.BlockPosition;
        RenderBlockToTilemap(pos.X, ghostY, _game.CurrentShape, BlockColor.Ghost, _currentBlockMap);
        RenderBlockToTilemap(pos.X, pos.Y, _game.CurrentShape, _game.Shapes.ColorMap[_game.CurrentBlockColor], _currentBlockMap);
    }

    /// <summary>
    /// ホールド、ネクストを画面上にレンダリングします。
    /// </summary>
    private void RenderHoldNext()
    {
        _holdView.BlockType = _game.CurrentHold;
        
        for (var i = 0; i < _nextViews.Length; i++)
        {
            _nextViews[i].BlockType = _game.NextQueue.ElementAtOrDefault(i);
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
        var tile = _blockTiles[blockColor];
        for (var i = 0; i < blockShape.GetLength(0); i++)
        {
            for (var j = 0; j < blockShape.GetLength(1); j++)
            {
                if (!blockShape[i, j]) continue;
                map[x + i, y + j] = tile;
            }
        }
    }
    
    private void RenderScoreBoard()
    {
        var leftScore = string.Join("\n\n", ScoreboardLeft.Select(kv => $"{kv.Key}\n{kv.Value}"));
        var bb = glyphRenderer.GetTextBounds(leftScore, _scoreboardLeftText.Font);
        _scoreboardLeftText.Content = leftScore;
        _scoreboardLeftText.Location = (11 * 8 - 32 - bb.Width, _scoreboardLeftText.Location.Y);
        _scoreboardRightText.Content = string.Join("\n\n", ScoreboardRight.Select(kv => $"{kv.Key}\n{kv.Value}"));
    }
}
