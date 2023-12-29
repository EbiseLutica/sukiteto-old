using DotFeather;

namespace Sukiteto;

public class ResourceLoader
{
    public Dictionary<BlockColor, Texture2D> Block { get; }
    
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
        Block = new Dictionary<BlockColor, Texture2D>
        {
            [BlockColor.O] = minos[0],
            [BlockColor.J] = minos[1],
            [BlockColor.L] = minos[2],
            [BlockColor.Z] = minos[3],
            [BlockColor.S] = minos[4],
            [BlockColor.T] = minos[5],
            [BlockColor.I] = minos[6],
            [BlockColor.Ghost] = minos[7]
        };
    }
}