using Promete.Audio;
using Promete.Windowing;

namespace Quadrix;

public class DefaultInputPlugin
{
    public float Das { get; set; } = 1f / 60 * 10;
    
    public float Arr { get; set; } = 1f / 60 * 2;
    
    private float _dasTimer;

    private readonly InputService _input;
    private readonly GameService _game;
    private readonly AudioPlayer _audio;
    private readonly Resources _resources;

    public DefaultInputPlugin(InputService input, GameService game, AudioPlayer audio, Resources resources)
    {
        _input = input;
        _game = game;
        _audio = audio;
        _resources = resources;
        
        game.SpawnNext += OnSpawnNext;
    }

    private void OnSpawnNext()
    {
        // 先行回転
        if (_input[InputType.RotateLeft] && _game.TriggerRotateLeft())
        {
            _audio.PlayOneShot(_resources.SfxInitial);
        }
        else if (_input[InputType.RotateRight] && _game.TriggerRotateRight())
        {
            _audio.PlayOneShot(_resources.SfxInitial);
        }
    }

    public void Process(float deltaTime)
    {
        ProcessDas(deltaTime);
        ProcessInput();
    }

    /// <summary>
    /// DAS（遅延付き連射入力）の制御
    /// </summary>
    private void ProcessDas(float deltaTime)
    {
        var moved = false;
        if (_input[InputType.MoveLeft].IsButtonDown) moved = _game.TriggerLeft();
        if (_input[InputType.MoveRight].IsButtonDown) moved = _game.TriggerRight();

        if (_input[InputType.MoveLeft].ElapsedTime >= Das)
        {
            _dasTimer += deltaTime;
            if (_dasTimer > Arr)
            {
                moved = _game.TriggerLeft();
                _dasTimer = 0;
            }
        }
        else if (_input[InputType.MoveRight].ElapsedTime >= Das)
        {
            _dasTimer += deltaTime;
            if (_dasTimer > Arr)
            {
                moved = _game.TriggerRight();
                _dasTimer = 0;
            }
        }

        if (_input[InputType.SoftDrop])
        {
            _dasTimer += deltaTime;
            if (_dasTimer > Arr)
            {
                moved = _game.TriggerDown();
                _dasTimer = 0;
            }
        }
        
        if (!_input[InputType.MoveLeft] && !_input[InputType.MoveRight] && !_input[InputType.SoftDrop])
        {
            _dasTimer = 0;
        }
        
        if (moved) _audio.PlayOneShot(_resources.SfxMove);
    }

    /// <summary>
    /// ユーザー入力の処理
    /// </summary>
    private void ProcessInput()
    {
        if (_input[InputType.HardDrop].IsButtonDown)
        {
            _game.TriggerHardDrop();
            _audio.PlayOneShot(_resources.SfxHardDrop);
        }

        // 左回転
        if (_input[InputType.RotateLeft].IsButtonDown && _game.TriggerRotateLeft())
        {
            _audio.PlayOneShot(_resources.SfxMove);
        }
        
        // 右回転
        if (_input[InputType.RotateRight].IsButtonDown && _game.TriggerRotateRight())
        {
            _audio.PlayOneShot(_resources.SfxMove);
        }
        
        // ホールド
        if (_input[InputType.Hold].IsButtonDown && _game.TriggerHold())
        {
            _audio.PlayOneShot(_resources.SfxHold);
        }
    }
}