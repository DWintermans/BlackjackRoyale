using BlackjackCommon.Entities.User;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlackjackDAL
{
    public class DBConnection
    {
        static DBConnection()
        {
            Env.Load();
        }

        private const string _SERVER = "DB_SERVER";
        private const string _USER = "DB_USER";
        private const string _PASSWORD = "DB_PASSWORD";
        private const string _DATABASE = "DB_DATABASE";

        public string ConnectionString()
        {
            string server = Env.GetString(_SERVER);
            string user = Env.GetString(_USER);
            string password = Env.GetString(_PASSWORD);
            string database = Env.GetString(_DATABASE);

            try
            {
                LogToFile($"Server={server};Database={database};User={user};Password={password};");
                return $"Server={server};Database={database};User={user};Password={password};";
            }
            catch
            {
                LogToFile("Unknown DB type");
                throw new InvalidOperationException("Unknown DB.");
            }
        }

        private void LogToFile(string errormsg)
        {
            string logFilePath = "app-log.txt";
            string logMessage = $"{DateTime.UtcNow}: {errormsg}{Environment.NewLine}";

            System.IO.File.AppendAllText(logFilePath, logMessage);
        }

    }
}
