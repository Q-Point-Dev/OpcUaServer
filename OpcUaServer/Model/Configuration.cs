//----------------------------------------------------------------------------------------------------------------------
// AMMANN GROUP CH-4900 LANGENTHAL                   © 2023 Alle Rechte vorbehalten
//
// Vervielfältigung, Veröffentlichung oder Weitergabe dieses Dokuments oder Teilen daraus sind, zu welchem Zweck und
// in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die Ammann-Group nicht gestattet.
//
// Ersteller der Datei:   Michael Serzhan
//----------------------------------------------------------------------------------------------------------------------

using OpcUaServer.Infrastructure;
using OpcUaServer.Interfaces;
using System.Xml.Serialization;

namespace OpcUaServer.Model;

[XmlRoot(ElementName = "OpcUAServer")]
public class Configuration : IOpcUaServiceConfiguration
{
  public Configuration()
  {
    OpcUaSettings = new OpcUaSettings();
    ConnectionPluginSettings = new ConnectionPluginSettings();
  }

  [XmlElement(ElementName = "OpcUASettings")]
  public OpcUaSettings OpcUaSettings { get; set; }

  [XmlElement(ElementName = "ConnectionPlugin")]
  public ConnectionPluginSettings ConnectionPluginSettings { get; set; }
}

public class OpcUaSettings : IOpcUaSettings
{
  public OpcUaSettings()
  {
    BaseAddress = "";
    Culture = "de-DE";
  }

  public string BaseAddress { get; set; }
  public int PortHttps { get; set; }
  public int PortTcp { get; set; }

  public string Culture { get; set; }

  
}

public class ConnectionPluginSettings
{ 
  public MessagingSettings? Messaging { get; set; }
}

public class MessagingSettings : IMessagingSettings
{ 
  public MessagingSettings()
  {
    HostName = "";
  }

  /// <summary>
  /// Host Name where to connect for websocket
  /// </summary>
  public string HostName { get; set; }
  
  /// <summary>
  /// Port to use for websocket
  /// </summary>
  public int Port { get; set; }
}