using Promete;

namespace Sukiteto;

public sealed class GameConfig
{
    /// <summary>
    /// フィールドのサイズ
    /// </summary>
    public VectorInt FieldSize { get; init; } = (10, 20);

    /// <summary>
    /// 窒息高度より上にどれくらい積めるか。
    /// これを超えようとするか、窒息高度付近でブロックを召喚できなくなったらゲームオーバーとなる
    /// </summary>
    public int TopMargin { get; init; } = 6;

    /// <summary>
    /// ホールドのモード
    /// </summary>
    public HoldModeFlag HoldMode { get; set; } = HoldModeFlag.Once;

    /// <summary>
    /// ブロックの落下速度。単位は、1秒あたりの落下マス数。
    /// </summary>
    public float FallSpeed { get; set; } = 1f;

    /// <summary>
    /// 固定猶予時間
    /// </summary>
    public float LockDelay { get; set; } = 0.5f;

    /// <summary>
    /// カスタムマップ定義文字列
    /// </summary>
    public string? CustomMapString { get; set; }

    /// <summary>
    /// 使用するブロックの種類
    /// </summary>
    public BlockColor[] UsingBlocks { get; set; } =
    [
        BlockColor.O,
        BlockColor.J,
        BlockColor.L,
        BlockColor.Z,
        BlockColor.S,
        BlockColor.T,
        BlockColor.I,
    ];

    /// <summary>
    /// クレイジーモードを有効にするか
    /// </summary>
    public bool IsCrazyMode { get; set; }

    /// <summary>
    /// 利用する回転システム
    /// </summary>
    public IRotationSystem RotationSystem { get; set; } = new StandardRotationSystem();

    /// <summary>
    /// 固定猶予リセットが可能な最大数
    /// </summary>
    public int LockDelayResetMaxCount { get; set; } = 15;
    
    private GameConfig() { }

    public static GameConfig Default => new();

    public static GameConfig Classic => new()
    {
        FallSpeed = 0.5f,
        LockDelayResetMaxCount = 0,
        RotationSystem = new ClassicRotationSystem(),
        HoldMode = HoldModeFlag.None,
    };

    public enum HoldModeFlag
    {
        None,
        Once,
        Infinite,
    }
}