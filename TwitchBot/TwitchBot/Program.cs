using System;
using System.Threading.Tasks;

namespace TwitchBot
{
    class Program
    {
        static async Task Main(string[] args)
        {

            IRCbot tBot = new IRCbot();

            await tBot.RunBot();

        }
    }
}
