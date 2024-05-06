// © 2023 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.

using System.Text.Json;

namespace BaseLibraryCode.WebSocketMessaging.Net.Data
{

  /// <summary>
  /// Base class for all messages
  /// </summary>
  public class SMsgBase
  {
    public string szMsgTypeClass { get; set; }
    public int MsgType { get; set; }

    public SMsgBase()
    {
      szMsgTypeClass = "";
    }

    /* Must be implemented in derived classes. The actual code in the derived class is identical to the code below, but
     there is a subtle difference in that "this" in the derived class refers to the derived class (and it's properties)*/
    public virtual JsonElement getAsJsonElement() { return new JsonElement(); }


  }
}
