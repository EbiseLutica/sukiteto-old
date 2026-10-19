using System.Drawing;
using Promete;
using Promete.Graphics.Fonts;
using Promete.Input;
using Promete.Nodes;

namespace Quadrix;

/// <summary>
/// 操作ごとの入力割り当てを表で編集するシーンの基底クラス。
/// 割り当てを誤っても操作不能にならないよう、カーソル操作は InputService を介さず固定の入力で行う。
/// </summary>
/// <typeparam name="TButton">割り当てるボタンの型</typeparam>
public abstract class BindingConfigSceneBase<TButton>(InputService input, Keyboard keyboard, ConsoleLayer console) : Scene
    where TButton : struct, Enum
{
    private static readonly Dictionary<InputType, string> Labels = new()
    {
        [InputType.MoveLeft] = "左移動",
        [InputType.MoveRight] = "右移動",
        [InputType.RotateLeft] = "左回転",
        [InputType.RotateRight] = "右回転",
        [InputType.Rotate180] = "180度回転",
        [InputType.SoftDrop] = "ソフトドロップ",
        [InputType.HardDrop] = "ハードドロップ",
        [InputType.Hold] = "ホールド",
        [InputType.Pause] = "ポーズ",
        [InputType.Quit] = "やめる",
        [InputType.Ok] = "決定",
        [InputType.MenuUp] = "メニュー上",
        [InputType.MenuDown] = "メニュー下",
    };

    protected InputService Input => input;
    protected Keyboard Keyboard => keyboard;

    /// <summary>
    /// 画面上部に表示する見出し
    /// </summary>
    protected abstract string Heading { get; }

    /// <summary>
    /// 編集対象の割り当て。スロットごとに 1 つの辞書を持つ。
    /// </summary>
    protected abstract Dictionary<InputType, TButton>[] Slots { get; }

    /// <summary>
    /// 割り当て待ちに入れるかどうか。入力デバイスが使えない場合などに false を返す。
    /// </summary>
    protected virtual bool CanListen => true;

    /// <summary>
    /// 見出しの下に表示する状態メッセージ。不要なら null。
    /// </summary>
    protected virtual string? StatusMessage => null;

    /// <summary>
    /// 割り当て待ちを自動でキャンセルするまでの秒数。0 以下なら自動キャンセルしない。
    /// </summary>
    protected virtual float ListeningTimeout => 0;

    protected virtual string NavigationHelp => "↑↓←→: 選択  Enter: 変更  Esc: 戻る";

    protected virtual string ListeningHelp => "割り当てるボタンを押してください  Esc: キャンセル  BackSpace: 解除";

    protected virtual bool IsUpPressed => keyboard.Up.IsKeyDown;
    protected virtual bool IsDownPressed => keyboard.Down.IsKeyDown;
    protected virtual bool IsLeftPressed => keyboard.Left.IsKeyDown;
    protected virtual bool IsRightPressed => keyboard.Right.IsKeyDown;
    protected virtual bool IsOkPressed => keyboard.Enter.IsKeyDown;
    protected virtual bool IsBackPressed => keyboard.Escape.IsKeyDown;

    private readonly InputType[] _types = Enum.GetValues<InputType>();

    private BindingTable _table = null!;
    private Text _backText = null!;
    private Text _statusText = null!;
    private Text _helpText = null!;

    private int _row;
    private int _slot;
    private bool _isListening;
    private float _listeningTimer;
    private bool _isFirstFrame = true;

    /// <summary>
    /// 「戻る」行のインデックス
    /// </summary>
    private int BackRow => _types.Length;

    /// <summary>
    /// このフレームで新たに押されたボタンを取得します。
    /// </summary>
    protected abstract bool TryGetPressedButton(out TButton button);

    public override void OnStart()
    {
        console.Clear();

        var heading = new Text(Heading, Font.GetDefault(20), Color.White)
            .Location(24, 16);
        _statusText = new Text("", Font.GetDefault(14), Color.Orange)
            .Location(24, 44);

        var slotHeaders = Enumerable.Range(1, InputService.KeySlotCount).Select(i => $"スロット{i}").ToArray();
        _table = new BindingTable(_types.Select(t => Labels.GetValueOrDefault(t, t.ToString())).ToArray(), slotHeaders)
            .Location(24, 68);

        _backText = new Text("←戻る", Font.GetDefault(16), BindingTable.NormalColor)
            .Location(24, _table.Location.Y + _table.Height + 8);
        _helpText = new Text("", Font.GetDefault(14), Color.LightGray)
            .Location(24, View.Height - 28);

        Root.AddRange(heading, _statusText, _table, _backText, _helpText);
        UpdateView();
    }

    public override void OnUpdate()
    {
        // 前のシーンで押した決定入力を拾わないよう、最初のフレームは入力を無視する
        if (_isFirstFrame)
            _isFirstFrame = false;
        else if (_isListening)
            ProcessListening();
        else
            ProcessNavigation();

        UpdateView();
    }

    private void ProcessNavigation()
    {
        if (IsBackPressed)
        {
            Back();
            return;
        }

        if (IsUpPressed) _row = _row <= 0 ? BackRow : _row - 1;
        if (IsDownPressed) _row = _row >= BackRow ? 0 : _row + 1;
        if (IsLeftPressed) _slot = _slot <= 0 ? InputService.KeySlotCount - 1 : _slot - 1;
        if (IsRightPressed) _slot = _slot >= InputService.KeySlotCount - 1 ? 0 : _slot + 1;

        if (!IsOkPressed) return;

        if (_row == BackRow)
        {
            Back();
        }
        else if (CanListen)
        {
            _isListening = true;
            _listeningTimer = 0;
        }
    }

    private void ProcessListening()
    {
        var slot = Slots[_slot];
        var type = _types[_row];

        if (keyboard.Escape.IsKeyDown || !CanListen)
        {
            _isListening = false;
            return;
        }

        if (keyboard.BackSpace.IsKeyDown)
        {
            slot.Remove(type);
            _isListening = false;
            return;
        }

        if (TryGetPressedButton(out var button))
        {
            slot[type] = button;
            _isListening = false;
            return;
        }

        _listeningTimer += Time.DeltaTime;
        if (ListeningTimeout > 0 && _listeningTimer >= ListeningTimeout)
        {
            _isListening = false;
        }
    }

    private void Back()
    {
        input.Save();
        App.LoadScene<ConfigScene>();
    }

    private void UpdateView()
    {
        for (var row = 0; row < _types.Length; row++)
        {
            for (var slot = 0; slot < InputService.KeySlotCount; slot++)
            {
                var content = Slots[slot].TryGetValue(_types[row], out var button) ? button.ToString() : "---";
                _table.SetCell(row, slot, content);
            }
        }

        _table.SelectedRow = _row == BackRow ? -1 : _row;
        _table.SelectedColumn = _slot;
        _table.IsListening = _isListening;
        _backText.Color = _row == BackRow ? BindingTable.SelectedColor : BindingTable.NormalColor;

        _statusText.Content = StatusMessage ?? "";

        if (!_isListening)
        {
            _helpText.Content = NavigationHelp;
        }
        else if (ListeningTimeout > 0)
        {
            var remaining = (int)MathF.Ceiling(ListeningTimeout - _listeningTimer);
            _helpText.Content = $"{ListeningHelp}  (あと{remaining}秒)";
        }
        else
        {
            _helpText.Content = ListeningHelp;
        }
    }
}
