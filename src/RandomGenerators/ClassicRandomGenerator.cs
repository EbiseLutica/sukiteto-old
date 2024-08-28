namespace Quadrix;

/// <summary>
/// 補正のないランダムジェネレータ
/// </summary>
public class ClassicRandomGenerator : IRandomGenerator
{
    private readonly Random _random = new();
    
    public IEnumerable<BlockColor> Generate(BlockColor[] usingBlocks)
    {
        for (var i = 0; i < 7; i++)
        {
            yield return usingBlocks[_random.Next(usingBlocks.Length)];
        }
    }
}