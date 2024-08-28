namespace Quadrix;

public class AISystem(GameService game)
{
    private readonly BlockColor[,] _thinkingBoard = new BlockColor[game.Config.FieldSize.X, game.Config.FieldSize.Y + game.Config.TopMargin];

    private readonly List<(AIResult result, float score)> _candidates = [];
    
    private readonly float _scoreLineClear1 = 1;
    private readonly float _scoreLineClear2 = 2;
    private readonly float _scoreLineClear3 = 3;
    private readonly float _scoreLineClear4 = 5;
    private readonly float _scorePerfectClear = 20;
    private readonly float _scoreHeight = -1;
    private readonly float _scoreHole = -9f;
    private readonly float _scoreBump = -4f;
    private readonly float _scoreOperationCount = 0;
    private readonly float _scoreUsingINotForQuad = -7;
    
    /// <summary>
    /// 現在の盤面における最善手を思考し、その手を返します。
    /// </summary>
    public AIResult Think()
    {
        // 思考用の盤面をコピー
        Buffer.BlockCopy(game.Field, 0, _thinkingBoard, 0, game.Field.Length * sizeof(BlockColor));
        
        // このAIは次のような行動を思考する。
        // ①: 回転を決める
        // ②: 左右移動を決める
        // ③: 下まで落とす
        // ④: 回転を決める
        // ⑤: 左右移動を決める
        // ⑥: 確定
        // この一連の操作において、取れる手の中で最も盤面の評価値が高い手を選ぶ。
        // なおホールドも考慮する。
        _candidates.Clear();
        var w = game.Config.FieldSize.X;
        
        for (var i = 0; i >= -w / 2; i--)
        {
            if (Evaluate(i, false) == 0) break;
        }
        
        for (var i = 0; i < w / 2; i++)
        {
            if (Evaluate(i, false) == 0) break;
        }
        
        for (var i = 0; i >= -w / 2; i--)
        {
            if (Evaluate(i, true) == 0) break;
        }
        
        for (var i = 0; i < w / 2; i++)
        {
            if (Evaluate(i, true) == 0) break;
        }

        return _candidates.MaxBy(r => r.score).result;
    }

    private int Evaluate(int x, bool useHold)
    {
        var candidateCount = 0;
        var (w, h) = game.Config.FieldSize + (0, game.Config.TopMargin);
        var shapes = game.Shapes;

        var hold = game.CurrentHold == BlockColor.None ? game.NextQueue.Peek() : game.CurrentHold;
        var currentColor = useHold ?  hold : game.CurrentBlockColor;
        var currentShape = shapes[currentColor];
        var (bx, by) = game.BlockPosition;
        x = bx + x;
            
        var beforeHoleCount = CheckHoleCount();
        var beforeBumpCount = CheckBumpCount();

        // 各移動・各回転を検証
        for (var rotation = 0; rotation < 4; rotation++)
        {
            var score = 100f;
            var shape = currentShape[rotation];

            // 置けないならスキップ
            if (!CanPlaceBlock(x, by, shape)) continue;
            
            // 落とす
            var y = by;
            while (CanPlaceBlock(x, y + 1, shape))
            {
                y++;
            }
            for (var iy = 0; iy < shape.GetLength(1); iy++)
            {
                for (var ix = 0; ix < shape.GetLength(0); ix++)
                {
                    if (!shape[ix, iy]) continue;
                    _thinkingBoard[x + ix, y + iy] = currentColor;
                }
            }
            
            // ライン消し処理
            var cleared = 0;
            for (var iy = h - 1; iy >= 0; iy--)
            {
                var isLineFilled = true;
                for (var ix = 0; ix < w; ix++)
                {
                    if (_thinkingBoard[ix, iy] != BlockColor.None) continue;
                    isLineFilled = false;
                    break;
                }

                if (!isLineFilled) continue;
                cleared++;
                ShiftDownField(iy);
                iy++;
            }

            var maxHeight = MaxHeight();
            
            // 評価1: ライン消し
            if (maxHeight > game.Config.FieldSize.Y - 10)
            {
                score += cleared switch
                {
                    1 => _scoreLineClear1,
                    2 => _scoreLineClear2,
                    3 => _scoreLineClear3,
                    4 => _scoreLineClear4,
                    _ => 0
                };
            }
            else if (cleared == 4)
            {
                score += _scoreLineClear4;
            }

            // 評価2: パーフェクトクリア
            if (cleared > 0 && _thinkingBoard.Cast<BlockColor>().All(c => c != BlockColor.None))
            {
                score += _scorePerfectClear;
            }
            
            // 評価3: 高さ
            score += MathF.Pow(maxHeight, 2) / 20;
            
            // 評価4: 相対空洞数
            var afterHoleCount = CheckHoleCount();
            score += afterHoleCount * _scoreHole;
            
            // 評価5: 地形の凸凹
            // 凹凸や深い穴の数を調べ、それに応じてスコアを下げる
            var afterBumpCount = CheckBumpCount();
            score += afterBumpCount * _scoreBump;
            
            
            // 評価6: Iミノ使用時に4ライン消しにならない場合
            if (currentColor == BlockColor.I && cleared < 4)
            {
                score += _scoreUsingINotForQuad;
            }
            
            // 評価7: 操作回数
            var relativeX = x - bx;
            var rotationCount = rotation switch
            {
                0 => 0,
                1 => 1,
                2 => 2,
                3 => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(rotation))
            };
            score += (Math.Abs(relativeX) + rotationCount) * _scoreOperationCount;
            
            // 評価値を記録
            _candidates.Add((new AIResult(x - bx, rotation, 0, rotation, useHold), score));
            candidateCount++;
            
            // 盤面を戻す
            if (cleared > 0)
            {
                // ライン消しの場合は配列ごとコピーしちゃう（面倒なので）
                Buffer.BlockCopy(game.Field, 0, _thinkingBoard, 0, game.Field.Length * sizeof(BlockColor));
            }
            else
            {
                // そうでない場合はブロックの部分だけ戻す
                for (var iy = 0; iy < shape.GetLength(1); iy++)
                {
                    for (var ix = 0; ix < shape.GetLength(0); ix++)
                    {
                        if (!shape[ix, iy]) continue;
                        _thinkingBoard[x + ix, y + iy] = BlockColor.None;
                    }
                }
            }
        }

        return candidateCount;

        int CheckHoleCount()
        {
            var holeCount = 0;
            for (var ix = 0; ix < w; ix++)
            {
                var isHole = false;
                for (var iy = h - 1; iy >= 0; iy--)
                {
                    if (_thinkingBoard[ix, iy] == BlockColor.None)
                    {
                        isHole = true;
                    }
                    
                    if (isHole && _thinkingBoard[ix, iy] != BlockColor.None)
                    {
                        holeCount++;
                    }
                }
            }
            
            return holeCount;
        }

        int MaxHeight()
        {
            var maxHeight = 0;
            for (var ix = 0; ix < w; ix++)
            {
                for (var iy = 0; iy < h; iy++)
                {
                    if (_thinkingBoard[ix, iy] == BlockColor.None) continue;
                    maxHeight = Math.Max(maxHeight, h - iy);
                    break;
                }
            }
            return maxHeight;
        }

        int CheckBumpCount()
        {
            var bump = 0;
            var lastHeight = 0;
            for (var ix = 0; ix < w; ix++)
            {
                var height = 0;
                for (var iy = 0; iy < h; iy++)
                {
                    if (_thinkingBoard[ix, iy] == BlockColor.None) continue;
                    height = h - iy;
                    break;
                }

                bump += (int)MathF.Abs(lastHeight - height);
                lastHeight = height;
            }

            return bump;
        }
    }

    /// <summary>
    /// フィールドの y 行を消して、それより上の行を下に1ずつずらします。
    /// </summary>
    /// <param name="y"></param>
    private void ShiftDownField(int y)
    {   
        var width = _thinkingBoard.GetLength(0);
        for (var iy = y; iy >= 0; iy--)
        {
            for (var ix = 0; ix < width; ix++)
            {
                _thinkingBoard[ix, iy] = iy > 0 ? _thinkingBoard[ix, iy - 1] : BlockColor.None;
            }
        }
        
        for (var i = 0; i < width; i++)
        {
            _thinkingBoard[i, 0] = BlockColor.None;
        }
    }

    private bool CanPlaceBlock(int x, int y, bool[,] blockShape)
    {
        var (w, h) = game.Config.FieldSize + (0, game.Config.TopMargin);
        for (var i = 0; i < blockShape.GetLength(0); i++)
        {
            for (var j = 0; j < blockShape.GetLength(1); j++)
            {
                if (!blockShape[i, j]) continue;
                if (x + i < 0 || x + i >= w || y + j < 0 || y + j >= h) return false;
                if (_thinkingBoard[x + i, y + j] != BlockColor.None) return false;
            }
        }

        return true;
    }
}

public record struct AIResult(int FirstMove, int FirstRotation, int LastMove, int LastRotation, bool UseHold);