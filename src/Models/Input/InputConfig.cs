using Promete.Input;

namespace Quadrix;

public class InputConfig
{
    public Dictionary<InputType, GamepadButtonType>[] Gamepads { get; init; } =
    [
        new()
        {
            { InputType.MoveLeft, GamepadButtonType.DpadLeft },
            { InputType.MoveRight, GamepadButtonType.DpadRight },
            { InputType.RotateLeft, GamepadButtonType.B },
            { InputType.RotateRight, GamepadButtonType.A },
            { InputType.Rotate180, GamepadButtonType.Y },
            { InputType.SoftDrop, GamepadButtonType.DpadDown },
            { InputType.HardDrop, GamepadButtonType.DpadUp },
            { InputType.Hold, GamepadButtonType.L1 },
            { InputType.Pause, GamepadButtonType.Plus },
            { InputType.Quit, GamepadButtonType.Minus },
            { InputType.Ok, GamepadButtonType.A },
            { InputType.MenuUp, GamepadButtonType.DpadUp },
            { InputType.MenuDown, GamepadButtonType.DpadDown },
        },
        new()
        {
            { InputType.Hold , GamepadButtonType.R1},
        },
        new(),
    ];

    public Dictionary<InputType, KeyCode>[] Keys { get; init; } =
    [
        new()
        {
            { InputType.MoveLeft, KeyCode.Left },
            { InputType.MoveRight, KeyCode.Right },
            { InputType.RotateLeft, KeyCode.Z },
            { InputType.RotateRight, KeyCode.X },
            { InputType.Rotate180, KeyCode.A },
            { InputType.SoftDrop, KeyCode.Down },
            { InputType.HardDrop, KeyCode.Space },
            { InputType.Hold, KeyCode.C },
            { InputType.Pause, KeyCode.Enter },
            { InputType.Quit, KeyCode.Escape },
            { InputType.Ok, KeyCode.Enter },
            { InputType.MenuUp, KeyCode.Up },
            { InputType.MenuDown, KeyCode.Down },
        },
        new()
        {
            { InputType.RotateLeft, KeyCode.ControlLeft },
            { InputType.RotateRight, KeyCode.Up },
            { InputType.Hold, KeyCode.ShiftLeft },
        },
        new(),
    ];
}