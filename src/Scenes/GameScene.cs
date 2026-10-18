using System.Drawing;
using System.Text;
using Promete;
using Promete.Audio;
using Promete.Nodes;
using Promete.Graphics;
using Promete.Graphics.Fonts;

namespace Quadrix;

public class GameScene(AudioPlayer audio, Resources resources, InputService input) : Scene
{
    private bool _isGameOver;
    private bool _isPausingGame = false;

    private GameBoard _gameBoard;
    private DefaultSoundPlugin _soundPlugin;
    private DefaultInputPlugin _inputPlugin;

    private float _time = 0;

    private int _level = 1;
    private int _lines = 1;
    private int _score = 0;

    private int _placedPieces = 0;
    private int _exp = 0;
    
    private const int ExpMax = 10;

    private int _startCount = 5;
    private float _startTimer = 0;

    private readonly Text _countdownTimerText = new Text("", Font.GetDefault(64, FontStyle.Normal), Color.White);

    private readonly IRotationSystem[] _rotationSystems = [
        new StandardRotationSystem(),
        new QuadrixRotationSystem(),
        new ArcadeRotationSystem(),
        new ClassicRotationSystem(),
        new JoyjoyRotationSystem(),
    ];

    private int _rotationSystemIndex = 0;
    
    private readonly GameService _game = new(new GameConfig
    {
        RotationSystem = new StandardRotationSystem(),
        // RandomGenerator = new SmartRandomGenerator(3),
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

        _game.Config.FallSpeed = CalculateSpeed(_level);

        _gameBoard = new GameBoard(_game);
        var mapLocation = View.Size / 2 - _gameBoard.Size / 2;
        _gameBoard.Location = mapLocation;
        Root.Add(_gameBoard);

        _countdownTimerText.Content = _startCount.ToString();
        _countdownTimerText.Location = View.Size / 2 - (64, 64);
        Root.Add(_countdownTimerText);
        audio.PlayOneShot(resources.SfxCounts[5]);
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
        UpdateScoreboard();
        ProcessInput();

        if (_startCount > 0)
        {
            if (input[InputType.MoveLeft].IsButtonDown)
            {
                _rotationSystemIndex--;
                if (_rotationSystemIndex < 0) 
                    _rotationSystemIndex = _rotationSystems.Length - 1;
                _game.Config.RotationSystem = _rotationSystems[_rotationSystemIndex];
                audio.PlayOneShot(resources.SfxLineClear);
            }
            else if (input[InputType.MoveRight].IsButtonDown)
            {
                _rotationSystemIndex++;
                if (_rotationSystemIndex >= _rotationSystems.Length) 
                    _rotationSystemIndex = 0;
                _game.Config.RotationSystem = _rotationSystems[_rotationSystemIndex];
                audio.PlayOneShot(resources.SfxLineClear);
            }
        }

        if (_startCount > -1)
        {
            ProcessCountdown();
            if (_startCount > 0) return;
        }

        if (_isPausingGame) return;
        _inputPlugin.Process(Time.DeltaTime);
        _time += Time.DeltaTime;
        _game.Tick(Time.DeltaTime);
    }

    private void ProcessCountdown()
    {
        _startTimer += Time.DeltaTime;
        if (!(_startTimer >= 1)) return;

        _startCount--;
        _startTimer = 0;

        switch (_startCount)
        {
            case -1:
                Root.Remove(_countdownTimerText);
                return;
            case 0:
                _countdownTimerText.Content = "GO!";
                _game.Start();
                audio.Gain = 0.3f;
                audio.Play(resources.BgmX030, resources.LoopX030);
                break;
            case > 0:
                _countdownTimerText.Content = _startCount.ToString();
                break;
        }

        audio.PlayOneShot(resources.SfxCounts[Math.Clamp(_startCount, 0, 5)]);
    }

    private void UpdateScoreboard()
    {
        _gameBoard.ScoreboardLeft["TIME"] = TimeString;
        _gameBoard.ScoreboardLeft["LEVEL"] = _level.ToString();
        _gameBoard.ScoreboardLeft["LINES"] = _lines.ToString();
        _gameBoard.ScoreboardLeft["SCORE"] = _score.ToString();
        
        _gameBoard.ScoreboardRight["PPS"] = Pps.ToString("F4");
        _gameBoard.ScoreboardRight["FALL SPEED"] = _game.Config.FallSpeed.ToString("F3");
        _gameBoard.ScoreboardRight["LINE TO NEXT"] = _exp.ToString();
        _gameBoard.ScoreboardRight["Rotation"] = _game.Config.RotationSystem switch
        {
            QuadrixRotationSystem => "QRS",
            StandardRotationSystem => "SRS",
            ArcadeRotationSystem => "ARS",
            ClassicRotationSystem => "CRS",
            JoyjoyRotationSystem => "JRS",
            _ => "Unknown"
        };
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
        _exp += e.ClearedLines;
        if (_exp >= ExpMax)
        {
            _level++;
            _exp -= ExpMax;
            _game.Config.FallSpeed = CalculateSpeed(_level);
        }

        if (_lines >= 150)
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
        var gameoverText = new Text("GAME OVER", Font.GetDefault(64, FontStyle.Normal), Color.Red);
        gameoverText.Location = (640 / 2 - gameoverText.Width / 2, 480 / 2 - gameoverText.Height / 2);
        audio.Play(resources.SfxGameOver);
        Root.Add(gameoverText);
    }

    /// <summary>
    /// ゲームクリアの処理
    /// </summary>
    private void ProcessGameClear()
    {
        audio.Stop();
        var gameClearText = new Text("GAME CLEAR!!", Font.GetDefault(64, FontStyle.Normal), Color.Turquoise);
        gameClearText.Location = (640 / 2 - gameClearText.Width / 2, 480 / 2 - gameClearText.Height / 2);
        Root.Add(gameClearText);
    }

    private static float CalculateSpeed(int level)
    {
        var lv = level - 1;
        return 1 / MathF.Pow(0.8f - (lv * 0.007f), lv);
    }
}