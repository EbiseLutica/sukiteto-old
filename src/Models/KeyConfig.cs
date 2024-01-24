using Promete.Input;

namespace Sukiteto;

public class KeyConfig
{
    public KeyCode MoveLeft { get; set; } = KeyCode.Left;
    public KeyCode MoveRight { get; set; } = KeyCode.Right;
    public KeyCode RotateLeft { get; set; } = KeyCode.Z;
    public KeyCode RotateRight { get; set; } = KeyCode.X;
    public KeyCode SoftDrop { get; set; } = KeyCode.Down;
    public KeyCode HardDrop { get; set; } = KeyCode.Up;
    public KeyCode Hold { get; set; } = KeyCode.C;

    public KeyCode Pause { get; set; } = KeyCode.Enter;
    
    public KeyCode Quit { get; set; } = KeyCode.Escape;
    
    public KeyCode Ok { get; set; } = KeyCode.Z;
}