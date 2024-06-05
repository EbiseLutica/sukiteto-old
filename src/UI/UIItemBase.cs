namespace Sukiteto;

public abstract class UIItemBase
{
    public abstract string Render();
    
    public virtual void OnCursorEnter() { }
    public virtual void OnCursorLeave() { }
    
    public virtual void OnLeftPress() { }
    public virtual void OnRightPress() { }
    public virtual void OnSelect() { }
}