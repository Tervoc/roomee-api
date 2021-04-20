/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu, Padgett, Matt matthew.padgett@ttu.edu, Schmidt, Max max.schmidt@ttu.edu
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

		[HttpPost]
		public IActionResult CreateRoom([FromBody][Required] Room room, [FromHeader][Required] string token) {
			if (room.RoomName == string.Empty || room.RoomName == null) {
				return Problem("room name cannot be empty");
			}
			
			Dictionary<string, string> roomVals;

			if (Authentication.IsTokenValid(token)) {
				roomVals = Authentication.ReadToken(token);
			} else {
				return Problem("token is not valid");
			}

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)){
				conn.Open();

				SqlCommand command = QueryBuilder.InsertBuilder<Room>("dbo.usp_InsertRoom", room, token);
				command.Connection = conn;

				using (SqlDataReader reader = command.ExecuteReader()){
					if (reader.HasRows){
						reader.Read();

						if (reader.GetInt32(0) < 1){
							return Problem(reader.GetString(1));
						}
						else{
							return Ok();
						}
					}
					else{
						return Problem("error executing");
					}
				}
			}
		}

		[HttpPatch("{id}")]
		public IActionResult UpdateRoom([FromRoute] int id, [FromHeader][Required] string token, [FromBody] Dictionary<string, string> patch) {
			if (!Authentication.IsTokenValid(token)){
				return Problem("token is not valid");
			}
			foreach (string key in patch.Keys){
				if (Array.IndexOf(Models.Room.UpdateNames, key) == -1){
					return BadRequest("invalid key");
				}
			}

			SqlCommand command = QueryBuilder.UpdateBuilder<Room>("dbo.usp_UpdateRoom", id, patch, token);

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)){
				conn.Open();

				command.Connection = conn;

				using (SqlDataReader reader = command.ExecuteReader()){
					if (reader.HasRows){
						reader.Read();

						if (reader.GetInt32(0) < 1){
							return Problem(reader.GetString(1));
						}
						else{
							return Ok();
						}
					}
					else{
						return Problem("error executing");
					}
				}
			}
		}
	}
}
