

using BaseLibraryCode.WebSocketMessaging.Net.Data;
using System.Text.Json;


namespace BaseLibraryCode.WindowsService.Net.Api.Messages
{

  public class SMsgTimeOfDay : SMsgBase
  {

    public static string MsgTypeClass = "SMsgTimeOfDay";

    public SMsgTimeOfDay()
    {
      szMsgTypeClass = MsgTypeClass;
      tod = "";
    }

    public string tod { get; set; }

    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }


}
