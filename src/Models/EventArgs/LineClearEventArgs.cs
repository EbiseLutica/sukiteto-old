using System.Collections.ObjectModel;

namespace Quadrix;

public class LineClearEventArgs : EventArgs
{
    /// <summary>
    /// 消したライン数
    /// </summary>
    public int ClearedLines => ClearedLineIndices.Length;
    
    public Memory<int> ClearedLineIndices { get; init; }

    /// <summary>
    /// ラインクリアがT-Spinを伴うかどうか
    /// T-Spin Miniの場合も<c>true</c>となる。
    /// </summary>
    public bool IsTSpin { get; init; }

    /// <summary>
    /// ラインクリアがT-Spin Miniを伴うかどうか
    /// </summary>
    public bool IsTSpinMini { get; init; }
    
    /// <summary>
    /// オールクリアを達成したかどうか
    /// </summary>
    public bool IsAllClear { get; set; }

    /// <summary>
    /// ブロック設置でラインを連続して消した回数。
    /// 2連続で1から始まり、以後ライン消しを伴わないブロック設置を行うまで続く。
    /// </summary>
    public int ComboCount { get; set; }
    
    /// <summary>
    /// QUADあるいはT-Spinを連続して放った回数。
    /// 対象外のライン消しを行うとリセットされる。
    /// </summary>
    public int SuperComboCount { get; set; }
}