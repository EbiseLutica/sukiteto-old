using Promete;
using Promete.Elements;
using Promete.Graphics;

namespace Quadrix;

public class BlockView : ContainableElementBase
{
    public BlockColor BlockType
    {
        get => _blockType;
        set
        {
            _blockType = value;
            RenderBlock();
        }
    }

    private BlockColor _blockType = BlockColor.None;
    
    private readonly Dictionary<BlockColor, ITile> _blockTiles;
    private readonly ShapeLoader _shapes;
    private readonly Tilemap _tilemap = new((16, 16));

    public BlockView(ShapeLoader shapes)
    {
        _shapes = shapes;
        _blockTiles = PrometeApp.Current?.GetPlugin<Resources>()?.BlockTiles ?? throw new InvalidOperationException("Promete is not initialized");
        
        children.Add(_tilemap);
    }

    private void RenderBlock()
    {
        _tilemap.Clear();
        if (_blockType == BlockColor.None) return;
        var shape = _shapes[_blockType][0];
        for (var y = 0; y < 4; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                if (shape[x, y])
                {
                    _tilemap.SetTile(x, y, _blockTiles[_shapes.ColorMap[_blockType]]);
                }
            }
        }
    }
}