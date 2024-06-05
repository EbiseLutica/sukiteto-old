namespace Sukiteto;

public class UIItemButton(string label, Action? onSelect) : UIItemBase
{
    public override string Render() => label;

    public override void OnSelect() => onSelect?.Invoke();
}