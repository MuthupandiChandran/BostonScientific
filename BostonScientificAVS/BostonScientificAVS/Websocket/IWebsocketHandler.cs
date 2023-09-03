using BostonScientificAVS.Models;
using System.Net.WebSockets;
using System.IO.Ports;

namespace BostonScientificAVS.Websocket
{
    public interface IWebsocketHandler
    {
        Task Handle(Guid id, WebSocket websocket);
        Task SendMessageToSockets(string message, PageStatus status);
        Task writeToUDPSocketConnection(String input);
    }
}
