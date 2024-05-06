using OpcUaServer.Interfaces;
using OpcUaServer.Model;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace OpcUaServer
{
  public static class AppEnvironment
  {
    /// <summary>
    /// Host service provider
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; set; }

    public static string ConfigurationPath { get; set; } = string.Empty;
    public static Configuration Configuration { get; set; } = new Configuration();

    public static XDocument GetPluginConfiguration()
    {
      var serializer = new XmlSerializer(typeof(ConnectionPluginSettings));
      using var writer = new StringWriter();

      serializer.Serialize(writer, Configuration.ConnectionPluginSettings);

      return XDocument.Parse(writer.ToString());
    }

    /// <summary>
    /// LoggerFactory
    /// </summary>
    public static ILoggerFactory? LoggerFactory => ServiceProvider?.GetService<ILoggerFactory>();
  }
}
