using System;
using System.Collections.Generic;
using System.Text;

namespace CandyCrushAccessible.Engine
{
    public class MatchGroup
    {
        public HashSet<int> Cells = new HashSet<int>();
        public SpecialType CreateType = SpecialType.None;
        public int CreateX = -1;
        public int CreateY = -1;
        public bool StripedVertical;
        public CandyColor Color;
    }

    public class TurnResult
    {
        public bool Valid;
        public bool SpecialSwap;
        public int MovesUsed;
        public int ScoreGained;
        public int CascadeLevels;
        public int TotalCandyDestroyed;
        public int JellyCleared;
        public int LicoriceBroken;
        public int ChocolateDestroyed;
        public int FrostingBroken;
        public int TimeGained;
        public int BombExploded;
        public int IngredientsCollected;
        public int SugarCrushMoves;
        public List<SpecialType> SpecialsCreated = new List<SpecialType>();
        public List<SpecialType> SpecialsActivated = new List<SpecialType>();
        public List<Tuple<SpecialType, int, int, bool>> ActivationsDetailed = new List<Tuple<SpecialType, int, int, bool>>();
        public List<string> Events = new List<string>();

        public int Score
        {
            get { return ScoreGained; }
            set { ScoreGained = value; }
        }
    }

    public class Board
    {
        public const int Cols = 8;
        public const int Rows = 8;

        private readonly Candy[,] _grid = new Candy[Cols, Rows];
        private readonly bool[,] _jelly = new bool[Cols, Rows];
        private readonly bool[,] _doubleJelly = new bool[Cols, Rows];
        private readonly bool[,] _chocolate = new bool[Cols, Rows];
        private readonly byte[,] _frosting = new byte[Cols, Rows];
        private readonly bool[,] _licorice = new bool[Cols, Rows];
        private readonly Random _rng;
        private readonly LevelDefinition _level;

        private bool[,] _activated;
        private readonly Queue<KeyValuePair<int, int>> _activationQueue = new Queue<KeyValuePair<int, int>>();
        private int _spawnedIngredients;
        private int _movesSinceIngredient;

        public int Score { get; private set; }
        public int MovesLeft { get; private set; }

        public void AddMoves(int amount)
        {
            if (amount > 0) MovesLeft += amount;
        }
        public double TimeLeft { get; private set; }
        public int InitialJelly { get; private set; }
        public int RemainingJelly { get; private set; }
        public int FrostingRemaining { get; private set; }
        public int IngredientsRemaining { get; private set; }
        public bool Completed { get; private set; }
        public bool HasChocolate { get; private set; }
        public readonly List<LevelOrder> Orders = new List<LevelOrder>();

        public LevelDefinition Level { get { return _level; } }

        public int OrdersFulfilled
        {
            get
            {
                int n = 0;
                foreach (LevelOrder o in Orders) if (o.Fulfilled) n++;
                return n;
            }
        }

        public int OrdersRemaining
        {
            get
            {
                int n = 0;
                foreach (LevelOrder o in Orders) n += o.Remaining;
                return n;
            }
        }

        public Board(LevelDefinition level)
        {
            _level = level;
            if (level.Orders != null)
            {
                foreach (LevelOrder o in level.Orders)
                {
                    Orders.Add(new LevelOrder { Kind = o.Kind, Color = o.Color, Count = o.Count, Filled = 0 });
                }
            }
            MovesLeft = level.Moves;
            if (level.Type == LevelType.Timed)
            {
                TimeLeft = level.TimeSeconds;
            }
            else
            {
                TimeLeft = -1;
            }
            _rng = new Random(level.Number * 7919 + DateTime.Now.Millisecond);

            HasChocolate = level.HasChocolate;

            SetupJelly();
            SetupChocolate();
            GenerateBoard();
            SetupIngredients();
            SetupBombs();
            SetupLicorice();
            SetupFrosting();

            if (!HasValidMoves() || HasPendingMatches())
            {
                Reshuffle();
            }
        }

        public void UpdateTime(double dt)
        {
            if (_level.Type != LevelType.Timed || Completed) return;
            TimeLeft -= dt;
            if (TimeLeft < 0) TimeLeft = 0;
        }

        public void AddTime(double seconds)
        {
            TimeLeft += seconds;
        }

        public int StarsEarned
        {
            get
            {
                if (Score >= _level.ThreeStarScore) return 3;
                if (Score >= _level.TwoStarScore) return 2;
                if (Score >= _level.OneStarScore) return 1;
                return 0;
            }
        }

        public bool Failed
        {
            get
            {
                if (Completed) return false;
                if (_level.Type == LevelType.Timed) return TimeLeft <= 0;
                return MovesLeft <= 0;
            }
        }

        public Candy GetCandy(int x, int y)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return null;
            return _grid[x, y];
        }

        public bool IsChocolate(int x, int y)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return false;
            return _chocolate[x, y];
        }

        public bool HasJelly(int x, int y)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return false;
            return _jelly[x, y] || _doubleJelly[x, y];
        }

        public bool IsDoubleJelly(int x, int y)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return false;
            return _doubleJelly[x, y];
        }

        public int RemainingJellyCells()
        {
            int count = 0;
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                    if (HasJelly(x, y)) count++;
            return count;
        }

        private void SetupJelly()
        {
            int total = 0;
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    bool has = false;
                    if (_level.Type == LevelType.Jelly && _level.AllBoardJelly)
                    {
                        has = true;
                    }
                    else if (_level.Type == LevelType.Jelly)
                    {
                        has = _rng.Next(100) < 38;
                    }
                    if (has)
                    {
                        bool dbl = _level.AllBoardJelly ? false : _rng.Next(100) < 30;
                        if (dbl) _doubleJelly[x, y] = true;
                        else _jelly[x, y] = true;
                        total++;
                    }
                }
            }
            RemainingJelly = total;
            InitialJelly = total;
        }

        private void SetupChocolate()
        {
            if (!HasChocolate) return;
            _chocolate[3, 0] = true;
            _chocolate[4, 0] = true;
            _chocolate[3, 7] = true;
            _chocolate[4, 7] = true;
        }

        private void SetupIngredients()
        {
            if (_level.Type != LevelType.Ingredient) return;
            IngredientsRemaining = _level.TargetIngredients;
            _spawnedIngredients = 0;
            SpawnIngredientAtTop();
        }

        private void SpawnIngredientAtTop()
        {
            if (_spawnedIngredients >= _level.TargetIngredients) return;
            int x = _rng.Next(Cols);
            if (_grid[x, 0] != null)
            {
                for (int i = 0; i < Cols; i++)
                {
                    if (_grid[i, 0] == null) { x = i; break; }
                }
            }
            if (_grid[x, 0] != null) return;
            Candy c = new Candy(CandyColor.Red);
            if (_level.Number >= 24)
            {
                c.Ingredient = IngredientType.Nut;
            }
            else
            {
                c.Ingredient = IngredientType.Cherry;
            }
            _grid[x, 0] = c;
            _spawnedIngredients++;
        }

        private void SetupBombs()
        {
            if (!_level.HasBombs) return;
            int count = 3;
            for (int i = 0; i < count; i++)
            {
                int x = _rng.Next(Cols);
                int y = 1 + _rng.Next(Rows - 2);
                if (_grid[x, y] != null && !_grid[x, y].IsIngredient)
                {
                    _grid[x, y].BombTimer = _level.BombTimerBase + _rng.Next(4);
                }
            }
        }

        private void SetupFrosting()
        {
            if (_level.TargetFrosting <= 0) return;
            int placed = 0;
            int guard = 0;
            while (placed < _level.TargetFrosting && guard < 400)
            {
                guard++;
                int x = _rng.Next(Cols);
                int y = _rng.Next(Rows);
                if (_chocolate[x, y]) continue;
                if (_grid[x, y] == null) continue;
                if (_grid[x, y].IsIngredient) continue;
                if (_frosting[x, y] > 0) continue;
                if (_licorice[x, y]) continue;
                _frosting[x, y] = (byte)(_rng.Next(100) < 30 ? 2 : 1);
                placed++;
            }
            FrostingRemaining = FrostingRemainingCells();
        }

        private void SetupLicorice()
        {
            if (_level.TargetLicorice <= 0) return;
            int placed = 0;
            int guard = 0;
            while (placed < _level.TargetLicorice && guard < 400)
            {
                guard++;
                int x = _rng.Next(Cols);
                int y = _rng.Next(Rows);
                if (_chocolate[x, y]) continue;
                if (_grid[x, y] == null) continue;
                if (_grid[x, y].IsIngredient) continue;
                if (_licorice[x, y]) continue;
                _licorice[x, y] = true;
                _grid[x, y].IsLicorice = true;
                placed++;
            }
            LicoriceRemaining = LicoriceRemainingCells();
        }

        public bool HasLicorice(int x, int y)
        {
            return x >= 0 && x < Cols && y >= 0 && y < Rows && _licorice[x, y];
        }

        public int LicoriceRemaining { get; private set; }

        private int LicoriceRemainingCells()
        {
            int count = 0;
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                    if (_licorice[x, y]) count++;
            return count;
        }

        public bool HasFrosting(int x, int y)
        {
            return x >= 0 && x < Cols && y >= 0 && y < Rows && _frosting[x, y] > 0;
        }

        public bool IsDoubleFrosting(int x, int y)
        {
            return x >= 0 && x < Cols && y >= 0 && y < Rows && _frosting[x, y] >= 2;
        }

        private int FrostingRemainingCells()
        {
            int count = 0;
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                    if (_frosting[x, y] > 0) count++;
            return count;
        }

        private void GenerateBoard()
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Cols; x++)
                {
                    if (_chocolate[x, y]) { _grid[x, y] = null; continue; }
                    CandyColor c = NextColorNoImmediateMatch(x, y);
                    _grid[x, y] = new Candy(c);
                }
            }
        }

        private CandyColor NextColorNoImmediateMatch(int x, int y)
        {
            CandyColor c;
            int guard = 0;
            do
            {
                c = (CandyColor)_rng.Next(_level.NumColors);
                guard++;
            }
            while (guard < 50 && CreatesMatchAt(x, y, c));
            return c;
        }

        private bool CreatesMatchAt(int x, int y, CandyColor c)
        {
            int h = 1;
            for (int i = x - 1; i >= 0 && _grid[i, y] != null && !_grid[i, y].IsIngredient && _grid[i, y].Color == c; i--) h++;
            for (int i = x + 1; i < Cols && _grid[i, y] != null && !_grid[i, y].IsIngredient && _grid[i, y].Color == c; i++) h++;
            if (h >= 3) return true;
            int v = 1;
            for (int i = y - 1; i >= 0 && _grid[x, i] != null && !_grid[x, i].IsIngredient && _grid[x, i].Color == c; i--) v++;
            for (int i = y + 1; i < Rows && _grid[x, i] != null && !_grid[x, i].IsIngredient && _grid[x, i].Color == c; i++) v++;
            return v >= 3;
        }

        public static string ColLetter(int x)
        {
            return ((char)('A' + x)).ToString();
        }

        public static string CellName(int x, int y)
        {
            return ColLetter(x) + (y + 1);
        }

        public bool IsValidMove(int x1, int y1, int x2, int y2)
        {
            if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1) return false;
            Candy a = _grid[x1, y1];
            Candy b = _grid[x2, y2];
            if (a == null || b == null) return false;
            if (a.IsLicorice || b.IsLicorice) return false;
            if (HasFrosting(x1, y1) || HasFrosting(x2, y2)) return false;
            if (a.IsIngredient || b.IsIngredient) return false;
            if (a.Special == SpecialType.ColorBomb || b.Special == SpecialType.ColorBomb) return true;
            if (a.IsSpecial && b.IsSpecial) return true;
            SwapCells(x1, y1, x2, y2);
            bool ok = MatchedCellsContain(x1, y1, x2, y2);
            SwapCells(x1, y1, x2, y2);
            return ok;
        }

        private bool MatchedCellsContain(int x1, int y1, int x2, int y2)
        {
            List<MatchGroup> groups = DetectMatches();
            foreach (MatchGroup g in groups)
            {
                int i1 = y1 * Cols + x1;
                int i2 = y2 * Cols + x2;
                if (g.Cells.Contains(i1) || g.Cells.Contains(i2)) return true;
            }
            return false;
        }

        public TurnResult TrySwap(int x1, int y1, int dx, int dy)
        {
            return ProcessTurn(x1, y1, x1 + dx, y1 + dy);
        }

        public TurnResult ProcessTurn(int x1, int y1, int x2, int y2)
        {
            TurnResult result = new TurnResult();
            if (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) != 1)
            {
                return result;
            }
            Candy a = _grid[x1, y1];
            Candy b = _grid[x2, y2];
            if (a == null || b == null)
            {
                return result;
            }
            if (a.IsLicorice || b.IsLicorice)
            {
                result.Events.Add(Localization.Get("msg.locked"));
                return result;
            }
            if (HasFrosting(x1, y1) || HasFrosting(x2, y2))
            {
                result.Events.Add(Localization.Get("msg.frosted"));
                return result;
            }
            if (a.IsIngredient || b.IsIngredient)
            {
                result.Events.Add(Localization.Get("msg.invalid"));
                return result;
            }

            SwapCells(x1, y1, x2, y2);

            if (a.Special == SpecialType.ColorBomb || b.Special == SpecialType.ColorBomb)
            {
                result.SpecialSwap = true;
                result.Valid = true;
                result.MovesUsed = 1;
                ApplyColorBombSwap(a, b, x1, y1, x2, y2, result);
                GravityAndRefill(result);
                RunCascades(result);
                FinishTurn(result);
                return result;
            }

            if (a.IsSpecial && b.IsSpecial)
            {
                result.SpecialSwap = true;
                result.Valid = true;
                result.MovesUsed = 1;
                ApplySpecialCombo(a, b, x1, y1, x2, y2, result);
                GravityAndRefill(result);
                RunCascades(result);
                FinishTurn(result);
                return result;
            }

            if (!MatchedCellsContain(x1, y1, x2, y2))
            {
                SwapCells(x1, y1, x2, y2);
                result.Valid = false;
                result.Events.Add(Localization.Get("msg.invalid"));
                return result;
            }

            result.Valid = true;
            result.MovesUsed = 1;
            RunCascades(result, x2, y2);
            FinishTurn(result);
            return result;
        }

        private void FinishTurn(TurnResult result)
        {
            if (_level.Type != LevelType.Timed)
            {
                MovesLeft -= result.MovesUsed;
            }

            TickBombs(result);
            GrowChocolate(result);
            SpawnMoreIngredients();
            CollectBottomIngredients(result);
            Score += result.ScoreGained;
            CheckCompletion(result);
        }

        private void CollectBottomIngredients(TurnResult result)
        {
            if (_level.Type != LevelType.Ingredient) return;
            for (int x = 0; x < Cols; x++)
            {
                Candy c = _grid[x, Rows - 1];
                if (c == null || !c.IsIngredient) continue;
                if (_chocolate[x, Rows - 1]) continue;
                _grid[x, Rows - 1] = null;
                result.IngredientsCollected++;
                result.Score += c.Ingredient == IngredientType.Nut ? 20000 : 10000;
                if (IngredientsRemaining > 0) IngredientsRemaining--;
            }
        }

        private void SpawnMoreIngredients()
        {
            if (_level.Type != LevelType.Ingredient) return;
            if (Completed) return;
            _movesSinceIngredient++;
            if (_movesSinceIngredient >= 8 && _spawnedIngredients < _level.TargetIngredients)
            {
                _movesSinceIngredient = 0;
                SpawnIngredientAtTop();
            }
        }

        private void CheckCompletion(TurnResult result)
        {
            switch (_level.Type)
            {
                case LevelType.Score:
                case LevelType.Timed:
                    if (Score >= _level.TargetScore)
                    {
                        Completed = true;
                        result.Events.Add(Localization.Get("msg.win"));
                    }
                    break;
                case LevelType.Jelly:
                    if (RemainingJelly == 0)
                    {
                        Completed = true;
                        result.Events.Add(Localization.Get("msg.win"));
                    }
                    break;
                case LevelType.Ingredient:
                    if (IngredientsRemaining == 0)
                    {
                        Completed = true;
                        result.Events.Add(Localization.Get("msg.win"));
                    }
                    break;
                case LevelType.Order:
                    if (Orders.Count > 0 && OrdersFulfilled == Orders.Count && Score >= _level.TargetScore)
                    {
                        Completed = true;
                        result.Events.Add(Localization.Get("msg.win"));
                    }
                    break;
            }
        }

        private void TickBombs(TurnResult result)
        {
            if (!_level.HasBombs) return;
            bool warned = false;
            bool exploded = false;
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Candy c = _grid[x, y];
                    if (c == null || c.BombTimer <= 0) continue;
                    c.BombTimer--;
                    if (c.BombTimer <= 3 && !warned)
                    {
                        warned = true;
                        result.Events.Add(string.Format(Localization.Get("msg.bomb.warning"), CellName(x, y), Math.Max(1, c.BombTimer)));
                    }
                    if (c.BombTimer <= 0)
                    {
                        exploded = true;
                        result.BombExploded++;
                        _activated = new bool[Cols, Rows];
                        _activationQueue.Clear();
                        bool[,] toDestroy = new bool[Cols, Rows];
                        DestroyCell(x, y, result, 1, toDestroy, false);
                        Blast(x, y, 1, result, toDestroy);
                        ProcessActivationQueue(result, toDestroy);
                        ApplyDestroyedCells(result, toDestroy);
                        result.Events.Add(Localization.Get("msg.bomb.explode"));
                    }
                }
            }
            if (exploded)
            {
                GravityAndRefill(result);
                RunCascades(result);
                CheckCompletion(result);
            }
        }

        private void GrowChocolate(TurnResult result)
        {
            if (!HasChocolate) return;
            List<KeyValuePair<int, int>> edges = new List<KeyValuePair<int, int>>();
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (!_chocolate[x, y]) continue;
                    AddChocolateEdge(x + 1, y, edges);
                    AddChocolateEdge(x - 1, y, edges);
                    AddChocolateEdge(x, y + 1, edges);
                    AddChocolateEdge(x, y - 1, edges);
                }
            }
            if (edges.Count == 0) return;
            KeyValuePair<int, int> target = edges[_rng.Next(edges.Count)];
            _chocolate[target.Key, target.Value] = true;
            _grid[target.Key, target.Value] = null;
        }

        private void AddChocolateEdge(int x, int y, List<KeyValuePair<int, int>> edges)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return;
            if (_chocolate[x, y]) return;
            if (_grid[x, y] == null) return;
            if (_grid[x, y].IsIngredient) return;
            edges.Add(new KeyValuePair<int, int>(x, y));
        }

        private void SwapCells(int x1, int y1, int x2, int y2)
        {
            Candy tmp = _grid[x1, y1];
            _grid[x1, y1] = _grid[x2, y2];
            _grid[x2, y2] = tmp;
        }

        private void ApplyColorBombSwap(Candy a, Candy b, int x1, int y1, int x2, int y2, TurnResult result)
        {
            _activated = new bool[Cols, Rows];
            _activationQueue.Clear();
            bool[,] toDestroy = new bool[Cols, Rows];

            if (a.Special == SpecialType.ColorBomb && b.Special == SpecialType.ColorBomb)
            {
                for (int x = 0; x < Cols; x++)
                    for (int y = 0; y < Rows; y++)
                        if (_grid[x, y] != null)
                            DestroyCell(x, y, result, 1, toDestroy, true);
            }
            else
            {
                Candy target = a.Special == SpecialType.ColorBomb ? b : a;
                int tx = a.Special == SpecialType.ColorBomb ? x2 : x1;
                int ty = a.Special == SpecialType.ColorBomb ? y2 : y1;
                DestroyAllColor(target.Color, target.Special, result, toDestroy);
            }

            ProcessActivationQueue(result, toDestroy);
            ApplyDestroyedCells(result, toDestroy);
            result.Score += 1000;
        }

        private void DestroyAllColor(CandyColor color, SpecialType transform, TurnResult result, bool[,] toDestroy)
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Candy c = _grid[x, y];
                    if (c == null || c.IsIngredient) continue;
                    if (c.Color != color) continue;
                    if (transform != SpecialType.None)
                    {
                        c.Special = transform;
                        if (transform == SpecialType.Striped) c.StripedVertical = x % 2 == 0;
                    }
                    DestroyCell(x, y, result, 1, toDestroy, true);
                }
            }
        }

        private void ApplySpecialCombo(Candy a, Candy b, int x1, int y1, int x2, int y2, TurnResult result)
        {
            _activated = new bool[Cols, Rows];
            _activationQueue.Clear();
            bool[,] toDestroy = new bool[Cols, Rows];

            SpecialType sa = a.Special;
            SpecialType sb = b.Special;

            if ((sa == SpecialType.Striped && sb == SpecialType.Striped))
            {
                ClearRow(y1, result, toDestroy);
                ClearColumn(x1, result, toDestroy);
            }
            else if ((sa == SpecialType.Striped && sb == SpecialType.Wrapped) || (sa == SpecialType.Wrapped && sb == SpecialType.Striped))
            {
                ClearRows(y1, result, toDestroy);
                ClearColumns(x1, result, toDestroy);
            }
            else if (sa == SpecialType.Wrapped && sb == SpecialType.Wrapped)
            {
                Blast(x1, y1, 2, result, toDestroy);
                Blast(x1, y1, 2, result, toDestroy);
            }
            else if ((sa == SpecialType.Fish && sb == SpecialType.Fish))
            {
                SpawnFish(x1, y1, 6, result, toDestroy);
            }
            else if ((sa == SpecialType.Fish && sb == SpecialType.Striped) || (sa == SpecialType.Striped && sb == SpecialType.Fish))
            {
                SpawnFish(x1, y1, 3, result, toDestroy);
                ClearRow(y1, result, toDestroy);
                ClearColumn(x1, result, toDestroy);
            }
            else if ((sa == SpecialType.Fish && sb == SpecialType.Wrapped) || (sa == SpecialType.Wrapped && sb == SpecialType.Fish))
            {
                SpawnFish(x1, y1, 3, result, toDestroy);
                Blast(x1, y1, 1, result, toDestroy);
            }
            else
            {
                Blast(x1, y1, 2, result, toDestroy);
            }

            ProcessActivationQueue(result, toDestroy);
            ApplyDestroyedCells(result, toDestroy);
            result.Score += 500;
        }

        private void RunCascades(TurnResult result, int targetX = -1, int targetY = -1)
        {
            int cascade = 1;
            _activated = new bool[Cols, Rows];
            _activationQueue.Clear();

            while (true)
            {
                List<MatchGroup> groups = DetectMatches();
                if (groups.Count == 0)
                {
                    result.CascadeLevels = cascade - 1;
                    break;
                }

                bool[,] toDestroy = new bool[Cols, Rows];
                List<MatchGroup> creations = new List<MatchGroup>();

                foreach (MatchGroup g in groups)
                {
                    int bonus = 60 * cascade;
                    if (targetX >= 0 && targetY >= 0 && g.Cells.Contains(targetY * Cols + targetX))
                    {
                        g.CreateX = targetX;
                        g.CreateY = targetY;
                    }
                    foreach (int cell in g.Cells)
                    {
                        int x = cell % Cols;
                        int y = cell / Cols;
                        if (!toDestroy[x, y])
                        {
                            toDestroy[x, y] = true;
                            result.TotalCandyDestroyed++;
                            result.Score += bonus;
                        }
                    }
                    if (g.CreateType != SpecialType.None)
                    {
                        creations.Add(g);
                    }
                }

                if (cascade == 1)
                {
                    result.Events.Add(Localization.Get("msg.match"));
                }

                ProcessActivationQueue(result, toDestroy);
                TrackOrderProgress(toDestroy);
                ApplyDestroyedCells(result, toDestroy);

                foreach (MatchGroup g in creations)
                {
                    if (_grid[g.CreateX, g.CreateY] == null)
                    {
                        _grid[g.CreateX, g.CreateY] = MakeSpecial(g.Color, g.CreateType, g.StripedVertical, g.CreateX);
                        result.SpecialsCreated.Add(g.CreateType);
                        result.Events.Add(string.Format(Localization.Get("msg.special.created"), Localization.S(g.CreateType)));
                        TrackSpecialCreated(g.CreateType);
                        switch (g.CreateType)
                        {
                            case SpecialType.Striped: result.Score += 120; break;
                            case SpecialType.Wrapped: result.Score += 180; break;
                            case SpecialType.ColorBomb: result.Score += 200; break;
                            case SpecialType.Fish: result.Score += 100; break;
                        }
                    }
                }

                GravityAndRefill(result);
                cascade++;
            }
        }

        private Candy MakeSpecial(CandyColor color, SpecialType type, bool stripedVertical, int x)
        {
            Candy c = new Candy(color);
            c.Special = type;
            if (type == SpecialType.Striped) c.StripedVertical = stripedVertical;
            return c;
        }

        private void TrackOrderProgress(bool[,] toDestroy)
        {
            if (Orders.Count == 0) return;
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (!toDestroy[x, y]) continue;
                    if (_chocolate[x, y] || _frosting[x, y] > 0 || _licorice[x, y]) continue;
                    Candy c = _grid[x, y];
                    if (c == null || c.Special != SpecialType.None || c.IsIngredient) continue;
                    foreach (LevelOrder o in Orders)
                    {
                        if (o.Kind == OrderKind.Candy && o.Color == c.Color && !o.Fulfilled)
                        {
                            o.Filled++;
                        }
                    }
                }
            }
        }

        private void TrackSpecialCreated(SpecialType type)
        {
            foreach (LevelOrder o in Orders)
            {
                if (o.Fulfilled) continue;
                OrderKind kind = OrderKind.Candy;
                switch (type)
                {
                    case SpecialType.Striped: kind = OrderKind.Striped; break;
                    case SpecialType.Wrapped: kind = OrderKind.Wrapped; break;
                    case SpecialType.ColorBomb: kind = OrderKind.ColorBomb; break;
                    case SpecialType.Fish: kind = OrderKind.Fish; break;
                }
                if (o.Kind == kind) o.Filled++;
            }
        }

        private void ProcessActivationQueue(TurnResult result, bool[,] toDestroy)
        {
            while (_activationQueue.Count > 0)
            {
                KeyValuePair<int, int> pos = _activationQueue.Dequeue();
                int x = pos.Key;
                int y = pos.Value;
                Candy c = _grid[x, y];
                if (c == null) continue;

                switch (c.Special)
                {
                    case SpecialType.Striped:
                        result.SpecialsActivated.Add(SpecialType.Striped);
                        result.ActivationsDetailed.Add(Tuple.Create(SpecialType.Striped, x, y, c.StripedVertical));
                        if (c.StripedVertical) ClearColumn(x, result, toDestroy);
                        else ClearRow(y, result, toDestroy);
                        break;
                    case SpecialType.Wrapped:
                        result.SpecialsActivated.Add(SpecialType.Wrapped);
                        result.ActivationsDetailed.Add(Tuple.Create(SpecialType.Wrapped, x, y, false));
                        Blast(x, y, 1, result, toDestroy);
                        Blast(x, y, 1, result, toDestroy);
                        break;
                    case SpecialType.ColorBomb:
                        result.SpecialsActivated.Add(SpecialType.ColorBomb);
                        result.ActivationsDetailed.Add(Tuple.Create(SpecialType.ColorBomb, x, y, false));
                        DestroyAllColor(c.Color, SpecialType.None, result, toDestroy);
                        break;
                    case SpecialType.Fish:
                        result.SpecialsActivated.Add(SpecialType.Fish);
                        result.ActivationsDetailed.Add(Tuple.Create(SpecialType.Fish, x, y, false));
                        SpawnFish(x, y, 3, result, toDestroy);
                        break;
                }
            }
        }

        private void DestroyCell(int x, int y, TurnResult result, int cascade, bool[,] toDestroy, bool isBlast)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return;
            if (toDestroy[x, y]) return;

            if (_chocolate[x, y])
            {
                toDestroy[x, y] = true;
                result.ChocolateDestroyed++;
                return;
            }

            if (_frosting[x, y] > 0)
            {
                _frosting[x, y]--;
                result.FrostingBroken++;
                return;
            }

            Candy c = _grid[x, y];

            if (_licorice[x, y])
            {
                _licorice[x, y] = false;
                if (c != null) c.IsLicorice = false;
                result.LicoriceBroken++;
                return;
            }

            if (c == null) return;

            toDestroy[x, y] = true;
            result.TotalCandyDestroyed++;
            result.Score += 60 * cascade;

            if (c.IsTimeCandy) result.TimeGained += 5;

            if (c.Special != SpecialType.None && !_activated[x, y])
            {
                _activated[x, y] = true;
                _activationQueue.Enqueue(new KeyValuePair<int, int>(x, y));
            }
        }

        private void ClearRow(int y, TurnResult result, bool[,] toDestroy)
        {
            if (y < 0 || y >= Rows) return;
            for (int x = 0; x < Cols; x++)
                DestroyCell(x, y, result, 1, toDestroy, false);
        }

        private void ClearColumn(int x, TurnResult result, bool[,] toDestroy)
        {
            if (x < 0 || x >= Cols) return;
            for (int y = 0; y < Rows; y++)
                DestroyCell(x, y, result, 1, toDestroy, false);
        }

        private void ClearRows(int centerY, TurnResult result, bool[,] toDestroy)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
                ClearRow(y, result, toDestroy);
        }

        private void ClearColumns(int centerX, TurnResult result, bool[,] toDestroy)
        {
            for (int x = centerX - 1; x <= centerX + 1; x++)
                ClearColumn(x, result, toDestroy);
        }

        private void Blast(int cx, int cy, int radius, TurnResult result, bool[,] toDestroy)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
                for (int y = cy - radius; y <= cy + radius; y++)
                    DestroyCell(x, y, result, 1, toDestroy, false);
        }

        private void SpawnFish(int cx, int cy, int count, TurnResult result, bool[,] toDestroy)
        {
            List<int> jellyCandidates = new List<int>();
            List<int> otherCandidates = new List<int>();
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Candy c = _grid[x, y];
                    if (c == null || toDestroy[x, y]) continue;
                    if (c.IsIngredient) continue;
                    if (c.Special == SpecialType.ColorBomb) continue;
                    if (HasJelly(x, y)) jellyCandidates.Add(y * Cols + x);
                    else otherCandidates.Add(y * Cols + x);
                }
            }

            Shuffle(jellyCandidates);
            Shuffle(otherCandidates);

            for (int i = 0; i < count; i++)
            {
                int cell = -1;
                if (jellyCandidates.Count > 0)
                {
                    cell = jellyCandidates[jellyCandidates.Count - 1];
                    jellyCandidates.RemoveAt(jellyCandidates.Count - 1);
                }
                else if (otherCandidates.Count > 0)
                {
                    cell = otherCandidates[otherCandidates.Count - 1];
                    otherCandidates.RemoveAt(otherCandidates.Count - 1);
                }
                else
                {
                    break;
                }
                DestroyCell(cell % Cols, cell / Cols, result, 1, toDestroy, false);
            }
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                T t = list[i]; list[i] = list[j]; list[j] = t;
            }
        }

        private void ApplyDestroyedCells(TurnResult result, bool[,] toDestroy)
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (!toDestroy[x, y]) continue;
                    if (_chocolate[x, y])
                    {
                        _chocolate[x, y] = false;
                        continue;
                    }
                    Candy c = _grid[x, y];
                    if (c == null) continue;
                    _grid[x, y] = null;
                    if (c.IsIngredient)
                    {
                        result.IngredientsCollected++;
                        result.Score += c.Ingredient == IngredientType.Nut ? 20000 : 10000;
                        if (IngredientsRemaining > 0) IngredientsRemaining--;
                    }
                    else
                    {
                        DamageAdjacentChocolate(x, y);
                        DamageAdjacentFrosting(x, y, result);
                        DamageAdjacentLicorice(x, y, result);
                        if (_doubleJelly[x, y])
                        {
                            _doubleJelly[x, y] = false;
                            _jelly[x, y] = true;
                            result.JellyCleared++;
                        }
                        else if (_jelly[x, y])
                        {
                            _jelly[x, y] = false;
                            result.JellyCleared++;
                            RemainingJelly--;
                        }
                    }
                }
            }
            RemainingJelly = RemainingJellyCells();
            FrostingRemaining = FrostingRemainingCells();
            LicoriceRemaining = LicoriceRemainingCells();
        }

        private void DamageAdjacentLicorice(int x, int y, TurnResult result)
        {
            if (x > 0 && _licorice[x - 1, y]) { BreakLicorice(x - 1, y); result.LicoriceBroken++; }
            if (x < Cols - 1 && _licorice[x + 1, y]) { BreakLicorice(x + 1, y); result.LicoriceBroken++; }
            if (y > 0 && _licorice[x, y - 1]) { BreakLicorice(x, y - 1); result.LicoriceBroken++; }
            if (y < Rows - 1 && _licorice[x, y + 1]) { BreakLicorice(x, y + 1); result.LicoriceBroken++; }
        }

        private void BreakLicorice(int x, int y)
        {
            _licorice[x, y] = false;
            if (_grid[x, y] != null) _grid[x, y].IsLicorice = false;
        }

        private void DamageAdjacentFrosting(int x, int y, TurnResult result)
        {
            if (x > 0 && _frosting[x - 1, y] > 0) { _frosting[x - 1, y]--; result.FrostingBroken++; }
            if (x < Cols - 1 && _frosting[x + 1, y] > 0) { _frosting[x + 1, y]--; result.FrostingBroken++; }
            if (y > 0 && _frosting[x, y - 1] > 0) { _frosting[x, y - 1]--; result.FrostingBroken++; }
            if (y < Rows - 1 && _frosting[x, y + 1] > 0) { _frosting[x, y + 1]--; result.FrostingBroken++; }
        }

        private void DamageAdjacentChocolate(int x, int y)
        {
            if (x > 0 && _chocolate[x - 1, y]) _chocolate[x - 1, y] = false;
            if (x < Cols - 1 && _chocolate[x + 1, y]) _chocolate[x + 1, y] = false;
            if (y > 0 && _chocolate[x, y - 1]) _chocolate[x, y - 1] = false;
            if (y < Rows - 1 && _chocolate[x, y + 1]) _chocolate[x, y + 1] = false;
        }

        private void GravityAndRefill(TurnResult result)
        {
            bool changed = true;
            int passes = 0;
            while (changed && passes < Rows * 2)
            {
                changed = false;
                passes++;
                for (int x = 0; x < Cols; x++)
                {
                    for (int y = Rows - 1; y > 0; y--)
                    {
                        if (_chocolate[x, y] || _frosting[x, y] > 0 || _licorice[x, y]) continue;
                        if (_grid[x, y] == null)
                        {
                            int readY = y - 1;
                            while (readY >= 0 && (_chocolate[x, readY] || _frosting[x, readY] > 0 || _licorice[x, readY]))
                            {
                                readY--;
                            }
                            if (readY >= 0 && _grid[x, readY] != null && !_chocolate[x, readY] && _frosting[x, readY] == 0 && !_licorice[x, readY])
                            {
                                _grid[x, y] = _grid[x, readY];
                                _grid[x, readY] = null;
                                changed = true;
                            }
                        }
                    }
                }

                for (int x = 0; x < Cols; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        if (_chocolate[x, y] || _frosting[x, y] > 0 || _licorice[x, y]) continue;
                        if (_grid[x, y] == null)
                        {
                            _grid[x, y] = SpawnCandy();
                            changed = true;
                        }
                    }
                }
            }
        }

        private Candy SpawnCandy()
        {
            CandyColor c = (CandyColor)_rng.Next(_level.NumColors);
            Candy candy = new Candy(c);
            if (_level.TimeCandies && _rng.Next(100) < 8)
            {
                candy.IsTimeCandy = true;
            }
            else if (_level.HasBombs && _rng.Next(100) < 5)
            {
                candy.BombTimer = _level.BombTimerBase + _rng.Next(3);
            }
            return candy;
        }

        private List<MatchGroup> DetectMatches()
        {
            List<Run> runs = new List<Run>();
            for (int y = 0; y < Rows; y++)
            {
                int x = 0;
                while (x < Cols)
                {
                    Candy c = _grid[x, y];
                    if (c == null || c.IsIngredient || HasFrosting(x, y) || c.IsLicorice)
                    {
                        x++;
                        continue;
                    }
                    CandyColor color = c.Color;
                    int end = x + 1;
                    while (end < Cols && _grid[end, y] != null && !_grid[end, y].IsIngredient && !HasFrosting(end, y) && !_grid[end, y].IsLicorice && _grid[end, y].Color == color)
                    {
                        end++;
                    }
                    int len = end - x;
                    if (len >= 3)
                    {
                        runs.Add(new Run { Color = color, Len = len, Horizontal = true, X = x, Y = y });
                    }
                    x = end;
                }
            }

            for (int x = 0; x < Cols; x++)
            {
                int y = 0;
                while (y < Rows)
                {
                    Candy c = _grid[x, y];
                    if (c == null || c.IsIngredient || HasFrosting(x, y) || c.IsLicorice)
                    {
                        y++;
                        continue;
                    }
                    CandyColor color = c.Color;
                    int end = y + 1;
                    while (end < Rows && _grid[x, end] != null && !_grid[x, end].IsIngredient && !HasFrosting(x, end) && !_grid[x, end].IsLicorice && _grid[x, end].Color == color)
                    {
                        end++;
                    }
                    int len = end - y;
                    if (len >= 3)
                    {
                        runs.Add(new Run { Color = color, Len = len, Horizontal = false, X = x, Y = y });
                    }
                    y = end;
                }
            }

            for (int y = 0; y < Rows - 1; y++)
            {
                for (int x = 0; x < Cols - 1; x++)
                {
                    Candy c1 = _grid[x, y];
                    Candy c2 = _grid[x + 1, y];
                    Candy c3 = _grid[x, y + 1];
                    Candy c4 = _grid[x + 1, y + 1];
                    if (c1 == null || c2 == null || c3 == null || c4 == null) continue;
                    if (c1.IsIngredient || c2.IsIngredient || c3.IsIngredient || c4.IsIngredient) continue;
                    if (HasFrosting(x, y) || HasFrosting(x + 1, y) || HasFrosting(x, y + 1) || HasFrosting(x + 1, y + 1)) continue;
                    if (c1.IsLicorice || c2.IsLicorice || c3.IsLicorice || c4.IsLicorice) continue;
                    if (c1.Color == c2.Color && c1.Color == c3.Color && c1.Color == c4.Color)
                    {
                        runs.Add(new Run { Color = c1.Color, Len = 4, Horizontal = true, X = x, Y = y, IsSquare = true });
                    }
                }
            }

            return MergeRunsIntoGroups(runs);
        }

        private List<MatchGroup> MergeRunsIntoGroups(List<Run> runs)
        {
            List<MatchGroup> groups = new List<MatchGroup>();
            bool[] used = new bool[runs.Count];

            for (int i = 0; i < runs.Count; i++)
            {
                if (used[i]) continue;
                List<Run> comp = new List<Run>();
                comp.Add(runs[i]);
                used[i] = true;
                bool grew = true;
                while (grew)
                {
                    grew = false;
                    for (int j = 0; j < runs.Count; j++)
                    {
                        if (used[j]) continue;
                        foreach (Run r in comp)
                        {
                            if (RunsOverlap(r, runs[j]))
                            {
                                comp.Add(runs[j]);
                                used[j] = true;
                                grew = true;
                                break;
                            }
                        }
                    }
                }

                MatchGroup g = new MatchGroup();
                foreach (Run r in comp)
                {
                    for (int yy = r.Y; yy < r.Y + (r.IsSquare ? 2 : 1); yy++)
                    {
                        for (int xx = r.X; xx < r.X + (r.IsSquare ? 2 : r.Len); xx++)
                        {
                            if (r.Horizontal || r.IsSquare)
                            {
                                g.Cells.Add(yy * Cols + xx);
                            }
                        }
                    }
                    if (!r.Horizontal && !r.IsSquare)
                    {
                        for (int k = 0; k < r.Len; k++)
                        {
                            g.Cells.Add((r.Y + k) * Cols + r.X);
                        }
                    }
                    g.Color = r.Color;
                }

                ClassifyGroup(comp, g);
                groups.Add(g);
            }
            return groups;
        }

        private void ClassifyGroup(List<Run> comp, MatchGroup g)
        {
            Run longest = null;
            Run square = null;
            Run h4 = null;
            Run v = null;
            Run h = null;
            foreach (Run r in comp)
            {
                if (r.IsSquare && square == null) square = r;
                if (r.Horizontal) h = r;
                else v = r;
                if (!r.IsSquare && r.Len == 4 && h4 == null) h4 = r;
                if (!r.IsSquare && (longest == null || r.Len > longest.Len)) longest = r;
            }

            if (longest != null && longest.Len >= 5)
            {
                g.CreateType = SpecialType.ColorBomb;
                if (longest.Horizontal)
                {
                    g.CreateX = longest.X + longest.Len / 2;
                    g.CreateY = longest.Y;
                }
                else
                {
                    g.CreateX = longest.X;
                    g.CreateY = longest.Y + longest.Len / 2;
                }
                return;
            }

            if (square != null)
            {
                g.CreateType = SpecialType.Fish;
                g.CreateX = square.X;
                g.CreateY = square.Y;
                return;
            }

            if (h != null && v != null)
            {
                foreach (int cell in g.Cells)
                {
                    int x = cell % Cols;
                    int y = cell / Cols;
                    if (InRun(h, x, y) && InRun(v, x, y))
                    {
                        g.CreateType = SpecialType.Wrapped;
                        g.CreateX = x;
                        g.CreateY = y;
                        return;
                    }
                }
            }

            if (h4 != null)
            {
                g.CreateType = SpecialType.Striped;
                g.StripedVertical = h4.Horizontal;
                if (g.CreateX < 0)
                {
                    if (h4.Horizontal)
                    {
                        g.CreateX = h4.X + 1;
                        g.CreateY = h4.Y;
                    }
                    else
                    {
                        g.CreateX = h4.X;
                        g.CreateY = h4.Y + 1;
                    }
                }
            }
        }

        private bool InRun(Run r, int x, int y)
        {
            if (r.IsSquare)
            {
                return x >= r.X && x <= r.X + 1 && y >= r.Y && y <= r.Y + 1;
            }
            if (r.Horizontal)
            {
                return y == r.Y && x >= r.X && x < r.X + r.Len;
            }
            return x == r.X && y >= r.Y && y < r.Y + r.Len;
        }

        private bool RunsOverlap(Run a, Run b)
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Cols; x++)
                {
                    if (InRun(a, x, y) && InRun(b, x, y)) return true;
                }
            }
            return false;
        }

        private class Run
        {
            public CandyColor Color;
            public int Len;
            public bool Horizontal;
            public bool IsSquare;
            public int X;
            public int Y;
        }

        public bool HasValidMoves()
        {
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (_grid[x, y] == null) continue;
                    if (_grid[x, y].IsIngredient || _grid[x, y].IsLicorice || HasFrosting(x, y)) continue;
                    if (x + 1 < Cols && IsValidMove(x, y, x + 1, y)) return true;
                    if (y + 1 < Rows && IsValidMove(x, y, x, y + 1)) return true;
                }
            }
            return false;
        }

        public bool HasPendingMatches()
        {
            return DetectMatches().Count > 0;
        }

        public bool GetHint(out int x1, out int y1, out int x2, out int y2)
        {
            List<int> indices = new List<int>();
            for (int x = 0; x < Cols; x++)
                for (int y = 0; y < Rows; y++)
                    indices.Add(y * Cols + x);

            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                int t = indices[i]; indices[i] = indices[j]; indices[j] = t;
            }

            foreach (int idx in indices)
            {
                int x = idx % Cols;
                int y = idx / Cols;
                if (_grid[x, y] == null) continue;
                if (_grid[x, y].IsIngredient || _grid[x, y].IsLicorice || HasFrosting(x, y)) continue;
                int[] dx = { 0, 0, -1, 1 };
                int[] dy = { -1, 1, 0, 0 };
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || nx >= Cols || ny < 0 || ny >= Rows) continue;
                    if (IsValidMove(x, y, nx, ny))
                    {
                        x1 = x; y1 = y; x2 = nx; y2 = ny;
                        return true;
                    }
                }
            }
            x1 = -1; y1 = -1; x2 = -1; y2 = -1;
            return false;
        }

        public void Reshuffle()
        {
            List<Candy> candies = new List<Candy>();
            List<int> positions = new List<int>();
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (_grid[x, y] == null) continue;
                    if (_grid[x, y].IsIngredient) continue;
                    if (_chocolate[x, y]) continue;
                    if (HasFrosting(x, y)) continue;
                    if (_grid[x, y].IsLicorice) continue;
                    positions.Add(y * Cols + x);
                    candies.Add(_grid[x, y]);
                }
            }

            for (int attempt = 0; attempt < 30; attempt++)
            {
                for (int i = candies.Count - 1; i > 0; i--)
                {
                    int j = _rng.Next(i + 1);
                    Candy t = candies[i]; candies[i] = candies[j]; candies[j] = t;
                }
                for (int k = 0; k < positions.Count; k++)
                {
                    _grid[positions[k] % Cols, positions[k] / Cols] = candies[k];
                }
                if (HasValidMoves() && !HasPendingMatches()) return;
            }
            GenerateBoard();
        }

        public List<string> GetSpecialPositions()
        {
            List<string> list = new List<string>();
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Candy c = _grid[x, y];
                    if (c != null && c.IsSpecial)
                    {
                        list.Add(CellName(x, y) + ": " + Localization.C(c.Color) + " " + Localization.S(c.Special));
                    }
                }
            }
            return list;
        }

        public string DescribeCell(int x, int y)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return "";
            string name = CellName(x, y);
            if (_chocolate[x, y])
            {
                return string.Format(Localization.Get("cell.chocolate"), name[0], name[1]);
            }
            Candy c = _grid[x, y];
            if (c == null)
            {
                string empty = string.Format(Localization.Get("cell.empty"), name[0], name[1]);
                return AppendJellySuffix(empty, x, y);
            }
            string desc;
            if (c.IsIngredient)
            {
                desc = string.Format(Localization.Get("cell.ingredient"), name[0], name[1], Localization.I(c.Ingredient));
            }
            else if (c.IsSpecial)
            {
                desc = string.Format(Localization.Get("cell.special"), name[0], name[1], Localization.C(c.Color), Localization.S(c.Special));
            }
            else
            {
                desc = string.Format(Localization.Get("cell.full"), name[0], name[1], Localization.C(c.Color));
            }
            if (c.IsLicorice) desc += Localization.Get("cell.locked");
            if (_frosting[x, y] > 0) desc += IsDoubleFrosting(x, y) ? Localization.Get("cell.frosting2") : Localization.Get("cell.frosting");
            if (c.BombTimer > 0) desc += string.Format(Localization.Get("cell.bomb.suffix"), c.BombTimer);
            if (c.IsTimeCandy) desc += Localization.Get("cell.time.suffix");
            return AppendJellySuffix(desc, x, y);
        }

        private string AppendJellySuffix(string desc, int x, int y)
        {
            if (_doubleJelly[x, y])
            {
                desc += ", " + Localization.Get("element.jelly2");
            }
            else if (_jelly[x, y])
            {
                desc += ", " + Localization.Get("element.jelly");
            }
            return desc;
        }

        public string DescribeRow(int y)
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < Cols; x++)
            {
                if (x > 0) sb.Append(". ");
                sb.Append(DescribeCell(x, y));
            }
            return sb.ToString();
        }

        public string DescribeColumn(int x)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < Rows; y++)
            {
                if (y > 0) sb.Append(". ");
                sb.Append(DescribeCell(x, y));
            }
            return sb.ToString();
        }

        public string DescribeBoard()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Localization.Get("msg.board")).Append(". ");
            for (int y = 0; y < Rows; y++)
            {
                sb.Append(string.Format(Localization.Get("row.read"), y + 1, DescribeRow(y)));
                if (y < Rows - 1) sb.Append(". ");
            }
            return sb.ToString();
        }

        public string StatusText()
        {
            string obj = _level.ObjectiveText;
            string extra = "";
            switch (_level.Type)
            {
                case LevelType.Score:
                    int scorePct = _level.TargetScore > 0 ? Math.Min(100, (int)Math.Round((double)Score / _level.TargetScore * 100.0)) : 100;
                    extra = string.Format(Localization.Get("score.status.detail"), Score, _level.TargetScore, scorePct);
                    break;
                case LevelType.Jelly:
                    int clearedJellyPct = InitialJelly > 0 ? (int)Math.Round((double)(InitialJelly - RemainingJelly) / InitialJelly * 100.0) : 100;
                    extra = string.Format(Localization.Get("jelly.status.detail"), RemainingJelly, clearedJellyPct);
                    break;
                case LevelType.Ingredient:
                    int totalIng = _level.TargetIngredients;
                    int collectedIng = totalIng - IngredientsRemaining;
                    int ingPct = totalIng > 0 ? (int)Math.Round((double)collectedIng / totalIng * 100.0) : 100;
                    extra = string.Format(Localization.Get("ingredient.status.detail"), IngredientsRemaining, totalIng, ingPct);
                    break;
                case LevelType.Timed:
                    int timePct = _level.TimeSeconds > 0 ? (int)Math.Round((double)TimeLeft / _level.TimeSeconds * 100.0) : 0;
                    extra = string.Format(Localization.Get("timed.status.detail"), (int)Math.Ceiling(TimeLeft), (int)_level.TimeSeconds, timePct);
                    break;
                case LevelType.Order:
                    int totalOrders = Orders.Count;
                    int fulfilled = OrdersFulfilled;
                    int orderPct = totalOrders > 0 ? (int)Math.Round((double)fulfilled / totalOrders * 100.0) : 100;
                    extra = string.Format(Localization.Get("order.status.detail"), fulfilled, totalOrders, orderPct, OrdersRemaining);
                    break;
            }
            string moves = _level.Type == LevelType.Timed
                ? string.Format(Localization.Get("time.count"), (int)Math.Ceiling(TimeLeft)) + "s"
                : string.Format(Localization.Get("moves.count"), MovesLeft);
            return string.Format(Localization.Get("status.format"), _level.Number, Score, _level.TargetScore, obj, extra, moves);
        }

        public string ApplyStartBoosters(List<BoosterType> boosters)
        {
            List<string> msgs = new List<string>();
            foreach (BoosterType b in boosters)
            {
                switch (b)
                {
                    case BoosterType.ExtraMoves:
                        MovesLeft += 5;
                        msgs.Add(Localization.Get("booster.plus.moves"));
                        break;
                    case BoosterType.ExtraTime:
                        if (_level.Type == LevelType.Timed)
                        {
                            TimeLeft += 15;
                            msgs.Add(Localization.Get("booster.plus.time"));
                        }
                        break;
                    case BoosterType.ColorBomb:
                        if (PlaceSpecialAtRandom(SpecialType.ColorBomb))
                        {
                            msgs.Add(Boosters.Name(BoosterType.ColorBomb));
                        }
                        break;
                    case BoosterType.JellyFish:
                        int placed = 0;
                        if (PlaceSpecialAtRandom(SpecialType.Fish)) placed++;
                        if (PlaceSpecialAtRandom(SpecialType.Fish)) placed++;
                        if (placed > 0)
                        {
                            msgs.Add(Boosters.Name(BoosterType.JellyFish));
                        }
                        break;
                }
            }

            TurnResult r = new TurnResult();
            if (HasPendingMatches())
            {
                RunCascades(r);
                Score += r.ScoreGained;
            }
            return string.Join(", ", msgs);
        }

        private bool PlaceSpecialAtRandom(SpecialType type)
        {
            List<int> spots = new List<int>();
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Candy c = _grid[x, y];
                    if (c == null || c.IsIngredient || c.IsSpecial) continue;
                    spots.Add(y * Cols + x);
                }
            }
            if (spots.Count == 0) return false;
            int i = spots[_rng.Next(spots.Count)];
            _grid[i % Cols, i / Cols].Special = type;
            return true;
        }

        public bool PlaceSpecialAt(int x, int y, SpecialType type)
        {
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return false;
            Candy c = _grid[x, y];
            if (c == null || c.IsIngredient || _chocolate[x, y] || HasFrosting(x, y) || c.IsLicorice) return false;
            c.Special = type;
            return true;
        }

        public TurnResult SmashCell(int x, int y)
        {
            TurnResult result = new TurnResult();
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return result;
            bool hasAnything = _chocolate[x, y] || _grid[x, y] != null;
            if (!hasAnything) return result;
            result.Valid = true;
            if (_frosting[x, y] > 0)
            {
                _frosting[x, y] = 0;
                result.FrostingBroken++;
            }
            if (_licorice[x, y])
            {
                BreakLicorice(x, y);
                result.LicoriceBroken++;
            }
            _activated = new bool[Cols, Rows];
            _activationQueue.Clear();
            bool[,] toDestroy = new bool[Cols, Rows];
            DestroyCell(x, y, result, 1, toDestroy, false);
            ProcessActivationQueue(result, toDestroy);
            ApplyDestroyedCells(result, toDestroy);
            GravityAndRefill(result);
            RunCascades(result);
            Score += result.ScoreGained;
            CheckCompletion(result);
            return result;
        }

        public TurnResult SugarCrush(int remainingMoves)
        {
            TurnResult result = new TurnResult();
            result.Valid = true;
            result.Events.Add(Localization.Get("msg.sugar"));

            int movesToConvert = Math.Min(remainingMoves, 10);
            int converted = 0;
            for (int i = 0; i < movesToConvert; i++)
            {
                if (PlaceSpecialAtRandom(_rng.Next(2) == 0 ? SpecialType.Striped : SpecialType.Wrapped))
                {
                    converted++;
                }
            }

            int executedActivations = 0;
            while (true)
            {
                List<KeyValuePair<int, int>> specials = new List<KeyValuePair<int, int>>();
                for (int x = 0; x < Cols; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        Candy c = _grid[x, y];
                        if (c != null && c.IsSpecial && !c.IsIngredient)
                        {
                            specials.Add(new KeyValuePair<int, int>(x, y));
                        }
                    }
                }
                if (specials.Count == 0) break;

                KeyValuePair<int, int> pick = specials[_rng.Next(specials.Count)];
                _activated = new bool[Cols, Rows];
                _activationQueue.Clear();
                bool[,] toDestroy = new bool[Cols, Rows];

                DestroyCell(pick.Key, pick.Value, result, 1, toDestroy, false);
                executedActivations++;
                ProcessActivationQueue(result, toDestroy);
                ApplyDestroyedCells(result, toDestroy);
                GravityAndRefill(result);
                RunCascades(result);
            }

            result.SugarCrushMoves = Math.Min(Math.Min(remainingMoves, 12), Math.Max(converted, executedActivations));
            if (result.SugarCrushMoves == 0 && remainingMoves > 0) result.SugarCrushMoves = Math.Min(remainingMoves, 12);
            Score += result.ScoreGained;
            return result;
        }
    }
}