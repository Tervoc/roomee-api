using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace roomee_api.Models {
	public class User {
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

		public static readonly string[] UpdateNames = { "firstName", "lastName", "preferredName", "password", "email", "statusId" };

		public User(int userId, string firstName, string lastName, string preferredName, string password, string email, int statusId) {
			UserId = userId;
			FirstName = firstName;
			LastName = lastName;
			PreferredName = preferredName;
			Password = password;
			Email = email;
			StatusId = statusId;
		}

		public static User FromUserId(int userId) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [User] WHERE UserId = @param1;", conn);
				command.Parameters.AddWithValue("@param1", userId);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						string prefName = string.Empty;

						if (!reader.IsDBNull(3)) {
							prefName = reader.GetString(3);
						}

						return new User(
							reader.GetInt32(0),
							reader.GetString(1),
							reader.GetString(2),
							prefName,
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

		public static User FromEmail(string email) {
			using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)) {
				conn.Open();

				SqlCommand command = new SqlCommand(@"SELECT * FROM [User] WHERE Email = @param1;", conn);
				command.Parameters.AddWithValue("@param1", email);

				using (SqlDataReader reader = command.ExecuteReader()) {
					if (reader.HasRows) {
						reader.Read();

						string prefName = string.Empty;

						if (!reader.IsDBNull(3)) {
							prefName = reader.GetString(3);
						}

						return new User(
							reader.GetInt32(0),
							reader.GetString(1),
							reader.GetString(2),
							prefName,
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

		public List<Claim> ToClaims() {
			return new List<Claim> {
				new Claim("userId", UserId.ToString()),
				new Claim("email", Email),
				new Claim("firstName", FirstName),
				new Claim("lastName", LastName),
				new Claim("preferredName", PreferredName),
				new Claim("statusId", StatusId.ToString())
			};
		}

	}
}
