/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu
 * Date Created: March 05 2021
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
	[Route("v1/roomTag")]
	[ApiController]
	public class RoomTagController : ControllerBase {
		[HttpGet("{id}")]
		public IActionResult GetRoomTag([FromRoute][Required] int id, [FromHeader][Required] string token) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}
			RoomTag roomTag;

			roomTag = Models.RoomTag.FromRoomTagId(id);

			if (roomTag == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(roomTag, Formatting.Indented));
			}
		}

		[HttpPost]
		public IActionResult GenerateNewTag ([FromQuery][Required] int roomId, [FromHeader][Required] string token) {
			Dictionary<string, string> userVals;

			if (Authentication.IsTokenValid(token)) {
				userVals = Authentication.ReadToken(token);
			} else {
				return Problem("token is not valid");
			}

			if(int.TryParse(userVals["userId"], out int userId)) {
				if (Models.User.FromUserId(userId) != null) {
					using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
						conn.Open();

						SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomAssignment] WHERE (UserId = @userId) AND (RoomId = @roomId) AND (StatusId = @statusId);", conn);
						command.Parameters.AddWithValue("@userId", userId);
						command.Parameters.AddWithValue("@roomId", roomId);
						command.Parameters.AddWithValue("@statusId", 1);

						using (SqlDataReader reader = command.ExecuteReader()) {
							if (!reader.HasRows) {
								return Problem("user is not assigned to this room");
							} 
						}
					}
				} else {
					return Problem("user does not exist");
				}
			} else {
				return Problem("user Id must be of type int");
			}



			string tagValue = Utilities.RoomTagGenerator.FindUnusedTag();

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"INSERT INTO [RoomTag] (RoomId, Tag, CreationTimestamp, ExpirationTimestamp, StatusId) VALUES (@roomId, @roomTag, CURRENT_TIMESTAMP, DATEADD(hh, 24, CURRENT_TIMESTAMP), @statusId );", conn);
				command.Parameters.AddWithValue("@roomId", roomId);
				command.Parameters.AddWithValue("@roomTag", tagValue);
				command.Parameters.AddWithValue("@statusId", 1);
				int rowsAffected = command.ExecuteNonQuery();
				
				if(rowsAffected != 0) {
					return Ok();
				} else {
					return Problem("could not generate new tag");
				}
			}
		}

		[HttpGet("match")]
		public IActionResult MatchUserToRoom([FromBody][Required] string roomTag, [FromHeader][Required] string token) {
			if (roomTag == string.Empty || roomTag == null) {
				return Problem("room tag cannot be empty");
			}

			Dictionary<string, string> userVals;

			if (Authentication.IsTokenValid(token)) {
				userVals = Authentication.ReadToken(token);
			} else {
				return Problem("token is not valid");
			}

			if (int.TryParse(userVals["userId"], out int userId)) {
				Room room = Models.Room.FromRoomTag(roomTag);
				
				if (room != null) {
					using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
						conn.Open();

						SqlCommand command = new SqlCommand(@"INSERT INTO [RoomAssignment] (UserId, RoomId, StartTimestamp, StatusId) VALUES (@userId, @roomId, CURRENT_TIMESTAMP, @statusId);", conn);
						command.Parameters.AddWithValue("@userId", userId);
						command.Parameters.AddWithValue("@roomId", room.RoomId);
						command.Parameters.AddWithValue("@statusId", 1);

						int rows = command.ExecuteNonQuery();

						if (rows == 0) {
							return Problem("could not generate row");
						}
						return Ok();
					}
				} else {
					return Problem("no room associated with that room tag");
				}
			} else {
				return Problem("userId must be of type int");
			}
		}

		[HttpPatch("{id}")]
		public IActionResult UpdateRoom([FromRoute] int id, [FromHeader][Required] string token, [FromBody] Dictionary<string, string> patch) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}
			foreach (string key in patch.Keys) {
				if (Array.IndexOf(Models.Room.UpdateNames, key) == -1) {
					return BadRequest("invalid key");
				}
			}

			SqlCommand command = QueryBuilder.UpdateBuilder(patch, "[RoomTag]", "RoomTagId", id);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				command.Connection = conn;

				int rows = command.ExecuteNonQuery();

				if (rows == 0) {
					return Problem("could not process");
				}
				return Ok();
			}
		}
	}
}
