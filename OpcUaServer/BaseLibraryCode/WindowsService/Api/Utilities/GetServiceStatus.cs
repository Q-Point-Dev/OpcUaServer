// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.




using BaseLibraryCode.WindowsService.Net.Api.Models;

namespace BaseLibraryCode.WindowsService.Net.Api
{
  public class GetServiceStatus
  {
    public ServiceStatus GetStatus()
    {
      ServiceStatus s = new ServiceStatus();
      s.description = "OK";
      s.connectedViaWebsocketApi = true;
      return s;
    }

  }











}
