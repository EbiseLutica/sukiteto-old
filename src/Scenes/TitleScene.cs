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
    private Text menu;
    private int index;
    private (string, Action)[] mainMenuItems;
    private (string, Action)[] gamemodeMenuItems;
    private (string, Action)[] currentItems;

    public override void OnStart()
    {
        window.Title = "Sukiteto";
        window.Size = (640, 480);
        App.BackgroundColor = Color.FromArgb(20, 20, 20);

        InitializeUI();
        InitializeMenuItems();
    }

    public override void OnUpdate()
    {
        if (input[InputType.MenuUp].IsButtonUp)
        {
            index--;
            if (index < 0) index = currentItems.Length - 1;
            UpdateMenu();
        }
        if (input[InputType.MenuDown].IsButtonUp)
        {
            index++;
            if (index >= currentItems.Length) index = 0;
            UpdateMenu();
        }
        if (input[InputType.Ok].IsButtonUp)
        {
            currentItems[index].Item2();
        }
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
    }
    
    private void InitializeMenuItems()
    {
        mainMenuItems =
        [
            ("ゲームをはじめる", OnEnterGameModeMenu),
            ("設定", () => app.LoadScene<ConfigScene>()),
            ("ウィンドウを閉じる", () => app.Exit()),
        ];

        gamemodeMenuItems =
        [
            ("150マラソン", () => app.LoadScene<GameScene>()),
            ("エンドレス", () => app.LoadScene<GameScene>()),
            ("40ライン（未実装）", () => { }),
            ("ミッション（未実装）", () => { }),
            ("DEATH OF G（未実装）", () => { }),
            ("←戻る", OnLeaveGameModeMenu)
        ];
        currentItems = mainMenuItems;

        UpdateMenu();
    }

    private void UpdateMenu()
    {
        var itemContents = currentItems.Select((item, i) => $"{(index == i ? "→" : "　")} {item.Item1}");
        menu.Content = string.Join('\n', itemContents);
    }
    
    private void OnEnterGameModeMenu()
    {
        currentItems = gamemodeMenuItems;
        index = 0;
        UpdateMenu();
    }
    
    private void OnLeaveGameModeMenu()
    {
        currentItems = mainMenuItems;
        index = 0;
        UpdateMenu();
    }
}