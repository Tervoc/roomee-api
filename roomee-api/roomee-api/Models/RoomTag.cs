using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace roomee_api.Models {
	public class RoomTag {
		[JsonProperty(PropertyName = "roomTagId")]
		public int RoomTagId { get; }

		[JsonProperty(PropertyName = "roomId")]
		public int RoomId { get; set; }

		[JsonProperty(PropertyName = "tag")]
		public string Tag { get; set; }

		[JsonProperty(PropertyName = "creationTimestamp")]
		public DateTime CreationTimestamp { get; }

		[JsonProperty(PropertyName = "expirationTimestamp")]
		public DateTime ExpirationTimestamp { get; }

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; }

		public static readonly string[] UpdateNames = { "roomId", "tag", "expirationTimestamp", "statusId" };

		public RoomTag(int roomTagId, int roomId, string tag, DateTime creationTimestamp, DateTime expirationTimestamp, int statusId) {
			RoomTagId = roomTagId;
			RoomId = roomId;
			Tag = tag;
			CreationTimestamp = creationTimestamp;
			ExpirationTimestamp = expirationTimestamp;
			StatusId = statusId;
		}

		public static RoomTag FromRoomTagId(int roomTagId) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomTag] WHERE RoomTagId = @param1;", conn);
				command.Parameters.AddWithValue("@param1", roomTagId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return new RoomTag(
							reader.GetInt32(0),
							reader.GetInt32(1),
							reader.GetString(2),
							reader.GetDateTime(3),
							reader.GetDateTime(4),
							reader.GetInt32(5)
						);
					} else {
						return null;
					}
				}
			}
		}

		public static RoomTag FromRoomTag(string tag) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomTag] WHERE Tag = @param1;", conn);
				command.Parameters.AddWithValue("@param1", tag);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return new RoomTag(
							reader.GetInt32(0),
							reader.GetInt32(1),
							reader.GetString(2),
							reader.GetDateTime(3),
							reader.GetDateTime(4),
							reader.GetInt32(5)
						);
					} else {
						return null;
					}
				}
			}
		}
	}
}
