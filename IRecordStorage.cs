namespace snakeGame
{
    public interface IRecordStorage
    {
        HighScores Load();
        void Save(HighScores highScores);
    }
}
