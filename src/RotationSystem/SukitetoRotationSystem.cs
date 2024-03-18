using Promete;

namespace Sukiteto;

/// <summary>
/// Sukiteto独自の回転システム。
/// </summary>
public class SukitetoRotationSystem : IRotationSystem
{
    public ShapeLoader Shapes { get; } = new("./assets/data/srs.txt");

    private readonly VectorInt[] kickTableR = [(0, 0), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1)];
    private readonly VectorInt[] kickTableL = [(0, 0), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1)];

    public VectorInt? TryKick(GameService game, BlockColor color, VectorInt currentPosition, int currentRotation,
        int nextRotation)
    {
        var isRightRotation = currentRotation == 0 && nextRotation == 1 || currentRotation == 1 && nextRotation == 2 ||
                             currentRotation == 2 && nextRotation == 3 || currentRotation == 3 && nextRotation == 0;
        var table = isRightRotation ? kickTableR : kickTableL;
        
        for (var k = 1; k < 10; k++)
        {
            foreach (var testCase in table.Select(c => c * k))
            {
                if (game.CanPlaceBlock(currentPosition.X + testCase.X, currentPosition.Y + testCase.Y, Shapes[color][nextRotation]))
                {
                    return testCase;
                }
            }
        }

        return null;
    }
}