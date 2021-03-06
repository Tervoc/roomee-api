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
		[HttpGet]
		public IActionResult GetRoomTag([FromQuery][Required] string type, [FromQuery][Required] string identifier) {
			RoomTag roomTag;
			if (type.ToLower().Equals("roomid")) {
				if(int.TryParse(identifier, out int roomId)) {
					roomTag = Models.RoomTag.FromRoomTagId(roomId);
				} else {
					return Problem("identifier must be of type int");
				}
			} else {
				return Problem("invalid type");
			}

			if (roomTag == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(roomTag, Formatting.Indented));
			}
		}

		[HttpPost("generate")]
		public IActionResult GenerateNewTag ([FromQuery][Required] int roomId) {
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

			Dictionary<string, string> userVals = Authentication.ReadToken(token);

			if (!userVals.ContainsKey("userId") || userVals["userId"] == string.Empty || userVals["userId"] == null) {
				return Problem("userId not found");
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
		public IActionResult UpdateRoom([FromRoute] int id, [FromBody] Dictionary<string, string> patch) {
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
