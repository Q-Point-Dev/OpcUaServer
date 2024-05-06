


using WebSocketSharp;

namespace BaseLibraryCode.WebSocketMessaging.Net.Server
{
  /// <summary>
  /// The interface that a parent websocket server must implement in order to use the generic DelegatedWebSocketBehavior.
  /// </summary>
  public interface IDelegatedWebSocketBehaviorCallback
  {
    void AddClient(DelegatedWebSocketBehavior cli);

    void RemoveClient(DelegatedWebSocketBehavior cli);

    void OnMessageFromClient(DelegatedWebSocketBehavior cli, MessageEventArgs e);
  }
}
