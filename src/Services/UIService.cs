using Promete;

namespace Quadrix;

public class UIService
{
    private readonly List<UIItemBase> _items = [];
    
    private int _cursorIndex = 0;
    private readonly InputService _input;

    public UIService(InputService input, PrometeApp app)
    {
        _input = input;
        app.Update += Tick;
        app.Destroy -= Tick;
        
        app.SceneWillChange += _ =>
        {
            Show();
        };
    }

    public void Show(params UIItemBase[] items)
    {
        _items.Clear();
        _items.AddRange(items);
        _cursorIndex = 0;
        items.FirstOrDefault()?.OnCursorEnter();
    }
    
    public string Render()
    {
        return string.Join("\n", _items.Select((it, i) => $"{(i == _cursorIndex ? "> " : "  ")} {it.Render()}"));
    }

    private void Tick()
    {
        if (_items.Count == 0) return;
        if (_input[InputType.MenuUp].IsButtonDown)
        {
            _items[_cursorIndex].OnCursorLeave();
            _cursorIndex--;
            if (_cursorIndex < 0) _cursorIndex = _items.Count - 1;
            _items[_cursorIndex].OnCursorEnter();
        }
        if (_input[InputType.MenuDown].IsButtonDown)
        {
            _items[_cursorIndex].OnCursorLeave();
            _cursorIndex++;
            if (_cursorIndex >= _items.Count) _cursorIndex = 0;
            _items[_cursorIndex].OnCursorEnter();
        }
        if (_input[InputType.MoveLeft].IsButtonDown)
        {
            _items[_cursorIndex].OnLeftPress();
        }
        if (_input[InputType.MoveRight].IsButtonDown)
        {
            _items[_cursorIndex].OnRightPress();
        }
        if (_input[InputType.Ok].IsButtonDown)
        {
            _items[_cursorIndex].OnSelect();
        }
    }
}