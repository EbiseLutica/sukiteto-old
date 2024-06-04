using Promete;

namespace Sukiteto;

/// <summary>
/// エンジョイジョイできそうな回転システム。
/// </summary>
public class JoyjoyRotationSystem : IRotationSystem
{
    public ShapeLoader Shapes { get; } = new("./assets/data/jrs.txt");

    private readonly VectorInt[] _kicks = [(0, 0), (1, 1), (-1, 1)];

    public VectorInt? TryKick(GameService game, BlockColor color, VectorInt currentPosition, int currentRotation, int nextRotation)
    {
        foreach (var testCase in _kicks)
        {
            if (game.CanPlaceBlock(currentPosition.X + testCase.X, currentPosition.Y + testCase.Y, Shapes[color][nextRotation]))
            {
                return testCase;
            }
        }

        return null;
    }
}