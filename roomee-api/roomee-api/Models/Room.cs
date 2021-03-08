/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu, Padgett, Matt matthew.padgett@ttu.edu
 * Date Created: March 01 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace roomee_api.Models {
	public class Room {
		[JsonProperty(PropertyName = "roomId")]
		public int RoomId { get; }

		[JsonProperty(PropertyName = "roomName")]
		public string RoomName { get; set; }

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; }

		public static readonly string[] UpdateNames = { "roomName",  "statusId" };

		public Room(int roomId, string roomName, int statusId) {
			RoomId = roomId;
			RoomName = roomName;
			StatusId = statusId;
		}

		public static Room FromRoomId(int roomId) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [Room] WHERE RoomId = @param1;", conn);
				command.Parameters.AddWithValue("@param1", roomId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return new Room(
							reader.GetInt32(0),
							reader.GetString(1),
							reader.GetInt32(2)
						);
					} else {
						return null;
					}
				}
			}
		}

		public static Room FromRoomTag(string roomTag) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomTag] WHERE Tag = @param1;", conn);
				command.Parameters.AddWithValue("@param1", roomTag);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return FromRoomId(reader.GetInt32(1));
					} else {
						return null;
					}
				}
			}
		}
	}
}
