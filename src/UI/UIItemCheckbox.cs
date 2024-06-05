namespace Sukiteto;

public class UIItemCheckbox(string label, Func<bool> isChecked, Action<bool>? onCheck) : UIItemBase
{
    public override string Render() => $"{(isChecked() ? "[x]" : "[ ]")} {label}";

    public override void OnSelect() => onCheck?.Invoke(!isChecked());
}