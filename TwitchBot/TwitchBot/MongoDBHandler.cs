using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Threading.Tasks;

namespace TwitchBot
{
    class MongoDBHandler
    {
        private MongoClient dbClient { get; set; }


        public MongoDBHandler(string connectionString)
        {
            dbClient = new MongoClient(connectionString);
        }

        public async Task<List<TextCommand>> getAllCommandsAsync()
        {
            var database = dbClient.GetDatabase("txtcommands");
            var collection = database.GetCollection<TextCommand>("commands");

            var documents = await collection.Find(new BsonDocument()).ToListAsync();

            return documents;
        }

        public async Task DeleteCmdAsync(string CommandName)
        {

            var database = dbClient.GetDatabase("txtcommands");
            var collection = database.GetCollection<BsonDocument>("commands");
            var DeleteFilter = Builders<BsonDocument>.Filter.Eq("CommandName", CommandName);

            await collection.DeleteOneAsync(DeleteFilter);
        }

        public async Task InsertCommandAsync(TextCommand command)
        {
            var database = dbClient.GetDatabase("txtcommands");
            var collection = database.GetCollection<TextCommand>("commands");

            await collection.InsertOneAsync(command);

        }

        public async Task UpdateCommandAsync(TextCommand command)
        {
            var database = dbClient.GetDatabase("txtcommands");
            var collection = database.GetCollection<TextCommand>("commands");
            var filter = Builders<TextCommand>.Filter.Eq("CommandName", command.CommandName);
            var update = Builders<TextCommand>.Update.Set("Text", command.Text);

            await collection.UpdateOneAsync(filter, update);

        }


        public async Task<TextCommand> FindCommandAsync(string commandName)
        {
            var database = dbClient.GetDatabase("txtcommands");
            var collection = database.GetCollection<BsonDocument>("commands");

            var filter = Builders<BsonDocument>.Filter.Eq("CommandName", $"{commandName}");

            var firstDoc = await collection.Find(filter).FirstOrDefaultAsync();
            TextCommand cmd = BsonSerializer.Deserialize<TextCommand>(firstDoc);

            return cmd;
        }



        public async Task<User> GetUserInfo(string username)
        {
            var database = dbClient.GetDatabase("txtcommands");
            var collection = database.GetCollection<BsonDocument>("pgoUserInfo");

            var filter = Builders<BsonDocument>.Filter.Eq("username", $"{username}");

            var firstDoc = await collection.Find(filter).FirstOrDefaultAsync();
            User User = BsonSerializer.Deserialize<User>(firstDoc);

            return User;
        }
    }
}
