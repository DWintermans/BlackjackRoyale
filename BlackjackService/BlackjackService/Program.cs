using BlackjackCommon.Interfaces;
using BlackjackCommon.Interfaces.Logic;
using BlackjackDAL;
using BlackjackLogic;
using BlackjackService;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
	public static void Main(string[] args)
	{
		//IChatLogic chatLogic = new ChatLogic();
		//IGroupLogic groupLogic = new GroupLogic();
		//IGameLogic gameLogic = new GameLogic();

		//Websocket websocket = new Websocket(chatLogic, groupLogic, gameLogic);
		//websocket.Run().Wait();


		var serviceCollection = new ServiceCollection();

		serviceCollection.AddScoped<IChatLogic, ChatLogic>(); 
		serviceCollection.AddScoped<IGroupLogic, GroupLogic>(); 
		serviceCollection.AddScoped<IGameLogic, GameLogic>();

		var serviceProvider = serviceCollection.BuildServiceProvider();

		var chatLogic = serviceProvider.GetService<IChatLogic>();
		var groupLogic = serviceProvider.GetService<IGroupLogic>();
		var gameLogic = serviceProvider.GetService<IGameLogic>();

		var websocket = serviceProvider.GetService<IWebsocket>();

		// Run the websocket service
		websocket.Run().Wait();
	}
}