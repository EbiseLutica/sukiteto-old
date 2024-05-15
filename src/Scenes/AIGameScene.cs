using System.Drawing;
using System.Text;
using Promete;
using Promete.Audio;
using Promete.Elements;
using Promete.Graphics;

namespace Sukiteto;

public class AIGameScene(AudioPlayer audio, Resources resources, InputService input) : Scene
{
    private bool _isGameOver;

    private GameBoard _gameBoard;
    
    private DefaultSoundPlugin _soundPlugin;

    private AISystem _ai;
    
    private readonly GameService _game = new(GameConfig.Default);

    private readonly Random _random = new();

    private readonly Queue<InputType> _inputQueue = [];

    private float aiInputDelay = 0.1f;

    private float aiInputTimer = 0;

    public override void OnStart()
    {
        _gameBoard = new GameBoard(_game);
        var mapLocation = Window.Size / 2 - _gameBoard.Size / 2;
        _gameBoard.Location = mapLocation;
        Root.Add(_gameBoard);
        
        _soundPlugin = new DefaultSoundPlugin(_game, audio, resources);
        
        _ai = new AISystem(_game);
        
        _game.LineClear += OnLineClear;
        _game.GameOver += OnGameOver;
        _game.SpawnNext += OnSpawnNext;

        _game.Start();

        audio.Gain = 0.3f;
        audio.Play(resources.BgmX030, resources.LoopX030);
    }

    public override void OnUpdate()
    {
        if (_isGameOver)
        {
            if (input[InputType.Ok].IsButtonUp)
            {
                App.LoadScene<TitleScene>();
            }

            return;
        }
        ProcessInput();
        ProcessAI();
        _game.Tick(Window.DeltaTime);
    }

    public override void OnDestroy()
    {
        audio.Stop();
        _soundPlugin.Dispose();
    }
    
    private void OnSpawnNext()
    {
        if (_isGameOver) return;
        var (firstMove, firstRotation, lastMove, lastRotation, useHold) = _ai.Think();
        
        _inputQueue.Clear();

        if (useHold)
        {
            _inputQueue.Enqueue(InputType.Hold);
        }

        switch (firstRotation)
        {
            case 1:
                _inputQueue.Enqueue(InputType.RotateRight);
                break;
            case 2:
                _inputQueue.Enqueue(InputType.RotateRight);
                _inputQueue.Enqueue(InputType.RotateRight);
                break;
            case 3:
                _inputQueue.Enqueue(InputType.RotateLeft);
                break;
        }
        
        for (var i = 0; i < Math.Abs(firstMove); i++)
        {
            _inputQueue.Enqueue(firstMove > 0 ? InputType.MoveRight : InputType.MoveLeft);
        }
        
        _inputQueue.Enqueue(InputType.HardDrop);
    }

    private void OnGameOver()
    {
        _isGameOver = true;
        ProcessGameOver();
    }

    private void OnLineClear(LineClearEventArgs e)
    {
        var builder = new StringBuilder();

        if (e.IsTSpin) builder.AppendLine(e.IsTSpinMini ? "T-Spin Mini" : "T-Spin");

        switch (e.ClearedLines)
        {
            case >= 4:
                builder.AppendLine("QUAD");
                break;
            case 3:
                builder.AppendLine("TRIPLE");
                break;
            case 2:
                builder.AppendLine("DOUBLE");
                break;
            case 1 when e.IsTSpin:
                builder.AppendLine("SINGLE");
                break;
        }

        var text = builder.ToString();

        if (string.IsNullOrWhiteSpace(text)) return;
        
        var effect = new EffectedText(text, 24, FontStyle.Normal, Color.White)
        {
            Effect = EffectedText.EffectType.SlideUp,
            EffectTime = 1,
            Location = (48, 120),
        };
        
        Root.Add(effect);
    }

    /// <summary>
    /// ユーザー入力の処理
    /// </summary>
    private void ProcessInput()
    {
        // リロード
        if (input[InputType.Quit].IsButtonDown)
        {
            App.LoadScene<TitleScene>();
        }
    }

    private void ProcessAI()
    {
        aiInputTimer += Window.DeltaTime;
        if (aiInputTimer < aiInputDelay) return;
        aiInputTimer = 0;

        if (_inputQueue.Count <= 0) return;

        var inputType = _inputQueue.Dequeue();
        switch (inputType)
        {
            case InputType.MoveLeft:
                _game.TriggerLeft();
                audio.PlayOneShot(resources.SfxMove);
                break;
            case InputType.MoveRight:
                _game.TriggerRight();
                audio.PlayOneShot(resources.SfxMove);
                break;
            case InputType.RotateLeft:
                _game.TriggerRotateLeft();
                audio.PlayOneShot(resources.SfxMove);
                break;
            case InputType.RotateRight:
                _game.TriggerRotateRight();
                audio.PlayOneShot(resources.SfxMove);
                break;
            case InputType.HardDrop:
                _game.TriggerHardDrop();
                audio.PlayOneShot(resources.SfxHardDrop);
                break;
            case InputType.Hold:
                _game.TriggerHold();
                audio.PlayOneShot(resources.SfxHold);
                break;
        }
    }
    
    /// <summary>
    /// ゲームオーバーの処理
    /// </summary>
    private void ProcessGameOver()
    {
        audio.Stop();
        var gameoverText = new Text("GAME OVER", Font.GetDefault(64, FontStyle.Normal), Color.Red);
        gameoverText.Location = (640 / 2 - gameoverText.Width / 2, 480 / 2 - gameoverText.Height / 2);
        audio.Play(resources.SfxGameOver);
        Root.Add(gameoverText);
    }
}