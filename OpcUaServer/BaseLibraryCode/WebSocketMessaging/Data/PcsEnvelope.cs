// © 2023 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.


using System.Text.Json;

namespace BaseLibraryCode.WebSocketMessaging.Net.Data
{
  /// <summary>
  /// Encapsulates a message going from dest to src. The actual message data will depend on what the client sends
  /// and what the server gives back.
  /// </summary>
  public struct PcsEnvelope
  {
    public string src { get; set; }
    public string dest { get; set; }
    public JsonElement data { get; set; }
  }


}
