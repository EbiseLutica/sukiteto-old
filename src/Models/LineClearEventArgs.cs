namespace Sukiteto;

public class LineClearEventArgs : EventArgs
{
    public bool IsTSpin { get; set; }
    public bool IsAllClear { get; set; }
    public int ClearedLines { get; set; }
    public int IsCombo { get; set; }
    public int SuperComboCount { get; set; }
}