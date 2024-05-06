//----------------------------------------------------------------------------------------------------------------------
// AMMANN GROUP CH-4900 LANGENTHAL                   © 2023 Alle Rechte vorbehalten
//
// Vervielfältigung, Veröffentlichung oder Weitergabe dieses Dokuments oder Teilen daraus sind, zu welchem Zweck und
// in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die Ammann-Group nicht gestattet.
//
// Ersteller der Datei:   Michael Serzhan
//----------------------------------------------------------------------------------------------------------------------

using OpcUaServer.Interfaces;
using OpcUaServer.Model;
using System.IO;
using System.Xml.Serialization;

namespace OpcUaServer.Manager;

public class ConfigurationConverter
{
  #region public methods

  /// <summary>
  /// Deserialize app config
  /// </summary>
  /// <param name="xmlData">Xml data to convert</param>
  /// <param name="logger">Logger</param>
  /// <returns><see cref="IOpcUaServiceConfiguration"/></returns>
  public static Configuration? Convert(string xmlData, ILogger? logger)
  {
    try
    {
      var serializer = new XmlSerializer(typeof(Configuration));
      using var reader = new StringReader(xmlData);

      if (serializer.Deserialize(reader) is Configuration result)
        return result;
    }
    catch (Exception ex)
    {
      logger?.LogError($"Failed to load configuration: {ex}");
    }

    return null;
  }
  #endregion
}