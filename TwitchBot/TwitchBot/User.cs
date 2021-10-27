using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchBot
{
    class User
    {
        [BsonId] public ObjectId Id { get; set; }
        private string uName;
        private int expires_in;
        private string access_token;
        private string refresh_token;
        private string[] scope;

        public override string ToString()
        {
            return $"{uName}";
        }
    }
}