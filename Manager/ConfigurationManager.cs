//----------------------------------------------------------------------------------------------------------------------
// AMMANN GROUP CH-4900 LANGENTHAL                   © 2023 Alle Rechte vorbehalten
//
// Vervielfältigung, Veröffentlichung oder Weitergabe dieses Dokuments oder Teilen daraus sind, zu welchem Zweck und
// in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die Ammann-Group nicht gestattet.
//
// Ersteller der Datei:   Michael Serzhan
//----------------------------------------------------------------------------------------------------------------------

using OpcUaServer.Properties;
using System.IO;
using System.Xml;

namespace OpcUaServer.Manager
{
  public class ConfigurationManager
  {
    #region members
    private readonly ILogger<ConfigurationManager>? _logger;
    #endregion

    #region properties
    #endregion

    #region constructors
    public ConfigurationManager(ILoggerFactory loggerFactory)
    {
      _logger = loggerFactory.CreateLogger<ConfigurationManager>();
      AppEnvironment.ConfigurationPath = Path.Combine(Program.ComaDataPath, "Services", "OpcUaServer");
    }
    #endregion

    #region public methods
    /// <summary>
    /// ProcessCloudGatewayConfig
    /// </summary>
    /// <returns><see cref="Task"/></returns>
    public void LoadConfiguration()
    {
      XmlElement? root = null;
      var filePath = Path.Combine(AppEnvironment.ConfigurationPath ?? string.Empty, "OpcUaServer.config");
      var doc = new XmlDocument();
      var settings = new XmlReaderSettings { Async = true };

      using var reader = XmlReader.Create(filePath, settings);
      try
      {
        if (!File.Exists(filePath))
        {
          _logger?.LogError($"ConfigManager: OpcUaServer.config not found.");

          using (var sw = File.CreateText(filePath))
          {
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sw.WriteLine("<OpcUAServer");
            sw.WriteLine("             xmlns=\"https://definitions.ammann-group.com/OpcUAServer/2023\">");
            sw.WriteLine("  <OpcUASettings>");
            sw.WriteLine("    <PortHttps>62541</PortHttps>");
            sw.WriteLine("    <PortTcp>62542</PortTcp>");
            sw.WriteLine("    <BaseAddress>Ammann/OpcUAServer/</BaseAddress>");
            sw.WriteLine("  </OpcUASettings>");
            sw.WriteLine("  <ConnectionPlugin>");
            sw.WriteLine("    <Messaging>");
            sw.WriteLine("      <HostName>127.0.0.1</HostName>");
            sw.WriteLine("      <Port>10015</Port>");
            sw.WriteLine("    </Messaging>");
            sw.WriteLine("  </ConnectionPlugin>");
            sw.WriteLine("  <Logging/>");
            sw.WriteLine("<OpcUAServer>");
          }

          _logger?.LogError($"ConfigManager: OpcUAServer.config create new one.");
        }

        // load the document
        doc.Load(reader);

        // get the root element
        root = doc.DocumentElement;
      }
      catch (Exception)
      {
        _logger?.LogError($"ConfigManager: OpcUAServer.config is invalid.");
      }

      var configuration = ConfigurationConverter.Convert(root?.OuterXml ?? string.Empty, _logger);
      if (null != configuration)
        AppEnvironment.Configuration = configuration;
    }
    #endregion
  }
}
