using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace roomee_api.Utilities {
	public class RoomTagGenerator {
		static readonly Random randomBytes = new Random();
		public static string FindUnusedTag () {
			int length = 8;
			string roomTag;
			byte[] bufferBytes = new byte[length / 2];
			randomBytes.NextBytes(bufferBytes);
			string hexString = String.Concat(bufferBytes.Select(x => x.ToString("X2")).ToArray());
			if (length % 2 == 0) {
				roomTag = hexString;
			} else {
				roomTag = hexString + randomBytes.Next(16).ToString("X");
			}

			if(IsTagTaken(roomTag)) {
				return FindUnusedTag();
			} else {
				return roomTag;
			}
		}

		private static bool IsTagTaken (string tag) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [RoomTag] WHERE Tag = @tag;", conn);
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
