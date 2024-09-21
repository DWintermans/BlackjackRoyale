using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;


namespace BlackjackService;

internal class Websocket
{
	private static Dictionary<string, WebSocket> connectedClients = new Dictionary<string, WebSocket>();

	public async Task Run()
	{
		HttpListener listener = new HttpListener();
		listener.Prefixes.Add("http://localhost:5000/ws/");
		listener.Start();
		Console.WriteLine("Listening for WebSocket connections...");
		while (true)
		{
			HttpListenerContext context = await listener.GetContextAsync();
			if (context.Request.IsWebSocketRequest)
			{
				ProcessRequest(context);
				continue;
			}
			context.Response.StatusCode = 400;
			context.Response.Close();
		}
	}

	private static async void ProcessRequest(HttpListenerContext context)
	{
		WebSocket socket = (await context.AcceptWebSocketAsync(null)).WebSocket;
		string client_id = Guid.NewGuid().ToString();
		connectedClients.Add(client_id, socket);

		Console.WriteLine("WebSocket connection established for client ID: " + client_id);

		byte[] buffer = new byte[1024];
		while (socket.State == WebSocketState.Open)
		{
			WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

			Console.WriteLine("Received message: " + receivedMessage);

			RouteMessage(receivedMessage, client_id, socket);

			if (result.MessageType == WebSocketMessageType.Close)
			{
				Console.WriteLine("WebSocket connection closed for client ID: " + client_id);

				connectedClients.Remove(client_id);

				string keyToRemove = SharedData.userIDToCliendIdMap.FirstOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Value == client_id).Key;
				if (keyToRemove != null)
				{
					SharedData.userIDToCliendIdMap.Remove(keyToRemove);
				}
				break;
			}
		}
	}

	private static async Task RouteMessage(string receivedMessage, string client_id, WebSocket socket)
	{
		dynamic message = JsonConvert.DeserializeObject(receivedMessage);

		//check if category and action are present
		if (string.IsNullOrEmpty(message.category.ToString()) && string.IsNullOrEmpty(message.action.ToString()))
		{
			await SendNotificationToSocket(socket, "Invalid message format");
			return;
		}

		//check if token is present
		if (string.IsNullOrEmpty(message.token.ToString()))
		{
			await SendNotificationToSocket(socket, "Missing token");
			return;
		}

		//check if valid user_id is present
		int user_id = 0;
		user_id = GetUserIDFromJWT(message.token.ToString());
		if (user_id <= 0)
		{
			await SendNotificationToSocket(socket, "Invalid or expired token");
			return;
		}

		switch (message.category.ToString())
		{
			case "acknowledge":
				Link_UserID_To_WebsocketID(user_id, client_id);
				break;

			case "chat":
				await Chat.HandleChatAction(message, user_id);
				break;

			case "group":
				await Group.HandleGroupAction(message, user_id);
				break;

			case "game":
				await Game.HandleGameAction(message, user_id);
				break;

			default:
				await SendNotificationToUserID(user_id, "Unknown category");
				break;
		}
	}

	public static async Task SendNotificationToSocket(WebSocket socket, string message)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(message);
		await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
	}

	public static async Task SendNotificationToUserID(int user_id, string message)
	{
		if (SharedData.userIDToCliendIdMap.TryGetValue(user_id.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	public static async Task SendNotificationToGroupID(string group_id, string message)
	{
		foreach (int user_id in SharedData.groupMembers[group_id])
		{
			if (SharedData.userIDToCliendIdMap.TryGetValue(user_id.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(message);
				await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
			}
		}
	}

	public static async Task SendPrivateChatMessageToUserID(int sender_id, int receiver_id, string message)
	{
		MessageModel messageModel = new MessageModel
		{
			Sender = sender_id,
			Receiver = receiver_id,
			Message = message,
			Datetime = DateTime.Now
		};

		string Message = JsonConvert.SerializeObject(messageModel);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		//send message to receiver when connected
		if (SharedData.userIDToCliendIdMap.TryGetValue(receiver_id.ToString(), out string receiver_client_id) && connectedClients.TryGetValue(receiver_client_id, out WebSocket receiverSocket))
		{
			await receiverSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}

		//send message to back sender
		if (SharedData.userIDToCliendIdMap.TryGetValue(sender_id.ToString(), out string sender_client_id) && connectedClients.TryGetValue(sender_client_id, out WebSocket senderSocket))
		{
			await senderSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	public static async Task SendChatMessageToUserID(int sender_id, int receiver_id, string message)
	{
		MessageModel messageModel = new MessageModel
		{
			Sender = sender_id,
			Receiver = receiver_id,
			Message = message,
			Datetime = DateTime.Now
		};

		string Message = JsonConvert.SerializeObject(messageModel);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		//send message to receiver when connected
		if (SharedData.userIDToCliendIdMap.TryGetValue(receiver_id.ToString(), out string receiver_client_id) && connectedClients.TryGetValue(receiver_client_id, out WebSocket receiverSocket))
		{
			await receiverSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	private static async void Link_UserID_To_WebsocketID(int user_id, string client_id)
	{
		SharedData.userIDToCliendIdMap[user_id.ToString()] = client_id;
		Console.WriteLine("Associating sender: " + user_id + " with client ID: " + client_id);

		foreach (KeyValuePair<string, string> item in SharedData.userIDToCliendIdMap)
		{
			Console.Write("Connected clients:");
			Console.WriteLine("USER_ID: " + item.Key + ", CLIENT_ID: " + item.Value);
		}
	}

	private static int GetUserIDFromJWT(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var validationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ah48MZ4amGS3VqakPxjsYSekeg3yar6MbirervAigfquZkcF8wSCS3VKTWMaQCMR8dSJh3McMCcoT59rUnTxqKoSyAELPRcdZVF9wtB8XxhUPpTQUA5nWoGVSfd8R4Go")),
			ValidateIssuer = false,
			ValidateAudience = false,
			ClockSkew = TimeSpan.Zero
		};

		try
		{
			ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
			string value = claimsPrincipal.FindFirst("user_id")?.Value;

			int user_id = 0;
			if (Int32.TryParse(value, out user_id))
			{
				return user_id;
			}

			return 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Token validation failed: " + ex.Message);
			return 0;
		}
	}
}
