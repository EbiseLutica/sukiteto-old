namespace Quadrix;

public interface IRandomGenerator
{
    IEnumerable<BlockColor> Generate(BlockColor[] usingBlocks);
}