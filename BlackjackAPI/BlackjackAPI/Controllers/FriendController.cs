using BlackjackAPI.Models.Friend;
using BlackjackCommon.Interfaces.Logic;
using BlackjackDAL.Repositories;
using BlackjackLogic;
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
		[HttpPost]
		[Route("Request")]
		public IActionResult SendFriendRequest(RequestModel model)
		{
			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { message = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			if (user_id == model.befriend_user_id)
			{
				return BadRequest(new { message = "You can't befriend yourself." });
			}

			try
			{
				_friendLogic.RequestFriendship(user_id, model.befriend_user_id);
				return Ok(new { message = "Friend request sent successfully." });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { message = "An error occurred while processing your request." });
			}
		}

		[HttpPut]
		[Route("Request/Status")]
		public IActionResult UpdateFriendStatus(StatusModel model)
		{
			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { message = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			if (model.status != "accepted" && model.status != "rejected")
			{
				return BadRequest(new { message = "Invalid status received." });
			}

			try
			{
				_friendLogic.UpdateFriendStatus(user_id, model.friend_user_id, model.status);
				return Ok(new { message = $"Friend status successfully updated to '{model.status}'." });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { message = "An error occurred while processing your request." });
			}
		}
	}
}
