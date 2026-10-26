using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RedMoonGames;

static class Checks
{
    sealed class Fixture
    {
        public string name { get; set; }
        public int[] board { get; set; }
        public int[] active { get; set; }
        public int points { get; set; }
        public int[] cells { get; set; }
        public int newStars { get; set; }
        public int remainingStars { get; set; }
    }
    static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Main()
    {
        var fixtures = JsonSerializer.Deserialize<List<Fixture>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures.json")));
        foreach (var fixture in fixtures)
        {
            var game = new MatchGame(new Random(1), fixture.board);
            var step = game.Analyze(new HashSet<int>(fixture.active));
            Assert(step.Points == fixture.points, fixture.name + ": score " + step.Points + " != " + fixture.points);
            Assert(step.Cells.OrderBy(i => i).SequenceEqual(fixture.cells), fixture.name + ": matched cells differ");
            int stars = game.Resolve(step, 1);
            Assert(stars == fixture.newStars, fixture.name + ": star creation differs");
            Assert(game.Board.Count(v => v == MatchGame.Star) == fixture.remainingStars, fixture.name + ": remaining stars differ");
        }
        for (int seed = 0; seed < 100; seed++)
        {
            var game = new MatchGame(new Random(seed));
            Assert(game.Board.Length == 49 && game.Analyze().Cells.Length == 0 && game.HasMove(), "Invalid fresh board");
        }
        Assert(!MatchGame.Adjacent(6, 7) && !MatchGame.Adjacent(-1, 0) && MatchGame.Adjacent(6, 13), "Adjacency");
        var board = Enumerable.Range(0, 49).Select(i => (i + i / 7) % 6).ToArray();
        var moves = new MatchGame(new Random(2), board);
        int original0 = moves.Board[0], original1 = moves.Board[1];
        var miss = moves.Swap(0, 1);
        Assert(miss.Cells.Length == 0 && moves.Score == -20 && moves.Moves == 27, "Non-match scoring");
        Assert(moves.Board[0] == original1 && moves.Board[1] == original0, "Non-match must stay swapped");
        while (moves.Moves > 0) { var step = moves.Swap(0, 1); Assert(step.Cells.Length == 0, "Fixture unexpectedly matches"); }
        Assert(moves.Score == -560 && moves.Swap(0, 1) == null && moves.Moves == 0, "Round must end after 28 swaps");
        board[0] = board[1] = board[9] = MatchGame.Star; board[2] = 0;
        var jackpot = new MatchGame(new Random(3), board);
        var initial = jackpot.Swap(2, 9);
        Assert(initial.Jackpot && initial.Points >= 3000, "User-made star jackpot");
        jackpot.Resolve(initial, 1);
        Assert(jackpot.Score >= 3000 && jackpot.Moves == 27, "Score threshold must not finish the round");
        var profile = new PlayerProgress();
        Assert(!profile.Record(5000) && !profile.crownUnlocked, "5000 is below reward threshold");
        Assert(profile.Record(5001) && profile.crownUnlocked && !profile.crownEquipped, "Reward must wait for claim");
        profile.crownEquipped = true; profile.Record(-20); profile.Validate();
        Assert(profile.bestScore == 5001 && profile.crownEquipped, "Persistence invariants");
        Console.WriteLine("PASS: " + fixtures.Count + " JS/C# parity fixtures; 100 boards; permanent swap penalties; 28-move ending; jackpots; reward threshold and equipment.");
    }
}
