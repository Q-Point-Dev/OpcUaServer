using BaseLibraryCode.WebSocketMessaging.Net.Data;
using OpcUaServer.Api.Messages;
using OpcUaServer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpcUaServer.Api
{
  public class ManagerApiHandler
  {

    OpcUaService _service;
    public ManagerApiHandler(OpcUaService service)
    {
      _service = service;
    }

    public string GetResult(string sIncoming)
    {

      PcsEnvelope msg = JsonSerializer.Deserialize<PcsEnvelope>(sIncoming);
      var msgType = JsonSerializer.Deserialize<SMsgBase>(msg.data);

      if (msgType!.szMsgTypeClass == SMsgGetOverviewRequest.MsgTypeClass)
      {
        PcsEnvelope env = new PcsEnvelope();
        env.dest = "Manager";
        env.src = "Service";
        SMsgGetOverviewResponse res = new SMsgGetOverviewResponse();

        res.ServerVersion = _service.GetServerVersion();
        res.OpcUaServerUri = _service.GetOpcUaServerUri();
        res.PluginConfiguration = _service.GetPluginConfiguration();
        res.IsConnectedToMsgDispatcher = _service.GetIsConnectedToMsgDispatcher();
        res.IsRtcConnectionOnline = _service.GetIsRtcConnectionOnline();

        env.data = res.getAsJsonElement();
        string sMsg = JsonSerializer.Serialize(env);
        return sMsg;

      }



/*
      // Sending any valid message back from an auth request should do the job
      if (msgType!.szMsgTypeClass == "SMsgAuthenticate")
      {
        PcsEnvelope env = new PcsEnvelope();
        env.dest = "Manager";
        env.src = "Service";
        SMsgGetMyValueResponse res = new SMsgGetMyValueResponse();
        res.myValue = _service.GetTheString();
        env.data = res.getAsJsonElement();
        string sMsg = JsonSerializer.Serialize(env);
        return sMsg;
      }

      if (msgType!.szMsgTypeClass == SMsgSetMyValue.MsgTypeClass)
      {
        SMsgSetMyValue? msgSet = JsonSerializer.Deserialize<SMsgSetMyValue>(msg.data);
        if (msgSet != null)
          _service.SetTheString(msgSet.myValue);
        return "";
      }*/

      return "";
    }
  }
}
