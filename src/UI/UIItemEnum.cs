namespace Sukiteto;

public class UIItemEnum<T>(string label, Func<T> value, Action<T>? onValueChange, Func<string>? valueLabel = null) : UIItemBase where T : struct, Enum
{
    public override string Render() => $"{label}: {valueLabel?.Invoke() ?? value().ToString()}";

    public override void OnLeftPress()
    {
        var values = Enum.GetValues<T>();
        var index = Array.IndexOf(values, value());
        index--;
        if (index < 0) index = values.Length - 1;
        onValueChange?.Invoke(values[index]);
    }
    
    public override void OnRightPress()
    {
        var values = Enum.GetValues<T>();
        var index = Array.IndexOf(values, value());
        index++;
        if (index >= values.Length) index = 0;
        onValueChange?.Invoke(values[index]);
    }
}