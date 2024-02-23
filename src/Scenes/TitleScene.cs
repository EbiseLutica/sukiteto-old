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
    InputService input) : Scene
{
    public override void OnStart()
    {
        window.Title = "Sukiteto";
        window.Size = (640, 480);
        
        var logo = new Sprite(resources.Logo);
        logo.Location = (window.Width / 2 - logo.Width / 2, 72);

        var buttonStart = new Button("マラソン", Color.FromArgb(44, 110, 61), Color.White, (window.Width - 32, 64))
            .Location(16, 180);
        buttonStart.Click += app.LoadScene<GameScene>;
        
        var buttonConfig = new Button("設定", Color.FromArgb(90, 90, 90), Color.White, (window.Width - 32, 64))
            .Location(16, buttonStart.Location.Y + 64 + 16);
        buttonConfig.Click += app.LoadScene<ConfigScene>;
        
        var buttonExit = new Button("おわる", Color.FromArgb(156, 48, 50), Color.White, (window.Width - 32, 64))
            .Location(16, buttonConfig.Location.Y + 64 + 16);
        buttonExit.Click += () => app.Exit();

        var copyright = new Text($"(C)2023-2024 Ebise Lutica and GitHub contributors\nversion {Global.Version}", Font.GetDefault(14), Color.LightGray);
        copyright.Location = (window.Width - copyright.Width - 2, window.Height - copyright.Height - 2);

        Root.AddRange(logo, buttonStart, buttonConfig, buttonExit, copyright);
    }

    public override void OnUpdate()
    {
        if (input[InputType.Ok].IsButtonUp)
        {
            app.LoadScene<GameScene>();
        }
    }
}