using BaseLibraryCode.WebSocketMessaging.Net.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpcUaServer.Api.Messages
{

  public class SMsgGetOverviewRequest : SMsgBase
  {

    public static string MsgTypeClass = "SMsgGetOverviewRequest";

    public SMsgGetOverviewRequest()
    {
      szMsgTypeClass = MsgTypeClass;
    }

    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }

  public class SMsgGetOverviewResponse : SMsgBase
  {

    public static string MsgTypeClass = "SMsgGetOverviewResponse";

    public SMsgGetOverviewResponse()
    {
      szMsgTypeClass = MsgTypeClass;
      ServerVersion = "";
      OpcUaServerUri = "";
      PluginConfiguration = "";
    }

    public string ServerVersion { get; set; }

    public string OpcUaServerUri { get; set; }


    public string PluginConfiguration { get; set; }

    public bool IsConnectedToMsgDispatcher { get; set; }

    public bool IsRtcConnectionOnline { get; set; }

    public bool IsOpcUaServerStarted { get; set; }



    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }


  public class SMsgServiceStatusChanged : SMsgBase
  {

    public static string MsgTypeClass = "SMsgServiceStatusChanged";

    public SMsgServiceStatusChanged()
    {
      szMsgTypeClass = MsgTypeClass;
    }

    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }
}
