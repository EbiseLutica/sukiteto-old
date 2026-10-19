using System.Text.Json;
using System.Text.Json.Serialization;
using Promete.Input;
using Promete;
using Promete.Backends;

namespace Quadrix;

public class InputService
{
    private static readonly string jsonFilePath = "keyconfig.json";
    
    private readonly InputConfig _inputConfig;
    private readonly Keyboard _keyboard;
    private readonly Gamepads _gamepads;
    private readonly Dictionary<InputType, InputButton> _inputButtons = new();
    
    public InputButton this[InputType type] => _inputButtons[type];

    /// <summary>
    /// 1 つの操作に割り当てられるキーのスロット数
    /// </summary>
    public const int KeySlotCount = 3;

    /// <summary>
    /// 現在の入力設定。変更は即座に入力判定へ反映される。
    /// </summary>
    public InputConfig Config => _inputConfig;

    public InputService(Keyboard keyboard, Gamepads gamepads, PrometeApp app, ITimeProvider time)
    {
        _keyboard = keyboard;
        _gamepads = gamepads;
        _inputConfig = LoadConfig();
        foreach (var type in Enum.GetValues<InputType>())
        {
            _inputButtons[type] = new InputButton(type, _inputConfig);
        }
        
        app.Update += () =>
        {
            var pad = _gamepads[0];
            foreach (var button in _inputButtons.Values)
            {
                button.Update(time.DeltaTime, pad, _keyboard);
            }
        };

        app.Destroy += Save;
    }

    private InputConfig LoadConfig()
    {
        if (!File.Exists(jsonFilePath)) return new InputConfig();

        var json = File.ReadAllText(jsonFilePath);
        var config = JsonSerializer.Deserialize<InputConfig>(json) ?? new InputConfig();
        if (config.Keys.Length >= KeySlotCount && config.Gamepads.Length >= KeySlotCount) return config;

        // スロット数が足りない古い設定ファイルは空のスロットで補う
        return new InputConfig
        {
            Gamepads = PadSlots(config.Gamepads),
            Keys = PadSlots(config.Keys),
        };

        static Dictionary<InputType, T>[] PadSlots<T>(Dictionary<InputType, T>[] slots) =>
            [..slots, ..Enumerable.Range(0, Math.Max(0, KeySlotCount - slots.Length)).Select(_ => new Dictionary<InputType, T>())];
    }
    
    public void Save()
    {
        var json = JsonSerializer.Serialize(_inputConfig);
        File.WriteAllText(jsonFilePath, json);
    }
}