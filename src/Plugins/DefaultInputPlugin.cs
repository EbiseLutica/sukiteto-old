using Promete.Audio;
using Promete.Windowing;

namespace Sukiteto;

public class DefaultInputPlugin(InputService input, GameService game, AudioPlayer audio, Resources resources)
{
    public float Das { get; set; } = 1f / 60 * 10;
    
    public float Arr { get; set; } = 1f / 60 * 2;
    
    private float _dasTimer;

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
        if (input[InputType.MoveLeft].IsButtonDown) moved = game.TriggerLeft();
        if (input[InputType.MoveRight].IsButtonDown) moved = game.TriggerRight();

        if (input[InputType.MoveLeft].ElapsedTime >= Das)
        {
            _dasTimer += deltaTime;
            if (_dasTimer > Arr)
            {
                moved = game.TriggerLeft();
                _dasTimer = 0;
            }
        }
        else if (input[InputType.MoveRight].ElapsedTime >= Das)
        {
            _dasTimer += deltaTime;
            if (_dasTimer > Arr)
            {
                moved = game.TriggerRight();
                _dasTimer = 0;
            }
        }

        if (input[InputType.SoftDrop])
        {
            _dasTimer += deltaTime;
            if (_dasTimer > Arr)
            {
                moved = game.TriggerDown();
                _dasTimer = 0;
            }
        }
        
        if (!input[InputType.MoveLeft] && !input[InputType.MoveRight] && !input[InputType.SoftDrop])
        {
            _dasTimer = 0;
        }
        
        if (moved) audio.PlayOneShot(resources.SfxMove);
    }

    /// <summary>
    /// ユーザー入力の処理
    /// </summary>
    private void ProcessInput()
    {
        if (input[InputType.HardDrop].IsButtonDown)
        {
            game.TriggerHardDrop();
            audio.PlayOneShot(resources.SfxHardDrop);
        }

        // 左回転
        if (input[InputType.RotateLeft].IsButtonDown && game.TriggerRotateLeft())
        {
            audio.PlayOneShot(resources.SfxMove);
        }
        
        // 右回転
        if (input[InputType.RotateRight].IsButtonDown && game.TriggerRotateRight())
        {
            audio.PlayOneShot(resources.SfxMove);
        }
        
        // ホールド
        if (input[InputType.Hold].IsButtonDown && game.TriggerHold())
        {
            audio.PlayOneShot(resources.SfxHold);
        }
    }
}