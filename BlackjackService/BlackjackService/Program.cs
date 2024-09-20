using BlackjackService;

internal class Program
{
	public static void Main(string[] args)
	{
		Websocket websocket = new Websocket();
		websocket.Run().Wait();
	}
}