using Promete.Input;

namespace Sukiteto;

public class InputConfig
{
    public Dictionary<InputType, GamepadButtonType>[] Gamepads { get; set; } =
    [
        new Dictionary<InputType, GamepadButtonType>
        {
            { InputType.MoveLeft, GamepadButtonType.DpadLeft },
            { InputType.MoveRight, GamepadButtonType.DpadRight },
            { InputType.RotateLeft, GamepadButtonType.B },
            { InputType.RotateRight, GamepadButtonType.A },
            { InputType.SoftDrop, GamepadButtonType.DpadDown },
            { InputType.HardDrop, GamepadButtonType.DpadUp },
            { InputType.Hold, GamepadButtonType.L1 },
            { InputType.Pause, GamepadButtonType.Plus },
            { InputType.Quit, GamepadButtonType.Minus },
            { InputType.Ok, GamepadButtonType.A },
            { InputType.MenuUp, GamepadButtonType.DpadUp },
            { InputType.MenuDown, GamepadButtonType.DpadDown },
        },
        new Dictionary<InputType, GamepadButtonType>
        {
            { InputType.Hold , GamepadButtonType.R1},
        },
        new Dictionary<InputType, GamepadButtonType>(),
    ];

    public Dictionary<InputType, KeyCode>[] Keys { get; set; } =
    [
        new Dictionary<InputType, KeyCode>
        {
            { InputType.MoveLeft, KeyCode.Left },
            { InputType.MoveRight, KeyCode.Right },
            { InputType.RotateLeft, KeyCode.Z },
            { InputType.RotateRight, KeyCode.X },
            { InputType.SoftDrop, KeyCode.Down },
            { InputType.HardDrop, KeyCode.Space },
            { InputType.Hold, KeyCode.C },
            { InputType.Pause, KeyCode.Enter },
            { InputType.Quit, KeyCode.Escape },
            { InputType.Ok, KeyCode.Enter },
            { InputType.MenuUp, KeyCode.Up },
            { InputType.MenuDown, KeyCode.Down },
        },
        new Dictionary<InputType, KeyCode>
        {
            { InputType.RotateLeft, KeyCode.ControlLeft },
            { InputType.RotateRight, KeyCode.Up },
            { InputType.Hold, KeyCode.ShiftLeft },
        },
        new Dictionary<InputType, KeyCode>(),
    ];
}