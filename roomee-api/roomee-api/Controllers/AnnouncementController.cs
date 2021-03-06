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
		public IActionResult CreateAnnouncement([FromBody][Required] User user) {

			return Ok(); 
		}
	}
}
