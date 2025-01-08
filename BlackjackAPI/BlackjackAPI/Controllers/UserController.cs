// <copyright file="UserController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BlackjackAPI.Controllers
{
    using BlackjackAPI.Models.User;
    using BlackjackCommon.Interfaces.Logic;
    using BlackjackCommon.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserLogic userLogic;

        public UserController(IUserLogic userLogic)
        {
            this.userLogic = userLogic;
        }

		[HttpGet]
		[Route("Healthcheck")]
		public IActionResult HealthCheck()
		{
			try
			{
				return this.Ok("API is running");
			}
			catch (Exception ex)
			{
				string logFilePath = "app-log.txt";
				string logMessage = $"{DateTime.UtcNow}: - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
				System.IO.File.AppendAllText(logFilePath, logMessage);

				return this.StatusCode(500, "An internal error occurred.");
			}
		}

		/// <summary>
		/// Authenticates a user.
		/// </summary>
		/// <param name="model">The model containing the username and password for login.</param>
		/// <returns>Returns a JWT token if the authentication is successful, or an error message if authentication fails.</returns>
		/// <response code="201">If the login is successful and a JWT token is generated.</response>
		/// <response code="400">If the provided data is invalid (missing required fields or validation errors).</response>
		/// <response code="401">If the username or password is incorrect and authentication fails.</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		[AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public IActionResult Login(Login model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    return this.BadRequest(this.ModelState);
                }

                int user_id = this.userLogic.ValidateUser(model.Username, model.Password);

                if (user_id <= 0)
                {
                    return this.Unauthorized(new { message = "Invalid credentials" });
                }

                string token = this.userLogic.CreateJWT(user_id, model.Username);

                return this.Created(string.Empty, new Response<string>(token, "SuccessfullLogin"));
            }
            catch (Exception ex)
            {
                this.LogToFile(ex, model.Username);

                return this.StatusCode(500, "An internal server error occurred.");
            }
        }

        private void LogToFile(Exception ex, string username)
        {
            string logFilePath = "app-log.txt";
            string logMessage = $"{DateTime.UtcNow}: Error for user {username} - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";

            System.IO.File.AppendAllText(logFilePath, logMessage);
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">The model containing the username and password for registration.</param>
        /// <returns>Returns a success message and a JWT token if the registration is successful, or an error message if the registration fails.</returns>
        /// <response code="201">If the user was registered successfully and a JWT token is generated.</response>
        /// <response code="400">If the provided data is invalid (missing required fields or validation errors).</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public IActionResult Register(Register model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var response = this.userLogic.CreateAccount(model.Username, model.Password);

                if (!response.Success)
                {
                    return this.BadRequest(new { message = response.Message });
                }

                return this.Created(string.Empty, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Updates the username for the authenticated user.
        /// </summary>
        /// <param name="model">The model containing the new username.</param>
        /// <returns>Returns a success message if the username is updated successfully, or an error message otherwise.</returns>
        /// <response code="201">If the username was changed successfully.</response>
        /// <response code="400">If the provided data is invalid (missing required fields or validation errors).</response>
        /// <response code="401">If the user is unauthorized.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        [Authorize]
        [HttpPut]
        [Route("Username")]
        public IActionResult ChangeUsername(ChangeUsername model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

            if (jwt_user_id == null)
            {
                return this.Unauthorized(new { message = "Invalid credentials" });
            }

            int user_id = int.Parse(jwt_user_id.Value);

            try
            {
                var response = this.userLogic.ChangeUsername(user_id, model.Username);

                if (!response.Success)
                {
                    return this.BadRequest(new { message = response.Message });
                }

				string token = this.userLogic.CreateJWT(user_id, model.Username);
				return this.Created(string.Empty, new Response<string>(token, "SuccessfullNameChange"));
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Updates the password for the authenticated user.
        /// </summary>
        /// <param name="model">The model containing the new password.</param>
        /// <returns>Returns a success message if the password is updated successfully, or an error message otherwise.</returns>
        /// <response code="200">If the password was changed successfully.</response>
        /// <response code="400">If the provided data is invalid (missing required fields or validation errors).</response>
        /// <response code="401">If the user is unauthorized.</response>
        /// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
        [Authorize]
        [HttpPut]
        [Route("Password")]
        public IActionResult ChangePassword(ChangePassword model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var jwt_user_id = this.HttpContext.User.FindFirst("user_id");

            if (jwt_user_id == null)
            {
                return this.Unauthorized(new { message = "Invalid credentials" });
            }

            int user_id = int.Parse(jwt_user_id.Value);

            try
            {
                var response = this.userLogic.ChangePassword(user_id, model.Old_password, model.New_password, model.Repeat_new_password);

                if (!response.Success)
                {
                    return this.BadRequest(new { message = response.Message });
                }

                return this.Ok(new { message = "Password changed succesfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
    }
}
