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
		[HttpGet]
		public IActionResult GetRoom([FromQuery][Required] string type, [FromQuery][Required] string identifier) {
			Room room = null;

			if (type.ToLower().Equals("id")) {
				int roomId;

				if (int.TryParse(identifier, out roomId)) {
					room = Models.Room.FromRoomId(roomId);
				} else {
					return Problem("identifier must be an integer when type is id");
				}
			} else {
				return Problem("invalid type");
			}

			if (room == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(room, Formatting.Indented));
			}
		}

		[HttpPost]
		public IActionResult CreateRoom([FromBody][Required] Room room, [FromQuery][Required] int userId) {
			if (room.RoomName == string.Empty || room.RoomName == null) {
				return Problem("could not process");
			}

			string roomTag = Utilities.RoomTagGenerator.FindUnusedTag(8);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"INSERT INTO [Room] (RoomName, StatusId) VALUES (@roomName, @statusId); INSERT INTO [RoomTag] (RoomId, RoomTag, CreationDate, ExpirationDate, StatusId) VALUES (SCOPE_IDENTITY(), @roomTag, CURRENT_TIMESTAMP, DATEADD(hh, 24, CURRENT_TIMESTAMP), @statusIdd)", conn);
				command.Parameters.AddWithValue("@roomName", room.RoomName);
				command.Parameters.AddWithValue("@statusId", 1);
				command.Parameters.AddWithValue("@roomTag", roomTag);
				command.Parameters.AddWithValue("@statusIdd", 1);

				int rows = command.ExecuteNonQuery();

				if (rows == 0) {
					return Problem("error creating");
				}

			}

			if (Models.User.AssignToRoom(Models.User.FromUserId(userId), Models.Room.FromRoomTag(roomTag))) {
				return Ok();
			} else {
				return Problem("could not process");
			}
		}

		[HttpPost("assign")]
		public IActionResult AssignToRoom([FromBody][Required] string roomTag, [FromQuery][Required] int userId) {
			if (roomTag == string.Empty || roomTag == null) {
				return Problem("could not process");
			}

			if (Models.User.AssignToRoom(Models.User.FromUserId(userId), Models.Room.FromRoomTag(roomTag))) {
				return Ok();
			} else {
				return Problem("could not process");
			}
		}

		[HttpPatch("{id}")]
		public IActionResult UpdateRoom([FromRoute] int id, [FromBody] Dictionary<string, string> patch) {
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
