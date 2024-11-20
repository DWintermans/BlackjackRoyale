using BlackjackCommon.Interfaces;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;
using BlackjackDAL.Repositories;
using BlackjackLogic;
using BlackjackService;
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

			serviceCollection.AddScoped<IChatLogic, ChatLogic>();
			serviceCollection.AddScoped<IGroupLogic, GroupLogic>();
			serviceCollection.AddScoped<IPlayerLogic, PlayerLogic>();
			serviceCollection.AddScoped<IGameLogic, GameLogic>();

			serviceCollection.AddTransient<Lazy<IGroupLogic>>(provider => new Lazy<IGroupLogic>(() => provider.GetRequiredService<IGroupLogic>()));
			serviceCollection.AddTransient<Lazy<IPlayerLogic>>(provider => new Lazy<IPlayerLogic>(() => provider.GetRequiredService<IPlayerLogic>()));

			serviceCollection.AddScoped<IWebsocket, Websocket>();

			serviceCollection.AddScoped<IUserRepository, UserRepository>();


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

		string fallbackDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "BlackjackService", "BlackjackService");
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