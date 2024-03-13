namespace Sukiteto;

public class ShapeLoader
{
    public Dictionary<BlockColor, bool[][,]> Shape { get; } = new();
    
    public bool[][,] this[BlockColor type] => Shape[type];
    
    public ShapeLoader(string path)
    {
        var lines = File.ReadAllLines(path);
        var lineBuffer = new List<string>(16);
        var currentSymbol = '\0';

        foreach (var line in lines)
        {
            if (line.EndsWith(':'))
            {
                if (lineBuffer.Count != 0)
                {
                    Shape.Add(ParseSymbol(currentSymbol), ParseBlock(lineBuffer));
                    lineBuffer.Clear();
                }
                currentSymbol = line[0];
            }
            else if (!string.IsNullOrEmpty(line))
            {
                lineBuffer.Add(line);
            }
        }

        if (lineBuffer.Count == 0) return;
        Shape.Add(ParseSymbol(currentSymbol), ParseBlock(lineBuffer));
        return;

        static BlockColor ParseSymbol(char c)
        {
            return c switch
            {
                'O' => BlockColor.O,
                'J' => BlockColor.J,
                'L' => BlockColor.L,
                'Z' => BlockColor.Z,
                'S' => BlockColor.S,
                'T' => BlockColor.T,
                'I' => BlockColor.I,
                _ => throw new ArgumentException("Invalid symbol")
            };
        }
    }
    
    private bool[][,] ParseBlock(List<string> lines)
    {
        bool[][,] mino = new bool[4][,];
        
        var column = lines.Count / 4;
        
        for (var i = 0; i < 4; i++)
        {
            mino[i] = new bool[4, 4];
            var yOffset = i * column;
            for (var y = yOffset; y < yOffset + column; y++)
            {
                var line = lines[y];
                for (var x = 0; x < line.Length; x++)
                {
                    mino[i][x, y - yOffset] = line[x] == '#';
                }
            }
        }

        return mino;
    }
}
