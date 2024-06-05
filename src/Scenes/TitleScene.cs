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
    InputService input,
    UIService ui) : Scene
{
    private Text menu;
    private int index;

    public override void OnStart()
    {
        window.Title = "Sukiteto";
        window.Size = (640, 480);
        App.BackgroundColor = Color.FromArgb(20, 20, 20);

        InitializeUI();
    }

    public override void OnUpdate()
    {
        menu.Content = ui.Render();
    }
    
    private void InitializeUI()
    {
        var logo = new Sprite(resources.Logo)
            .Scale(2, 2)
            .Location(48, 48);

        menu = new Text("", Font.GetDefault(18), Color.White)
            .Location(48, 192);

        var copyright = new Text($"(C)2023-2024 Ebise Lutica and GitHub contributors\nversion {Global.Version}", Font.GetDefault(14), Color.LightGray);
        copyright.Location = (window.Width - copyright.Width - 2, window.Height - copyright.Height - 2);

        Root.AddRange(logo, menu, copyright);
        
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        ui.Show([
            new UIItemButton("ゲームをはじめる", ShowGameModeMenu),
            new UIItemButton("設定", app.LoadScene<ConfigScene>),
            new UIItemButton("ウィンドウを閉じる", () => app.Exit())
        ]);
    }
    
    private void ShowGameModeMenu()
    {
        ui.Show([
            new UIItemButton("150マラソン", app.LoadScene<GameScene>),
            new UIItemButton("エンドレス（未実装）", () => { }),
            new UIItemButton("40ライン（未実装）", () => { }),
            new UIItemButton("ミッション（未実装）", () => { }),
            new UIItemButton("DEATH OF G（Alpha）", app.LoadScene<DeathGameScene>),
            new UIItemButton("AI (β)", app.LoadScene<AIGameScene>),
            new UIItemButton("←戻る", ShowMainMenu)
        ]);
    }
}