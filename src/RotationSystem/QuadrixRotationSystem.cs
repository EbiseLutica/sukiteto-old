using Promete;

namespace Quadrix;

/// <summary>
/// Quadrix独自の回転システム。
/// </summary>
public class QuadrixRotationSystem : IRotationSystem
{
    public ShapeLoader Shapes { get; } = new("./assets/data/qrs.txt");

    private readonly VectorInt[] kickTable = [(0, 0), (1, 0), (-1, 0), (0, -1), (1, -1), (-1, -1), (0, 1), (1, 1), (-1, 1)];

    public VectorInt? TryKick(GameService game, BlockColor color, VectorInt currentPosition, int currentRotation,
        int nextRotation)
    {
        foreach (var testCase in kickTable)
        {
            if (game.CanPlaceBlock(currentPosition.X + testCase.X, currentPosition.Y + testCase.Y, Shapes[color][nextRotation]))
            {
                return testCase;
            }
        }

        return null;
    }
}