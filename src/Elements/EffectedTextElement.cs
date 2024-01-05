using System.Drawing;
using DotFeather;

namespace Sukiteto;

public class EffectedTextElement : TextElement
{
    public EffectType Effect { get; set; }
    public float EffectTime { get; set; }

    private float timer;

    public EffectedTextElement()
    {
    }

    public EffectedTextElement(string text) : base(text)
    {
    }

    public EffectedTextElement(string text, DFFont font) : base(text, font)
    {
    }

    public EffectedTextElement(string text, DFFont font, Color? color) : base(text, font, color)
    {
    }

    public EffectedTextElement(string text, float fontSize) : base(text, fontSize)
    {
    }

    public EffectedTextElement(string text, float fontSize, DFFontStyle fontStyle) : base(text, fontSize, fontStyle)
    {
    }

    public EffectedTextElement(string text, float fontSize, DFFontStyle fontStyle, Color? color) : base(text, fontSize, fontStyle, color)
    {
    }

    protected override void OnUpdate()
    {
        timer += Time.DeltaTime;
        
        if (timer > EffectTime)
        {
            Parent.Remove(this);
            Destroy();
            return;
        }

        var time = timer / EffectTime;
        
        switch (Effect)
        {
            case EffectType.SlideUp:
                Location += Vector.Up * (12 * Time.DeltaTime);
                break;
            case EffectType.ScaleUp:
                Scale *= 1.4f * Time.DeltaTime;
                break;
        }

        if (time > 0.5f)
        {
            var a = (int)(((1 - time) * 2) * 255);
            var c = Color ?? System.Drawing.Color.White;
            Color = System.Drawing.Color.FromArgb(a, c.R, c.G, c.B);
        }
    }

    public enum EffectType
    {
        SlideUp,
        ScaleUp,
    }
}