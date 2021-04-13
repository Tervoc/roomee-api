/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu 
 * Date Created: April 7 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace roomee_api.Models {
	public class Chore {
		[JsonProperty(PropertyName = "choreId")]
		public int ChoreId { get; }

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		[JsonProperty(PropertyName = "roomId")]
		public int RoomId { get; set; }

		[JsonProperty(PropertyName = "privateFlag")]
		public bool PrivateFlag { get; set; }

		[JsonProperty(PropertyName = "choreFrequencyId")]
		public int ChoreFrequencyId { get; set; }

		[JsonProperty(PropertyName = "choreDayAssignmentId")]
		public int ChoreDayAssignmentId { get; set; }

		[JsonProperty(PropertyName = "choreTimeAssignmentId")]
		public int ChoreTimeAssignmentId { get; set; }

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; }

		public static readonly string[] UpdateNames = { "name", "description", "roomId", "privateFlag", "choreFrequencyId", "choreDayAssignmentId", "choreTimeAssignmentId", "statusId" };

		public Chore(int choreId, string name, string description, int roomId, bool privateFlag, int choreFrequencyId, int choreDayAssignmentId, int choreTimeAssignmentId, int statusId) {
			ChoreId = choreId;
			Name = name;
			Description = description;
			RoomId = roomId;
			PrivateFlag = privateFlag;
			ChoreFrequencyId = choreFrequencyId;
			ChoreDayAssignmentId = choreDayAssignmentId;
			ChoreTimeAssignmentId = choreTimeAssignmentId;
			StatusId = statusId;
		}

		public static Chore FromChoreId(int id) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM Chore WHERE ChoreId = @id;", conn);
				command.Parameters.AddWithValue("@id", id);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						bool flag = reader.GetBoolean(4);

						string desc = reader.IsDBNull(2) ? null : reader.GetString(2);
						int freq = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
						int day = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
						int time = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);

						return new Chore(
							reader.GetInt32(0),
							reader.GetString(1),
							desc,
							reader.GetInt32(3),
							flag,
							freq,
							day,
							time,
							reader.GetInt32(8)
						);
					} else {
						return null;
					}
				}
			}
		}
	}
}
