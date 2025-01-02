// <copyright file="StatisticsController.cs" company="PlaceholderCompany">
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
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsLogic statisticsLogic;

        public StatisticsController(IStatisticsLogic statisticsLogic)
        {
            this.statisticsLogic = statisticsLogic;
        }

        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> RetrieveStatistics()
        {
			var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return this.Unauthorized(new { Chat = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{
				var response = await this.statisticsLogic.RetrieveStatisticsAsync(user_id);

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

		[HttpGet]
		[Route("leaderboard")]
		public async Task<IActionResult> RetrieveLeaderboard()
		{
			try
			{
				var response = await this.statisticsLogic.RetrieveLeaderboardAsync();

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
