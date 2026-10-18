using Promete;
using Promete.Input;

namespace Quadrix;

public class JoystickConfigScene : Scene
{
    private readonly Gamepads _pads;
    private readonly ConsoleLayer _console;

    public JoystickConfigScene(Gamepads pads, ConsoleLayer console)
    {
        _pads = pads;
        _console = console;
        
        
    }

    public override void OnStart()
    {
        ShowInitial();
    }

    private void ShowInitial()
    {
        _console.Clear();
        _console.Print("ジョイスティック設定\n");
        _console.Print("[A] ボタンで設定開始");
    }
}