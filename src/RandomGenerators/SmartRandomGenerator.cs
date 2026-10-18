namespace Quadrix;

/// <summary>
/// 指定した数の生成履歴を持ち、履歴に含まれるピースを避けるランダムジェネレータ
/// </summary>
/// <param name="maxHistory"></param>
public class SmartRandomGenerator(int maxHistory) : IRandomGenerator
{
    /// <summary>
    /// 乱数ジェネレータ
    /// </summary>
    private readonly Random _random = new();

    /// <summary>
    /// 補正に用いる、過去のピース生成履歴
    /// </summary>
    private readonly Queue<BlockColor> _history = new();
    
    public IEnumerable<BlockColor> Generate(BlockColor[] usingBlocks)
    {
        var count = 0;
        var retried = false;
        while (count < 7)
        {
            var block = usingBlocks[_random.Next(usingBlocks.Length)];
            if (_history.Contains(block) && !retried)
            {
                retried = true;
                continue;
            }

            _history.Enqueue(block);
            if (_history.Count > maxHistory)
            {
                _history.Dequeue();
            }
            yield return block;
            retried = false;
            count++;
        }
    }
}