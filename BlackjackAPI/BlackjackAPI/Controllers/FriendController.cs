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

		public FriendController()
		{
			_friendLogic = new FriendLogic(new FriendRepository());
		}

		/// <summary>
		/// Request to become friends with anonther user.
		/// </summary>
		[HttpPost]
		[Route("Request")]
		public IActionResult SendFriendRequest(FriendRequestModel model)
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

			//var result = _friendLogic.Request(user_id, model.befriend_user_id);

			//if (!result.Success)
			//{
			//	return BadRequest(new { message = result.Message });
			//}

			return Ok(new { message = "Friend request send." });
		}

		[HttpPut]
		[Route("Request/Accept")]
		public IActionResult AcceptFriendRequest(FriendRequestModel model)
		{
			return Ok(new { message = "Friend request accepted." });
		}

		[HttpPut]
		[Route("Request/Reject")]
		public IActionResult RejectFriendRequest(FriendRequestModel model)
		{
			return Ok(new { message = "Friend request rejecetd." });
		}
	}
}
