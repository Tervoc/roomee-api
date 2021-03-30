/*
 * Author(s): Parrish, Christian christian.parrish@ttu.edu 
 * Date Created: March 27 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace roomee_api.Models {
	public class UserPreferences {
		[JsonProperty(PropertyName = "userPreferencesId")]
		public int UserPreferencesId { get; }

		[JsonProperty(PropertyName = "userId")]
		public int UserId { get; }

		[JsonProperty(PropertyName = "userStatus")]
		public string UserStatus { get; set; }

		public static readonly string[] UpdateNames = { "userStatus" };

		public UserPreferences(int userPreferencesId, int userId, string userStatus) {
			UserPreferencesId = userPreferencesId;
			UserId = userId;
			UserStatus = userStatus;
		}

		public static UserPreferences FromUserId(int userId) {
			UserPreferences preferences;
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [UserPreferences] WHERE UserId = @userId;", conn);
				command.Parameters.AddWithValue("@userId", userId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						preferences = new UserPreferences(
							reader.GetInt32(0),
							reader.GetInt32(1),
							reader.GetString(2)
						);
					} else {
						return null;
					}
				}
			}

			return preferences;
		}
	}
}
