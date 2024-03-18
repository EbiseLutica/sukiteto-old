using System.Drawing;
using Promete;
using Promete.Input;

namespace Sukiteto;

public class ConfigScene : Scene
{
    private readonly Mouse _mouse;
    private readonly Button _btn;

    public ConfigScene(Mouse mouse)
    {
        _mouse = mouse;

        Root =
        [
            _btn = new Button("Test", Color.Violet, Color.Black, (32, 32)),
        ];
    }

    public override void OnUpdate()
    {
        _btn.Location = _mouse.Position;
    }
}