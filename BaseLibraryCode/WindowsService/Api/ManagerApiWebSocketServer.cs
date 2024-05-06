// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.



using BaseLibraryCode.WebSocketMessaging.Net;
using BaseLibraryCode.WebSocketMessaging.Net.Data;
using BaseLibraryCode.WebSocketMessaging.Net.Server;
using BaseLibraryCode.WindowsService.Net.Api.Messages;
using BaseLibraryCode.WindowsService.Net.Api.Models;
using BaseLibraryCode.WindowsService.Net.Utilities;
using System.Text.Json;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace BaseLibraryCode.WindowsService.Net.WebSocketApi;

/// <summary>
/// The websocket server that interacts with Manager clients
/// </summary>
public class ManagerApiWebSocketServer: IDelegatedWebSocketBehaviorCallback
{
  /// <summary>
  /// Callback definition to allow host windows servcie to respond to service specific manager messages
  /// </summary>
  /// <param name="sIncoming"></param>
  /// <returns></returns>
  public delegate string ProcessIncomingManagerMessageDelegate(string sIncoming);

  /// <summary>
  /// Instance of callback delegate
  /// </summary>
  public ProcessIncomingManagerMessageDelegate? OnProcessIncomingManagerMessage { get; set; }


  private readonly ILogger<ManagerApiWebSocketServer> _logger;
  private readonly ILoggerFactory _loggerFactory;

  /// <summary>
  /// Configuration (host and port) of websocket
  /// </summary>
  private ClientConnectionInfo _managerWebSocketApiConfig;

  /// <summary>
  /// 
  /// </summary>
  private WebSocketServer? _webSocketServer;

  /// <summary>
  /// A collection of currently attached clients. There may be more than one manager attached at once. Used for broadcasting.
  /// </summary>
  private List<DelegatedWebSocketBehavior> _Clients { get; set; } = new();

  // timer for polling connectivity
  private System.Timers.Timer? _tmServer;

  public ManagerApiWebSocketServer(ILoggerFactory loggerFactory)
  {
    _loggerFactory = loggerFactory;
    _logger = loggerFactory.CreateLogger<ManagerApiWebSocketServer>();
    _managerWebSocketApiConfig = new ClientConnectionInfo();
  }


  public void AddClient(DelegatedWebSocketBehavior cli)
  {
    _Clients.Add(cli);

    // Send initial status
    var sss = BaseServiceAppEnvironment.ServiceProvider!.GetService(typeof(IStatusSentinelService)) as IStatusSentinelService;
    ServiceStatus currStatus = sss!.GetCurrentServiceStatus();
    SMsgServiceStatusResponse res = new SMsgServiceStatusResponse();
    res.status = currStatus;
    PcsEnvelope env = new PcsEnvelope();
    env.dest = "Manager";
    env.src = "Service";
    env.data = res.getAsJsonElement();
    string sMsg = JsonSerializer.Serialize(env);
    cli.SendData(sMsg);
  }

  public void RemoveClient(DelegatedWebSocketBehavior cli)
  {
    _Clients.Remove(cli);
  }

  public void OnMessageFromClient(DelegatedWebSocketBehavior cli, MessageEventArgs e)
  {
    if (OnProcessIncomingManagerMessage != null)
    {
      string sReturnValue = OnProcessIncomingManagerMessage(e.Data);
      if (!string.IsNullOrEmpty(sReturnValue))
      {
        cli.SendData(sReturnValue);
        return;
      }
    }
  }



  /// <summary>
  /// 
  /// </summary>
  /// <param name="cfg"></param>
  /// <param name="dgProcessIncomingManagerMessage"></param>
  public void Configure(ClientConnectionInfo cfg, ProcessIncomingManagerMessageDelegate dgProcessIncomingManagerMessage)
  {
    try
    {
      _managerWebSocketApiConfig = cfg;
      OnProcessIncomingManagerMessage = dgProcessIncomingManagerMessage;

      _webSocketServer = new WebSocketServer(GetWebsocketUrlStem());

  #if DEBUG
      // To change the logging level.
      // _webSocketServer.Log.Level = LogLevel.Trace;
  #endif
      // To change the wait time for the response to the WebSocket Ping or Close.
      _webSocketServer.WaitTime = TimeSpan.FromSeconds(30);

      // Not to remove the inactive sessions periodically.
      _webSocketServer.KeepClean = false;

      // An empty path must be given as "/"
      string sPath = "/";
      if(!string.IsNullOrEmpty(_managerWebSocketApiConfig.Path))
        sPath = _managerWebSocketApiConfig.Path;

      // Add Service
      _webSocketServer.AddWebSocketService<DelegatedWebSocketBehavior>(sPath, () => new DelegatedWebSocketBehavior(this));

    }
    catch (Exception ex) //some other exception
    {
      Console.WriteLine(ex.Message);
    }


  }

  /// <summary>
  /// Start WebsocketServer
  /// </summary>
  public void Start()
  {
    _webSocketServer?.Start();

    _tmServer = new System.Timers.Timer(2000);
    _tmServer.Elapsed += TmServer_Elapsed;
    _tmServer.Start();
  }


  /// <summary>
  /// Sends hearbeat message
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private void TmServer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
  {
    try
    {
      _tmServer!.Stop();
      PcsEnvelope env = new PcsEnvelope();
      env.dest = "manager";
      env.src = "service";
      // Pack the data attribute with the data from the derived class, not SMsgBase
      SMsgTimeOfDay data = new SMsgTimeOfDay();
      data.tod = DateTime.Now.ToString();
      env.data = data.getAsJsonElement();
      string sMsg = JsonSerializer.Serialize(env);
      Broadcast(sMsg);
      _tmServer!.Start();
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }


  }

  /// <summary>
  /// Stop WebsocketServer
  /// </summary>
  public void Stop()
  {
    _webSocketServer?.Stop();
  }
  
  /// <summary>
  /// Sends message to all connected clients
  /// </summary>
  /// <param name="sData"></param>
  public void Broadcast(string sData)
  {

    _Clients.ForEach(cli =>
    {
      Task.Run(() =>
      {
        try
        {
          cli.SendData(sData);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      });

    });
  }

  /// <summary>
  /// Returns URI of socket
  /// </summary>
  /// <returns></returns>
  private string GetWebsocketUrlStem()
  {
    // todo mkmkmk use the function from WebSocketMessaging.Net.WebSocketManager.BuildUri
    var uriBuilder = new UriBuilder();

    uriBuilder.Scheme = _managerWebSocketApiConfig.Scheme;
    uriBuilder.Host = Environment.MachineName;
    uriBuilder.Port = int.Parse(_managerWebSocketApiConfig.Port ?? "80");    ;
    // Path must be blank here. Any Path is added with AddWebSocketService later
    uriBuilder.Path = "";

    return uriBuilder.ToString();
  }

}

