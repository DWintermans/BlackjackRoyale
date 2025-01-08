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

		/// <summary>
		/// Retrieves statistics data for the authenticated user.
		/// </summary>
		/// <response code="200">Returns the statistics data if successful.</response>
		/// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
		/// <response code="404">Not Found. Indicates that the statistics could not be retrieved successfully.</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		/// <returns></returns>
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

		/// <summary>
		/// Retrieves the leaderboard data.
		/// </summary>
		/// <response code="200">Returns the leaderboard data if successful.</response>
		/// <response code="404">Not Found. Indicates that the leaderboard data could not be retrieved successfully.</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		/// <returns></returns>
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
