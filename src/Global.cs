using System.Diagnostics.CodeAnalysis;
using DotFeather;

namespace Sukiteto;

public static class Global
{
    public static readonly string Version = "1.0.0-alpha.1";
    
    public static ResourceLoader Resources { get; private set; }
    public static ShapeLoader Shapes { get; private set; }
    public static AudioPlayer Audio { get; private set; }

    public static void Initialize()
    {
        Resources = new ResourceLoader();
        Shapes = new ShapeLoader();
        Audio = new AudioPlayer();
    }
}