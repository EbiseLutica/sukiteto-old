using Sukiteto;

namespace SukiTeto;

public class MinoLoader
{
    public Dictionary<MinoType, bool[][,]> Mino { get; } = new();
    
    public bool[][,] this[MinoType type] => Mino[type];
    
    public MinoLoader()
    {
        var lines = File.ReadAllLines("./assets/data/mino.txt");
        var lineBuffer = new List<string>(16);
        var currentSymbol = '\0';
        
        static MinoType ParseSymbol(char c)
        {
            return c switch
            {
                'O' => MinoType.O,
                'J' => MinoType.J,
                'L' => MinoType.L,
                'Z' => MinoType.Z,
                'S' => MinoType.S,
                'T' => MinoType.T,
                'I' => MinoType.I,
                _ => throw new ArgumentException("Invalid symbol")
            };
        }
        
        foreach (var line in lines)
        {
            if (line.EndsWith(":"))
            {
                if (lineBuffer.Count != 0)
                {
                    Mino.Add(ParseSymbol(currentSymbol), ParseMino(lineBuffer));
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
        Mino.Add(ParseSymbol(currentSymbol), ParseMino(lineBuffer));
    }
    
    private bool[][,] ParseMino(List<string> lines)
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
