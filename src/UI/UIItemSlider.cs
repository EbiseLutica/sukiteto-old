namespace Sukiteto;

public class UIItemSlider(string label, Func<int> value, Action<int>? onValueChange, Func<string>? valueLabel = null) : UIItemBase
{
    public override string Render() => $"{label}: {valueLabel?.Invoke() ?? value().ToString()}";

    public override void OnLeftPress() => onValueChange?.Invoke(value() - 1);
    public override void OnRightPress() => onValueChange?.Invoke(value() + 1);
}