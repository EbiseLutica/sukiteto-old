using System.Text.Json;
using System.Text.Json.Serialization;
using Promete.Input;
using Promete.Windowing;

namespace Quadrix;

public class InputService
{
    private static readonly string jsonFilePath = "keyconfig.json";
    
    private readonly InputConfig _inputConfig;
    private readonly Keyboard _keyboard;
    private readonly Gamepads _gamepads;
    private readonly Dictionary<InputType, InputButton> _inputButtons = new();
    
    public InputButton this[InputType type] => _inputButtons[type];

    public InputService(Keyboard keyboard, Gamepads gamepads, IWindow window)
    {
        _keyboard = keyboard;
        _gamepads = gamepads;
        _inputConfig = LoadConfig();
        foreach (var type in Enum.GetValues<InputType>())
        {
            _inputButtons[type] = new InputButton(type, _inputConfig);
        }
        
        window.Update += () =>
        {
            var pad = _gamepads[0];
            foreach (var button in _inputButtons.Values)
            {
                button.Update(window.DeltaTime, pad, _keyboard);
            }
        };

        window.Destroy += Save;
    }

    private InputConfig LoadConfig()
    {
        if (!File.Exists(jsonFilePath)) return new InputConfig();

        var json = File.ReadAllText(jsonFilePath);
        return JsonSerializer.Deserialize<InputConfig>(json) ?? new InputConfig();
    }
    
    public void Save()
    {
        var json = JsonSerializer.Serialize(_inputConfig);
        File.WriteAllText(jsonFilePath, json);
    }
}