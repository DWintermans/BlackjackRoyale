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
	private static Dictionary<string, string> userIDToCliendIdMap = new Dictionary<string, string>();

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

				string keyToRemove = userIDToCliendIdMap.FirstOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> x) => x.Value == client_id).Key;
				if (keyToRemove != null)
				{
					userIDToCliendIdMap.Remove(keyToRemove);
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
			await ReturnMessageToSender(socket, "Invalid message format");
			return;
		}

		//check if token is present
		if (string.IsNullOrEmpty(message.token.ToString()))
		{
			await ReturnMessageToSender(socket, "Missing token");
			return;
		}

		//check if valid user_id is present
		int user_id = 0;
		user_id = GetUserIDFromJWT(message.token.ToString());
		if (user_id <= 0)
		{
			await ReturnMessageToSender(socket, "Invalid or expired token");
			return;
		}

		switch (message.category.ToString())
		{
			case "acknowledge":
				Link_UserID_To_WebsocketID(user_id, client_id);
				break;

			case "chat":
				await HandleChatAction(message, user_id);
				break;

			case "group":
				await Group.HandleGroupAction(message, user_id);
				break;

			case "game":
				await HandleGameAction(message, user_id);
				break;

			default:
				await ReturnMessageToUserID(user_id, "Unknown category");
				break;
		}
	}

	public static async Task ReturnMessageToSender(WebSocket socket, string message)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(message);
		await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
	}

	public static async Task ReturnMessageToUserID(int user_id, string message)
	{
		if (userIDToCliendIdMap.TryGetValue(user_id.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}


	private static async void Link_UserID_To_WebsocketID(int user_id, string client_id)
	{
		userIDToCliendIdMap[user_id.ToString()] = client_id;
		Console.WriteLine("Associating sender: " + user_id + " with client ID: " + client_id);

		foreach (KeyValuePair<string, string> item in userIDToCliendIdMap)
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

	private static async Task HandleChatAction(dynamic message, int client_id)
	{
		if (message.action == "send_message")
		{
			string groupId = message.groupId;
			string chatMessage = message.message;

			// Broadcast message to all clients in the group
			if (groupId != null && chatMessage != null)
			{
				//await BroadcastMessageToGroupAsync(groupId, chatMessage);
			}
		}
	}

	private static async Task HandleGameAction(dynamic message, int user_id)
	{
		switch (message.action)
		{
			case "hit":
				//await ProcessHitAction(client_id);
				break;

			case "stand":
				//await ProcessStandAction(client_id);
				break;

			// Add cases for "split", "surrender", etc.

			default:
				//await SendMessageAsync(connectedClients[client_id], "Unknown game action");
				break;
		}
	}


}
