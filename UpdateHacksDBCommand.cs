using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using MySql.Data.MySqlClient;
using rhacktool.utils;

namespace rhacktool
{
    public class UpdateHacksDbCommand : ICommand
    {
        private readonly string _connectionString;
        private readonly string _hacksPath;
        private readonly string _logFilePath;

        public UpdateHacksDbCommand(string hacksPath, string logFilePath, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
            if (string.IsNullOrEmpty(hacksPath))
                throw new ArgumentNullException(nameof(hacksPath), "Hacks path cannot be null or empty.");
            if (string.IsNullOrEmpty(logFilePath))
                throw new ArgumentNullException(nameof(logFilePath), "Log file path cannot be null or empty.");

            _connectionString = connectionString;
            _hacksPath = hacksPath;
            _logFilePath = logFilePath;

            // Ensure the directory for the log file exists
            string logDirectory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public async Task ExecuteAsync()
        {
            string query = @"
            SELECT h.hackkey, h.filename, c.consolekey
            FROM Hacks h
            JOIN console c ON h.consolekey = c.consoleid";

            List<(int hackkey, string oldFileName, string consolekey)> hackInfoList = new List<(int, string, string)>();

            // Read the data first
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand command = new MySqlCommand(query, connection))
                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int hackkey = (int)reader["hackkey"];
                        string oldFileName = reader["filename"].ToString();
                        string consolekey = reader["consolekey"].ToString();

                        hackInfoList.Add((hackkey, oldFileName, consolekey));
                    }
                }
            }

            // Update the database with the new filenames
            foreach (var (hackkey, oldFileName, consolekey) in hackInfoList)
            {
                string consoleDirectory = Path.Combine(_hacksPath, consolekey);
                string searchPattern = $"[{hackkey}] *";

                var files = Directory.EnumerateFiles(consoleDirectory, searchPattern, SearchOption.AllDirectories);

                foreach (var oldFilePath in files)
                {
                    string newFileName = Path.GetFileName(oldFilePath);

                    // Ensure the filename has the correct format
                    if (newFileName.StartsWith($"[{hackkey}]", StringComparison.OrdinalIgnoreCase))
                    {
                        string newFileNameFromPath = newFileName;

                        // Update database if the filename in the filesystem is different from the database
                        if (!newFileName.Equals(oldFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            await UpdateFileNameInDbAsync(hackkey, newFileNameFromPath);
                        }
                    }
                    else
                    {
                        Log($"Filename {newFileName} does not match expected format for hackkey {hackkey}. Skipping...");
                    }
                }
            }
        }

        private async Task UpdateFileNameInDbAsync(int hackkey, string newFileName)
        {
            string updateQuery = "UPDATE Hacks SET filename = @filename WHERE hackkey = @hackkey";

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@filename", newFileName);
                    updateCommand.Parameters.AddWithValue("@hackkey", hackkey);

                    try
                    {
                        await updateCommand.ExecuteNonQueryAsync();
                        Log($"Updated filename for hackkey {hackkey} to {newFileName}");
                    }
                    catch (Exception ex)
                    {
                        Log($"Error updating filename for hackkey {hackkey}: {ex.Message}");
                    }
                }
            }
        }

        private void Log(string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
                throw new ArgumentNullException(nameof(_logFilePath), "Log file path cannot be null or empty.");

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
