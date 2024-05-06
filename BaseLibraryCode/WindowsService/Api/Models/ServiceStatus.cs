



namespace BaseLibraryCode.WindowsService.Net.Api.Models
{


  public class ServiceStatus
  {
    public bool connectedViaWebsocketApi { get; set; }
    public string description { get; set; }


    public ServiceStatus()
    {
      connectedViaWebsocketApi = false;
      description = "Unconnected";
    }

  }
}
