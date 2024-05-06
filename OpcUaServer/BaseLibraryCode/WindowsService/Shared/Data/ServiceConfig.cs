// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.



using BaseLibraryCode.WebSocketMessaging.Net;

namespace BaseLibraryCode.WindowsService.Net.Shared.Data
{
  /// <summary>
  /// 
  /// </summary>
  public class ServiceConfig
  {

    public string ServiceName { get; set; }


    public ServiceConfig()
    {
      ServiceName = "";
      ManagerWebSocketApiConfig = new ClientConnectionInfo();
    }

    public ClientConnectionInfo ManagerWebSocketApiConfig { get; set; }
  }

}
