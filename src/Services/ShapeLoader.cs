namespace Sukiteto;

public class ShapeLoader
{
    public Dictionary<BlockColor, bool[][,]> Shape { get; } = new();
    
    public Dictionary<BlockColor, BlockColor> ColorMap { get; } = new()
    {
        {BlockColor.O, BlockColor.O},
        {BlockColor.J, BlockColor.J},
        {BlockColor.L, BlockColor.L},
        {BlockColor.Z, BlockColor.Z},
        {BlockColor.S, BlockColor.S},
        {BlockColor.T, BlockColor.T},
        {BlockColor.I, BlockColor.I},
    };
    
    public bool[][,] this[BlockColor type] => Shape[type];
    
    public ShapeLoader(string path)
    {
        var lines = File.ReadAllLines(path);
        var lineBuffer = new List<string>(16);
        var currentSymbol = '\0';
        var currentColorSymbol = '\0';

        foreach (var line in lines)
        {
            if (line.EndsWith(':'))
            {
                if (lineBuffer.Count != 0)
                {
                    Shape.Add(ParseSymbol(currentSymbol), ParseBlock(lineBuffer));
                    if (currentColorSymbol != '\0')
                    {
                        ColorMap[ParseSymbol(currentSymbol)] = ParseSymbol(currentColorSymbol);
                    }
                    lineBuffer.Clear();
                }
                currentSymbol = line[0];
                currentColorSymbol = line.Length > 2 ? line[1] : '\0';
            }
            else if (!string.IsNullOrEmpty(line))
            {
                lineBuffer.Add(line);
            }
        }

        if (lineBuffer.Count == 0) return;
        Shape.Add(ParseSymbol(currentSymbol), ParseBlock(lineBuffer));
        if (currentColorSymbol != '\0')
        {
            ColorMap[ParseSymbol(currentSymbol)] = ParseSymbol(currentColorSymbol);
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

    private static BlockColor ParseSymbol(char c)
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
