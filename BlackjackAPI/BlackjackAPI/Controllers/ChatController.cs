using BlackjackCommon.Interfaces.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackjackAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ChatController : ControllerBase
	{
		private readonly IChatLogic _chatLogic;

		public ChatController(IChatLogic ChatLogic)
		{
			_chatLogic = ChatLogic;
		}

		/// <summary>
		/// Retrieves the list of messages exchanged between the user and others, returning the latest message from each conversation.
		/// </summary>
		/// <returns>
		/// Returns a list of messages where each entry represents the last message exchanged between the user and another user. If no messages are found, a 404 Not Found response is returned.
		/// </returns>
		/// <response code="200">If messages are found, returns a list of the latest messages exchanged between the authenticated user and others.</response>
		/// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
		/// <response code="404">If no messages are found for the authenticated user.</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request (e.g., database failure).</response>
		[Authorize]
		[HttpGet]
		[Route("")]
		public IActionResult RetrieveMessageList()
		{
			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { Chat = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{
				var response = _chatLogic.RetrieveMessageList(user_id);

				//no data but did succesfully retrieve.
				if (response.Data == null)
				{
					return Ok(new { Message = response.Message });
				}

				//failed to retrieve due to issue
				if (!response.Success)
				{
					return NotFound(new { Message = response.Message });
				}

				return Ok(new { Messages = response.Data });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { Message = "An error occurred while processing your request." });
			}
		}

		/// <summary>
		/// Retrieves the messages exchanged between the user and another user.
		/// </summary>
		/// <returns>
		/// Returns a list of messages sent between the user and others.
		/// </returns>
		/// <response code="200">If messages are found, returns a list of the latest messages exchanged between the authenticated user and others.</response>
		/// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
		/// <response code="404">If no messages are found for the authenticated user.</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request (e.g., database failure).</response>
		[Authorize]
		[HttpGet]
		[Route("messages/{other_user_id}")]
		public IActionResult RetrieveMessages(int other_user_id)
		{
			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { Chat = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{
				var response = _chatLogic.RetrievePrivateMessages(user_id, other_user_id);

				if (!response.Success)
				{
					return NotFound(new { Message = response.Message });
				}

				return Ok(new { response });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { Message = "An error occurred while processing your request." });
			}

		}
	}
}
