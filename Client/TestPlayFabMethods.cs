using PlayFab;
using System;
using System.Reflection;

namespace TestPlayFab
{
    public class Program
    {
        public static void Main()
        {
            // Get the PlayFabClientAPI type
            var clientApiType = typeof(PlayFab.PlayFabClientAPI);
            
            // Get all static methods
            var methods = clientApiType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            
            Console.WriteLine("Available PlayFabClientAPI methods:");
            foreach (var method in methods)
            {
                if (method.Name.Contains("Login") || method.Name.Contains("Register"))
                {
                    Console.WriteLine($"- {method.Name}");
                }
            }
        }
    }
}
