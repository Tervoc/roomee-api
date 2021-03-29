/*
 * Author(s): Padgett, Matt matthew.padgett@ttu.edu, Parrish, Christian christian.parrish@ttu.edu 
 * Date Created: February 17 2021
 * Notes: N/A
*/
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace roomee_api.Models {
	public class UserWithPreferences {
		[JsonProperty(PropertyName = "userId")]
		public int UserId { get; }

		[JsonProperty(PropertyName = "firstName")]
		public string FirstName { get; set; }

		[JsonProperty(PropertyName = "lastName")]
		public string LastName { get; set; }

		[JsonProperty(PropertyName = "preferredName")]
		public string PreferredName { get; set; }

		[JsonProperty(PropertyName = "password")]
		public string Password { get; set; }

		[JsonProperty(PropertyName = "email")]
		public string Email { get; set; }

		[JsonProperty(PropertyName = "statusId")]
		public int StatusId { get; }

		[JsonProperty(PropertyName = "userStatus")]
		public string UserStatus { get; set; }

		public static readonly string[] UpdateNames = { "firstName", "lastName", "preferredName", "password", "email", "statusId", "userStatus" };

		public UserWithPreferences(int userId, string firstName, string lastName, string preferredName, string password, string email, int statusId, string userStatus) {
			UserId = userId;
			FirstName = firstName;
			LastName = lastName;
			PreferredName = preferredName;
			Password = password;
			Email = email;
			StatusId = statusId;
			UserStatus = userStatus;
		}

		public static UserWithPreferences FromUserId(int userId) {
			UserWithPreferences user;
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [User] WHERE UserId = @userId;", conn);
				command.Parameters.AddWithValue("@userId", userId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						string prefName = string.Empty;

						if (!reader.IsDBNull(3)) {
							prefName = reader.GetString(3);
						}

						user = new UserWithPreferences(
							reader.GetInt32(0),
							reader.GetString(1),
							reader.GetString(2),
							prefName,
							reader.GetString(4),
							reader.GetString(5),
							reader.GetInt32(6),
							""
						);
					} else {
						return null;
					}
				}

				SqlCommand commandTwo = new SqlCommand(@"SELECT * FROM [UserPreferences] WHERE UserId = @userId;", conn);
				commandTwo.Parameters.AddWithValue("@userId", userId);

				using (SqlDataReader reader = commandTwo.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						user.UserStatus = reader.GetString(2);

					} else {
						return null;
					}
				}
			}

			return user;
		}
	}
}
