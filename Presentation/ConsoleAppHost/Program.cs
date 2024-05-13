// See https://aka.ms/new-console-template for more information
using Altium.Application.SortingEngine;

Console.WriteLine("Hello, World!");

var inputFileName = @"D:\Temp\FileCreator\file1G.txt";
string outputFileName = @"D:\Temp\FileCreator\output1G.txt";
string tempFilesFolder = @"D:\Temp\FileCreator\X";

DateTime startTime = DateTime.UtcNow;

Console.WriteLine($"Start time: {startTime.ToLocalTime()}");

FileCreator.Create(inputFileName, 1024000000);

var currentTime = DateTime.UtcNow;
Console.WriteLine($"Creation time: {currentTime - startTime}");

await FileProcessor.SplitFileAndSortChunks(inputFileName, tempFilesFolder);
Console.WriteLine($"Splitting time: {DateTime.UtcNow - currentTime}");
currentTime = DateTime.UtcNow;

await FileProcessor.SortLargeFile(outputFileName, tempFilesFolder);
Console.WriteLine($"Sorting time: {DateTime.UtcNow - currentTime}");
currentTime = DateTime.UtcNow;

FileProcessor.Clean(tempFilesFolder);
Console.WriteLine($"Cleaning time: {DateTime.UtcNow - currentTime}");
currentTime = DateTime.UtcNow;
Console.WriteLine($"Stop time: {currentTime.ToLocalTime()}");
Console.WriteLine($"Working time: {currentTime - startTime}");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();