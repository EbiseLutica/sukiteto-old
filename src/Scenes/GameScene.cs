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

    private bool _isPausingGame = false;

    private GameBoard _gameBoard;
    
    private DefaultSoundPlugin _soundPlugin;
    
    private DefaultInputPlugin _inputPlugin;
    
    private readonly GameService _game = new(GameConfig.Default);

    public override void OnStart()
    {
        _gameBoard = new GameBoard(_game);
        var mapLocation = Window.Size / 2 - _gameBoard.Size / 2;
        _gameBoard.Location = mapLocation;
        Root.Add(_gameBoard);
        
        _soundPlugin = new DefaultSoundPlugin(_game, audio, resources);
        _inputPlugin = new DefaultInputPlugin(input, _game, audio, resources);
        
        _game.LineClear += OnLineClear;
        _game.GameOver += OnGameOver;

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

        if (_isPausingGame) return;
        _inputPlugin.Process(Window.DeltaTime);
        ProcessInput();
        _game.Tick(Window.DeltaTime);
    }

    public override void OnDestroy()
    {
        audio.Stop();
        _soundPlugin.Dispose();
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