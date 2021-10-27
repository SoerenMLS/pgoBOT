using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace TwitchBot
{
    class IRCbot
    {
        private string Ip = "irc.chat.twitch.tv";
        private int Port = 6667;
        private string OauthPW = "PLACEHOLDER";
        private string BotName = "BOTNAME";
        private string channelName = "CHANNELNAME";
        private string commandPrefix = "!";
        private MongoDBHandler dbHandler = new MongoDBHandler("MONGODBCONNECTIONPLACEHOLDER");
        private List<TextCommand> commandList = new List<TextCommand>();
        WebSocketNodeConnector wsServerConnection = new WebSocketNodeConnector();
        private User user = new User();

        private TcpClient tcpClient = new TcpClient();

        //public IRCbot(string oauthPW, string botName, string channelName, string commandPrefix)
        //{
        //    OauthPW = oauthPW;
        //    BotName = botName;
        //    this.channelName = channelName;
        //    this.commandPrefix = commandPrefix;
        //}

        private StreamReader streamReader { get; set; }
        private StreamWriter streamWriter { get; set; }

        private async Task InitBot()
        {
            await tcpClient.ConnectAsync(Ip, Port);

            streamReader = new StreamReader(tcpClient.GetStream());
            streamWriter = new StreamWriter(tcpClient.GetStream()) { NewLine = "\n\r", AutoFlush = true};

            commandList = await dbHandler.getAllCommandsAsync();
            user = await dbHandler.GetUserInfo(channelName);
            await streamWriter.WriteLineAsync($"PASS {OauthPW}");
            await streamWriter.WriteLineAsync($"NICK {BotName}");
            await streamWriter.WriteLineAsync($"JOIN #{channelName}");

            Console.WriteLine($"Connection established to {channelName}");

        }

        public async Task RunBot()
        {
            await InitBot();

            bool running = true;           

            do
            {
                await ListeningLoop();
                await wsServerConnection.ConnectToServer();
                if (Console.ReadLine() == "quit")
                    running = false;
            } while (running);

        }

        private async Task ListeningLoop()
        {
            while (true)
            {
                string line = await streamReader.ReadLineAsync();
                Console.WriteLine(line);

                await HandleInput(line);
            }
        }

        private async Task HandleInput(string line)
        {
            string[] splitStr = line.Split(" ");
            if (line.StartsWith("PING"))
            {
                Console.WriteLine("PING");
                await streamWriter.WriteLineAsync($"PONG {splitStr[1]}");
            }

            await CommandHandlerAsync(splitStr);

        }

        private async Task CommandHandlerAsync(string[] splitStr)
        {
            if (splitStr[1] == "PRIVMSG")
            {
                int ExclMrkIndex = splitStr[0].IndexOf('!');
                string messageSender = splitStr[0].Substring(1, ExclMrkIndex - 1);
                string message = splitStr[3].Trim(':');

                if (message.StartsWith(commandPrefix) && message.Length >= 2)
                {
                    await CommandInvoker(splitStr, message);
                }
            }
        }

        private async Task CommandInvoker(string[] splitStr, string message)
        {
            if (message == $"{commandPrefix}newcmd")
            {
                await CreateNewCmdAsync(splitStr);
            } 
            else if (message == $"{commandPrefix}updatecmd")
            {
                await UpdateCmdAsync(splitStr);
            } 
            else if (message == $"{commandPrefix}deletecmd")
            {
                await DeleteCmdAsync(splitStr);

            }
            else if (commandList.Any(x => x.CommandName == message))
            {
                TextCommand cmd = await dbHandler.FindCommandAsync(message);
                Console.WriteLine("Command: " + cmd.CommandName + " has been called");
                await streamWriter.WriteLineAsync($"PRIVMSG #{channelName} :{cmd.Text}");
            }

        }

        private async Task DeleteCmdAsync(string[] splitStr)
        {
            string cmdToDelete = splitStr[4];

            if (commandList.Any(x => x.CommandName == cmdToDelete))
            {
                commandList.RemoveAll(x => x.CommandName == cmdToDelete);
                await dbHandler.DeleteCmdAsync(cmdToDelete);
                await streamWriter.WriteLineAsync($"PRIVMSG #{channelName} :\"{cmdToDelete}\" has been deleted");
                Console.WriteLine("Command: " + cmdToDelete + " has been deleted");
            }
            else
                await streamWriter.WriteLineAsync($"PRIVMSG #{channelName} :command: \"{cmdToDelete}\" does not exist");
        }

        private async Task UpdateCmdAsync(string[] splitStr)
        {
            string cmdToUpdate = splitStr[4];

            if (commandList.Any(x => x.CommandName == cmdToUpdate))
            {
                string textUpdate = TextRepairer(splitStr);
                TextCommand updatedCmd = new TextCommand(cmdToUpdate, textUpdate);

                commandList.RemoveAll(x => x.CommandName == cmdToUpdate);
                commandList.Add(updatedCmd);

                await dbHandler.UpdateCommandAsync(updatedCmd);
                Console.WriteLine("Command updated: " + updatedCmd);
                await streamWriter.WriteLineAsync($"PRIVMSG #{channelName} :\"{cmdToUpdate}\" has been updated");
            } else
                await streamWriter.WriteLineAsync($"PRIVMSG #{channelName} :Command \"{cmdToUpdate}\" does not exist");
        }

        private async Task CreateNewCmdAsync(string[] splitStr)
        {
            string newCommandCall = splitStr[4];

            if(!(commandList.Any(x => x.CommandName == newCommandCall)))
            {
                string commandText = TextRepairer(splitStr);
                TextCommand newCommand = new TextCommand(newCommandCall, commandText);
                commandList.Add(newCommand);
                await dbHandler.InsertCommandAsync(newCommand);
                Console.WriteLine("New command added: " + newCommand);
            }
            else
            {
                await streamWriter.WriteLineAsync($"PRIVMSG #{channelName} :Command \"{newCommandCall}\" already exists");
            }

        }

        private static string TextRepairer(string[] splitStr)
        {
            string textToFix = "";                                     

            for (int i = 0; i < splitStr.Length - 5; i++)
            {
                textToFix += String.Join(" ", splitStr[i + 5]);     //This can probably be made into something more practical than string.join
                textToFix += String.Join(" ", " ");
            }

            return textToFix;
        }

    }
}
