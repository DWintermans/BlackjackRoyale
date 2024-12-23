// <copyright file="ReplayController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BlackjackAPI.Controllers
{
    using BlackjackCommon.Interfaces.Logic;
    using BlackjackLogic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class ReplayController : ControllerBase
    {
        private readonly IReplayLogic replayLogic;

        public ReplayController(IReplayLogic replayLogic)
        {
            this.replayLogic = replayLogic;
        }

        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> RetrieveReplayList()
        {
			var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return this.Unauthorized(new { Chat = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{
				var response = await this.replayLogic.RetrieveReplayListAsync(user_id);

				if (!response.Success)
				{
					return this.NotFound(new { Message = response.Message });
				}

				return this.Ok(new { Messages = response.Data });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return this.StatusCode(500, new { Message = "An error occurred while processing your request." });
			}
		}

        [Authorize]
        [HttpGet]
        [Route("{group_id}")]
        public async Task<IActionResult> RetrieveReplay(string group_id)
        {
            var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

            if (jwt_user_id == null)
            {
                return this.Unauthorized(new { Chat = "Invalid credentials" });
            }

            int user_id = int.Parse(jwt_user_id.Value);

            try
            {
                var response = await this.replayLogic.RetrieveReplayAsync(user_id, group_id);

                if (!response.Success)
                {
                    return this.NotFound(new { Message = response.Message });
                }

                return this.Ok(new { Messages = response.Data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { Message = "An error occurred while processing your request." });
            }
        }
    }
}
