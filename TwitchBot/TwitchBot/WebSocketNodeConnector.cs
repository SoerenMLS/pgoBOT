using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchBot
{
    class WebSocketNodeConnector
    {

        private string connection = "ws://localhost:3000";

        public async Task ConnectToServer()
        {
            do
            {
                using (var clientSocket = new ClientWebSocket())
                {
                    try
                    {
                        await clientSocket.ConnectAsync(new Uri(connection), CancellationToken.None);

                        await Send(clientSocket, "Hello");
                        await Receive(clientSocket);

                    }
                    catch (Exception)
                    {
                        throw;
                    }


                }
            } while (true);


        }

        private async Task Receive(ClientWebSocket clientSocket)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            do
            {
                WebSocketReceiveResult result;

                using (var memStream = new MemoryStream())
                {
                    do
                    {
                        result = await clientSocket.ReceiveAsync(buffer, CancellationToken.None);
                        memStream.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    memStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(memStream, Encoding.UTF8))
                    {
                        Console.WriteLine("From WebSocket:" + await reader.ReadToEndAsync());
                    }
                }

            } while (true);
        }

        private async Task Send(ClientWebSocket clientSocket, string data)
        {
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
