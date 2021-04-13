/* Author(s): Schmidt, Max, max.schmidt@ttu.edu
 * Date Created: 03/01/2021
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
	[Route("v1/announcement")]
	[ApiController]
	public class AnnouncementController : ControllerBase {
		[HttpGet]	
		public IActionResult GetAnnouncements([FromHeader][Required] string token) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}

			List<Announcement> announcements = new List<Announcement>();

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand("SELECT * FROM Announcement WHERE StatusId = 1 ORDER BY CreationTimestamp DESC;", conn);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						while (reader.Read()) {
							announcements.Add(new Announcement(
								reader.GetInt32(0),
								reader.GetInt32(1),
								reader.GetInt32(2),
								reader.GetDateTime(3),
								reader.GetString(4),
								reader.GetString(5),
								reader.GetInt32(6)
							));
						}
					}
				}
			}

			List<Dictionary<string, object>> returnList = new List<Dictionary<string, object>>();

			foreach (Announcement announcement in announcements) {
				Dictionary<string, object> dict = new Dictionary<string, object> {
					{ "announcement", announcement },
					{ "user", Models.User.FromUserId(announcement.CreatedByUserId) }
				};

				returnList.Add(dict);
			}

			return Ok(JsonConvert.SerializeObject(returnList, Formatting.Indented));
		}

		[Route ("fromType")]//post create patch update
		[HttpGet]
		public IActionResult GetAnnouncement([FromQuery][Required] string type, [FromQuery][Required] string identifier) {
			Announcement announcement = null;

			if (type.ToLower().Equals("id")) {
				int announcementId;

				if (int.TryParse(identifier, out announcementId)) {
					announcement = Models.Announcement.FromAnnouncementId(announcementId);
				} else {
					return Problem("identifier must be an integer when type is id");
				}
			} 

			if (announcement == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(announcement, Formatting.Indented));
			}
		}

		[HttpPost]
		public IActionResult CreateAnnouncement([FromBody][Required] Announcement announcement, [FromHeader][Required] string token) {
			Dictionary<string, string> userVals;

			if (Authentication.IsTokenValid(token)){
				userVals = Authentication.ReadToken(token);
			}
			else{
				return Problem("token is not valid");
			}

			if (announcement.Title == string.Empty || announcement.Title == null || announcement.Body == string.Empty || announcement.Body == null){
				return Problem("could not process");
			}

			if (int.TryParse(userVals["userId"], out int userId)){
				using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)){
					conn.Open();

					SqlCommand command = new SqlCommand(@"INSERT INTO dbo.Announcement (RoomId, CreatedByUserId, CreationTimestamp, Title, Body, StatusId) VALUES (@roomId, @createdByUserId, CURRENT_TIMESTAMP, @title, @body, @statusId);", conn);
					command.Parameters.AddWithValue("@roomId", announcement.RoomId);
					command.Parameters.AddWithValue("@createdByUserId", announcement.CreatedByUserId);
					command.Parameters.AddWithValue("@title", announcement.Title);
					command.Parameters.AddWithValue("@body", announcement.Body);
					command.Parameters.AddWithValue("@statusId", 1);

					int rows = command.ExecuteNonQuery();

					if (rows == 0){
						return Problem("error creating");
					}

				}
			}
			return Ok(); 
		}

		[HttpPatch("{id}")]
		public IActionResult UpdateAnnouncement([FromRoute] int id, [FromHeader][Required] string token, [FromBody] Dictionary<string, string> patch){
			if (!Authentication.IsTokenValid(token)){
				return Problem("token is not valid");
			}

			foreach (string key in patch.Keys){
				if (Array.IndexOf(Models.Announcement.UpdateNames, key) == -1){
					return BadRequest("invalid key");
				}
			}

			SqlCommand command = QueryBuilder.UpdateBuilder(patch, "[Announcement]", "AnnouncementId", id);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)){
				conn.Open();

				command.Connection = conn;

				int rows = command.ExecuteNonQuery();

				if (rows != 0){
					return Ok();
				}
				else{
					return Problem("could not process");
				}
			}
		}
	}
}
