using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace roomee_api.Utilities {
	public class RoomTagGenerator {
		static readonly Random random = new Random();
		public static string FindUnusedTag (int length) {
			string roomTag;
			byte[] buffer = new byte[length / 2];
			random.NextBytes(buffer);
			string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
			if (length % 2 == 0) {
				roomTag = result;
			} else {
				roomTag = result + random.Next(16).ToString("X");
			}

			if(TagIsTaken(roomTag)) {
				return FindUnusedTag(length);
			} else {
				return roomTag;
			}
		}

		private static bool TagIsTaken (string tag) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomTag] WHERE RoomTag = @tag;", conn);
				command.Parameters.AddWithValue("@tag", tag);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						return true;
					} else {
						return false;
					}
				}
			}
		}

	}
}
