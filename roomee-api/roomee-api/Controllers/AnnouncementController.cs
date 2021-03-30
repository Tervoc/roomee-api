/* Author(s): Schmidt, Max, fuckyou123@gmail.com
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
	[Route("v1/anouncement")]
	[ApiController]
	public class AnnouncementController : ControllerBase{
		[HttpGet]//post create patch update
		public IActionResult GetAnnouncement([FromQuery][Required] string type, [FromQuery][Required] string identifier) {
			Announcement announcement = null;

			if (type.ToLower().Equals("id")) {
				int annoucementId;

				if (int.TryParse(identifier, out annoucementId)) {
					announcement = Models.Announcement.FromAnnouncementId(annoucementId);
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

					SqlCommand command = new SqlCommand(@"INSERT INTO [Announcement] (RoomId, CreatedByUserId, CreationTimestamp, Title, Body, StatusId) VALUES (@roomId, @createdByUserId, CURRENT_TIMESTAMP, @title, @body, @statusId);", conn);
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
		public IActionResult UpdateEvent([FromRoute] int id, [FromHeader][Required] string token, [FromBody] Dictionary<string, string> patch){
			if (!Authentication.IsTokenValid(token)){
				return Problem("token is not valid");
			}

			foreach (string key in patch.Keys){
				if (Array.IndexOf(Models.Event.UpdateNames, key) == -1){
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
