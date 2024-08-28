using Promete;

namespace Quadrix;

/// <summary>
/// 補正の無い回転システム。
/// </summary>
public class ClassicRotationSystem : IRotationSystem
{
    public ShapeLoader Shapes { get; } = new("./assets/data/srs.txt");

    public VectorInt? TryKick(GameService game, BlockColor color, VectorInt currentPosition, int currentRotation,
        int nextRotation)
    {
        return game.CanPlaceBlock(currentPosition.X, currentPosition.Y, Shapes[color][nextRotation])
            ? (0, 0)
            : null;
    }
}