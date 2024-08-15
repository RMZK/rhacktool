using MySql.Data.MySqlClient;
using rhacktool.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace rhacktool
{
    public class RenameCommand : ICommand
    {
        private readonly string _hacksPath;
        private readonly string _connectionString;
        private readonly string _logFilePath;

        public RenameCommand(string hacksPath, string logFilePath, string connectionString)
        {
            _hacksPath = hacksPath;
            _connectionString = connectionString;
            _logFilePath = logFilePath;
        }

        public async Task ExecuteAsync()
        {
            string query = @"
            SELECT h.hackkey, h.hacktitle, h.filename, c.consolekey, g.gametitle
            FROM Hacks h
            JOIN console c ON h.consolekey = c.consoleid
            JOIN gamedata g ON h.gamekey = g.gamekey
            ORDER BY h.hackkey"; // Ensure records are ordered by hackkey

            Log("Starting the renaming process...");

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    var renameTasks = new List<Task>();

                    while (await reader.ReadAsync())
                    {
                        int hackkey = (int)reader["hackkey"];
                        string hackTitle = reader["hacktitle"].ToString();
                        string oldFileName = reader["filename"].ToString();
                        string consolekey = reader["consolekey"].ToString();
                        string gametitle = reader["gametitle"].ToString();

                        hackTitle = HttpUtility.HtmlDecode(hackTitle)
                            .Replace("'", "_")
                            .Replace("/", "_")
                            .Replace("?", "")
                            .Replace("*", "_")
                            .Replace(":", "_")
                            .Replace("\"", "_")
                            .Replace("<", "_")
                            .Replace(">", "_");

                        gametitle = HttpUtility.HtmlDecode(gametitle)
                            .Replace("'", "_")
                            .Replace("/", "_")
                            .Replace("?", "")
                            .Replace("*", "_")
                            .Replace("|", "_")
                            .Replace(":", "_");

                        string consoleDirectory = Path.Combine(_hacksPath, consolekey);
                        string searchPattern = $"[{hackkey}]*";

                        var files = Directory.EnumerateFiles(consoleDirectory, searchPattern, SearchOption.AllDirectories);

                        // Execute renaming tasks sequentially based on hackkey
                        foreach (var oldFilePath in files)
                        {
                            string extension = Path.GetExtension(oldFilePath);
                            string newFileName = CommonUtils.SanitizeFileName($"[{hackkey}] {gametitle} - {hackTitle}{extension}");
                            string newFilePath = Path.Combine(Path.GetDirectoryName(oldFilePath), newFileName);

                            if (oldFilePath.Equals(newFilePath, StringComparison.OrdinalIgnoreCase))
                            {
                                Log($"File {Path.GetFileName(oldFilePath)} is already renamed.");
                                continue;
                            }

                            if (File.Exists(newFilePath))
                            {
                                Log($"File {newFileName} already exists. Skipping...");
                                continue;
                            }

                            try
                            {
                                File.Move(oldFilePath, newFilePath);
                                Log($"Renamed {Path.GetFileName(oldFilePath)} to {newFileName}");
                            }
                            catch (Exception ex)
                            {
                                string errorMessage = $"Error renaming {Path.GetFileName(oldFilePath)} to {newFileName}: {ex.Message} - hack for {consolekey}";
                                Log(errorMessage);
                            }
                        }
                    }
                }
            }

            Log("Renaming process completed.");
        }

        private void Log(string message)
        {
            string logMessage = $"{DateTime.Now}: {message}{Environment.NewLine}";

            // Log to file
            lock (_logFilePath)
            {
                File.AppendAllText(_logFilePath, logMessage);
            }

            // Print to console
            Console.Write(logMessage);
        }
    }
}
