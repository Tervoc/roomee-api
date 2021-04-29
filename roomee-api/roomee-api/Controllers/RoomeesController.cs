/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu 
 * Date Created: March 29 2021
 * Notes: N/A
*/
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using roomee_api.Models;
using roomee_api.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace roomee_api.Controllers {
	[Route("v1/roomees")]
	[ApiController]
	public class RoomeesController : ControllerBase {
		[HttpGet]
		[Route("{roomId}")]
		public IActionResult GetMyRoomees([FromRoute][Required] int roomId, [FromHeader][Required] string token) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}

			List<int> roomUserIds = new List<int>();
			List<Dictionary<string, string>> roomUserPreferences = new List<Dictionary<string, string>>();

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomAssignment] WHERE (RoomId = @roomId) AND (StatusId = @statusId);", conn);
				command.Parameters.AddWithValue("@roomId", roomId);
				command.Parameters.AddWithValue("@statusId", 1);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						while (reader.Read()) {
							roomUserIds.Add(reader.GetInt32(1));
						}
					} else {
						return Problem("No roomees found");
					}
				}
			}

			foreach (int userId in roomUserIds) {

				UserPreferences user = UserPreferences.FromUserId(userId);

				if (user == null) {
					return Problem("problem fetching roomees");
				}
				Dictionary<string, string> userAndPrefs = new Dictionary<string, string>();

				userAndPrefs.Add("userId", userId.ToString());
				userAndPrefs.Add("userStatus", UserPreferences.FromUserId(userId).UserStatus);

				roomUserPreferences.Add(userAndPrefs);
			}

			return Ok(JsonConvert.SerializeObject(roomUserPreferences, Formatting.Indented));

		}

		[HttpGet]
		[Route("objects/{roomId}")]
		public IActionResult GetMyRoomeesObjects([FromRoute][Required] int roomId, [FromHeader][Required] string token) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}

			List<int> roomUserIds = new List<int>();

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomAssignment] WHERE (RoomId = @roomId) AND (StatusId = @statusId);", conn);
				command.Parameters.AddWithValue("@roomId", roomId);
				command.Parameters.AddWithValue("@statusId", 1);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						while (reader.Read()) {
							roomUserIds.Add(reader.GetInt32(1));
						}
					} else {
						return Problem("No roomees found");
					}
				}
			}

			List<User> users = new List<User>();

			foreach (int userId in roomUserIds) {

				User x = Models.User.FromUserId(userId);

				users.Add(x);
			}

			return Ok(JsonConvert.SerializeObject(users, Formatting.Indented));

		}
	}
}
