using System;
using System.IO;
using System.Xml.Serialization;

namespace snakeGame
{
    public class XmlRecordStorage : IRecordStorage
    {
        private readonly string filePath;

        public XmlRecordStorage(string filePath)
        {
            this.filePath = filePath;
        }

        public HighScores Load()
        {
            try
            {
                if (!File.Exists(filePath))
                    return new HighScores();

                XmlSerializer serializer = new XmlSerializer(typeof(HighScores));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    return (HighScores)serializer.Deserialize(stream);
            }
            catch
            {
                return new HighScores();
            }
        }

        public void Save(HighScores highScores)
        {
            if (highScores == null)
                return;

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            XmlSerializer serializer = new XmlSerializer(typeof(HighScores));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
                serializer.Serialize(stream, highScores);
        }
    }
}
