namespace snakeGame
{
    public class HighScores
    {
        public int ClassicBest { get; set; }
        public int HardBest { get; set; }

        public int GetBest(GameMode mode)
        {
            if (mode == GameMode.Hard)
                return HardBest;

            return ClassicBest;
        }

        public bool SaveIfBest(GameMode mode, int score)
        {
            if (mode == GameMode.Hard)
            {
                if (score <= HardBest)
                    return false;

                HardBest = score;
                return true;
            }

            if (score <= ClassicBest)
                return false;

            ClassicBest = score;
            return true;
        }
    }
}
