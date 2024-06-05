using System.Drawing;
using Promete;
using Promete.Input;

namespace Sukiteto;

public class ConfigScene : Scene
{
    private readonly UIService _ui;
    private readonly ConsoleLayer _console;

    private int _windowScaleIndex;

    public ConfigScene(UIService ui, ConsoleLayer console)
    {
        _ui = ui;
        _console = console;
        _windowScaleIndex = (int)Math.Log2(Window.Scale);
    }

    public override void OnStart()
    {
        ShowMainMenu();
    }

    public override void OnUpdate()
    {
        _console.Clear();
        _console.Print(_ui.Render());
    }

    private void ShowMainMenu()
    {
        _ui.Show([
            new UIItemButton("キーボード設定", App.LoadScene<KeyConfigScene>),
            new UIItemButton("ジョイスティック設定", App.LoadScene<JoystickConfigScene>),
            new UIItemButton("ビデオ設定", ShowVideoMenu),
            new UIItemButton("サウンド設定"),
            new UIItemButton("←戻る", App.LoadScene<TitleScene>),
        ]);
    }
    
    private void ShowVideoMenu()
    {
        _ui.Show([
            new UIItemSlider("ウィンドウスケール", () => _windowScaleIndex, v =>
            {
                if (v is < 0 or > 3) return;
                _windowScaleIndex = v;
                Window.Scale = (int)Math.Pow(2, v);
            }, () => $"{Window.Scale}x"),
            new UIItemCheckbox("垂直同期（VSYNC）", () => Window.IsVsyncMode, v => Window.IsVsyncMode = v),
            new UIItemSlider("目標フレームレート", () => Window.RefreshRate, v => Window.RefreshRate = v),
            new UIItemButton("←戻る", ShowMainMenu),
        ]);
    }
}