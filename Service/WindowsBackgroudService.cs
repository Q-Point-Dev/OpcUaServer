//----------------------------------------------------------------------------------------------------------------------
// AMMANN GROUP CH-4900 LANGENTHAL                   © 2023 Alle Rechte vorbehalten
//
// Vervielfältigung, Veröffentlichung oder Weitergabe dieses Dokuments oder Teilen daraus sind, zu welchem Zweck und
// in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die Ammann-Group nicht gestattet.
//
// Ersteller der Datei:   Michael Serzhan
//----------------------------------------------------------------------------------------------------------------------
using BaseLibraryCode.WebSocketMessaging.Net.Data;
using BaseLibraryCode.WindowsService.Net;
using BaseLibraryCode.WindowsService.Net.Api;
using BaseLibraryCode.WindowsService.Net.Api.Messages;
using BaseLibraryCode.WindowsService.Net.Shared.Data;
using OpcUaServer.Api;
using OpcUaServer.Api.Messages;
using OpcUaServer.Service;
using System.Text.Json;


namespace OpcUaServer
{
  public class WindowsBackgroundService : BackgroundServiceBase
  {

    private readonly ILogger<WindowsBackgroundService> _logger;

    private OpcUaService? _service;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="appLifetime"></param>
    public WindowsBackgroundService(
      ILogger<WindowsBackgroundService> logger,
      ILoggerFactory loggerFactory, IHostApplicationLifetime appLifetime)
      : base(logger, loggerFactory, appLifetime)
    {
      _logger = logger;
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      try
      {
        var loggerFactory = AppEnvironment.LoggerFactory;
        if (null == loggerFactory)
        {
          loggerFactory = new LoggerFactory();
        }

        _service = new OpcUaService(loggerFactory);

        _logger.LogInformation("Before service start");
        bool started = _service.Start();
        _logger.LogInformation($"After service start {started}");

        ServiceConfig cfg = GetConfig();
        cfg.ServiceName = "OpcUaService";
        cfg.ManagerWebSocketApiConfig.Scheme = "ws";
        cfg.ManagerWebSocketApiConfig.Path = "/";
        cfg.ManagerWebSocketApiConfig.Port = "20042";
        StartManagerWebSocketApiServer();

        _service!.OnPluginStatusChanged += (sender, e) =>
        {
          SMsgServiceStatusChanged res = new SMsgServiceStatusChanged();
          PcsEnvelope env = new PcsEnvelope();
          env.dest = "Manager";
          env.src = "Service";
          env.data = res.getAsJsonElement();
          string sMsg = JsonSerializer.Serialize(env);
          if (m_webSocketManagerApiServer != null)
            m_webSocketManagerApiServer.Broadcast(sMsg);
        };

        while (!stoppingToken.IsCancellationRequested)
        {
          if (!started)
          {
            _logger.LogInformation("Tick by starting");
            started = _service.Start();
          }

          string testString = DateTime.Now.ToString();
          //_logger.LogInformation("{TimerTick}", testString);

          await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        if (started)
          _service.Stop();
      }
      catch (TaskCanceledException)
      {
        // When the stopping token is canceled, for example, a call made from services.msc,
        // we shouldn't exit with a non-zero exit code. In other words, this is expected...
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "{Message}", ex.Message);

        // Terminates this process and returns an exit code to the operating system.
        // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
        // performs one of two scenarios:
        // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
        // 2. When set to "StopHost": will cleanly stop the host, and log errors.
        //
        // In order for the Windows Service Management system to leverage configured
        // recovery options, we need to terminate the process with a non-zero exit code.
        Environment.Exit(1);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sIncoming"></param>
    /// <returns></returns>
    override public string ProcessIncomingManagerMessage(string sIncoming)
    {
      // See if this is a message specific to this service
      if (WindowsServiceBaseApiHandler.IsValidJson(sIncoming))
      {
        if (!WindowsServiceBaseApiHandler.IsPcsEnvelopeMessage(sIncoming))
        {
          _logger.LogError("Not a valid PcsEnvelope: " + sIncoming);
          return "";
        }

        // Handle message via service specific api, if possible
        ManagerApiHandler handler = new ManagerApiHandler(_service!);
        string s = handler.GetResult(sIncoming);
        if (!string.IsNullOrEmpty(s))
          return s;
      }

      // else pass on to base service
      return base.ProcessIncomingManagerMessage(sIncoming);
    }


  }

}


