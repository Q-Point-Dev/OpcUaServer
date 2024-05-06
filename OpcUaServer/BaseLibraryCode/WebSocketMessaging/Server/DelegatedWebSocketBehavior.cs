
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BaseLibraryCode.WebSocketMessaging.Net.Server
{
  /// <summary>
  /// Generic class to delegate the interesting socket processing back to the parent.
  /// </summary>
  public class DelegatedWebSocketBehavior : WebSocketBehavior
  {

    private IDelegatedWebSocketBehaviorCallback _parent;


    public DelegatedWebSocketBehavior(IDelegatedWebSocketBehaviorCallback parent)
    {
      _parent = parent;
    }

    public void SendData(string sData)
    {
      Send(sData);
    }


    protected override void OnMessage(MessageEventArgs e)
    {
      _parent.OnMessageFromClient(this, e);
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
      base.OnError(e);
    }

    protected override void OnClose(CloseEventArgs e)
    {
      _parent.RemoveClient(this);
      base.OnClose(e);
    }

    protected override void OnOpen()
    {
      _parent.AddClient(this);
      base.OnOpen();
    }


  }
}
