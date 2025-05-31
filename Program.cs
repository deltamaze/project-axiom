// Program.cs
using System;

try
{
    using var game = new project_axiom.Game1();
    game.Run();
}
catch (Exception ex)
{
    Console.WriteLine("🛑 AN ERROR OCCURRED:");
    Console.WriteLine(ex.ToString()); // This will print the full exception details
    // You might want to log this to a file as well in a real project
    // System.IO.File.WriteAllText("crash_log.txt", ex.ToString());
    Console.WriteLine("\nPress any key to exit.");
    Console.ReadKey(); // Keeps the console window open so you can read the error
}