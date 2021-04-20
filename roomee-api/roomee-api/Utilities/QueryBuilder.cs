/*
 * Author(s): Padgett, Matt matthew.padgett@ttu.edu
 * Date Created: February 15 2021
 * Notes: N/A
*/
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace roomee_api.Utilities {
	public class QueryBuilder
	{
		public static SqlCommand UpdateBuilder<T>(string proc, int id, Dictionary<string, string> patch, string token)
		{
			SqlCommand command = new SqlCommand(proc);
			command.CommandType = System.Data.CommandType.StoredProcedure;

			PropertyInfo[] properties = typeof(T).GetProperties();

			command.Parameters.AddWithValue("@" + properties[0].Name, id);
			command.Parameters.AddWithValue("@ModifierUserId", Authentication.ReadToken(token)["userId"]);

			for (int i = 1; i < properties.Length; i++)
			{
				string name = properties[i].Name;
				command.Parameters.AddWithValue("@" + name, patch.ContainsKey(ToCamelCase(name)) ? patch[ToCamelCase(name)] : (object)DBNull.Value);
			}

			return command;
		}

		public static SqlCommand InsertBuilder<T>(string proc, T newObject, string token)
		{
			SqlCommand command = new SqlCommand(proc);
			command.CommandType = System.Data.CommandType.StoredProcedure;

			command.Parameters.AddWithValue("@ModifierUserId", Authentication.ReadToken(token)["userId"]);

			foreach (PropertyInfo info in typeof(T).GetProperties())
			{
				string name = info.Name;

				if (info.CanWrite)
				{
					command.Parameters.AddWithValue("@" + name, info.GetValue(newObject) != null ? info.GetValue(newObject) : (object)DBNull.Value);
				}
			}

			return command;
		}

		public static string ToCamelCase(string str)
		{
			return char.ToLower(str[0]) + str.Substring(1);
		}
	}
}
