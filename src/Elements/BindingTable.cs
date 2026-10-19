using System.Drawing;
using Promete;
using Promete.Graphics.Fonts;
using Promete.Nodes;

namespace Quadrix;

/// <summary>
/// 行ラベルと複数列のセルからなる、入力割り当て表示用の表。
/// 表示だけを担い、カーソル移動や割り当ての処理は持たない。
/// </summary>
public class BindingTable : ContainableNode
{
    /// <summary>
    /// カーソルがある行。-1 のときはどの行も選択していない。
    /// </summary>
    public int SelectedRow
    {
        get => _selectedRow;
        set
        {
            _selectedRow = value;
            UpdateHighlight();
        }
    }

    /// <summary>
    /// カーソルがある列
    /// </summary>
    public int SelectedColumn
    {
        get => _selectedColumn;
        set
        {
            _selectedColumn = value;
            UpdateHighlight();
        }
    }

    /// <summary>
    /// 選択中のセルが割り当て待ちかどうか
    /// </summary>
    public bool IsListening
    {
        get => _isListening;
        set
        {
            _isListening = value;
            UpdateHighlight();
        }
    }

    public int RowCount => _rowLabels.Length;

    public int ColumnCount => _columnHeaders.Length;

    public static readonly Color NormalColor = Color.White;
    public static readonly Color HeaderColor = Color.Gray;
    public static readonly Color SelectedColor = Color.Yellow;
    public static readonly Color ListeningColor = Color.Cyan;

    private const string ListeningText = "...";

    private readonly Text[] _columnHeaders;
    private readonly Text[] _rowLabels;
    private readonly Text[,] _cells;
    private readonly string[,] _cellContents;

    private int _selectedRow = -1;
    private int _selectedColumn;
    private bool _isListening;

    /// <param name="rowLabels">各行のラベル</param>
    /// <param name="columnHeaders">各列の見出し</param>
    /// <param name="labelWidth">行ラベル列の幅（px）</param>
    /// <param name="columnWidth">セル列 1 つあたりの幅（px）</param>
    /// <param name="rowHeight">1 行の高さ（px）</param>
    /// <param name="fontSize">フォントサイズ</param>
    public BindingTable(string[] rowLabels, string[] columnHeaders, int labelWidth = 160, int columnWidth = 140, int rowHeight = 22, float fontSize = 16)
    {
        var font = Font.GetDefault(fontSize);

        _columnHeaders = new Text[columnHeaders.Length];
        for (var col = 0; col < columnHeaders.Length; col++)
        {
            _columnHeaders[col] = new Text(columnHeaders[col], font, HeaderColor)
                .Location(labelWidth + col * columnWidth, 0);
            Add(_columnHeaders[col]);
        }

        _rowLabels = new Text[rowLabels.Length];
        _cells = new Text[rowLabels.Length, columnHeaders.Length];
        _cellContents = new string[rowLabels.Length, columnHeaders.Length];
        for (var row = 0; row < rowLabels.Length; row++)
        {
            var y = (row + 1) * rowHeight;
            _rowLabels[row] = new Text(rowLabels[row], font, NormalColor)
                .Location(0, y);
            Add(_rowLabels[row]);

            for (var col = 0; col < columnHeaders.Length; col++)
            {
                _cellContents[row, col] = "";
                _cells[row, col] = new Text("", font, NormalColor)
                    .Location(labelWidth + col * columnWidth, y);
                Add(_cells[row, col]);
            }
        }

        Size = (labelWidth + columnHeaders.Length * columnWidth, (rowLabels.Length + 1) * rowHeight);
    }

    /// <summary>
    /// セルの表示内容を設定します。
    /// </summary>
    public void SetCell(int row, int column, string content)
    {
        if (_cellContents[row, column] == content) return;
        _cellContents[row, column] = content;
        UpdateCell(row, column);
    }

    private void UpdateHighlight()
    {
        for (var row = 0; row < RowCount; row++)
        {
            _rowLabels[row].Color = row == _selectedRow ? SelectedColor : NormalColor;
            for (var col = 0; col < ColumnCount; col++)
            {
                UpdateCell(row, col);
            }
        }
    }

    private void UpdateCell(int row, int column)
    {
        var cell = _cells[row, column];
        var isSelected = row == _selectedRow && column == _selectedColumn;

        if (isSelected && _isListening)
        {
            cell.Content = ListeningText;
            cell.Color = ListeningColor;
            return;
        }

        cell.Content = _cellContents[row, column];
        cell.Color = isSelected ? SelectedColor : NormalColor;
    }
}
