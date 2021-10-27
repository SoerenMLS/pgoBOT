using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchBot
{
    class TextCommand
    {
        public string CommandName { get; set; }
        public string Text { get; set; }
        [BsonId] public ObjectId Id { get; set; }

        public TextCommand(string commandName, string text)
        {
            CommandName = commandName;
            Text = text;
        }

        public override string ToString()
        {
            return $"{CommandName} {Text}";
        }
    }
}
