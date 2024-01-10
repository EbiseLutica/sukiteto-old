using System.Drawing;
using DotFeather;

namespace Sukiteto;

public class TitleScene : Scene
{
    public override void OnStart(Dictionary<string, object> args)
    {
        var logo = new Sprite(Global.Resources.Logo);
        logo.Location = (DF.Window.Width / 2 - logo.Width / 2, 72);
        
        var keyHelp = new TextElement("[Z]: Rotate Left\n" +
                                      "[X]: Rotate Right\n" +
                                      "[C]: Hold\n" +
                                      "[←][→]: Move\n" +
                                      "[ESC]: Back to Title\n" +
                                      "\n" +
                                      "PRESS [Z] TO PLAY", DFFont.GetDefault(18), Color.White);
        keyHelp.Location = (DF.Window.Width / 2 - keyHelp.Width / 2, logo.Location.Y + logo.Height + 64);
        
        var copyright = new TextElement($"(C)2023-2024 Ebise Lutica\nversion {Global.Version}", DFFont.GetDefault(14), Color.LightGray);
        copyright.Location = (DF.Window.Width - copyright.Width - 2, DF.Window.Height - copyright.Height - 2);

        Root.AddRange(logo, keyHelp, copyright);
    }

    public override void OnUpdate()
    {
        if (DFKeyboard.Z.IsKeyUp)
        {
            DF.Router.ChangeScene<GameScene>();
        }
    }
}