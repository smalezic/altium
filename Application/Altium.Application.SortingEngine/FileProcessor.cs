using Altium.Domain;
using Altium.Domain.Comparer;

namespace Altium.Application.SortingEngine
{
    public static class FileProcessor
    {
        private static readonly LineComparer comparer = new();

        public static void SplitFileAndSortChunks(
            string inputFile,
            string tempFilesFolder,
            long limit)
        {
            bool done = false;
            int lineNumber = -1;
            int chunkIndex = 0;

            using (var reader = new StreamReader(inputFile))
            {
                var heap = new SortedDictionary<LineInfo, Queue<string>>(new LineComparer());

                while (!done)
                {
                    // Get a line from the file, split it on two parts and put them into the data structure
                    ExtractLineFromFile(limit, ++lineNumber, reader, heap);

                    // Create temp file
                    CreateTempFile(tempFilesFolder, ++chunkIndex, heap);

                    if (lineNumber < limit)
                    {
                        // If there is no more lines in the file, the method is done
                        done = true;
                    }

                    lineNumber = 0; // Reset it to 0 for a new file
                    heap.Clear();
                }
            }

            static void ExtractLineFromFile(long limit, long lineNumber, StreamReader reader, SortedDictionary<LineInfo, Queue<string>> heap)
            {
                string? line;

                while (lineNumber < limit
                    && (line = reader.ReadLine()) is not null)
                {
                    var key = CreateKey(line);

                    if (!heap.ContainsKey(key))
                    {
                        heap.Add(key, new Queue<string>());
                    }

                    heap[key].Enqueue(line);
                }
            }

            static void CreateTempFile(string tempFilesFolder, int chunkIndex, SortedDictionary<LineInfo, Queue<string>> heap)
            {
                var tempFileName = $"{tempFilesFolder}\\temp_chunk_{chunkIndex}.txt";

                using var writer = new StreamWriter(tempFileName);
                
                foreach (var pair in heap)
                {
                    if (pair.Value.Count == 1)
                    {
                        writer.WriteLine(pair.Value.Dequeue());
                    }
                    else
                    {
                        while (pair.Value.Count > 0)
                        {
                            writer.WriteLine(pair.Value.Dequeue());
                        }
                    }
                }
            }
        }

        public static void CombineSortedFiles(string outputFile, string tempFilesFolder)
        {
            var tempFiles = GetFiles(tempFilesFolder);
            var heap = new SortedDictionary<LineInfo, Queue<LineInfoReader>>(comparer);

            using var outputFileStream = new StreamWriter(outputFile);

            List<StreamReader> readers = InitiateReaders(tempFiles);

            ReadTopLines(heap, readers);

            int readerId = 0;

            try
            {
                while (heap.Count > 0)
                {
                    var minKey = heap.First().Key;
                    var minValue = heap[minKey].Dequeue();
                    readerId = minValue.ReaderId;
                    var minLine = minValue.Line;

                    heap.Remove(minKey);

                    ReadLine(heap, readerId, readers[readerId]);

                    outputFileStream.WriteLine(minLine);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }

            foreach (var reader in readers)
            {
                reader.Close();
            }

            static List<StreamReader> InitiateReaders(List<string> tempFiles)
            {
                List<StreamReader> readers = [];

                foreach (var tempFile in tempFiles)
                {
                    readers.Add(new StreamReader(tempFile));
                }

                return readers;
            }

            static void ReadTopLines(SortedDictionary<LineInfo, Queue<LineInfoReader>> heap, List<StreamReader> readers)
            {
                int readerId = 0;

                foreach (var reader in readers)
                {
                    ReadLine(heap, readerId, reader);

                    ++readerId;
                }
            }
        }

        public static void Clean(string tempFilesFolder)
        {
            var tempFiles = GetFiles(tempFilesFolder);
            foreach (var tempFile in tempFiles)
            {
                File.Delete(tempFile);
            }
        }

        private static List<string> GetFiles(string folderName)
        {
            return Directory
                .EnumerateFiles(folderName)
                .ToList();
        }

        private static void ReadLine(SortedDictionary<LineInfo, Queue<LineInfoReader>> heap, int readerId, StreamReader reader)
        {
            var line = reader.ReadLine();
            if (line != null)
            {
                var key = CreateKey(line);

                if (!heap.ContainsKey(key))
                {
                    heap.Add(key, new Queue<LineInfoReader>());
                }

                heap[key].Enqueue(new LineInfoReader { ReaderId = readerId, Line = line });
            }
        }

        private static LineInfo CreateKey(string line)
        {
            var split = line.Split(new[] { '.' }, 2);
            return new LineInfo
            {
                Line = line,
                IntPart = int.Parse(split[0]),
                StringPart = split[1]
            };
        }
    }
}
