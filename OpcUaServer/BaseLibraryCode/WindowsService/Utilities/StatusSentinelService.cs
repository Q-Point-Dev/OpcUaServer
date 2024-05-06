// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.


using BaseLibraryCode.WindowsService.Net.Api.Models;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BaseLibraryCode.WindowsService.Net.Utilities
{



  public interface IStatusSentinelService
  {
    IObservable<ServiceStatus> getOnServiceStatusChanged();

    ServiceStatus GetCurrentServiceStatus();

    void SetStatusDescription(string newStatusDescription);


  }
  /// <summary>
  /// Service to allow consumers to set the current status, and to publish any changes to status to subscribers (remote managers)
  /// </summary>
  public class StatusSentinelService: IStatusSentinelService
  {



    /// <summary>
    /// Publishes incoming data
    /// </summary>
    public IObservable<ServiceStatus> onServiceStatusChanged => _serviceStatusChangedSubject.AsObservable();
    private readonly BehaviorSubject<ServiceStatus> _serviceStatusChangedSubject = new BehaviorSubject<ServiceStatus>(new ServiceStatus());

    /// <summary>
    /// The current status
    /// </summary>
    private ServiceStatus _currentStatus = new ServiceStatus();

    public IObservable<ServiceStatus> getOnServiceStatusChanged()
    {
      return onServiceStatusChanged;
    }

    public ServiceStatus GetCurrentServiceStatus()
    {
      return _currentStatus;
    }

    public void SetStatusDescription(string newStatusDescription)
    {
      if(_currentStatus.description == newStatusDescription)
        return;

      _currentStatus.description = newStatusDescription;
      _serviceStatusChangedSubject.OnNext(_currentStatus);
    }

  }


}
