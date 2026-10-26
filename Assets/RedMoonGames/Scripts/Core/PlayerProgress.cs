using System;

namespace RedMoonGames
{
    [Serializable]
    public sealed class PlayerProgress
    {
        public int bestScore;
        public bool crownUnlocked;
        public bool crownEquipped;

        public void Validate()
        {
            bestScore = Math.Max(0, bestScore);
            crownUnlocked = crownUnlocked || bestScore > 5000;
            crownEquipped = crownUnlocked && crownEquipped;
        }

        public bool Record(int score)
        {
            bestScore = Math.Max(bestScore, score);
            bool newlyUnlocked = !crownUnlocked && bestScore > 5000;
            crownUnlocked |= newlyUnlocked;
            return newlyUnlocked;
        }
    }
}
