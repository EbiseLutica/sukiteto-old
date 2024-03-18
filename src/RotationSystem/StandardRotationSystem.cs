using Promete;

namespace Sukiteto;

/// <summary>
/// ガイドライン標準の回転システム。
/// </summary>
public class StandardRotationSystem : IRotationSystem
{
    public ShapeLoader Shapes { get; } = new("./assets/data/srs.txt");

    /// <summary>
    /// キックテーブル
    /// </summary>
    private static readonly Dictionary<(int fromRot, int toRot), VectorInt[]> kickTable = new()
    {
        [(0, 1)] = [(0, 0), (-1, 0), (-1, -1), (0, +2), (-1, +2)],
        [(1, 0)] = [(0, 0), (+1, 0), (+1, +1), (0, -2), (+1, -2)],
        [(1, 2)] = [(0, 0), (+1, 0), (+1, +1), (0, -2), (+1, -2)],
        [(2, 1)] = [(0, 0), (-1, 0), (-1, -1), (0, +2), (-1, +2)],
        [(2, 3)] = [(0, 0), (+1, 0), (+1, -1), (0, +2), (+1, +2)],
        [(3, 2)] = [(0, 0), (-1, 0), (-1, +1), (0, -2), (-1, -2)],
        [(3, 0)] = [(0, 0), (-1, 0), (-1, +1), (0, -2), (-1, -2)],
        [(0, 3)] = [(0, 0), (+1, 0), (+1, -1), (0, +2), (+1, +2)],
    };

    /// <summary>
    /// キックテーブル（Iブロック用）
    /// </summary>
    private static readonly Dictionary<(int fromRot, int toRot), VectorInt[]> kickTableI = new()
    {
        [(0, 1)] = [(0, 0), (-2, 0), (+1, 0), (-2, +1), (+1, -2)],
        [(1, 0)] = [(0, 0), (+2, 0), (-1, 0), (+2, -1), (-1, +2)],
        [(1, 2)] = [(0, 0), (-1, 0), (+2, 0), (-1, -2), (+2, +1)],
        [(2, 1)] = [(0, 0), (+1, 0), (-2, 0), (+1, +2), (-2, -1)],
        [(2, 3)] = [(0, 0), (+2, 0), (-1, 0), (+2, -1), (-1, +2)],
        [(3, 2)] = [(0, 0), (-2, 0), (+1, 0), (-2, +1), (+1, -2)],
        [(3, 0)] = [(0, 0), (+1, 0), (-2, 0), (+1, +2), (-2, -1)],
        [(0, 3)] = [(0, 0), (-1, 0), (+2, 0), (-1, -2), (+2, +1)],
    };

    /// <summary>
    /// キックテーブルを元にブロックのキックを試みます。
    /// </summary>
    /// <param name="game">現在の<see cref="GameService"/>。</param>
    /// <param name="color">ブロックの色</param>
    /// <param name="currentPosition">現在の座標</param>
    /// <param name="currentRotation">現在の回転</param>
    /// <param name="nextRotation">次の回転</param>
    /// <returns>キックが成功した場合はその相対座標、失敗した場合は<c>null</c>。</returns>
    public VectorInt? TryKick(GameService game, BlockColor color, VectorInt currentPosition, int currentRotation, int nextRotation)
    {
        var table = color == BlockColor.I ? kickTableI : kickTable;
        var testCases = table[(currentRotation, nextRotation)];
        foreach (var testCase in testCases)
        {
            if (game.CanPlaceBlock(currentPosition.X + testCase.X, currentPosition.Y + testCase.Y, Shapes[color][nextRotation]))
            {
                return testCase;
            }
        }

        return null;
    }
}
