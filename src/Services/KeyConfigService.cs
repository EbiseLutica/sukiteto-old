using System.Text.Json;
using System.Text.Json.Serialization;
using Promete.Input;
using Promete.Windowing;

namespace Sukiteto;

public class KeyConfigService
{
    private static readonly string jsonFilePath = "keyconfig.json";

    [JsonIgnore]
    public Key KeyMoveLeft => _keyboard.KeyOf(_keyConfig.MoveLeft);
    [JsonIgnore]
    public Key KeyMoveRight => _keyboard.KeyOf(_keyConfig.MoveRight);
    [JsonIgnore]
    public Key KeyRotateLeft => _keyboard.KeyOf(_keyConfig.RotateLeft);
    [JsonIgnore]
    public Key KeyRotateRight => _keyboard.KeyOf(_keyConfig.RotateRight);
    [JsonIgnore]
    public Key KeySoftDrop => _keyboard.KeyOf(_keyConfig.SoftDrop);
    [JsonIgnore]
    public Key KeyHardDrop => _keyboard.KeyOf(_keyConfig.HardDrop);
    [JsonIgnore]
    public Key KeyHold => _keyboard.KeyOf(_keyConfig.Hold);
    [JsonIgnore]
    public Key KeyPause => _keyboard.KeyOf(_keyConfig.Pause);
    [JsonIgnore]
    public Key KeyQuit => _keyboard.KeyOf(_keyConfig.Quit);
    [JsonIgnore]
    public Key KeyOk => _keyboard.KeyOf(_keyConfig.Ok);

    private readonly Keyboard _keyboard;
    private readonly KeyConfig _keyConfig;

    public KeyConfigService(Keyboard keyboard, IWindow window)
    {
        _keyboard = keyboard;
        _keyConfig = Load();

        window.Destroy += Save;
    }

    private KeyConfig Load()
    {
        if (!File.Exists(jsonFilePath)) return new KeyConfig();

        var json = File.ReadAllText(jsonFilePath);
        return JsonSerializer.Deserialize<KeyConfig>(json) ?? new KeyConfig();
    }
    
    public void Save()
    {
        var json = JsonSerializer.Serialize(_keyConfig);
        File.WriteAllText(jsonFilePath, json);
    }
}