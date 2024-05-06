// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.




using BaseLibraryCode.WebSocketMessaging.Net.Data;
using BaseLibraryCode.WindowsService.Net.Api;
using BaseLibraryCode.WindowsService.Net.Api.Messages;
using BaseLibraryCode.WindowsService.Net.Shared.Data;
using BaseLibraryCode.WindowsService.Net.Utilities;
using BaseLibraryCode.WindowsService.Net.WebSocketApi;
using System.Text.Json;


namespace BaseLibraryCode.WindowsService.Net
{
  public class BackgroundServiceBase : BackgroundService
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureBaseServices(IServiceCollection services)
    {
      services.AddSingleton<IStatusSentinelService>(new StatusSentinelService());
      // services.AddSingleton<IWriteJournalEntry>(new WriteJournalEntry());  
    }

    private readonly ILogger<BackgroundServiceBase> _logger;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// The web socket that will allow manager clients to connect to sens and receive manager messages 
    /// </summary>
    protected ManagerApiWebSocketServer? m_webSocketManagerApiServer;

    /// <summary>
    /// 
    /// </summary>
    private ServiceConfig m_serviceConfig;

    /// <summary>
    /// Encapsulates handlers for standard manager messages
    /// </summary>
    protected WindowsServiceBaseApiHandler m_windowsServiceBaseApiHandler;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ServiceConfig GetConfig() { return m_serviceConfig; }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="appLifetime"></param>
    public BackgroundServiceBase(
      ILogger<BackgroundServiceBase> logger,
      ILoggerFactory loggerFactory, IHostApplicationLifetime appLifetime)
    {
      _logger = logger;
      _loggerFactory = loggerFactory;
      m_serviceConfig = new ServiceConfig();
      m_windowsServiceBaseApiHandler = new WindowsServiceBaseApiHandler(m_serviceConfig);

      var sss = BaseServiceAppEnvironment.ServiceProvider!.GetService(typeof(IStatusSentinelService)) as IStatusSentinelService;
      sss!.getOnServiceStatusChanged().Subscribe( newStat =>
      {
        SMsgServiceStatusResponse res = new SMsgServiceStatusResponse();
        res.status = newStat;
        PcsEnvelope env = new PcsEnvelope();
        env.dest = "Manager";
        env.src = "Service";
        env.data = res.getAsJsonElement();
        string sMsg = JsonSerializer.Serialize(env);
        if(m_webSocketManagerApiServer != null)
          m_webSocketManagerApiServer.Broadcast(sMsg);
      });

    }

    /// <summary>
    /// 
    /// </summary>
    protected void StartManagerWebSocketApiServer()
    {
      m_webSocketManagerApiServer = ServiceFactory.Create<ManagerApiWebSocketServer>(_loggerFactory);
      m_webSocketManagerApiServer!.Configure(this.GetConfig().ManagerWebSocketApiConfig, ProcessIncomingManagerMessage);
      m_webSocketManagerApiServer.Start();
    }

    protected void SetServiceStatusDescription(string desc)
    {
      var sss = BaseServiceAppEnvironment.ServiceProvider!.GetService(typeof(IStatusSentinelService)) as IStatusSentinelService;
      sss!.SetStatusDescription(desc);
    }


    /// <summary>
    /// Just to keep compiler happy
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      return Task.Run(() =>
      {
        try
        {
          while (!stoppingToken.IsCancellationRequested)
          {

          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }
      });
    }

    /// <summary>
    /// Routes common manager messages to common handler
    /// </summary>
    /// <param name="sIncoming"></param>
    /// <returns></returns>
    virtual public string ProcessIncomingManagerMessage(string sIncoming)
    {
      try
      {
        return m_windowsServiceBaseApiHandler.HandleIncomingManagerMessage(sIncoming);  
      }
      catch (Exception exp)
      {
        return exp.Message;
      }

    }


  }



}
