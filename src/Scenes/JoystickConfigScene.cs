using Promete;
using Promete.Input;

namespace Quadrix;

/// <summary>
/// ゲームパッドの割り当てを変更するシーン。
/// パッドのボタンはすべて割り当て対象にするため、割り当て待ちはキーボードかタイムアウトでキャンセルする。
/// </summary>
public class JoystickConfigScene(InputService input, Keyboard keyboard, Gamepads gamepads, ConsoleLayer console)
    : BindingConfigSceneBase<GamepadButtonType>(input, keyboard, console)
{
    private Gamepad? Pad => gamepads[0] is { IsConnected: true } pad ? pad : null;

    protected override string Heading => "ジョイスティック設定";

    protected override Dictionary<InputType, GamepadButtonType>[] Slots => Input.Config.Gamepads;

    protected override bool CanListen => Pad != null;

    protected override string? StatusMessage => Pad is { } pad ? pad.Name : "ゲームパッドが接続されていません";

    protected override float ListeningTimeout => 5;

    protected override string NavigationHelp => "十字キー/↑↓←→: 選択  A/Enter: 変更  B/Esc: 戻る";

    protected override string ListeningHelp => "割り当てるボタンを押してください  Esc: キャンセル  BackSpace: 解除";

    // 割り当て待ちの間は十字キーも割り当て対象になるため、カーソル操作は待機中以外でのみ参照される
    protected override bool IsUpPressed => base.IsUpPressed || IsPadDown(GamepadButtonType.DpadUp);
    protected override bool IsDownPressed => base.IsDownPressed || IsPadDown(GamepadButtonType.DpadDown);
    protected override bool IsLeftPressed => base.IsLeftPressed || IsPadDown(GamepadButtonType.DpadLeft);
    protected override bool IsRightPressed => base.IsRightPressed || IsPadDown(GamepadButtonType.DpadRight);
    protected override bool IsOkPressed => base.IsOkPressed || IsPadDown(GamepadButtonType.A);
    protected override bool IsBackPressed => base.IsBackPressed || IsPadDown(GamepadButtonType.B);

    protected override bool TryGetPressedButton(out GamepadButtonType button)
    {
        // GamepadButton からは種別を取得できないため、種別ごとに押下を調べる
        foreach (var type in Enum.GetValues<GamepadButtonType>())
        {
            if (type == GamepadButtonType.Unknown || !IsPadDown(type)) continue;
            button = type;
            return true;
        }

        button = default;
        return false;
    }

    private bool IsPadDown(GamepadButtonType type) => Pad?[type].IsButtonDown ?? false;
}
