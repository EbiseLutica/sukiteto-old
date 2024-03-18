using System.Collections.Specialized;
using System.Drawing;
using Promete;
using Promete.Elements;
using Promete.Graphics;
using Promete.Input;

namespace Sukiteto;

public class Button : ContainableElementBase
{
    public string Content { get; set; }
   
    public Color BackgroundColor { get; set; }
   
    public Color TextColor { get; set; }

    private Mouse mouse;
    private NineSliceSprite backdrop;
    private Text text;
    private bool isPressing;

    private static Texture9Sliced? textureNormal, textureActive;
    
    public Button(string content, Color? backgroundColor = default, Color? textColor = default, VectorInt? size = default)
    {
        Content = content;
        BackgroundColor = backgroundColor ?? Color.White;
        TextColor = textColor ?? Color.Black;
        base.Size = size ?? (100, 100);

        var app = PrometeApp.Current ?? throw new InvalidOperationException("Promete App is not initialized");
        mouse = app.GetPlugin<Mouse>() ?? throw new InvalidOperationException("Mouse plugin is not registered illegally!!");

        if (textureNormal == null || textureActive == null)
        {
            textureNormal = app.Window.TextureFactory.Load9Sliced("./assets/textures/ui_button.png", 5, 9, 5, 5);
            textureActive = app.Window.TextureFactory.Load9Sliced("./assets/textures/ui_button_active.png", 5, 9, 5, 5);
        }

        backdrop = new NineSliceSprite(textureNormal.Value, BackgroundColor)
            .Size(base.Size);

        text = new Text(Content, Font.GetDefault(16), TextColor);
        text.Location = (base.Size / 2 - text.Size / 2);
        
        children.Add(backdrop);
        children.Add(text);
        // isTrimmable = true;
    }

    protected override void OnUpdate()
    {
        backdrop.Size = Size;
        backdrop.TintColor = BackgroundColor;
        text.Location = (Size / 2 - text.Size / 2);
        text.Color = TextColor;
        
        var mousePos = mouse.Position;
        var isHover = IsHover(mousePos);
        
        backdrop.Texture = isPressing && isHover && mouse[MouseButtonType.Left] ? textureActive.Value : textureNormal.Value;

        if (mouse[MouseButtonType.Left].IsButtonDown && isHover)
        {
            isPressing = true;
        }

        if (isPressing && isHover && mouse[MouseButtonType.Left].IsButtonUp)
        {
            OnClick();
        }
        
        if (mouse[MouseButtonType.Left].IsButtonUp)
        {
            isPressing = false;
        }
    }

    private bool IsHover(VectorInt mousePos)
    {
        return mousePos.In(new Rect(Location, Size));
    }

    private void OnClick()
    {
        Click?.Invoke();
    }

    public event Action Click;
}