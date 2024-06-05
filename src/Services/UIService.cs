using Promete;
using Promete.Windowing;

namespace Sukiteto;

public class UIService
{
    private readonly List<UIItemBase> items = [];
    
    private int _cursorIndex = 0;
    private readonly InputService _input;

    public UIService(InputService input, IWindow window, PrometeApp app)
    {
        _input = input;
        window.Update += Tick;
        window.Destroy -= Tick;
        
        app.SceneWillChange += () =>
        {
            Show();
        };
    }

    public void Show(params UIItemBase[] items)
    {
        this.items.Clear();
        this.items.AddRange(items);
        _cursorIndex = 0;
        items.FirstOrDefault()?.OnCursorEnter();
    }
    
    public string Render()
    {
        return string.Join("\n", items.Select((it, i) => $"{(i == _cursorIndex ? "> " : "  ")} {it.Render()}"));
    }

    private void Tick()
    {
        if (items.Count == 0) return;
        if (_input[InputType.MenuUp].IsButtonDown)
        {
            items[_cursorIndex].OnCursorLeave();
            _cursorIndex--;
            if (_cursorIndex < 0) _cursorIndex = items.Count - 1;
            items[_cursorIndex].OnCursorEnter();
        }
        if (_input[InputType.MenuDown].IsButtonDown)
        {
            items[_cursorIndex].OnCursorLeave();
            _cursorIndex++;
            if (_cursorIndex >= items.Count) _cursorIndex = 0;
            items[_cursorIndex].OnCursorEnter();
        }
        if (_input[InputType.MoveLeft].IsButtonDown)
        {
            items[_cursorIndex].OnLeftPress();
        }
        if (_input[InputType.MoveRight].IsButtonDown)
        {
            items[_cursorIndex].OnRightPress();
        }
        if (_input[InputType.Ok].IsButtonDown)
        {
            items[_cursorIndex].OnSelect();
        }
    }
}