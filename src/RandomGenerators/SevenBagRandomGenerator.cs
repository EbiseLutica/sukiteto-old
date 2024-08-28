namespace Quadrix;

/// <summary>
/// 7種1巡の法則に従ってピースを生成するランダムジェネレータ
/// </summary>
public class SevenBagRandomGenerator : IRandomGenerator
{
    private readonly Random _random = new();
    
    public IEnumerable<BlockColor> Generate(BlockColor[] usingBlocks)
    {
        return usingBlocks.OrderBy(_ => _random.Next());
    }
}