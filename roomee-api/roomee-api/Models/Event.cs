//Author(s): Schmidt, Max(max.schmidt@ttu.edu)
//Date Created: 03 / 07 / 2021
//Notes: N/A
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace roomee_api.Models
{
    public class Event{
        [JsonProperty(PropertyName = "eventId")]
        public int EventId { get; }

        [JsonProperty(PropertyName = "creatorUserId")]
        public int CreatorUserId { get; }

        [JsonProperty(PropertyName = "roomId")]
        public int RoomId { get; set; }

        [JsonProperty(PropertyName = "creationTimestamp")]
        public DateTime CreationTimestamp { get; }

        [JsonProperty(PropertyName = "startTimestamp")]
        public DateTime StartTimestamp { get; set; }

        [JsonProperty(PropertyName = "endTimestamp")]
        public DateTime EndTimestamp { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "statusId")]
        public int StatusId { get; }

        public static readonly string[] UpdateNames = { "roomId", "startTimestamp", "endTimestamp", "title", "description", "statusId" };

        public Event(int eventId, int creatorUserId, int roomId, DateTime creationTimestamp, DateTime startTimestamp, DateTime endTimestamp, string title, string description, int statusId)
        {
            EventId = eventId;
            CreatorUserId = creatorUserId;
            RoomId = roomId;
            CreationTimestamp = creationTimestamp;
            StartTimestamp = startTimestamp;
            EndTimestamp = endTimestamp;
            Title = title;
            Description = description;
            StatusId = statusId;
        }

        public static Event FromEventId(int eventId){
            using (SqlConnection conn = new SqlConnection(Startup.ConnectionString)){
                conn.Open();

                SqlCommand command = new SqlCommand(@"SELECT * FROM [Event] WHERE EventId = @param1;", conn);
                command.Parameters.AddWithValue("@param1", eventId);

                using (SqlDataReader reader = command.ExecuteReader()){
                    if (reader.HasRows){
                        reader.Read();

                        return new Event(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetDateTime(3),
                            reader.GetDateTime(4),
                            reader.GetDateTime(5),
                            reader.GetString(6),
                            reader.GetString(7),
                            reader.GetInt32(8)
                        );
                    }
                    else{
                        return null;
                    }
                }
            }
        }
    }
}
