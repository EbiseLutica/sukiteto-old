namespace Sukiteto.RandomGenerators;

public interface IRandomGenerator
{
    IEnumerable<BlockColor> Generate(BlockColor[] usingBlocks);
}