/* 
 * Author(s): Majors, Andrew, admajors00@gmail.com
 * Date Created: March 01 2021
 * Notes: N/A
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace roomee_api.Models {
	public class Announcement {
		[JsonProperty(PropertyName = "annoucementId")]
		public int AnnoucementId { get; }

		[JsonProperty(PropertyName = "roomId")]
		public int RoomId { get; set; }

		[JsonProperty(PropertyName = "createdByUserId")]
		public int CreatedByUserId { get; }

		[JsonProperty(PropertyName = "creationTimestamp")]
		public DateTime CreationTimestamp { get; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "body")]
		public string Body { get; set; }

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; }

		public static readonly string[] UpdateNames = { "roomId", "creationTimestamp", "title", "body", "statusId" };

		public Announcement(int annoucementId, int roomId, int createdByUserId, DateTime creationTimestamp, string title, string body, int statusId) {
			AnnoucementId = annoucementId;
			RoomId = roomId;
			CreatedByUserId = createdByUserId;
			CreationTimestamp = creationTimestamp;
			Title = title;
			Body = body;
			StatusId = statusId;
		}

		public static Announcement FromAnnouncementId(int announcementId) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [Announcement] WHERE AnnouncementId = @param1;", conn);
				command.Parameters.AddWithValue("@param1", announcementId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return new Announcement(
							reader.GetInt32(0),
							reader.GetInt32(1),
							reader.GetInt32(2),
							reader.GetDateTime(3),
							reader.GetString(4),
							reader.GetString(5),
							reader.GetInt32(6)
						);
					} else {
						return null;
					}
				}
			}
		}
	}
}
