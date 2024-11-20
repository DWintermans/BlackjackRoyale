using BlackjackCommon.Interfaces.Logic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackjackAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class FriendController : ControllerBase
	{
		private readonly IFriendLogic _friendLogic;

		public FriendController(IFriendLogic friendLogic)
		{
			_friendLogic = friendLogic;
		}

		/// <summary>
		/// Request to become friends with another user.
		/// </summary>
		/// <param name="model">The request model containing the ID of the user to send the friend request to.</param>
		/// <response code="200">Friend request sent successfully.</response>
		/// <response code="400">Bad request. Occurs if the user tries to send a request to themselves or if the model is invalid.</response>
		/// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		[Authorize]
		[HttpPost]
		[Route("Request/{friend_id}")]
		public IActionResult SendFriendRequest(int friend_id)
		{
			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { message = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			if (user_id == friend_id)
			{
				return BadRequest(new { message = "You can't befriend yourself." });
			}

			try
			{
				var response = _friendLogic.RequestFriendship(user_id, friend_id);

				if (!response.Success)
				{
					return BadRequest(new { message = response.Message });
				}

				return Ok(new { message = "Friend request sent successfully." });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { message = "An error occurred while processing your request." });
			}
		}

		/// <summary>
		/// Update the status of a friend request.
		/// </summary>
		/// <param name="model">The status model containing the ID of the user and the new status ("accepted" or "rejected").</param>
		/// <response code="200">Friend status successfully updated.</response>
		/// <response code="400">Bad request. Occurs if an invalid status is provided.</response>
		/// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		[Authorize]
		[HttpPut]
		[Route("requests/{friend_id}/{status}")]
		public IActionResult UpdateFriendStatus(int friend_id, string status)
		{
			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { message = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{			
				var response = _friendLogic.UpdateFriendStatus(user_id, friend_id, status);

				if (!response.Success)
				{
					return BadRequest(new { message = response.Message });
				}

				return Ok(new { message = $"Friend status successfully updated to '{status}'." });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { message = "An error occurred while processing your request." });
			}
		}
	}
}
