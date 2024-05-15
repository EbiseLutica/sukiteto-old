using Promete.Audio;

namespace Sukiteto;

public class DefaultSoundPlugin : IDisposable
{
    private readonly GameService _game;
    private readonly AudioPlayer _audio;
    private readonly Resources _resources;
    
    public DefaultSoundPlugin(GameService game, AudioPlayer audio, Resources resources)
    {
        _game = game;
        _audio = audio;
        _resources = resources;

        game.LineClear += OnLineClear;
        game.BlockHit += OnBlockHit;
        game.TspinRotate += OnTspinRotate;
    }

    private void OnTspinRotate()
    {
        _audio.PlayOneShot(_resources.SfxTspinRotate);
    }

    private void OnBlockHit()
    {
        _audio.PlayOneShot(_resources.SfxHit);
    }
    
    private void OnLineClear(LineClearEventArgs e)
    {
        _audio.PlayOneShot(_resources.GetLineClearSound(e));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _game.LineClear -= OnLineClear;
        _game.BlockHit -= OnBlockHit;
        _game.TspinRotate -= OnTspinRotate;
    }
}