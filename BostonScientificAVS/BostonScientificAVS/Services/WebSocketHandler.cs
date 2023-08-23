using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BostonScientificAVS.Models;
using Context;
using Newtonsoft.Json;

namespace BostonScientificAVS.Services
{
    public class WebSocketHandler
    { 
        private static readonly List<SocketConnection> websocketConnections = new List<SocketConnection>();
        private static readonly object lockObject = new object();



        public async Task SendMessageToSockets(string message, Result result)
        {
            IEnumerable<SocketConnection> toSendTo;

            lock (lockObject)
            {
                toSendTo = websocketConnections.ToList();
            }

            var tasks = toSendTo.Select(async websocketConnection =>
            {
                if (websocketConnection.WebSocket.State == WebSocketState.Open)
                {
                    var jsonData = JsonConvert.SerializeObject(result);
                    var bytes = Encoding.UTF8.GetBytes(jsonData);
                    var arraySegment = new ArraySegment<byte>(bytes);
                    await websocketConnection.WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            });

            await Task.WhenAll(tasks);
        }

        public class SocketConnection
        {
            public Guid Id { get; set; }
            public WebSocket WebSocket { get; set; }
        }

        // Add methods for handling WebSocket connections and disconnections here
    }
}
