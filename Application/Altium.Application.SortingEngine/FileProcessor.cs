namespace Altium.Application.SortingEngine
{
    internal struct LineInfo
    {
        public int ReaderId { get; set; }
        public string Line { get; set; }
    }

    public static class FileProcessor
    {
        public static async Task SortLargeFile(string outputFile, string tempFilesFolder)
        {
            var tempFiles = GetFiles(tempFilesFolder);
            var heap = new SortedDictionary<string, Queue<LineInfo>>();

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

                while (heap.Count > 0)
                {
                    var minKey = heap.Keys.Min();
                    var minValue = heap[minKey].Dequeue();
                    readerId = minValue.ReaderId;
                    var minLine = minValue.Line;
                    
                    heap.Remove(minKey);

                    ParseLine(heap, readerId, readers[readerId]);

                    outputFileStream.WriteLine(minLine);
                }

                foreach (var reader in readers)
                {
                    reader.Close();
                }
            }

            // Clean up temporary files
            //foreach (var tempFile in tempFiles)
            //{
            //    File.Delete(tempFile);
            //}
        }

        public static List<string> GetFiles(string folderName)
        {
            return Directory
                .EnumerateFiles(folderName)
                .ToList();
        }

        public static async Task SplitFileAndSortChunks(string inputFile, string tempFilesFolder)
        {
            int limit = 10000;
            int lineNumber = 0;
            bool done = false;
            int chunkIndex = 0;

            using (var reader = new StreamReader(inputFile))
            {
                var heap = new SortedDictionary<string, Queue<string>>();

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

        private static void ParseLine(SortedDictionary<string, Queue<LineInfo>> heap, int readerId, StreamReader reader)
        {
            var line = reader.ReadLine();
            if (line != null)
            {
                var key = CreateKey(line);

                if (!heap.ContainsKey(key))
                {
                    heap.Add(key, new Queue<LineInfo>());
                }

                heap[key].Enqueue(new LineInfo { ReaderId = readerId, Line = line });
            }
        }

        private static string? CreateKey(string line)
        {
            var split = line.Split(new[] { '.' }, 2);
            return split[1] + split[0]; // String part first, then number part
        }
    }
}
