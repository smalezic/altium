// See https://aka.ms/new-console-template for more information
using Altium.Application.SortingEngine;

Console.WriteLine("Hello, World!");

var inputFileName = @"D:\Temp\FileCreator\file.txt";
string outputFileName = @"D:\Temp\FileCreator\outputExample.txt";
string tempFilesFolder = @"D:\Temp\FileCreator\X";

//FileCreator.Create(inputFileName, 102400000);

//await FileProcessor.SplitFile(inputFileName);

//await FileProcessor.SortLargeFile(inputFileName, outputFileName);

await FileProcessor.SplitFileAndSortChunks(inputFileName, tempFilesFolder);

await FileProcessor.SortLargeFile(outputFileName, tempFilesFolder);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();