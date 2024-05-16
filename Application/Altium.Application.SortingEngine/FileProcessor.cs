namespace Altium.Application.SortingEngine
{
    internal record LineInfo
    {
        public string Line { get; init; }
        public int IntPart { get; init; }
        public string StringPart { get; init; }
    }

    internal class LineComparer : IComparer<LineInfo>
    {
        public int Compare(LineInfo? x, LineInfo? y)
        {
            int lineComparison = string.Compare(x.StringPart, y.StringPart);

            if(lineComparison == 0)
            {
                return x.IntPart.CompareTo(y.IntPart);
            }

            return lineComparison;
        }
    }

    internal record LineInfoReader
    {
        public int ReaderId { get; init; }
        public string Line { get; init; }
    }

    public static class FileProcessor
    {

        public static async Task SplitFileAndSortChunks(string inputFile, string tempFilesFolder)
        {
            //int limit = 1000000;
            int limit = 5;
            int lineNumber = 0;
            bool done = false;
            int chunkIndex = 0;

            using (var reader = new StreamReader(inputFile))
            {
                var heap = new SortedDictionary<LineInfo, Queue<string>>(new LineComparer());

                while (!done)
                {
                    string? line;
                    while (lineNumber < limit
                        && (line = await reader.ReadLineAsync()) is not null)
                    {
                        var key = CreateKey(line);

                        if (!heap.ContainsKey(key))
                        {
                            heap.Add(key, new Queue<string>());
                        }

                        heap[key].Enqueue(line);
                        ++lineNumber;
                    }

                    // Create temp file
                    var tempFileName = $"{tempFilesFolder}\\temp_chunk_{++chunkIndex}.txt";
                    using (var writer = new StreamWriter(tempFileName))
                    {
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

                    if (lineNumber < limit)
                    {
                        done = true;
                    }

                    lineNumber = 0;
                    heap.Clear();
                }
            }
        }

        private static LineComparer comparer = new LineComparer();

        public static async Task SortLargeFile(string outputFile, string tempFilesFolder)
        {
            var tempFiles = GetFiles(tempFilesFolder);
            var heap = new SortedDictionary<LineInfo, Queue<LineInfoReader>>(comparer);

            using (var outputFileStream = new StreamWriter(outputFile))
            {
                List<StreamReader> readers = new List<StreamReader>();

                foreach (var tempFile in tempFiles)
                {
                    readers.Add(new StreamReader(tempFile));
                }

                int readerId = 0;

                foreach (var reader in readers)
                {
                    ParseLine(heap, readerId, reader);

                    ++readerId;
                }

                try
                {
                    while (heap.Count > 0)
                    {
                        var minKey = heap.First().Key;
                        var minValue = heap[minKey].Dequeue();
                        readerId = minValue.ReaderId;
                        var minLine = minValue.Line;

                        heap.Remove(minKey);

                        ParseLine(heap, readerId, readers[readerId]);

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
            }
        }

        public static void Clean(string tempFilesFolder)
        {
            // Clean up temporary files
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

        private static void ParseLine(SortedDictionary<LineInfo, Queue<LineInfoReader>> heap, int readerId, StreamReader reader)
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

        //private static string? CreateKey(string line)
        private static LineInfo CreateKey(string line)
        {
            var split = line.Split(new[] { '.' }, 2);
            //return split[1] + split[0]; // String part first, then number part
            return new LineInfo
            {
                Line = line,
                IntPart = int.Parse(split[0]),
                StringPart = split[1]
            };
        }
    }
}
