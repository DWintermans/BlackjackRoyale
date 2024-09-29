using BlackjackLogic.Interfaces.Logic;
using BlackjackLogic.Interfaces.Repository;
using Konscious.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlackjackLogic
{
	public class AccountLogic : IAccountLogic
	{
		private readonly IAccountRepository _accountDAL;

		public AccountLogic(IAccountRepository accountDAL)
		{
			_accountDAL = accountDAL;
		}

		private const int DEGREE_OF_PARALLELISM = 16;
		private const int NUMBER_OF_ITERATIONS = 4;
		private const int MEMORY_TO_USE_IN_KB = 600000;

		public class AccountResult
		{
			public required bool Success { get; set; }
			public string? Message { get; set; }
			public string? JWT { get; set; }
		}

		public string CreateJWT(int user_id, string username)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes("ah48MZ4amGS3VqakPxjsYSekeg3yar6MbirervAigfquZkcF8wSCS3VKTWMaQCMR8dSJh3McMCcoT59rUnTxqKoSyAELPRcdZVF9wtB8XxhUPpTQUA5nWoGVSfd8R4Go");
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new System.Security.Claims.ClaimsIdentity(new[]
				{
						new System.Security.Claims.Claim("user_id", user_id.ToString()),
						new System.Security.Claims.Claim("user_name", username.ToString())
					}),
				Expires = DateTime.UtcNow.AddDays(30),
				Issuer = "Issuer",
				Audience = "Audience",
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var jwt = tokenHandler.WriteToken(token);

			return jwt;
		}

		public int GetUserIDFromJWT(string token)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ah48MZ4amGS3VqakPxjsYSekeg3yar6MbirervAigfquZkcF8wSCS3VKTWMaQCMR8dSJh3McMCcoT59rUnTxqKoSyAELPRcdZVF9wtB8XxhUPpTQUA5nWoGVSfd8R4Go")),
				ValidateIssuer = false,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero
			};

			try
			{
				ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
				string value = claimsPrincipal.FindFirst("user_id")?.Value;

				int user_id = 0;
				if (Int32.TryParse(value, out user_id))
				{
					return user_id;
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Token validation failed: " + ex.Message);
				return 0;
			}
		}

		public AccountResult CreateAccount(string username, string password)
		{
			if (_accountDAL.IsUsernameTaken(username))
			{
				return new AccountResult { Success = false, Message = "Username is already in use." };
			}

			byte[] salt = CreateSalt();
			byte[] hashed_password = HashPassword(password, salt);

			int user_id = _accountDAL.CreateAccount(username, Convert.ToBase64String(hashed_password), Convert.ToBase64String(salt));

			if (user_id <= 0)
			{
				return new AccountResult { Success = false, Message = "An error occurred. Please try again later." };
			}

			string token = CreateJWT(user_id, username);

			if (token.Length <= 0)
			{
				return new AccountResult { Success = false, Message = "An error occurred. Please try again later." };
			}

			return new AccountResult { Success = true, Message = "Account created successfully", JWT = token };
		}

		//ref: https://security.stackexchange.com/questions/228993/argon2id-configuration
		public int ValidateUser(string username, string password)
		{
			//Get old password and salt from db based on username
			var loginInfo = _accountDAL.RetrieveLoginInformation(username);

			if (loginInfo.user_id != 0 && loginInfo.user_name != null && loginInfo.hashed_pw != null && loginInfo.salt != null)
			{
				byte[] hashed_pw = loginInfo.hashed_pw;
				byte[] salt = loginInfo.salt;

				bool isValid = VerifyHash(password, salt, hashed_pw);

				if (isValid)
				{
					return loginInfo.user_id;
				}
			}

			return 0;
		}

		public AccountResult ChangePassword(int user_id, string old_password, string new_password, string repeat_new_password)
		{
			if (new_password != repeat_new_password)
			{
				return new AccountResult { Success = false, Message = "New passwords don't match" };
			}

			if (old_password == new_password)
			{
				return new AccountResult { Success = false, Message = "New password can't be the same as old password!" };
			}

			//Get old password and salt from db based on user_id
			var loginInfo = _accountDAL.RetrieveSalt_HashInformation(user_id);

			if (loginInfo.hashed_pw == null || loginInfo.hashed_pw.Length == 0 || loginInfo.salt == null || loginInfo.salt.Length == 0)
			{
				return new AccountResult { Success = false, Message = "An error occurred. Please try again later." };
			}
			else
			{
				byte[] db_hashed_pw = loginInfo.hashed_pw;
				byte[] db_salt = loginInfo.salt;

				if (!VerifyHash(old_password, db_salt, db_hashed_pw))
				{
					return new AccountResult { Success = false, Message = "Old passwords don't match" };
				}
			}

			byte[] salt = CreateSalt();
			byte[] hashed_password = HashPassword(new_password, salt);

			if (!_accountDAL.UpdatePassword(user_id, Convert.ToBase64String(hashed_password), Convert.ToBase64String(salt)))
			{
				return new AccountResult { Success = false, Message = "An error occurred. Please try again later." };
			}

			return new AccountResult { Success = true };
		}

		public AccountResult ChangeUsername(int user_id, string user_name)
		{
			if (_accountDAL.IsUsernameTaken(user_name))
			{
				return new AccountResult { Success = false, Message = "Username already in use." };
			}

			if (!_accountDAL.UpdateUsername(user_id, user_name))
			{
				return new AccountResult { Success = false, Message = "An error occurred. Please try again later." };
			}

			return new AccountResult { Success = true };
		}

		private static byte[] CreateSalt()
		{
			var salt = new byte[16];
			var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(salt);

			return salt;
		}

		private static byte[] HashPassword(string password, byte[] salt)
		{
			var argon2id = new Argon2id(Encoding.UTF8.GetBytes(password));
			argon2id.Salt = salt;
			argon2id.DegreeOfParallelism = DEGREE_OF_PARALLELISM;
			argon2id.Iterations = NUMBER_OF_ITERATIONS;
			argon2id.MemorySize = MEMORY_TO_USE_IN_KB;

			return argon2id.GetBytes(16);
		}

		private static bool VerifyHash(string password, byte[] salt, byte[] hash)
		{
			var newHash = HashPassword(password, salt);
			return hash.SequenceEqual(newHash);
		}

	}
}