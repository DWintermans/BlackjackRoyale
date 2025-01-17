// <copyright file="FriendController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BlackjackAPI.Controllers
{
    using BlackjackLogic.Interfaces.Logic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class FriendController : ControllerBase
    {
        private readonly IFriendLogic friendLogic;

        public FriendController(IFriendLogic friendLogic)
        {
            this.friendLogic = friendLogic;
        }

        /// <summary>
        /// Retrieves the list of pending friend requests.
        /// </summary>
        /// <response code="200">Returns the list of friend requests or a message if no data is found.</response>
        /// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
        /// <response code="404">Not Found. Indicates that friend requests could not be retrieved successfully.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("request")]
        public IActionResult GetFriendRequests()
        {
            var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

            if (jwt_user_id == null)
            {
                return this.Unauthorized(new { message = "Invalid credentials" });
            }

            int user_id = int.Parse(jwt_user_id.Value);

            try
            {
                var response = this.friendLogic.GetFriendRequests(user_id);

                // no data but did succesfully retrieve.
                if (response.Data == null)
                {
                    return this.Ok(new { Message = response.Message });
                }

                // failed to retrieve due to issue
                if (!response.Success)
                {
                    return this.NotFound(new { Message = response.Message });
                }

                return this.Ok(new { Messages = response.Data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Searches for users based on the provided search term.
        /// </summary>
        /// <param name="searchTerm">The term used to search for users.</param>
        /// <response code="200">Returns the list of matching users or a message if no data is found.</response>
        /// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
        /// <response code="404">Not Found. Indicates that the search could not be completed successfully.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("find/{searchTerm}")]
        public IActionResult SearchUser(string searchTerm)
        {
            var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

            if (jwt_user_id == null)
            {
                return this.Unauthorized(new { message = "Invalid credentials" });
            }

            int user_id = int.Parse(jwt_user_id.Value);

            try
            {
                var response = this.friendLogic.FindUser(user_id, searchTerm);

                // no data but did succesfully retrieve.
                if (response.Data == null)
                {
                    return this.Ok(new { Message = response.Message });
                }

                // failed to retrieve due to issue
                if (!response.Success)
                {
                    return this.NotFound(new { Message = response.Message });
                }

                return this.Ok(new { Messages = response.Data });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Request to become friends with another user.
        /// </summary>
        /// <param name="friend_id">The request model containing the ID of the user to send the friend request to.</param>
        /// <response code="201">Friend request sent successfully.</response>
        /// <response code="400">Bad request. Occurs if the user tries to send a request to themselves or if the model is invalid.</response>
        /// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("request/{friend_id}")]
        public IActionResult SendFriendRequest(int friend_id)
        {
            var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

            if (jwt_user_id == null)
            {
                return this.Unauthorized(new { message = "Invalid credentials" });
            }

            int user_id = int.Parse(jwt_user_id.Value);

            if (user_id == friend_id)
            {
                return this.BadRequest(new { message = "You can't befriend yourself." });
            }

            try
            {
                var response = this.friendLogic.RequestFriendship(user_id, friend_id);

                if (!response.Success)
                {
                    return this.BadRequest(new { message = response.Message });
                }

                return this.Created(string.Empty, new { message = "Friend request sent successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Update the status of a friend request.
        /// </summary>
        /// <param name="friend_id">The status model containing the ID of the user.</param>
        /// <param name="status">The status model containing the new status ("accepted" or "rejected").</param>
        /// <response code="200">Friend status successfully updated.</response>
        /// <response code="400">Bad request. Occurs if an invalid status is provided.</response>
        /// <response code="401">Unauthorized. Occurs if the user's JWT token is missing or invalid.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        [Route("request/{friend_id}/{status}")]
        public IActionResult UpdateFriendStatus(int friend_id, string status)
        {
            var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

            if (jwt_user_id == null)
            {
                return this.Unauthorized(new { message = "Invalid credentials" });
            }

            int user_id = int.Parse(jwt_user_id.Value);

            try
            {
                var response = this.friendLogic.UpdateFriendStatus(user_id, friend_id, status);

                if (!response.Success)
                {
                    return this.BadRequest(new { message = response.Message });
                }

                return this.Ok(new { message = $"Friend request successfully {status}." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
    }
}
