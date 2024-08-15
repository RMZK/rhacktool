using System;
using System.Collections.Generic;

namespace rhacktool
{
    public static class CommandFactory
    {
        public static ICommand CreateCommand(string commandName, Dictionary<string, string> args)
        {
            string hacksPath = args.ContainsKey("--hacks-path") ? args["--hacks-path"] : "./";
            string logFilePath = args.ContainsKey("--log-file-path") ? args["--log-file-path"] : "./rhacktool.log";
            string connectionString = args.ContainsKey("--connection-string") ? args["--connection-string"] : "Server=192.168.1.56;Port=3306;Database=romhacking;User Id=root;Password=RomHacking;";

            return commandName switch
            {
                "rename" => new RenameCommand(
                    hacksPath,
                    logFilePath,
                    connectionString
                ),

                "update-hacksdb" => new UpdateHacksDbCommand(
                    hacksPath,
                    logFilePath,
                    connectionString
                ),

                _ => throw new InvalidOperationException("Unknown command")
            };
        }
    }
}
