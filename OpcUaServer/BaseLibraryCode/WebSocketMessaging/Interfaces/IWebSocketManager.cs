// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.


using System.Net.WebSockets;

namespace BaseLibraryCode.WebSocketMessaging.Net;


/// <summary>
/// The parts fo the uri for the remote endpoint
/// </summary>
public class ClientConnectionInfo
{ 
  /// <summary>
  /// eg "ws" (usually), or "wss"
  /// </summary>
  public string? Scheme { get; set; }

  /// <summary>
  /// eg "A-9B56TG3"
  /// </summary>
  public string? Hostname { get; set; }

  /// <summary>
  /// Although the port is a number, it's never used in calculations so is represented in a string
  /// </summary>
  public string? Port { get; set; }

  /// <summary>
  /// Optional suffix for servers that implement several endpoints on the the same port
  /// </summary>
  public string? Path { get; set; }
}


/// <summary>
/// Websocket manager
/// </summary>
public interface IWebSocketManager
{
  /// <summary>
  /// Current websocket connection state
  /// </summary>
  WebSocketState State { get; }

  /// <summary>
  /// Open websocket server connection
  /// </summary>
  /// <param name="connectionInfo">ConnectionInfo object</param>
  /// <param name="token">CancellationToken</param>
  /// <returns><see cref="Task{Boolean}"/>True or False</returns>
  Task<bool> OpenWebsocketServerConnectionAsync(ClientConnectionInfo? connectionInfo, CancellationToken token);

  /// <summary>
  /// Close websocket server connection
  /// </summary>
  /// <param name="token">CancellationToken</param>
  /// <returns><see cref="Task"/></returns>
  Task CloseWebsocketServerConnectionAsync(CancellationToken token);

  /// <summary>
  /// Send data to client websocket server
  /// </summary>
  /// <param name="data">Item</param>
  /// <param name="token">CancellationToken</param>
  /// <returns><see cref="Task"/></returns>
  Task SendDataToWebsocketServerAsync(string? data, CancellationToken token);

  /// <summary>
  /// Receive data from client websocket server
  /// </summary>
  /// <param name="token">CancellationToken</param>
  /// <returns><see cref="Task{String}"/></returns>
  Task<string?> ReceiveDataFromWebsocketServerAsync(CancellationToken token);
}
