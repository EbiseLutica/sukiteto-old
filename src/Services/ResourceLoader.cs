using DotFeather;

namespace Sukiteto;

public class ResourceLoader
{
    public Dictionary<MinoType, Texture2D> Mino { get; }
    
    public Texture2D Wall { get; } = Texture2D.LoadFrom("./assets/textures/wall.png");

    public IAudioSource BgmTypeA { get; } = new VorbisAudioSource("./assets/sounds/type_a.ogg");
    
    public IAudioSource SfxMove { get; } = new WaveAudioSource("./assets/sounds/move.wav");
    public IAudioSource SfxHold { get; } = new WaveAudioSource("./assets/sounds/hold.wav");
    public IAudioSource SfxLineClear { get; } = new WaveAudioSource("./assets/sounds/lineclear.wav");
    public IAudioSource SfxHardDrop { get; } = new WaveAudioSource("./assets/sounds/hard_drop.wav");
    public IAudioSource SfxGameOver { get; } = new WaveAudioSource("./assets/sounds/gameover.wav");

    public ResourceLoader()
    {
        var minos = Texture2D.LoadAndSplitFrom("./assets/textures/minos.png", 8, 1, (8, 8));
        Mino = new Dictionary<MinoType, Texture2D>
        {
            [MinoType.O] = minos[0],
            [MinoType.J] = minos[1],
            [MinoType.L] = minos[2],
            [MinoType.Z] = minos[3],
            [MinoType.S] = minos[4],
            [MinoType.T] = minos[5],
            [MinoType.I] = minos[6],
            [MinoType.Ghost] = minos[7]
        };
    }
}