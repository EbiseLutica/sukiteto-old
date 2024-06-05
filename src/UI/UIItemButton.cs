namespace Sukiteto;

public class UIItemButton(string label, Action? onSelect = null) : UIItemBase
{
    public override string Render() => label;

    public override void OnSelect() => onSelect?.Invoke();
}