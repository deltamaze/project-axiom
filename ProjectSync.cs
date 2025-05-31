#!/usr/bin/env dotnet-script

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security; // Required for SecurityException

// Configure which file extensions to include (without the dot)
var INCLUDED_EXTENSIONS = new List<string> { "cs", "csproj", "xml", "mgcb", "md", "sln" };

// --- Helper Methods ---

/**
 * Check if a file has one of the included extensions
 */
bool IsIncludedFile(string filename)
{
    string extension = Path.GetExtension(filename); // Gets ".ext"
    if (string.IsNullOrEmpty(extension)) return false;
    return INCLUDED_EXTENSIONS.Contains(extension.Substring(1).ToLowerInvariant()); // Remove dot and compare case-insensitively
}

/**
 * Check if a directory should be excluded
 */
bool ShouldExcludeDirectory(string dirname)
{
    return dirname.StartsWith(".") || dirname.Equals("bin", StringComparison.OrdinalIgnoreCase) || dirname.Equals("obj", StringComparison.OrdinalIgnoreCase);
}

/**
 * Helper to repeat a string
 */
string RepeatString(string value, int count)
{
    if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
    if (count == 0 || string.IsNullOrEmpty(value)) return string.Empty;
    return new StringBuilder(value.Length * count).Insert(0, value, count).ToString();
}

/**
 * Check if a directory contains any target files (recursively) - used for pre-filtering in GetDirectoryStructureAsync
 */
async Task<bool> HasTargetFilesRecursiveCheckAsync(string dirPath)
{
    DirectoryInfo currentDirectoryInfo;
    try
    {
        currentDirectoryInfo = new DirectoryInfo(dirPath);
        if (!currentDirectoryInfo.Exists) return false;
    }
    catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
    {
        // Console.Error.WriteLine($"Debug (HasTargetFilesRecursiveCheckAsync Dir): Access or IO error for {dirPath} - {ex.Message}");
        return false; // Cannot access or doesn't exist, so treat as no target files
    }

    try
    {
        foreach (var file in currentDirectoryInfo.EnumerateFiles())
        {
            if (IsIncludedFile(file.Name))
            {
                return true;
            }
        }
    }
    catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
    {
        // Console.Error.WriteLine($"Debug (HasTargetFilesRecursiveCheckAsync Files): Access or IO error for files in {dirPath} - {ex.Message}");
        // Continue to check subdirectories even if files in current are inaccessible
    }


    try
    {
        foreach (var subDir in currentDirectoryInfo.EnumerateDirectories())
        {
            if (!ShouldExcludeDirectory(subDir.Name))
            {
                if (await HasTargetFilesRecursiveCheckAsync(subDir.FullName))
                {
                    return true;
                }
            }
        }
    }
    catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
    {
        // Console.Error.WriteLine($"Debug (HasTargetFilesRecursiveCheckAsync SubDirs): Access or IO error for subdirs in {dirPath} - {ex.Message}");
        // If subdirectories are inaccessible, cannot confirm target files there
    }

    return false;
}


/**
 * Generate a formatted string of the directory structure
 */
async Task<string> GetDirectoryStructureAsync(string startPath)
{
    var structureLines = new List<string>();

    async Task TraverseAsync(string currentPath, int level)
    {
        DirectoryInfo currentDirInfo;
        try
        {
            currentDirInfo = new DirectoryInfo(currentPath);
            if (!currentDirInfo.Exists) return;
        }
        catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
        {
            Console.Error.WriteLine($"Warning (GetDirectoryStructureAsync Traverse): Cannot access directory {currentPath}. Skipping. Error: {ex.Message}");
            return;
        }

        List<FileSystemInfo> fileSystemInfos;
        try
        {
            fileSystemInfos = currentDirInfo.EnumerateFileSystemInfos().ToList();
        }
        catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
        {
            Console.Error.WriteLine($"Warning (GetDirectoryStructureAsync Traverse): Cannot enumerate items in {currentPath}. Skipping. Error: {ex.Message}");
            return;
        }


        var indent = RepeatString("  ", level);

        bool hasTargetFilesInCurrentDir = false;
        try
        {
            foreach (var item in fileSystemInfos.OfType<FileInfo>())
            {
                if (IsIncludedFile(item.Name))
                {
                    hasTargetFilesInCurrentDir = true;
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
        {
             Console.Error.WriteLine($"Warning (GetDirectoryStructureAsync Traverse): Error checking files in {currentPath}. Error: {ex.Message}");
        }


        bool subDirHasTargetFiles = false;
        if (!hasTargetFilesInCurrentDir)
        {
            foreach (var item in fileSystemInfos.OfType<DirectoryInfo>())
            {
                if (!ShouldExcludeDirectory(item.Name))
                {
                    if (await HasTargetFilesRecursiveCheckAsync(item.FullName))
                    {
                        subDirHasTargetFiles = true;
                        break;
                    }
                }
            }
        }

        if (hasTargetFilesInCurrentDir || subDirHasTargetFiles)
        {
            var folderName = currentDirInfo.Name;
            if (level == 0)
            {
                structureLines.Add($"Root directory: {folderName}");
            }
            else
            {
                structureLines.Add($"{indent}└── {folderName}");
            }

            fileSystemInfos.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            foreach (var item in fileSystemInfos)
            {
                if (item is FileInfo fileInfo && IsIncludedFile(fileInfo.Name))
                {
                    structureLines.Add($"{indent}  ├── {fileInfo.Name}");
                }
                else if (item is DirectoryInfo dirInfo && !ShouldExcludeDirectory(dirInfo.Name))
                {
                    await TraverseAsync(dirInfo.FullName, level + 1);
                }
            }
        }
    }

    await TraverseAsync(startPath, 0);
    return string.Join("\n", structureLines);
}


/**
 * Read and return the contents of a file
 */
async Task<string> ReadFileContentsAsync(string filepath)
{
    Console.Error.WriteLine($"DEBUG: Attempting to read file: '{filepath}'"); // Log attempt
    try
    {
        string fileContent = await File.ReadAllTextAsync(filepath);
        Console.Error.WriteLine($"DEBUG: Successfully read file: '{filepath}'. Content length: {fileContent.Length}. First 50 chars: '{fileContent.Substring(0, Math.Min(fileContent.Length, 50))}'"); // Log success and snippet
        return fileContent;
    }
    catch (FileNotFoundException)
    {
        Console.Error.WriteLine($"DEBUG: File not found: '{filepath}'");
        return "[File not found]";
    }
    catch (IOException ex)
    {
        Console.Error.WriteLine($"DEBUG: IOException for '{filepath}': {ex.Message}");
        return $"[Error reading file: {ex.Message}]";
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.Error.WriteLine($"DEBUG: UnauthorizedAccessException for '{filepath}': {ex.Message}");
        return $"[Access denied reading file: {ex.Message}]";
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"DEBUG: Unexpected error for '{filepath}': {ex.GetType().Name} - {ex.Message}"); // Log other errors
        Console.Error.WriteLine($"DEBUG: StackTrace: {ex.StackTrace}");
        return $"[Unexpected error reading file: {ex.Message}]";
    }
}

/**
 * Recursively collect all target files, respecting excluded directories.
 */
async Task CollectTargetFilesRecursivelyAsync(string currentPath, List<string> collectedFiles)
{
    DirectoryInfo currentDirectoryInfo;
    try
    {
        currentDirectoryInfo = new DirectoryInfo(currentPath);
        if (!currentDirectoryInfo.Exists) return;
    }
    catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
    {
        Console.Error.WriteLine($"Warning (CollectTargetFilesRecursivelyAsync): Cannot access directory {currentPath}. Skipping. Error: {ex.Message}");
        return;
    }

    // Process files
    try
    {
        foreach (var file in currentDirectoryInfo.EnumerateFiles())
        {
            if (IsIncludedFile(file.Name))
            {
                collectedFiles.Add(file.FullName);
            }
        }
    }
    catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
    {
         Console.Error.WriteLine($"Warning (CollectTargetFilesRecursivelyAsync): Cannot enumerate files in {currentPath}. Skipping files in this directory. Error: {ex.Message}");
    }

    // Process subdirectories
    try
    {
        foreach (var subDir in currentDirectoryInfo.EnumerateDirectories())
        {
            if (!ShouldExcludeDirectory(subDir.Name))
            {
                await CollectTargetFilesRecursivelyAsync(subDir.FullName, collectedFiles);
            }
        }
    }
     catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
    {
         Console.Error.WriteLine($"Warning (CollectTargetFilesRecursivelyAsync): Cannot enumerate subdirectories of {currentPath}. Skipping subdirectories. Error: {ex.Message}");
    }
}

/**
 * Get all target files paths (recursively)
 */
async Task<(bool HasFiles, List<string> FoundFiles)> GetAllTargetFilesAsync(string dirPath)
{
    var allFoundFiles = new List<string>();
    await CollectTargetFilesRecursivelyAsync(dirPath, allFoundFiles);
    allFoundFiles.Sort(StringComparer.OrdinalIgnoreCase); // Sort for consistent output
    return (allFoundFiles.Any(), allFoundFiles);
}


/**
 * Generate the complete project sync output
 */
async Task<string> GenerateProjectSyncAsync(string directoryPath)
{
    var outputLines = new List<string>();

    var (hasAnyFiles, targetFiles) = await GetAllTargetFilesAsync(directoryPath);

    if (!hasAnyFiles)
    {
        return "No files matching the specified extensions were found in accessible directories.";
    }

    outputLines.Add(RepeatString("=", 80));
    outputLines.Add("DIRECTORY STRUCTURE");
    outputLines.Add(RepeatString("=", 80));
    outputLines.Add(await GetDirectoryStructureAsync(directoryPath));
    outputLines.Add("\n");

    outputLines.Add(RepeatString("=", 80));
    outputLines.Add("FILE CONTENTS");
    outputLines.Add(RepeatString("=", 80));

    foreach (var fullPath in targetFiles)
    {
        string relativePath;
        try
        {
            relativePath = Path.GetRelativePath(directoryPath, fullPath);
        }
        catch (ArgumentException) // Can happen if paths are on different drives on Windows, etc.
        {
            relativePath = fullPath; // Fallback to full path
        }


        outputLines.Add(RepeatString("-", 80));
        outputLines.Add($"File: {relativePath}");
        outputLines.Add(RepeatString("-", 80));

        var content = await ReadFileContentsAsync(fullPath);
        outputLines.Add(content);
        outputLines.Add("\n");
    }

    return string.Join("\n", outputLines);
}

// --- Main Execution ---
try
{
    string currentDir = "";
    try
    {
        currentDir = Directory.GetCurrentDirectory();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Fatal Error: Could not get current directory. {ex.Message}");
        Environment.Exit(1);
    }

    Console.WriteLine($"Processing directory: {currentDir}");
    var outputContent = await GenerateProjectSyncAsync(currentDir);

    var outputFile = "project_sync.txt";
    try
    {
        await File.WriteAllTextAsync(outputFile, outputContent);
        Console.WriteLine($"Project sync has been written to {Path.Combine(currentDir, outputFile)}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Fatal Error: Could not write to output file {outputFile}. {ex.Message}");
        Environment.Exit(1);
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
    Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");
    Environment.Exit(1);
}