using Promete;
using Promete.Audio;

namespace Quadrix;

public class Nostalgia60GameScene(ConsoleLayer console, InputService input, AudioPlayer audio, Resources resources) : Scene
{
    private readonly GameService _game = new(GameConfig.Default);
    private ClassicInputPlugin _inputPlugin;

    public override void OnStart()
    {
        _inputPlugin = new ClassicInputPlugin(input, _game, audio, resources);
        _game.Start();
    }
    
    public override void OnUpdate()
    {
        Window.Title = $"Nostalgia60 - {Window.FramePerSeconds}fps";
        if (input[InputType.Quit])
        {
            App.LoadScene<TitleScene>();
        }

        Render();
        _inputPlugin.Process(Window.DeltaTime);
        _game.Tick(Window.DeltaTime);
    }

    private void Render()
    {
        console.Clear();
        RenderBoard();
    }

    private void RenderBoard()
    {
        // Draw game board
        var boardOffset = new VectorInt(30, 5);
        var size = _game.Config.FieldSize;
        for (var y = 0; y < size.Y; y++)
        {
            for (var x = -2; x < size.X * 2 + 2; x += 2)
            {
                var consolePos = boardOffset + (x, y);
                var fx = x / 2;
                console.Cursor = consolePos;
                if (x == -2)
                {
                    console.Print("<!");
                }
                else if (x == size.X * 2)
                {
                    console.Print("!>");
                }
                else
                {
                    var block = _game.Field[fx, y + _game.Config.TopMargin];
                    console.Print(block != BlockColor.None ? "[]" : " .");
                }
            }
        }
        console.Cursor = boardOffset + (-2, size.Y);
        console.Print($"<!{new string('=', size.X * 2)}!>");
        
        // Draw falling piece
        var fallingPieceOffset = boardOffset + (_game.BlockPosition.X * 2, _game.BlockPosition.Y);
        for (var y = 0; y < _game.CurrentShape.GetLength(1); y++)
        {
            for (var x = 0; x < _game.CurrentShape.GetLength(0); x++)
            {
                if (!_game.CurrentShape[x, y]) continue;
                var consolePos = fallingPieceOffset + (x * 2, y);
                console.Cursor = consolePos;
                console.Print("[]");
            }
        }

        console.Cursor = (55, 5);
        console.Print($"Pos: {_game.BlockPosition}");
    }
}