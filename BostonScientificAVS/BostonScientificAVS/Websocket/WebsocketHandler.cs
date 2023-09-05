using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BostonScientificAVS.Models;
using System.IO.Ports;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;

namespace BostonScientificAVS.Websocket
{
  public class WebsocketHandler : IWebsocketHandler
  {
    public List<SocketConnection> websocketConnections = new List<SocketConnection>();
    public UdpClient _udpSocket;
        private TcpClient tcpClient;
        private NetworkStream stream;
        public WebsocketHandler(UdpClient udpSocket)
    {
      Console.WriteLine("Constructor Called.....................");
      SetupCleanUpTask();
      _udpSocket = udpSocket;
       
        }

    public async Task Handle(Guid id, WebSocket webSocket)
    {
      Boolean connectionEstablished = false;
      lock (websocketConnections)
      {
        websocketConnections.Add(new SocketConnection
        {
          Id = id,
          WebSocket = webSocket
        });
      }

      while (webSocket.State == WebSocketState.Open)
      {
        try
        {
          if (!connectionEstablished)
          {
            connectionEstablished = true;
            openUDPSocketConnection();
             StartListener("127.0.0.1", 3001);
                    }
          var message = await ReceiveMessage(id, webSocket);

          if (message != null)
          {
            int startIndex = message.IndexOf('{');
            string jsonMessage = message.Substring(startIndex);
            var parsedMessage = JsonConvert.DeserializeObject<dynamic>(jsonMessage);
            string text = parsedMessage.text;
            if (text == "application closed!")
            {
              closeUDPSocketConnection();
            }
            writeToUDPSocketConnection(text);
            PageStatus status = new PageStatus();
            await SendMessageToSockets(message, status);
          }
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }


      }
    }

    public void openUDPSocketConnection()
        {
            try
            {
                _udpSocket = new UdpClient();
                string ipaddress = DotNetEnv.Env.GetString("CLIENT_IP_ADDRESS");
                int port = DotNetEnv.Env.GetInt("CLIENT_PORT");
                _udpSocket.Connect(new IPEndPoint(IPAddress.Parse(ipaddress), port));
                PageStatus status = new PageStatus();
                Error error = new Error();
                error.errorMsg = "UDP Connection Successful";
                status.pageError = error;
                SendMessageToSockets("message from server", status);
                Console.WriteLine("UDP Connection Established.....................");
            }
            catch(Exception ex)
            {
                PageStatus status = new PageStatus();
                Error error = new Error();
                error.errorMsg = "Error in UDP Socket Communication";
                status.pageError = error;
                SendMessageToSockets("message from server", status);
                Console.WriteLine("Error establishing UDP connection........");
                Console.WriteLine(ex);
            }
   
     }


        public void closeUDPSocketConnection()
        {
            try
            {
                _udpSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    private async Task<string> ReceiveMessage(Guid id, WebSocket webSocket)
    {
      var arraySegment = new ArraySegment<byte>(new byte[4096]);
      var receivedMessage = await webSocket.ReceiveAsync(arraySegment, CancellationToken.None);
      if (receivedMessage.MessageType == WebSocketMessageType.Text)
      {
        var message = Encoding.Default.GetString(arraySegment).TrimEnd('\0');
        if (!string.IsNullOrWhiteSpace(message))
          return $"<b>{id}</b>: {message}";
      }
      return null;
    }

    public async Task SendMessageToSockets(string message, PageStatus pageStatus)
    {
      IEnumerable<SocketConnection> toSentTo;

      lock (websocketConnections)
      {
        toSentTo = websocketConnections.ToList();
      }

      var tasks = toSentTo.Select(async websocketConnection =>
      {
        if (websocketConnection.WebSocket.State == WebSocketState.Open)
        {
          var jsonData = JsonConvert.SerializeObject(pageStatus);
          var bytes = Encoding.Default.GetBytes(jsonData);
          var arraySegment = new ArraySegment<byte>(bytes);
          await websocketConnection.WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
      });
      await Task.WhenAll(tasks);
    }

    private void SetupCleanUpTask()
    {
      Task.Run(async () =>
      {
        while (true)
        {
          IEnumerable<SocketConnection> openSockets;
          IEnumerable<SocketConnection> closedSockets;
          lock (websocketConnections)
          {
            openSockets = websocketConnections.Where(x => x.WebSocket.State == WebSocketState.Open || x.WebSocket.State == WebSocketState.Connecting);
            closedSockets = websocketConnections.Where(x => x.WebSocket.State != WebSocketState.Open && x.WebSocket.State != WebSocketState.Connecting);
            websocketConnections = openSockets.ToList();
          }
          foreach (var closedWebsocketConnection in closedSockets)
          {
            PageStatus status = new PageStatus();
            await SendMessageToSockets($"Following socket connection is closed {closedWebsocketConnection.Id}", status);
          }

          await Task.Delay(5000);
        }

      });
    }

    public async Task writeToUDPSocketConnection(String input)
        {
            try
            {
                // Check if the UDP client socket is closed
                if (_udpSocket.Client == null || !_udpSocket.Client.Connected)
                {
                    openUDPSocketConnection();
                    Console.WriteLine("send Message");
                }
             
                byte[] data = Encoding.ASCII.GetBytes(input);
                _udpSocket.Send(data, data.Length);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Writing to UDP Socket Connection");
                Console.WriteLine(e.ToString());
            }
        }
        public void StartListener(string ip, Int32 port)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.

                // Prefer a using declaration to ensure the instance is Disposed later.
                tcpClient = new TcpClient(ip, port);

                ListenAsync();
                // Receive the server response.

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }


        }
        public void StopListener(string ip, Int32 port)
        {
            try
            {
                stream.Close();
                tcpClient.Close();

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }


        }

        private async Task ListenAsync()
        {
            while (tcpClient.Connected)
            {
                // Get a client stream for reading and writing.
                stream = tcpClient.GetStream();
                var buffer = new byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(buffer, 0, buffer.Length);
                responseData = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);
                PageStatus status = new PageStatus();
                Error error = new Error();
                error.errorMsg = responseData;
                status.pageError = error;
                await SendMessageToSockets("FromTCPClient",status);
            }
        }

    }


  public class SocketConnection
  {
    public Guid Id { get; set; }
    public WebSocket WebSocket { get; set; }
  }
}
