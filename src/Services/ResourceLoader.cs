using DotFeather;

namespace Sukiteto;

public class ResourceLoader
{
    public Dictionary<BlockColor, Texture2D> Block { get; }

    public IAudioSource BgmTypeA { get; } = new VorbisAudioSource("./assets/sounds/type_a.ogg");
    
    public IAudioSource SfxMove { get; } = new WaveAudioSource("./assets/sounds/move.wav");
    public IAudioSource SfxHold { get; } = new WaveAudioSource("./assets/sounds/hold.wav");
    public IAudioSource SfxLineClear { get; } = new WaveAudioSource("./assets/sounds/lineclear.wav");
    public IAudioSource SfxLineClearDouble { get; } = new WaveAudioSource("./assets/sounds/lineclear_double.wav");
    public IAudioSource SfxLineClearTriple { get; } = new WaveAudioSource("./assets/sounds/lineclear_triple.wav");
    public IAudioSource SfxLineClearQuad { get; } = new WaveAudioSource("./assets/sounds/lineclear_quad.wav");
    public IAudioSource SfxLineClearAll { get; } = new WaveAudioSource("./assets/sounds/lineclear.wav");
    public IAudioSource SfxLineClearTspin { get; } = new WaveAudioSource("./assets/sounds/lineclear_tspin.wav");
    public IAudioSource SfxLineClearFix { get; } = new WaveAudioSource("./assets/sounds/hard_drop.wav");
    public IAudioSource SfxHardDrop { get; } = new WaveAudioSource("./assets/sounds/hard_drop.wav");
    public IAudioSource SfxHit { get; } = new WaveAudioSource("./assets/sounds/hit.wav");
    public IAudioSource SfxGameOver { get; } = new WaveAudioSource("./assets/sounds/gameover.wav");
    public IAudioSource SfxTspinRotate { get; } = new WaveAudioSource("./assets/sounds/tspin_rotate.wav");

    public ResourceLoader()
    {
        var minos = Texture2D.LoadAndSplitFrom("./assets/textures/shapes.png", 9, 1, (16, 16));
        Block = new Dictionary<BlockColor, Texture2D>
        {
            [BlockColor.O] = minos[0],
            [BlockColor.J] = minos[1],
            [BlockColor.L] = minos[2],
            [BlockColor.Z] = minos[3],
            [BlockColor.S] = minos[4],
            [BlockColor.T] = minos[5],
            [BlockColor.I] = minos[6],
            [BlockColor.Ghost] = minos[7],
            [BlockColor.Wall] = minos[8],
        };
    }

    public IAudioSource GetLineClearSound(LineClearEventArgs e)
    {
        if (e.IsAllClear) return SfxLineClearAll;
        if (e.IsTSpin || e.IsTSpinMini) return SfxLineClearTspin;
        
        return e.ClearedLines switch
        {
            1 => SfxLineClear,
            2 => SfxLineClearDouble,
            3 => SfxLineClearTriple,
            >= 4 => SfxLineClearQuad,
            _ => SfxLineClear,
        };
    }
}