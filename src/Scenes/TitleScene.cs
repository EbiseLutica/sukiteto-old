using System.Drawing;
using DotFeather;

namespace SukiTeto;

public class TitleScene : Scene
{
    public override void OnStart(Dictionary<string, object> args)
    {
        var title = new TextElement("SUKITETO", DFFont.GetDefault(32), Color.White);
        title.Location = (DF.Window.Width / 4 - title.Width / 2, 32);

        var press = new TextElement("PRESS Z", DFFont.GetDefault(16), Color.White);
        press.Location = (DF.Window.Width / 4 - press.Width / 2, 160);

        Root.AddRange(title, press);
    }

    public override void OnUpdate()
    {
        if (DFKeyboard.Z.IsKeyUp)
        {
            DF.Router.ChangeScene<GameScene>();
        }
    }
}