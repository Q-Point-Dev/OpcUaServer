// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.

using BaseLibraryCode.WebSocketMessaging.Net.Data;
using BaseLibraryCode.WindowsService.Net.Api.Messages;
using BaseLibraryCode.WindowsService.Net.Api.Models;
using BaseLibraryCode.WindowsService.Net.Shared.Data;
using System.Text.Json;


namespace BaseLibraryCode.WindowsService.Net.Api
{
  public class WindowsServiceBaseApiHandler
  {

    private ServiceConfig _serviceConfig;

    public WindowsServiceBaseApiHandler(ServiceConfig serviceConfig)
    {
      _serviceConfig = serviceConfig;
    }

    public string HandleIncomingManagerMessage(string sIncoming)
    {
      try
      {
        if (!IsValidJson(sIncoming))
        {
          return "not valid json";
        }

        if (!IsPcsEnvelopeMessage(sIncoming))
        {
          return "not valid PcsEnvelope";
        }

        PcsEnvelope msg = JsonSerializer.Deserialize<PcsEnvelope>(sIncoming);
        var msgType = JsonSerializer.Deserialize<SMsgBase>(msg.data);

        //
        if (msgType!.szMsgTypeClass == SMsgServiceStatusRequest.MsgTypeClass)
        {
          GetServiceStatus gss = new GetServiceStatus();
          ServiceStatus s = gss.GetStatus();

          SMsgServiceStatusResponse res = new SMsgServiceStatusResponse();
          res.status = s;

          PcsEnvelope env = new PcsEnvelope();
          env.dest = "Manager";
          env.src = "Service";
          env.data = res.getAsJsonElement();
          string sMsg = JsonSerializer.Serialize(env);
          return sMsg;
        }

        //
        if (msgType!.szMsgTypeClass == SMsgJournalEntriesReq.MsgTypeClass)
        {
          GetJournalEntries gje = new GetJournalEntries();
          var lst = gje.GetEntries(50, _serviceConfig.ServiceName);

          SMsgJournalEntriesResponse res = new SMsgJournalEntriesResponse();
          res.entries = lst;

          PcsEnvelope env = new PcsEnvelope();
          env.dest = "Manager";
          env.src = "Service";
          env.data = res.getAsJsonElement();
          string sMsg = JsonSerializer.Serialize(env);
          return sMsg;
        }

        // proces other message here.



        // Message not recognised so not processed
        return "";


      }
      catch (Exception exp)
      {
        //  Debug.WriteLine($"Exception - WebsocketManager.OnMessage: {exp}");
        return exp.Message;
      }

    }




    public static bool IsValidJson(string strInput)
    {
      strInput = strInput.Trim();
      if ((!strInput.StartsWith("{") || !strInput.EndsWith("}")) &&
          (!strInput.StartsWith("[") || !strInput.EndsWith("]")))
      {
        return false;
      }

      try
      {
        JsonDocument jsonDoc = JsonDocument.Parse(strInput);
        return true;
      }
      catch (System.Text.Json.JsonException jex)
      {
        Console.WriteLine(jex.Message);
        return false;
      }
      catch (Exception ex) //some other exception
      {
        Console.WriteLine(ex.Message);
        return false;
      }
    }


    public static bool IsPcsEnvelopeMessage(string strInput)
    {
      strInput = strInput.Trim();
      if ((!strInput.StartsWith("{") || !strInput.EndsWith("}")) &&
          (!strInput.StartsWith("[") || !strInput.EndsWith("]")))
      {
        return false;
      }

      try
      {
        PcsEnvelope msg = JsonSerializer.Deserialize<PcsEnvelope>(strInput);
        return true;
      }
      catch (System.Text.Json.JsonException jex)
      {
        Console.WriteLine(jex.Message);
        return false;
      }
      catch (Exception ex) //some other exception
      {
        Console.WriteLine(ex.Message);
        return false;
      }
    }

  }
}
