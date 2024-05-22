using System.Runtime.CompilerServices;
using Altium.Application.SortingEngine;

try
{
    Console.WriteLine("Enter the name of the source file");
    Console.WriteLine("(Correct folder path must be specified. If the file in the folder doesn't exist it will be created)");
    var inputFileName = Console.ReadLine();
    var folderPath = Path.GetDirectoryName(inputFileName);
    var outputFileName = Path.Combine(folderPath!, "Output.txt");
    string tempFilesFolder = Path.Combine(folderPath!, "Temp");
    Directory.CreateDirectory(tempFilesFolder);

    DateTime startTime = DateTime.UtcNow;
    DateTime currentTime = DateTime.UtcNow;

    Console.WriteLine($"Start time: {startTime.ToLocalTime()}");

    if(!File.Exists(inputFileName))
    {
        Console.WriteLine($"The file '{inputFileName}' does not exist, it will be created");
        Console.WriteLine("Enter the size of the file (in bytes):");
        var size = Console.ReadLine();
        long.TryParse(size, out long sizeInBytes);
        FileCreator.Create(inputFileName!, sizeInBytes);
        
        currentTime = DateTime.UtcNow;
        Console.WriteLine($"Creation time: {currentTime - startTime}");
    }

    var tmpFileSizeInBytes = GetFileSize(inputFileName!);

    Console.WriteLine("Sorting is started...");
    FileProcessor.SplitFileAndSortChunks(inputFileName!, tempFilesFolder, tmpFileSizeInBytes);
    Console.WriteLine($"Splitting time: {DateTime.UtcNow - currentTime}");
    currentTime = DateTime.UtcNow;

    FileProcessor.CombineSortedFiles(outputFileName, tempFilesFolder);
    
    Console.WriteLine($"Sorting time: {DateTime.UtcNow - currentTime}");
    currentTime = DateTime.UtcNow;

    FileProcessor.Clean(tempFilesFolder);
    Console.WriteLine($"Cleaning time: {DateTime.UtcNow - currentTime}");
    currentTime = DateTime.UtcNow;
    Console.WriteLine($"Stop time: {currentTime.ToLocalTime()}");
    Console.WriteLine($"Working time: {currentTime - startTime}");

    long GetFileSize(string inputFileName)
    {
        FileInfo fileInfo = new FileInfo(inputFileName);
        return fileInfo.Length / 100;
    }
}
catch (Exception exc)
{
    Console.WriteLine($"An error occured: {exc.Message}");
}


Console.WriteLine("Press any key to exit...");
Console.ReadKey();