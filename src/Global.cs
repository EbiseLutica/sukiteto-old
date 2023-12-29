using System.Diagnostics.CodeAnalysis;
using DotFeather;
using SukiTeto;

namespace Sukiteto;

public static class Global
{
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