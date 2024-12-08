using BlackjackCommon.Interfaces.Logic;
using BlackjackLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackjackAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ReplayController : ControllerBase
	{
		private readonly IReplayLogic _replayLogic;

		public ReplayController(IReplayLogic ReplayLogic)
		{
			_replayLogic = ReplayLogic;
		}

		[Authorize]
		[HttpGet]
		[Route("")]
		public IActionResult RetrieveReplayList()
		{
			return Ok();
		}

		[Authorize]
		[HttpGet]
		[Route("{group_id}")]
		public async Task<IActionResult> RetrieveReplay(string group_id)
		{
			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { Chat = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{
				var response = await _replayLogic.RetrieveReplayAsync(user_id, group_id);

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

	}
}
