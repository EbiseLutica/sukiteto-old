using System.Drawing;
using DotFeather;

namespace Sukiteto;

public class TitleScene : Scene
{
    public override void OnStart(Dictionary<string, object> args)
    {
        var title = new TextElement("SUKITETO", DFFont.GetDefault(64), Color.White);
        title.Location = (DF.Window.Width / 2 - title.Width / 2, 72);

        var press = new TextElement("PRESS Z", DFFont.GetDefault(32), Color.White);
        press.Location = (DF.Window.Width / 2 - press.Width / 2, 320);

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