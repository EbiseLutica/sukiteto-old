using System.Drawing;
using Promete;
using Promete.Elements;
using Promete.Input;

namespace Sukiteto;

public class ConfigScene(ConsoleLayer layer, Mouse mouse) : Scene
{
    private Button btn;
    protected override Container Setup()
    {
        var tex = Window.TextureFactory.Load9Sliced("./assets/textures/ui_button.png", 5, 9, 5, 5);
        return
        [
            btn = new Button("Test", Color.Violet, Color.Black, (32, 32)),
        ];
    }
    
    public override void OnUpdate()
    {
        btn.Location = mouse.Position;
    }
}