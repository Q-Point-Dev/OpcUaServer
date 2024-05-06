// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.

using BaseLibraryCode.WebSocketMessaging.Net.Data;
using BaseLibraryCode.WindowsService.Net.Api.Models;
using System.Text.Json;


namespace BaseLibraryCode.WindowsService.Net.Api.Messages
{
  internal class JournalEntriesMessage
  {
  }


  public class SMsgJournalEntriesReq : SMsgBase
  {

    public static string MsgTypeClass = "SMsgJournalEntriesReq";

    public SMsgJournalEntriesReq()
    {
      szMsgTypeClass = MsgTypeClass;
    }

    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }

  public class SMsgJournalEntriesResponse : SMsgBase
  {

    public static string MsgTypeClass = "SMsgJournalEntriesResponse";

    public SMsgJournalEntriesResponse()
    {
      szMsgTypeClass = MsgTypeClass;
      entries = new List<JournalEntry>();
    }

    public List<JournalEntry> entries { get; set; }

    override public JsonElement getAsJsonElement()
    {
      JsonElement elJSON = JsonSerializer.SerializeToElement(this);
      return elJSON;
    }
  }



}
