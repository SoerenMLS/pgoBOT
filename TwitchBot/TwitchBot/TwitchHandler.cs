using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class TwitchHandler
    {
        private User user = new User();
        static readonly HttpClient httpClient = new HttpClient();

        public TwitchHandler(User User)
        {
            this.user = User;
        }

        public async Task hitApi()
        {



        }


    }
}
