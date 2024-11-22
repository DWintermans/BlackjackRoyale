using BlackjackAPI.Models.User;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Models;
using BlackjackLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlackjackAPI.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserController : ControllerBase
	{
		private readonly IUserLogic _userLogic;

		public UserController(IUserLogic userLogic)
		{
			_userLogic = userLogic;
		}

		/// <summary>
		/// Authenticates a user.
		/// </summary>
		/// <param name="model">The model containing the username and password for login.</param>
		/// <returns>Returns a JWT token if the authentication is successful, or an error message if authentication fails.</returns>
		/// <response code="200">If the login is successful and a JWT token is generated.</response>		
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
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				int user_id = _userLogic.ValidateUser(model.username, model.password);

				if (user_id <= 0)
				{
					return Unauthorized(new { message = "Invalid credentials" });
				}

				string token = _userLogic.CreateJWT(user_id, model.username);
			
				return Ok(new Response<string>(token, "SuccessfullLogin"));
			}
			catch (Exception ex)
			{
				LogToFile(ex, model.username);

				return StatusCode(500, "An internal server error occurred.");
			}

		}

		private void LogToFile(Exception ex, string username)
		{
			string logFilePath = "app-log.txt";
			string logMessage = $"{DateTime.UtcNow}: Error for user {username} - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";

			System.IO.File.AppendAllText(logFilePath, logMessage);
		}

		[HttpGet]
		[Route("Healthcheck")]
		public IActionResult HealthCheck()
		{
			try
			{
				return Ok("API is running");
			}
			catch (Exception ex)
			{
				string logFilePath = "app-log.txt";
				string logMessage = $"{DateTime.UtcNow}: - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
				System.IO.File.AppendAllText(logFilePath, logMessage);

				return StatusCode(500, "An internal error occurred.");	
			}
		}

		/// <summary>
		/// Registers a new user.
		/// </summary>
		/// <param name="model">The model containing the username and password for registration.</param>
		/// <returns>Returns a success message and a JWT token if the registration is successful, or an error message if the registration fails.</returns>
		/// <response code="200">If the user was registered successfully and a JWT token is generated.</response>
		/// <response code="400">If the provided data is invalid (missing required fields or validation errors).</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		[AllowAnonymous]
		[HttpPost]
		[Route("Register")]
		public IActionResult Register(Register model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var response = _userLogic.CreateAccount(model.username, model.password);

				if (!response.Success)
				{
					return BadRequest(new { message = response.Message });
				}

				return Ok(response);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { message = "An error occurred while processing your request." });
			}
		}

		/// <summary>
		/// Updates the username for the authenticated user.
		/// </summary>
		/// <param name="model">The model containing the new username.</param>
		/// <returns>Returns a success message if the username is updated successfully, or an error message otherwise.</returns>
		/// <response code="200">If the username was changed successfully</response>
		/// <response code="400">If the provided data is invalid (missing required fields or validation errors).</response>
		/// <response code="401">If the user is unauthorized</response>
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		[Authorize]
		[HttpPut]
		[Route("Username")]
		public IActionResult ChangeUsername(ChangeUsername model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { message = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{
				var response = _userLogic.ChangeUsername(user_id, model.username);

				if (!response.Success)
				{
					return BadRequest(new { message = response.Message });
				}

				return Ok(new { message = "Username changed succesfully" });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { message = "An error occurred while processing your request." });
			}
		}

		/// <summary>
		/// Updates the password for the authenticated user.
		/// </summary>
		/// <param name="model">The model containing the new password.</param>
		/// <returns>Returns a success message if the password is updated successfully, or an error message otherwise.</returns>
		/// <response code="200">If the password was changed successfully</response>
		/// <response code="400">If the provided data is invalid (missing required fields or validation errors).</response>
		/// <response code="401">If the user is unauthorized</response>		
		/// <response code="500">Internal Server Error. Occurs if there is an unexpected error during the request.</response>
		[Authorize]
		[HttpPut]
		[Route("Password")]
		public IActionResult ChangePassword(ChangePassword model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var jwt_user_id = HttpContext.User.FindFirst("user_id");

			if (jwt_user_id == null)
			{
				return Unauthorized(new { message = "Invalid credentials" });
			}

			int user_id = int.Parse(jwt_user_id.Value);

			try
			{
				var response = _userLogic.ChangePassword(user_id, model.old_password, model.new_password, model.repeat_new_password);

				if (!response.Success)
				{
					return BadRequest(new { message = response.Message });
				}

				return Ok(new { message = "Password changed succesfully" });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return StatusCode(500, new { message = "An error occurred while processing your request." });
			}
		}

	}
}
