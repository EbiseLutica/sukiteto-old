using System.Text.Json;
using System.Text.Json.Serialization;
using DotFeather;

namespace Sukiteto;

public class KeyConfigService
{
    private static readonly string jsonFilePath = "keyconfig.json";

    public DFKeyCode MoveLeft { get; set; } = DFKeyCode.Left;
    public DFKeyCode MoveRight { get; set; } = DFKeyCode.Right;
    public DFKeyCode RotateLeft { get; set; } = DFKeyCode.Z;
    public DFKeyCode RotateRight { get; set; } = DFKeyCode.X;
    public DFKeyCode SoftDrop { get; set; } = DFKeyCode.Down;
    public DFKeyCode HardDrop { get; set; } = DFKeyCode.Up;
    public DFKeyCode Hold { get; set; } = DFKeyCode.C;

    public DFKeyCode Pause { get; set; } = DFKeyCode.Enter;
    
    public DFKeyCode Quit { get; set; } = DFKeyCode.Escape;
    
    public DFKeyCode Ok { get; set; } = DFKeyCode.Z;

    [JsonIgnore]
    public DFKey KeyMoveLeft => DFKeyboard.KeyOf(MoveLeft);
    [JsonIgnore]
    public DFKey KeyMoveRight => DFKeyboard.KeyOf(MoveRight);
    [JsonIgnore]
    public DFKey KeyRotateLeft => DFKeyboard.KeyOf(RotateLeft);
    [JsonIgnore]
    public DFKey KeyRotateRight => DFKeyboard.KeyOf(RotateRight);
    [JsonIgnore]
    public DFKey KeySoftDrop => DFKeyboard.KeyOf(SoftDrop);
    [JsonIgnore]
    public DFKey KeyHardDrop => DFKeyboard.KeyOf(HardDrop);
    [JsonIgnore]
    public DFKey KeyHold => DFKeyboard.KeyOf(Hold);
    [JsonIgnore]
    public DFKey KeyPause => DFKeyboard.KeyOf(Pause);
    [JsonIgnore]
    public DFKey KeyQuit => DFKeyboard.KeyOf(Quit);
    [JsonIgnore]
    public DFKey KeyOk => DFKeyboard.KeyOf(Ok);

    public static KeyConfigService Load()
    {
        if (!File.Exists(jsonFilePath)) return new KeyConfigService();

        var json = File.ReadAllText(jsonFilePath);
        return JsonSerializer.Deserialize<KeyConfigService>(json) ?? new KeyConfigService();
    }
    
    public void Save()
    {
        var json = JsonSerializer.Serialize(this);
        File.WriteAllText(jsonFilePath, json);
    }
}