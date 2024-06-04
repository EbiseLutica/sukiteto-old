using Promete;

namespace Sukiteto;

/// <summary>
/// アーケードゲーム風の回転法則。
/// </summary>
public class ArcadeRotationSystem : IRotationSystem
{
    public ShapeLoader Shapes { get; } = new("./assets/data/ars.txt");

    private readonly VectorInt[] _kicks = [(0, 0), (1, 0), (-1, 0)];

    public VectorInt? TryKick(GameService game, BlockColor color, VectorInt currentPosition, int currentRotation, int nextRotation)
    {
        var (x, y) = currentPosition;
        var shape = Shapes[color][nextRotation];

        var result = CanPlaceBlockEx(game, color, x, y, shape);
        if (result == PlaceableResult.Placeable) return (0, 0);
        if (result == PlaceableResult.Unkickable) return null;
        
        if (CanPlaceBlockEx(game, color, x + 1, y, shape) == PlaceableResult.Placeable) return (1, 0);
        if (CanPlaceBlockEx(game, color, x - 1, y, shape) == PlaceableResult.Placeable) return (-1, 0);
        return null;
    }

    private PlaceableResult CanPlaceBlockEx(GameService game, BlockColor color, int x, int y, bool[,] blockShape)
    {
        var (w, h) = game.Config.FieldSize + (0, game.Config.TopMargin);
        for (var iy = 0; iy < blockShape.GetLength(1); iy++)
        {
            for (var ix = 0; ix < blockShape.GetLength(0); ix++)
            {
                var isCenterColumn = ix == 1 && color is BlockColor.L or BlockColor.J or BlockColor.T;
                var failureResult = color == BlockColor.I || isCenterColumn ? PlaceableResult.Unkickable : PlaceableResult.Kickable;
                if (!blockShape[ix, iy]) continue;
                if (x + ix < 0 || x + ix >= w || y + iy < 0 || y + iy >= h) return failureResult;
                if (game.Field[x + ix, y + iy] != BlockColor.None) return failureResult;
            }
        }

        return PlaceableResult.Placeable;
    }

    enum PlaceableResult
    {
        /// <summary>
        /// キック無しに設置可能。
        /// </summary>
        Placeable,

        /// <summary>
        /// 設置不可能だが、キック可能。
        /// </summary>
        Kickable,
        
        /// <summary>
        /// キック不可能。
        /// </summary>
        Unkickable,
    }
}
