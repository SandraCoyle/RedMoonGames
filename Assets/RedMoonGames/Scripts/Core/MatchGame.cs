using System;
using System.Collections.Generic;
using System.Linq;

namespace RedMoonGames
{
    // Pure C#: shared rules can be tested without an editor or Unity licence.
    public sealed class MatchGame
    {
        public const int Size = 7, Star = 6, TotalMoves = 28;
        public int[] Board { get; private set; }
        public int Score { get; private set; }
        public int Moves { get; private set; } = TotalMoves;
        readonly Random random;

        public sealed class MatchStep
        {
            public List<List<int>> Groups = new List<List<int>>();
            public int[] Cells = Array.Empty<int>();
            public int Points;
            public bool Jackpot;
        }

        public MatchGame(Random random = null, int[] initialBoard = null)
        {
            this.random = random ?? new Random();
            if (initialBoard != null)
            {
                if (initialBoard.Length != Size * Size || initialBoard.Any(v => v < 0 || v > Star))
                    throw new ArgumentException("A board must contain 49 tiles of type 0–6.");
                Board = (int[])initialBoard.Clone();
            }
            else NewBoard();
        }

        public static bool Adjacent(int a, int b) => a >= 0 && b >= 0 && a < 49 && b < 49 &&
            Math.Abs(a / Size - b / Size) + Math.Abs(a % Size - b % Size) == 1;

        void Exchange(int a, int b) { int value = Board[a]; Board[a] = Board[b]; Board[b] = value; }

        public void NewBoard()
        {
            do
            {
                Board = new int[49];
                for (int i = 0; i < Board.Length; i++)
                {
                    var choices = new List<int>();
                    for (int v = 0; v < 6; v++)
                        if (!(i % Size >= 2 && Board[i - 1] == v && Board[i - 2] == v) &&
                            !(i >= Size * 2 && Board[i - Size] == v && Board[i - Size * 2] == v)) choices.Add(v);
                    Board[i] = choices[random.Next(choices.Count)];
                }
            } while (!HasMove());
        }

        public bool HasMove()
        {
            for (int i = 0; i < 49; i++) foreach (int j in new[] { i + 1, i + Size })
            {
                if (!Adjacent(i, j)) continue;
                Exchange(i, j);
                bool hit = Analyze(ActiveStarsForMove(i, j)).Cells.Length > 0;
                Exchange(i, j);
                if (hit) return true;
            }
            return false;
        }

        List<List<int>> Lines(HashSet<int> active, bool starsOnly = false)
        {
            var lines = new List<List<int>>();
            foreach (int step in new[] { 1, Size }) for (int lane = 0; lane < Size; lane++)
            for (int type = starsOnly ? Star : 0; type <= (starsOnly ? Star : 5); type++)
            {
                var run = new List<int>();
                for (int offset = 0; offset <= Size; offset++)
                {
                    int i = step == 1 ? lane * Size + offset : offset * Size + lane;
                    bool match = offset < Size && (starsOnly
                        ? Board[i] == Star && active.Contains(i)
                        : Board[i] == type || Board[i] == Star && active.Contains(i));
                    if (match) run.Add(i);
                    else
                    {
                        if (run.Count >= 3 && (starsOnly || run.Any(k => Board[k] == type))) lines.Add(run);
                        run = new List<int>();
                    }
                }
            }
            return lines;
        }

        List<List<int>> AllLines(HashSet<int> active)
        {
            var lines = Lines(active); lines.AddRange(Lines(active, true));
            return lines.Where((line, index) => !lines.Where((other, j) => j != index && other.Count >= line.Count &&
                (other.Count > line.Count || j < index)).Any(other => line.All(other.Contains))).ToList();
        }

        HashSet<int> ActiveStarsForMove(int a, int b)
        {
            var all = new HashSet<int>(Enumerable.Range(0, 49).Where(i => Board[i] == Star));
            return new HashSet<int>(AllLines(all).Where(line => line.Contains(a) || line.Contains(b))
                .SelectMany(line => line).Where(i => Board[i] == Star));
        }

        public MatchStep Analyze(HashSet<int> active = null)
        {
            active = active ?? new HashSet<int>();
            var result = new MatchStep();
            foreach (var line in AllLines(active))
            {
                var joined = new HashSet<int>(line);
                for (int i = result.Groups.Count - 1; i >= 0; i--)
                    if (result.Groups[i].Any(joined.Contains)) { joined.UnionWith(result.Groups[i]); result.Groups.RemoveAt(i); }
                result.Groups.Add(joined.ToList());
            }
            var starLines = Lines(active, true);
            result.Jackpot = starLines.Count > 0;
            result.Points = starLines.Sum(line => 3000 + (line.Count - 3) * 1000);
            foreach (var group in result.Groups)
            {
                if (group.All(i => Board[i] == Star)) continue;
                result.Points += group.Count * 20 + (group.Count >= 5 ? 200 : group.Count == 4 ? 40 : 0);
                if (group.Any(i => Board[i] == Star)) result.Points += group.Count >= 5 ? 300 : group.Count == 4 ? 120 : 0;
            }
            result.Cells = result.Groups.SelectMany(g => g).Distinct().ToArray();
            return result;
        }

        // Every adjacent swap stays in place and consumes one of the 28 moves.
        public MatchStep Swap(int a, int b)
        {
            if (Moves <= 0 || !Adjacent(a, b)) return null;
            Exchange(a, b); Moves--;
            var step = Analyze(ActiveStarsForMove(a, b));
            if (step.Cells.Length == 0) Score -= 20;
            return step;
        }

        public int Resolve(MatchStep step, int chain, params int[] preferred)
        {
            if (step == null || chain < 1) throw new ArgumentException("A match and a positive chain are required.");
            Score += step.Points * chain;
            var stars = new HashSet<int>();
            foreach (var group in step.Groups.Where(g => g.Count >= 5))
            {
                int position = preferred.Where(group.Contains).Cast<int?>().FirstOrDefault() ?? group[0];
                stars.Add(position);
            }
            foreach (int i in step.Cells) Board[i] = stars.Contains(i) ? Star : -1;
            for (int c = 0; c < Size; c++)
            {
                var column = new List<int>();
                for (int r = Size - 1; r >= 0; r--) if (Board[r * Size + c] >= 0) column.Add(Board[r * Size + c]);
                while (column.Count < Size) column.Add(random.Next(6));
                for (int r = Size - 1; r >= 0; r--) Board[r * Size + c] = column[Size - 1 - r];
            }
            return stars.Count;
        }
    }
}
