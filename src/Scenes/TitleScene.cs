using System.Drawing;
using Promete;
using Promete.Elements;
using Promete.Graphics;
using Promete.Windowing;

namespace Sukiteto;

public class TitleScene(
    PrometeApp app,
    IWindow window,
    Resources resources,
    GlyphRenderer glyphRenderer,
    KeyConfigService keys) : Scene
{
    public override void OnStart()
    {
        window.Title = "Sukiteto";
        window.Size = (640, 480);

        var helpText = "[Z]: Rotate Left\n" +
                       "[X]: Rotate Right\n" +
                       "[C]: Hold\n" +
                       "[←][→]: Move\n" +
                       "[ESC]: Back to Title\n" +
                       "\n" +
                       "PRESS [Z] TO PLAY";

        var logo = new Sprite(resources.Logo);
        logo.Location = (window.Width / 2 - logo.Width / 2, 72);

        var keyHelp = new Text(glyphRenderer, helpText, Font.GetDefault(18));
        keyHelp.Location = (window.Width / 2 - keyHelp.Width / 2, logo.Location.Y + logo.Height + 64);

        var copyright = new Text(glyphRenderer, $"(C)2023-2024 Ebise Lutica and GitHub contributors\nversion {Global.Version}", Font.GetDefault(14), Color.LightGray);
        copyright.Location = (window.Width - copyright.Width - 2, window.Height - copyright.Height - 2);

        Root.AddRange(logo, keyHelp, copyright);
    }

    public override void OnUpdate()
    {
        if (keys.KeyOk.IsKeyUp)
        {
            app.LoadScene<GameScene>();
        }
    }
}