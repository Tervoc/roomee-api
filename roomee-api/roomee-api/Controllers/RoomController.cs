/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu, Padgett, Matt matthew.padgett@ttu.edu
 * Date Created: March 01 2021
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
	[Route("v1/room")]
	[ApiController]
	public class RoomController : ControllerBase {
		[HttpGet("{id}")]
		public IActionResult GetRoom([FromRoute][Required] int id, [FromHeader][Required] string token) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}
			Room room;

			room = Models.Room.FromRoomId(id);

			if (room == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(room, Formatting.Indented));
			}
		}

		[HttpPost("create")]
		public IActionResult CreateRoom([FromBody][Required] Room room, [FromHeader][Required] string token) {
			if (room.RoomName == string.Empty || room.RoomName == null) {
				return Problem("room name cannot be empty");
			}

			Dictionary<string, string> userVals;

			if (Authentication.IsTokenValid(token)) {
				userVals = Authentication.ReadToken(token);
			} else {
				return Problem("token is not valid");
			}
			
			if (int.TryParse(userVals["userId"], out int userId)) {
				using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
					conn.Open();

					SqlCommand command = new SqlCommand(@"INSERT INTO [Room] (RoomName, StatusId) VALUES (@roomName, @roomStatusId); INSERT INTO [RoomAssignment] (UserId, RoomId, StartTimestamp, StatusId) VALUES (@userId, SCOPE_IDENTITY(), CURRENT_TIMESTAMP, @assignStatusId)", conn);
					command.Parameters.AddWithValue("@roomName", room.RoomName);
					command.Parameters.AddWithValue("@roomStatusId", 1);
					command.Parameters.AddWithValue("@userId", userId);
					command.Parameters.AddWithValue("@assignStatusId", 1);

					int rows = command.ExecuteNonQuery();

					if (rows == 0) {
						return Problem("error creating");
					}
				}
				return Ok();
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

			SqlCommand command = QueryBuilder.UpdateBuilder(patch, "[Room]", "RoomId", id);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				command.Connection = conn;

				int rows = command.ExecuteNonQuery();

				if (rows != 0) {
					return Ok();
				} else {
					return Problem("could not process");
				}
			}
		}
	}
}
