using BlackjackCommon.Interfaces;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;
using BlackjackDAL;
using BlackjackDAL.Repositories;
using BlackjackLogic;
using BlackjackWebsocket;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    public static void Main(string[] args)
    {
        string logFilePath = DetermineLogFilePath();

        LogToFile("STARTING NOW");

        try
        {
            LogToFile("Initializing services...");

            var serviceCollection = new ServiceCollection();

            string envPath = FindEnvFile();
            if (!string.IsNullOrEmpty(envPath))
            {
                Console.WriteLine($"Loading .env from: {envPath}");
                Env.Load(envPath);
            }
            else
            {
                Console.WriteLine("No .env file found.");
            }

            string dbServer = Env.GetString("DB_SERVER");
            string dbUser = Env.GetString("DB_USER");
            string dbPassword = Env.GetString("DB_PASSWORD");
            string dbDatabase = Env.GetString("DB_DATABASE");

            string connectionString = $"Server={dbServer};User={dbUser};Password={dbPassword};Database={dbDatabase}";
            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            serviceCollection.AddScoped<IChatLogic, ChatLogic>();
            serviceCollection.AddScoped<IGroupLogic, GroupLogic>();
            serviceCollection.AddScoped<IPlayerLogic, PlayerLogic>();
            serviceCollection.AddScoped<IGameLogic, GameLogic>();
            serviceCollection.AddScoped<IFriendLogic, FriendLogic>();
            serviceCollection.AddScoped<IReplayLogic, ReplayLogic>();
            serviceCollection.AddScoped<IStatisticsLogic, StatisticsLogic>();

            serviceCollection.AddTransient<Lazy<IGroupLogic>>(provider => new Lazy<IGroupLogic>(() => provider.GetRequiredService<IGroupLogic>()));
            serviceCollection.AddTransient<Lazy<IPlayerLogic>>(provider => new Lazy<IPlayerLogic>(() => provider.GetRequiredService<IPlayerLogic>()));

            serviceCollection.AddScoped<IWebsocket, Websocket>();

            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IChatRepository, ChatRepository>();
            serviceCollection.AddScoped<IGameRepository, GameRepository>();
            serviceCollection.AddScoped<IFriendRepository, FriendRepository>();
            serviceCollection.AddScoped<IReplayRepository, ReplayRepository>();
            serviceCollection.AddScoped<IStatisticsRepository, StatisticsRepository>();


            var serviceProvider = serviceCollection.BuildServiceProvider();

            var websocket = serviceProvider.GetService<IWebsocket>();

            // Run the websocket service
            websocket.Run().Wait();
        }
        catch (AggregateException ex)
        {
            foreach (var innerException in ex.InnerExceptions)
            {
                LogToFile(innerException);
                Console.WriteLine($"AggregateException: {innerException.Message}");
            }
        }
        catch (InvalidOperationException ex)
        {
            LogToFile(ex);
            Console.WriteLine($"InvalidOperationException: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogToFile(ex);
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

    }

    private static void LogToFile(string message)
    {
        string logFilePath = DetermineLogFilePath();
        string logMessage = $"{DateTime.UtcNow}: {message}";
        System.IO.File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }

    private static void LogToFile(Exception ex)
    {
        string logFilePath = DetermineLogFilePath();
        string logMessage = $"{DateTime.UtcNow}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
        System.IO.File.AppendAllText(logFilePath, logMessage);
    }

    private static string DetermineLogFilePath()
    {
        string envFilePath = FindEnvFile();

        if (!string.IsNullOrEmpty(envFilePath))
        {
            string envDirectory = Path.GetDirectoryName(envFilePath);
            return Path.Combine(envDirectory, "program-log.txt");
        }

        string fallbackDirectory = Path.Combine(Directory.GetCurrentDirectory(), "program-log.txt");
        Console.WriteLine($"Log file will be created in fallback location: {fallbackDirectory}");
        return fallbackDirectory;
    }

    private static string FindEnvFile()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        Console.WriteLine($"Current working directory: {currentDirectory}");

        string potentialPath = Path.Combine(currentDirectory, ".env");
        if (File.Exists(potentialPath))
        {
            Console.WriteLine($"Found .env file in current directory: {currentDirectory}");
            return potentialPath;
        }

        while (Directory.GetParent(currentDirectory) != null)
        {
            currentDirectory = Directory.GetParent(currentDirectory).FullName;
            potentialPath = Path.Combine(currentDirectory, ".env");

            if (File.Exists(potentialPath))
            {
                Console.WriteLine($"Found .env file in parent directory: {currentDirectory}");
                return potentialPath;
            }
        }

        string fallbackDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "BlackjackWebsocket", "BlackjackWebsocket");
        potentialPath = Path.Combine(fallbackDirectory, ".env");
        if (File.Exists(potentialPath))
        {
            Console.WriteLine($"Found .env file in fallback directory: {fallbackDirectory}");
            return potentialPath;
        }

        Console.WriteLine("No .env file found.");
        return null;
    }

}