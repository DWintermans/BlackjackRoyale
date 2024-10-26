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
}