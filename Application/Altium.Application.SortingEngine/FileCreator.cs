namespace Altium.Application.SortingEngine
{
    public static class FileCreator
    {
        public static void Create(string filePath, long fileSizeInBytes)
        {
            const int stringPartLength = 50; // Length of each line in characters
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // Characters to generate random lines

            var random = new Random();

            using var writer = new StreamWriter(filePath);
            long bytesWritten = 0;
            while (bytesWritten < fileSizeInBytes)
            {
                var numberPart = random.Next();
                var numberPartLength = numberPart.ToString().Length;
                var lineLength = numberPartLength + stringPartLength + 1;

                var lineBuffer = new char[lineLength];

                CreateNumberPart(numberPart, numberPartLength, lineBuffer);
                AddSeparator(numberPartLength, lineBuffer);
                CreateTextPart(stringPartLength, characters, random, numberPartLength, lineBuffer);

                var line = new string(lineBuffer);

                writer.WriteLine(line);

                bytesWritten += line.Length + Environment.NewLine.Length;
            }

            static void CreateNumberPart(int numberPart, int numberPartLength, char[] lineBuffer)
            {
                for (int i = 0; i < numberPartLength; i++)
                {
                    lineBuffer[i] = numberPart.ToString()[i];
                }
            }

            static void AddSeparator(int numberPartLength, char[] lineBuffer)
            {
                lineBuffer[numberPartLength] = '.';
            }

            static void CreateTextPart(int stringPartLength, string characters, Random random, int numberPartLength, char[] lineBuffer)
            {
                for (int i = numberPartLength + 1; i < numberPartLength + stringPartLength; i++)
                {
                    lineBuffer[i] = characters[random.Next(characters.Length)];
                }
            }
        }
    }
}
