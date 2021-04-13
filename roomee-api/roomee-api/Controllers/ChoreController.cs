/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu 
 * Date Created: April 8 2021
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
	[Route("v1/chore")]
	[ApiController]
	public class ChoreController : ControllerBase {
		[HttpGet("{id}")]
		public IActionResult GetChore([FromRoute][Required] int id, [FromHeader][Required] string token) {
			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}
			Chore chore;

			chore = Models.Chore.FromChoreId(id);

			if (chore == null) {
				return NotFound("not found");
			} else {
				return Ok(JsonConvert.SerializeObject(chore, Formatting.Indented));
			}
		}

		[HttpPost]
		public IActionResult CreateChore([FromBody][Required] Chore chore, [FromHeader][Required] string token) {
			if (chore.Name == string.Empty || chore.Name == null) {
				return Problem("chore requires a name");
			}

			if (!Authentication.IsTokenValid(token)) {
				return Problem("token is not valid");
			}

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"INSERT INTO dbo.Chore (Name, Description, RoomId, PrivateFlag, ChoreFrequencyId, ChoreDayAssignmentId, ChoreTimeAssignmentId, StatusId) VALUES (@name, @description, @roomId, @privateFlag, @choreFrequencyId, @choreDayAssignmentId, @choreTimeAssignmentId, @statusId);", conn);
				command.Parameters.AddWithValue("@name", chore.Name);

				if (chore.Description == string.Empty || chore.Description == null) {
					command.Parameters.AddWithValue("@description", DBNull.Value);
				} else {
					command.Parameters.AddWithValue("@description", chore.Description);
				}

				command.Parameters.AddWithValue("@roomId", chore.RoomId);
				command.Parameters.AddWithValue("@privateFlag", chore.PrivateFlag);

				if (chore.ChoreFrequencyId == 0) {
					command.Parameters.AddWithValue("@choreFrequencyId", DBNull.Value);
				} else {
					command.Parameters.AddWithValue("@choreFrequencyId", chore.ChoreFrequencyId);
				}

				if (chore.ChoreDayAssignmentId == 0) {
					command.Parameters.AddWithValue("@choreDayAssignmentId", DBNull.Value);
				} else {
					command.Parameters.AddWithValue("@choreDayAssignmentId", chore.ChoreDayAssignmentId);
				}

				if (chore.ChoreTimeAssignmentId == 0) {
					command.Parameters.AddWithValue("@choreTimeAssignmentId", DBNull.Value);
				} else {
					command.Parameters.AddWithValue("@choreTimeAssignmentId", chore.ChoreTimeAssignmentId);
				}

				command.Parameters.AddWithValue("@statusId", 1);

				int rows = command.ExecuteNonQuery();

				if (rows == 0) {
					return Problem("error creating");
				}

			}
			return Ok();
		}
	}
}
