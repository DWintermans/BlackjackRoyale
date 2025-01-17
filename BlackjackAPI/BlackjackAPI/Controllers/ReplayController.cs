// <copyright file="ReplayController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BlackjackAPI.Controllers
{
    using BlackjackLogic;
    using BlackjackLogic.Interfaces.Logic;
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

        /// <summary>
        /// Retrieves the list of replay data for the authenticated user.
        /// </summary>
        /// <response code="200">Returns the list of replays if successful.</response>
        /// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
        /// <response code="404">Not Found. Indicates that the replays could not be retrieved successfully.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieves replay data for a specific group based on the provided group ID.
        /// </summary>
        /// <param name="group_id">The ID of the group whose replay data is being requested.</param>
        /// <response code="200">Returns the replay data if successful.</response>
        /// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
        /// <response code="404">Not Found. Indicates that the replay data could not be retrieved successfully.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        /// <returns></returns>
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
