using System.Diagnostics.CodeAnalysis;
using DotFeather;
using SukiTeto;

namespace Sukiteto;

public static class Global
{
    public static ResourceLoader Resources { get; private set; }
    public static MinoLoader Minos { get; private set; }
    public static AudioPlayer Audio { get; private set; }

    public static void Initialize()
    {
        Resources = new ResourceLoader();
        Minos = new MinoLoader();
        Audio = new AudioPlayer();
    }
}