using Promete;

namespace Sukiteto;

/// <summary>
/// Sukitetoにおける回転システムのインターフェース。
/// </summary>
public interface IRotationSystem
{
    ShapeLoader Shapes { get; }
    
    /// <summary>
    /// キックテーブルを元にブロックのキックを試みます。
    /// </summary>
    /// <param name="game">現在の<see cref="GameService"/>。</param>
    /// <param name="color">ブロックの色</param>
    /// <param name="currentPosition">現在の座標</param>
    /// <param name="currentRotation">現在の回転</param>
    /// <param name="nextRotation">次の回転</param>
    /// <returns>キックが成功した場合はその相対座標、失敗した場合は<c>null</c>。</returns>
    VectorInt? TryKick(GameService game, BlockColor color, VectorInt currentPosition, int currentRotation, int nextRotation);
}