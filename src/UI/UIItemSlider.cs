namespace Sukiteto;

public class UIItemSlider(string label, Func<int> value, Action<int>? onValueChange) : UIItemBase
{
    public override string Render() => $"{label,-100}: {value()}";

    public override void OnLeftPress() => onValueChange?.Invoke(value() - 1);
    public override void OnRightPress() => onValueChange?.Invoke(value() + 1);
}