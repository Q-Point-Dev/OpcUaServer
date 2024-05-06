//----------------------------------------------------------------------------------------------------------------------
// AMMANN GROUP CH-4900 LANGENTHAL                   © 2023 Alle Rechte vorbehalten
//
// Vervielfältigung, Veröffentlichung oder Weitergabe dieses Dokuments oder Teilen daraus sind, zu welchem Zweck und
// in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die Ammann-Group nicht gestattet.
//
// Ersteller der Datei:   Michael Serzhan
//----------------------------------------------------------------------------------------------------------------------

using OpcUaServer.Domain;
using OpcUaServer.Properties;
using System.Reflection;
using System.Xml;

namespace OpcUaServer.Service
{
  public class OpcUaService
  { 
    OpcUaServerManager? _manager;
    private ILogger _logger;
    private ILoggerFactory _loggerFactory;


    public event EventHandler<int>? OnPluginStatusChanged;

    public OpcUaService(ILoggerFactory loggerFactory)
    { 
      _loggerFactory = loggerFactory;
      _logger = loggerFactory.CreateLogger<OpcUaService>();
    }

    /// <summary>
    /// Start the service
    /// </summary>
    /// <returns></returns>
    public bool Start()
    {
      _logger.LogInformation("Manager Start 1");
      ReadConfiguration();
      _manager = new OpcUaServerManager(_loggerFactory);
      _manager?.Start(AppEnvironment.GetPluginConfiguration(), AppEnvironment.Configuration.OpcUaSettings, 
        Settings.Default.ServiceName);

      _logger.LogInformation("Manager Start 2");

      _manager!.OnPluginStatusChanged += (sender, e) =>
      {
        if (OnPluginStatusChanged != null)
        {
          OnPluginStatusChanged(sender, e);
        }
      };


      return true;
    }

    /// <summary>
    /// Stop the service
    /// </summary>
    /// <returns></returns>
    public bool Stop() 
    {
      _manager?.Stop();
      return true;
    }

    private void ReadConfiguration()
    {
      var manager = new Manager.ConfigurationManager(_loggerFactory);
      manager.LoadConfiguration();
    }

    public string GetServerVersion()
    {
      return Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    }

    public string GetOpcUaServerUri()
    {
      UriBuilder b = new UriBuilder();
      b.Scheme = "opc.tcp";
      b.Host = Environment.MachineName;
      b.Port = AppEnvironment.Configuration.OpcUaSettings.PortTcp;
      b.Path = AppEnvironment.Configuration.OpcUaSettings.BaseAddress;
      return b.ToString();
    }    

    public string GetPluginConfiguration()
    {
      System.Xml.Linq.XDocument doc = AppEnvironment.GetPluginConfiguration();
      return doc.ToString();
    }

    public bool GetIsConnectedToMsgDispatcher()
    {
      return (_manager?.GetPluginStatus() >= 2);
    }

    public bool GetIsRtcConnectionOnline()
    {
      return (_manager?.GetPluginStatus() >= 3);
    }

  }
}
