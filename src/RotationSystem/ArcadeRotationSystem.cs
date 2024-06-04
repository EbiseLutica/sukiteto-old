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
        foreach (var kick in _kicks)
        {
            var (x, y) = currentPosition + kick;
            var result = CanPlaceBlockEx(game, color, x, y, Shapes[color][nextRotation]);
            if (result == PlaceableResult.Placeable) return kick;
            if (result == PlaceableResult.Unkickable) break;
        }

        return null;
    }

    private PlaceableResult CanPlaceBlockEx(GameService game, BlockColor color, int x, int y, bool[,] blockShape)
    {
        var (w, h) = game.Config.FieldSize + (0, game.Config.TopMargin);
        for (var i = 0; i < blockShape.GetLength(0); i++)
        {
            var failureResult = color == BlockColor.I || i == 1 ? PlaceableResult.Unkickable : PlaceableResult.Kickable;
            for (var j = 0; j < blockShape.GetLength(1); j++)
            {
                if (!blockShape[i, j]) continue;
                if (x + i < 0 || x + i >= w || y + j < 0 || y + j >= h) return failureResult;
                if (game.Field[x + i, y + j] != BlockColor.None) return failureResult;
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
