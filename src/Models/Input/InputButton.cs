using Promete.Input;

namespace Sukiteto;

public class InputButton(InputType inputType, InputConfig config)
{
    public bool IsPressed { get; private set; }
    public bool IsButtonDown { get; private set; }
    public bool IsButtonUp { get; private set; }
    public float ElapsedTime { get; private set; }
    
    private bool prevIsPressed;

    public void Update(float deltaTime, Gamepad? gamepad, Keyboard keyboard)
    {
        var isAnyButtonPressed = gamepad != null && config.Gamepads.Any(padConfig => padConfig.TryGetValue(inputType, out var buttonType) && gamepad[buttonType].IsPressed);
        var isAnyKeyPressed = config.Keys.Any(keyConfig => keyConfig.TryGetValue(inputType, out var keyCode) && keyboard.KeyOf(keyCode).IsPressed);
        
        IsPressed = isAnyButtonPressed || isAnyKeyPressed;
        IsButtonDown = IsPressed && !prevIsPressed;
        IsButtonUp = !IsPressed && prevIsPressed;
        ElapsedTime = IsPressed ? ElapsedTime + deltaTime : 0;
        
        prevIsPressed = IsPressed;
    }
    
    public static implicit operator bool(InputButton button) => button.IsPressed;
}