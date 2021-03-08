//Author(s): Schmidt, Max(max.schmidt@ttu.edu)
//Date Created: 03 / 07 / 2021
//Notes: N/A
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

namespace roomee_api.Controllers{
    [Route("v1/event")]
    [ApiController]
    public class EventController : ControllerBase{
		[HttpGet("{id}")]
		public IActionResult GetEvent([FromRoute][Required] int id){
            Event Event = null; //It did not like it when I named the object event, it breaks everything, so I named it Event with a capital E. 

			int eventId = id;

			Event = Models.Event.FromEventId(eventId);
	
			if (Event == null){
				return NotFound("not found");
			}
			else{
				return Ok(JsonConvert.SerializeObject(Event, Formatting.Indented));
			}
		}

		[HttpPost]
		public IActionResult CreateEvent([FromBody][Required] Event Event, [FromQuery][Required] int userId)
		{
			if (Event.Title == string.Empty || Event.Title == null || Event.Description == string.Empty || Event.Description == null){
				return Problem("could not process");
			}

			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)){
				conn.Open();

				SqlCommand command = new SqlCommand(@"INSERT INTO [Event] (CreatorUserId, RoomId, CreationTimestamp, StartTimestamp, EndTimestamp, Title, Description, StatusId) VALUES (@creatorUserId, @roomId, @creationTimestamp, @startTimestamp, @endTimestamp, @title, @description, @statusId);", conn);
				command.Parameters.AddWithValue("@creatorUserId", Event.CreatorUserId);
				command.Parameters.AddWithValue("@roomId", Event.RoomId);
				command.Parameters.AddWithValue("@creationTimestamp", Event.CreationTimestamp);
				command.Parameters.AddWithValue("@startTimestamp", Event.StartTimestamp);
				command.Parameters.AddWithValue("@endTimeStamp", Event.EndTimestamp);
				command.Parameters.AddWithValue("@title", Event.Title);
				command.Parameters.AddWithValue("@description", Event.Description);
				command.Parameters.AddWithValue("@statusId", 1);

				int rows = command.ExecuteNonQuery();

				if (rows == 0){
					return Problem("error creating");
				}

			}
			return Ok();
		}

		[HttpPatch("{id}")]
		public IActionResult UpdateEvent([FromRoute] int id, [FromBody] Dictionary<string, string> patch){
			foreach (string key in patch.Keys){
				if (Array.IndexOf(Models.Event.UpdateNames, key) == -1){
					return BadRequest("invalid key");
				}
			}

			SqlCommand command = QueryBuilder.UpdateBuilder(patch, "[Event]", "EventId", id);

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
