using System;
using System.IO;
using System.Diagnostics;
using System.Text.Json;


private static readonly string LocalAgentPath = @"C:\Main\Tools\LocalMultiplayerAgent";
private static readonly string ServerProjectPath = @"project-axiom-server";
private static readonly string SharedProjectPath = @"Shared";


try
{
  Console.WriteLine("🚀 Building and deploying project-axiom-server to LocalMultiplayerAgent...\n");

  // Step 1: Clean and build the server project
  BuildServerProject();

  // Step 2: Copy server files to LocalMultiplayerAgent directory
  CopyServerFiles();

  // Step 3: Create or update MultiplayerSettings.json
  CreateMultiplayerSettings();

  Console.WriteLine("✅ Deployment complete!");
  Console.WriteLine($"📁 Server files copied to: {LocalAgentPath}");
  Console.WriteLine("🎮 You can now run LocalMultiplayerAgent.exe to start testing.");
  Console.WriteLine("\n💡 To debug:");
  Console.WriteLine("   1. Run LocalMultiplayerAgent.exe");
  Console.WriteLine("   2. In VS Code: Ctrl+Shift+P → 'Debug: Attach to Process'");
  Console.WriteLine("   3. Select 'project-axiom-server.exe'");
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Error: {ex.Message}");
  Environment.Exit(1);
}
    
    
    private static void BuildServerProject()
{
  Console.WriteLine("🔨 Building server project...");

  // Build in Release mode for better performance
  var buildProcess = Process.Start(new ProcessStartInfo
  {
    FileName = "dotnet",
    Arguments = $"build {ServerProjectPath} --configuration Release --verbosity minimal",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true
  });

  buildProcess?.WaitForExit();

  if (buildProcess?.ExitCode != 0)
  {
    throw new Exception("Failed to build server project. Check build output for errors.");
  }

  Console.WriteLine("✅ Build successful");
}

private static void CopyServerFiles()
{
  Console.WriteLine("📦 Copying server files...");

  var sourceBinPath = Path.Combine(ServerProjectPath, "bin", "Release", "net8.0");

  if (!Directory.Exists(sourceBinPath))
  {
    throw new Exception($"Build output not found at: {sourceBinPath}");
  }

  // Ensure LocalMultiplayerAgent directory exists
  Directory.CreateDirectory(LocalAgentPath);

  // Copy all files from the build output
  CopyDirectory(sourceBinPath, LocalAgentPath, overwrite: true);

  Console.WriteLine($"✅ Files copied from {sourceBinPath} to {LocalAgentPath}");
}

private static void CreateMultiplayerSettings()
{
  Console.WriteLine("⚙️ Creating MultiplayerSettings.json...");

  var settings = new
  {
    RunContainer = false,
    OutputFolder = Path.Combine(LocalAgentPath, "output"),
    NumHeartBeatsForActivateResponse = 10,
    NumHeartBeatsForTerminateResponse = 10,
    TitleId = "",
    BuildId = "00000000-0000-0000-0000-000000000000",
    Region = "LocalTestRegion",
    ProcessStartParameters = new
    {
      StartGameCommand = "project-axiom-server.exe",
      Arguments = "--debug"
    }
  };

  var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
  {
    WriteIndented = true
  });

  var settingsPath = Path.Combine(LocalAgentPath, "MultiplayerSettings.json");
  File.WriteAllText(settingsPath, json);

  // Create output directory
  Directory.CreateDirectory(Path.Combine(LocalAgentPath, "output"));

  Console.WriteLine($"✅ MultiplayerSettings.json created at {settingsPath}");
}

private static void CopyDirectory(string sourceDir, string destDir, bool overwrite = false)
{
  var dir = new DirectoryInfo(sourceDir);

  if (!dir.Exists)
  {
    throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
  }

  // Copy files
  foreach (var file in dir.GetFiles())
  {
    var destPath = Path.Combine(destDir, file.Name);
    file.CopyTo(destPath, overwrite);
  }

  // Copy subdirectories
  foreach (var subDir in dir.GetDirectories())
  {
    var destPath = Path.Combine(destDir, subDir.Name);
    Directory.CreateDirectory(destPath);
    CopyDirectory(subDir.FullName, destPath, overwrite);
  }


}