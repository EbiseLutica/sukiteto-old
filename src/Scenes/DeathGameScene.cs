using System.Drawing;
using System.Text;
using Promete;
using Promete.Audio;
using Promete.Elements;
using Promete.Graphics;

namespace Sukiteto;

public class DeathGameScene(AudioPlayer audio, Resources resources, InputService input) : Scene
{
    private bool _isGameOver;

    private bool _isPausingGame = false;

    private GameBoard _gameBoard;
    
    private DefaultSoundPlugin _soundPlugin;
    
    private DefaultInputPlugin _inputPlugin;

    private float _time = 0;

    private int _lines = 1;
    private int _score = 0;

    private int _placedPieces = 0;
    
    private const int ExpMax = 10;
    
    private readonly GameService _game = new(new GameConfig
    {
        RotationSystem = new ArcadeRotationSystem(),
        FallSpeed = float.MaxValue,
    });

    private string TimeString
    {
        get
        {
            var time = TimeSpan.FromSeconds(_time);
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}:{time.Milliseconds:D3}";
        }
    }
    
    private float Pps => _placedPieces / _time;

    public override void OnStart()
    {
        _soundPlugin = new DefaultSoundPlugin(_game, audio, resources);
        _inputPlugin = new DefaultInputPlugin(input, _game, audio, resources);
        
        _game.LineClear += OnLineClear;
        _game.BlockPlace += OnBlockPlace;
        _game.GameOver += OnGameOver;

        _gameBoard = new GameBoard(_game);
        var mapLocation = Window.Size / 2 - _gameBoard.Size / 2;
        _gameBoard.Location = mapLocation;
        Root.Add(_gameBoard);

        _game.Start();
        audio.Gain = 0.3f;
        audio.Play(resources.BgmX030, resources.LoopX030);
    }

    private void OnBlockPlace()
    {
        _placedPieces++;
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
        _time += Window.DeltaTime;
        ProcessInput();
        _game.Tick(Window.DeltaTime);
        
        _gameBoard.ScoreboardLeft["TIME"] = TimeString;
        _gameBoard.ScoreboardLeft["LINES"] = _lines.ToString();
        _gameBoard.ScoreboardLeft["SCORE"] = _score.ToString();
        
        _gameBoard.ScoreboardRight["PPS"] = Pps.ToString("F4");
        _gameBoard.ScoreboardRight["PLACED PIECES"] = _placedPieces.ToString();
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
        _lines += e.ClearedLines;

        if (_lines >= 1000)
        {
            _isGameOver = true;
            ProcessGameClear();
        }

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
        var gameoverText = new Text("YOU DIED", Font.GetDefault(64, FontStyle.Normal), Color.Red);
        gameoverText.Location = (640 / 2 - gameoverText.Width / 2, 480 / 2 - gameoverText.Height / 2);
        audio.Play(resources.SfxGameOver);
        Root.Add(gameoverText);
    }

    /// <summary>
    /// ゲームオーバーの処理
    /// </summary>
    private void ProcessGameClear()
    {
        audio.Stop();
        var gameClearText = new Text("ASCENDED!!", Font.GetDefault(64, FontStyle.Normal), Color.Turquoise);
        gameClearText.Location = (640 / 2 - gameClearText.Width / 2, 480 / 2 - gameClearText.Height / 2);
        Root.Add(gameClearText);
    }
}