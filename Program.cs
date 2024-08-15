using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rhacktool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: rhacktool <command> [options]");
                return;
            }

            string commandName = args[0];
            var commandArgs = ParseArguments(args.Skip(1).ToArray());

            ICommand command = CommandFactory.CreateCommand(commandName, commandArgs);
            await command.ExecuteAsync();
        }

        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    string key = args[i];
                    string value = i + 1 < args.Length && !args[i + 1].StartsWith("--") ? args[i + 1] : null;
                    result[key] = value;
                }
            }
            return result;
        }
    }
}
