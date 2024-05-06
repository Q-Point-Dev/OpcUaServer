// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.

using BaseLibraryCode.WebSocketMessaging.Net.Data;
using BaseLibraryCode.WindowsService.Net.Api.Models;
using System.Text.Json;


namespace BaseLibraryCode.WindowsService.Net.Api.Messages
{

  public class SMsgServiceStatusRequest : SMsgBase
  {

    public static string MsgTypeClass = "SMsgServiceStatusRequest";

    public SMsgServiceStatusRequest()
    {
      szMsgTypeClass = MsgTypeClass;
    }

    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }


  public class SMsgServiceStatusResponse : SMsgBase
  {

    public static string MsgTypeClass = "SMsgServiceStatusResponse";

    public SMsgServiceStatusResponse()
    {
      szMsgTypeClass = MsgTypeClass;      
    }

    public ServiceStatus? status { get; set; }

    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }


}
