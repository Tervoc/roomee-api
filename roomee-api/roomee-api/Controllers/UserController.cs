/*
 * Author(s): Padgett, Matt matthew.padgett@ttu.edu, Parrish, Christian christian.parrish@ttu.edu 
 * Date Created: February 17 2021
 * Notes: N/A
*/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Microsoft.AspNetCore.JsonPatch.Adapters;
using roomee_api.Models;
using roomee_api.Utilities;

namespace roomee_api.Controllers {
	[Route("v1/user")]
	[ApiController]
	public class UserController : ControllerBase {
		[HttpGet]
		public IActionResult GetUser([FromQuery][Required] string type, [FromQuery][Required] string identifier) {
			User user;
			
			if (type.ToLower().Equals("id")) {
				int userId;

				if(int.TryParse(identifier, out userId)) {
					user = Models.User.FromUserId(userId);
				} else {
					return Problem("identifier must be an integer when type is id");
				}
			} else if (type.ToLower().Equals("email")) {
				user = Models.User.FromEmail(identifier);
			} else {
				return Problem("invalid type");
			}

			if (user == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(user, Formatting.Indented));
			}
		}

		[HttpPost]
		public IActionResult CreateUser([FromBody][Required] User user, [FromHeader][Required] string token) {
			if(user.Email == string.Empty || user.Email == null || user.Password == string.Empty || user.Password == null) {
				return Problem("email or password is empty");
			}

			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				PasswordHasher hasher = new PasswordHasher();

				SqlCommand command = new SqlCommand(@"INSERT INTO [User] (FirstName, LastName, PreferredName, Password, Email, StatusId) VALUES (@firstName, @lastName, @preferredName, @password, @email, @statusId);", conn);
				command.Parameters.AddWithValue("@firstName", user.FirstName);
				command.Parameters.AddWithValue("@lastName", user.LastName);
				command.Parameters.AddWithValue("@preferredName", user.PreferredName);
				command.Parameters.AddWithValue("@password", hasher.Hash(user.Password));
				command.Parameters.AddWithValue("@email", user.Email);
				command.Parameters.AddWithValue("@statusId", 1);

				int rows = command.ExecuteNonQuery();

				if(rows == 0) {
					return Problem("error creating");
				}

			}
			return Ok();
		}

		[HttpPatch("{id}")]
		public IActionResult UpdateUser([FromRoute] int id, [FromHeader][Required] string token, [FromBody] Dictionary<string,string> patch){
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}
			foreach (string key in patch.Keys) {
				if(Array.IndexOf(Models.User.UpdateNames, key) == -1) {
					return BadRequest("invalid key");
				}
			}
			
			if (patch.ContainsKey("password")) {
				PasswordHasher hasher = new PasswordHasher();
				patch["password"] = hasher.Hash(patch["password"]);
			}

			SqlCommand command = QueryBuilder.UpdateBuilder(patch, "[User]", "UserId", id);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				command.Connection = conn;

				int rows = command.ExecuteNonQuery();

				if(rows != 0) {
					return Ok();
				} else {
					return Problem("could not process");
				}
			}
		}

		[HttpGet]
		[Route("login")]
		public IActionResult LoginUser([FromQuery][Required] string email, [FromQuery][Required] string password) {
			PasswordHasher hasher = new PasswordHasher();

			User user = Models.User.FromEmail(email);

			if (user == null) {
				return Problem(detail: "invalid email or password");
			} else if (hasher.Check(user.Password, password).Verified) {
				var tokenHandler = new JwtSecurityTokenHandler();
				var token = new JwtSecurityToken(
					issuer: "roomee-api",
					audience: "roomee-client",
					claims: user.ToClaims(),
					expires: DateTime.Now.AddHours(6),
					signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(Startup.JWTSecret)), SecurityAlgorithms.HmacSha256Signature)
				);

				return Ok(new Dictionary<string, string> { { "token", tokenHandler.WriteToken(token) } });
			} else {
				return Problem(detail: "invalid email or password");
			}
		}
	}
}
