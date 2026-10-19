using Promete;
using Promete.Input;

namespace Quadrix;

/// <summary>
/// キーボードの割り当てを変更するシーン。
/// </summary>
public class KeyConfigScene(InputService input, Keyboard keyboard, ConsoleLayer console)
    : BindingConfigSceneBase<KeyCode>(input, keyboard, console)
{
    protected override string Heading => "キーボード設定";

    protected override Dictionary<InputType, KeyCode>[] Slots => Input.Config.Keys;

    protected override string ListeningHelp => "割り当てるキーを押してください  Esc: キャンセル  BackSpace: 解除";

    protected override bool TryGetPressedButton(out KeyCode button)
    {
        // Esc と BackSpace はキャンセル・解除に使うため割り当てない
        foreach (var key in Keyboard.AllDownKeys)
        {
            if (key is KeyCode.Escape or KeyCode.BackSpace) continue;
            button = key;
            return true;
        }

        button = default;
        return false;
    }
}
