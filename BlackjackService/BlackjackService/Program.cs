using BlackjackCommon.Interfaces;
using BlackjackCommon.Interfaces.Logic;
using BlackjackLogic;
using BlackjackService;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
	public static void Main(string[] args)
	{
		var serviceCollection = new ServiceCollection();

		serviceCollection.AddScoped<IChatLogic, ChatLogic>();
		serviceCollection.AddScoped<IGroupLogic, GroupLogic>();
		serviceCollection.AddScoped<IPlayerLogic, PlayerLogic>();
		serviceCollection.AddScoped<IGameLogic, GameLogic>();

		serviceCollection.AddScoped<IWebsocket, Websocket>();

		var serviceProvider = serviceCollection.BuildServiceProvider();

		var chatLogic = serviceProvider.GetService<IChatLogic>();
		var groupLogic = serviceProvider.GetService<IGroupLogic>();
		var playerLogic = serviceProvider.GetService<IPlayerLogic>();
		var gameLogic = serviceProvider.GetService<IGameLogic>();

		var websocket = serviceProvider.GetService<IWebsocket>();

		// Run the websocket service
		websocket.Run().Wait();
	}
}