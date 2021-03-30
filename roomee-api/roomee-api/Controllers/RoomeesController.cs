/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu 
 * Date Created: March 29 2021
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
	[Route("v1/roomees")]
	[ApiController]
	public class roomeesController : ControllerBase {
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
	}
}
