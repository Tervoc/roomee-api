using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
//using roomee_api.Models;

namespace roomee_api.Models {
	public class Room {
		[JsonProperty(PropertyName = "roomId")]
		public int RoomId { get; }

		[JsonProperty(PropertyName = "roomName")]
		public string RoomName { get; set; }
		/*
		[JsonProperty(PropertyName = "roomTag")]
		public string RoomTag { get; }*/

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; }

		public static readonly string[] UpdateNames = { "roomName",  "statusId" };

		public Room(int roomId, string roomName, /*string roomTag,*/ int statusId) {
			RoomId = roomId;
			RoomName = roomName;
			//RoomTag = roomTag;
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
							//reader.GetString(2),
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

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomTag] WHERE RoomTag = @param1;", conn);
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

		public static string GenerateNewTag(int roomId) {

			string roomTag = Utilities.RoomTagGenerator.FindUnusedTag(8);
		
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"INSERT INTO [RoomTag] (RoomId, RoomTag, CreationDate, ExpirationDate, StatusId) VALUES (@roomId, @roomTag, CURRENT_TIMESTAMP, DATEADD(hh, 24, CURRENT_TIMESTAMP), @statusId );", conn);
				command.Parameters.AddWithValue("@roomId", roomId);
				command.Parameters.AddWithValue("@roomTag", roomTag);
				command.Parameters.AddWithValue("@statusId", 1);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						return roomTag;
					} else {
						return null;
					}
				}
			}
		}

		public List<Claim> ToClaims() {
			return new List<Claim> {
				new Claim("roomId", RoomId.ToString()),
				new Claim("roomName", RoomName),
				//new Claim("roomTag", RoomTag),
				new Claim("statusId", StatusId.ToString())
			};
		}

	}
}
