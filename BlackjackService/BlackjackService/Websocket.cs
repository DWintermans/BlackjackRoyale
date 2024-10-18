using BlackjackCommon.Data.SharedData;
using BlackjackCommon.Interfaces;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using BlackjackLogic;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace BlackjackService;

internal class Websocket : IWebsocket
{
	private readonly IChatLogic _chatLogic;
	private readonly IGroupLogic _groupLogic;
	private readonly IGameLogic _gameLogic;
	private readonly IPlayerLogic _playerLogic;

	public Websocket(IChatLogic chatLogic, IGroupLogic groupLogic, IGameLogic gameLogic, IPlayerLogic playerLogic)
	{
		_chatLogic = chatLogic;
		_groupLogic = groupLogic;
		_gameLogic = gameLogic;
		_playerLogic = playerLogic;

		SubscribeToChatEvents();
		SubscribeToGroupEvents();
		SubscribeToGameEvents();
	}

	#region event subscriptions
	private void SubscribeToChatEvents()
	{
		_chatLogic.OnNotification += HandleNotification;
		_chatLogic.OnMessage += HandleMessage;
		_chatLogic.OnPrivateMessage += HandlePrivateMessage; 
	}

	private void SubscribeToGroupEvents()
	{
		_groupLogic.OnNotification += HandleNotification;
		_groupLogic.OnGroupNotification += HandleGroupNotification;
		_groupLogic.OnGroupInfoToPlayer += HandleGroupInfoToPlayer;
		_groupLogic.OnLobbyInfoToPlayer += HandleLobbyInfoToPlayer;
	}

	private void SubscribeToGameEvents()
	{
		_gameLogic.OnNotification += HandleNotification;
		_gameLogic.OnGroupNotification += HandleGroupNotification;
		_gameLogic.OnGameInfoToGroup += HandleGameInfoToGroup;
		_gameLogic.OnGameInfoToPlayer += HandleGameInfoToPlayer;
	}
	#endregion

	#region event handlers
	private async Task HandleGameInfoToPlayer(Player player, GameModel gameModel)
	{
		await SendGameInfoToPlayer(player, gameModel);
	}

	private async Task HandleGameInfoToGroup(Group group, GameModel gameModel)
	{
		await SendGameInfoToGroup(group, gameModel);
	}

	private async Task HandleGroupInfoToPlayer(Player player, GroupModel groupModel)
	{
		await SendGroupInfoToPlayer(player, groupModel);
	}

	private async Task HandleLobbyInfoToPlayer(Player player, LobbyModel lobbyModel)
	{
		await SendLobbyInfoToPlayer(player, lobbyModel);
	}

	private async Task HandleNotification(Player player, string message, NotificationType notificationType, ToastType? toastType)
	{
		await SendNotificationToPlayer(player, message, notificationType, toastType);
	}
	
	private async Task HandleGroupNotification(Group group, string message, NotificationType notificationType, ToastType? toastType)
	{
		await SendNotificationToGroup(group, message, notificationType, toastType);
	}

	private async Task HandleMessage(Player player, int receiver_id, string message, MessageType type)
	{
		await SendChatMessageToPlayer(player, receiver_id, message, type);
	}

	private async Task HandlePrivateMessage(Player player, int receiver_id, string message)
	{
		await SendPrivateChatMessageToPlayer(player, receiver_id, message);
	}
	#endregion

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

	private async void ProcessRequest(HttpListenerContext context)
	{
		WebSocket socket = (await context.AcceptWebSocketAsync(null)).WebSocket;
		string client_id = Guid.NewGuid().ToString();
		connectedClients.Add(client_id, socket);

		Console.WriteLine("WebSocket connection established for client ID: " + client_id);

		byte[] buffer = new byte[1024];
		while (socket.State == WebSocketState.Open)
		{
			try
			{
				WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

				//Console.WriteLine("Received message: " + receivedMessage);

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
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}

	private async Task RouteMessage(string receivedMessage, string client_id, WebSocket socket)
	{
		dynamic message;

		try
		{
			message = JsonConvert.DeserializeObject(receivedMessage);
		}
		catch (Exception e)
		{
			Console.WriteLine($"Deserialization error: {e.Message}");

			await SendNotificationToSocket(socket, "An error occurred, please try again later.");
			return;
		}

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

		string token = message.token.ToString();

		//check if valid user_id/name is present
		(int user_id, string user_name) = GetUserInfoFromJWT(token);
		if (user_id <= 0 || user_name == null)
		{
			await SendNotificationToSocket(socket, "Invalid or expired token");
			return;
		}

		//Player player = SharedData.TryGetExistingPlayer(user_id) ?? new Player(user_id, user_name);
		Player player;

		if (SharedData.TryGetExistingPlayer(user_id) != null)
		{
			player = SharedData.TryGetExistingPlayer(user_id);
		}
		else
		{
			player = new Player(user_id, user_name);
			_playerLogic.RetrieveCredits(player);
		}

		if (!SharedData.Players.ContainsKey(user_id))
		{
			SharedData.Players[user_id] = player;
		}

		switch (message.category.ToString())
		{
			case "acknowledge":
				Link_UserID_To_WebsocketID(player, client_id);
				break;

			case "chat":
				await _chatLogic.HandleChatAction(player, message);
				break;

			case "group":
				await _groupLogic.HandleGroupAction(player, message);
				break;

			case "game":
				await _gameLogic.HandleGameAction(player, message);
				break;

			default:
				await SendNotificationToPlayer(player, "Unknown category", NotificationType.TOAST, ToastType.ERROR);
				break;
		}
	}

	public async Task SendNotificationToSocket(WebSocket socket, string message)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(message);
		await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
	}

	public async Task SendNotificationToPlayer(Player player, string message, NotificationType type, ToastType? toasttype = null)
	{
		NotificationModel notificationModel = new NotificationModel
		{
			Type = type,
			Message = message,
			ToastType = toasttype,
		};

		//convert emuns to strings e.g. CARD_DRAWN instead of 0
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(notificationModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		if (SharedData.userIDToCliendIdMap.TryGetValue(player.User_ID.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
		{
			await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	public async Task SendNotificationToGroup(Group group, string message, NotificationType type, ToastType? toasttype = null)
	{
		NotificationModel notificationModel = new NotificationModel
		{
			Type = type,
			Message = message,
			ToastType = toasttype,
		};

		//convert emuns to strings e.g. CARD_DRAWN instead of 0
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(notificationModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		foreach (Player player in group.Members)
		{
			if (SharedData.userIDToCliendIdMap.TryGetValue(player.User_ID.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
			{
				await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
			}
		}
	}

	public async Task SendPrivateChatMessageToPlayer(Player player, int receiver_id, string message)
	{
		MessageModel messageModel = new MessageModel
		{
			Type = MessageType.PRIVATE,
			SenderName = player.Name,
			Sender = player.User_ID,
			Receiver = receiver_id,
			Message = message,
			Datetime = DateTime.Now
		};

		//convert emuns to strings e.g. CARD_DRAWN instead of 0
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(messageModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		//send message to receiver when connected
		if (SharedData.userIDToCliendIdMap.TryGetValue(receiver_id.ToString(), out string receiver_client_id) && connectedClients.TryGetValue(receiver_client_id, out WebSocket receiverSocket))
		{
			await receiverSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}

		//send message to back sender
		if (SharedData.userIDToCliendIdMap.TryGetValue(player.User_ID.ToString(), out string sender_client_id) && connectedClients.TryGetValue(sender_client_id, out WebSocket senderSocket))
		{
			await senderSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	public async Task SendChatMessageToPlayer(Player player, int receiver_id, string message, MessageType type)
	{
		MessageModel messageModel = new MessageModel
		{
			Type = type,
			SenderName = player.Name,
			Sender = player.User_ID,
			Receiver = receiver_id,
			Message = message,
			Datetime = DateTime.Now
		};

		//convert emuns to strings e.g. CARD_DRAWN instead of 0
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(messageModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		//send message to receiver when connected
		if (SharedData.userIDToCliendIdMap.TryGetValue(receiver_id.ToString(), out string receiver_client_id) && connectedClients.TryGetValue(receiver_client_id, out WebSocket receiverSocket))
		{
			await receiverSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	public async Task SendGameInfoToGroup(Group group, GameModel gameModel)
	{
		//convert emuns to strings e.g. CARD_DRAWN instead of 0
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(gameModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		foreach (Player player in group.Members)
		{
			if (SharedData.userIDToCliendIdMap.TryGetValue(player.User_ID.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
			{
				await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
			}
		}
	}

	//send credits update to specific player only.
	public async Task SendGameInfoToPlayer(Player player, GameModel gameModel)
	{
		//convert emuns to strings e.g. CARD_DRAWN instead of 0
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(gameModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		if (SharedData.userIDToCliendIdMap.TryGetValue(player.User_ID.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
		{
			await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	public async Task SendGroupInfoToPlayer(Player player, GroupModel groupModel)
	{
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(groupModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		if (SharedData.userIDToCliendIdMap.TryGetValue(player.User_ID.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
		{
			await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	public async Task SendLobbyInfoToPlayer(Player player, LobbyModel lobbyModel)
	{
		var settings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter() }
		};

		string Message = JsonConvert.SerializeObject(lobbyModel, settings);
		byte[] bytes = Encoding.UTF8.GetBytes(Message);

		if (SharedData.userIDToCliendIdMap.TryGetValue(player.User_ID.ToString(), out string client_id) && connectedClients.TryGetValue(client_id, out WebSocket socket))
		{
			await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
	}

	private static async void Link_UserID_To_WebsocketID(Player player, string client_id)
	{
		SharedData.userIDToCliendIdMap[player.User_ID.ToString()] = client_id;
		Console.WriteLine("Associating sender: " + player.User_ID + " with client ID: " + client_id);

		foreach (KeyValuePair<string, string> item in SharedData.userIDToCliendIdMap)
		{
			Console.Write("Connected clients:");
			Console.WriteLine("USER_ID: " + item.Key + ", CLIENT_ID: " + item.Value);
		}
	}

	private static (int user_id, string user_name) GetUserInfoFromJWT(string token)
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
			Int32.TryParse(value, out user_id);

			string user_name = claimsPrincipal.FindFirst("user_name")?.Value;


			return (user_id, user_name);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Token validation failed: " + ex.Message);
			return (0, null);
		}
	}
}
