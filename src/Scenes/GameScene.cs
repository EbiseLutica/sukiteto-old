using System.Drawing;
using System.Text;
using Promete;
using Promete.Audio;
using Promete.Elements;
using Promete.Graphics;

namespace Sukiteto;

public class GameScene(AudioPlayer audio, Resources resources, InputService input) : Scene
{
    private bool _isGameOver;
    
    private float _dasTimer;

    private bool _isPausingGame = false;

    private readonly GameService _game = new(GameConfig.Default);
    
    private GameBoard _gameBoard;

    /// <summary>
    /// 長押ししてからブロックが動き始めるまでの時間
    /// </summary>
    private static readonly float das = 1f / 60 * 10;
    
    /// <summary>
    /// 長押し中のブロックの移動速度
    /// </summary>
    private static readonly float arr = 1f / 60 * 2;

    public override void OnStart()
    {
        _gameBoard = new GameBoard(_game);
        var mapLocation = Window.Size / 2 - _gameBoard.Size / 2;
        _gameBoard.Location = mapLocation;
        Root.Add(_gameBoard);
        
        _game.LineClear += OnLineClear;
        _game.BlockHit += OnBlockHit;
        _game.GameOver += OnGameOver;
        _game.TspinRotate += OnTspinRotate;

        _game.Start();

        audio.Gain = 0.1f;
        audio.Play(resources.BgmTypeA, 0);
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

        if (_isPausingGame) return;
        ProcessDas();
        ProcessInput();
        _game.Tick(Window.DeltaTime);
    }

    public override void OnDestroy()
    {
        audio.Stop();
    }

    private void OnTspinRotate()
    {
        audio.PlayOneShotAsync(resources.SfxTspinRotate);
    }

    private void OnGameOver()
    {
        _isGameOver = true;
        ProcessGameOver();
    }

    private void OnBlockHit()
    {
        audio.PlayOneShotAsync(resources.SfxHit);
    }

    private void OnLineClear(LineClearEventArgs e)
    {
        audio.PlayOneShotAsync(resources.GetLineClearSound(e));

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
    /// DAS（遅延付き連射入力）の制御
    /// </summary>
    private void ProcessDas()
    {
        var moved = false;
        if (input[InputType.MoveLeft].IsButtonDown) moved = _game.TriggerLeft();
        if (input[InputType.MoveRight].IsButtonDown) moved = _game.TriggerRight();

        if (input[InputType.MoveLeft].ElapsedTime >= das)
        {
            _dasTimer += Window.DeltaTime;
            if (_dasTimer > arr)
            {
                moved = _game.TriggerLeft();
                _dasTimer = 0;
            }
        }
        else if (input[InputType.MoveRight].ElapsedTime >= das)
        {
            _dasTimer += Window.DeltaTime;
            if (_dasTimer > arr)
            {
                moved = _game.TriggerRight();
                _dasTimer = 0;
            }
        }

        if (input[InputType.SoftDrop])
        {
            _dasTimer += Window.DeltaTime;
            if (_dasTimer > arr)
            {
                moved = _game.TriggerDown();
                _dasTimer = 0;
            }
        }
        
        if (!input[InputType.MoveLeft] && !input[InputType.MoveRight] && !input[InputType.SoftDrop])
        {
            _dasTimer = 0;
        }
        
        if (moved) audio.PlayOneShotAsync(resources.SfxMove);
    }

    /// <summary>
    /// ユーザー入力の処理
    /// </summary>
    private void ProcessInput()
    {
        if (input[InputType.HardDrop].IsButtonDown)
        {
            _game.TriggerHardDrop();
            audio.PlayOneShotAsync(resources.SfxHardDrop);
        }

        // 左回転
        if (input[InputType.RotateLeft].IsButtonDown && _game.TriggerRotateLeft())
        {
            audio.PlayOneShotAsync(resources.SfxMove);
        }
        
        // 右回転
        if (input[InputType.RotateRight].IsButtonDown && _game.TriggerRotateRight())
        {
            audio.PlayOneShotAsync(resources.SfxMove);
        }
        
        // リロード
        if (input[InputType.Quit].IsButtonDown)
        {
            App.LoadScene<TitleScene>();
        }
        
        // ホールド
        if (input[InputType.Hold].IsButtonDown && _game.TriggerHold())
        {
            audio.PlayOneShotAsync(resources.SfxHold);
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