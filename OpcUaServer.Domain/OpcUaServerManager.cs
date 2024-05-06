/*
 * @license
 * © 2023 Ammann-Group Switzerland. All rights reserved
 * Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
 * purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.
 */

using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;
using OpcUaServer.Infrastructure;
using System.Reflection;
using System.Xml.Linq;

namespace OpcUaServer.Domain
{
  /// <summary>
  /// Manager for the OPC UA server.
  /// </summary>
  public class OpcUaServerManager
  {
    /// <summary>
    /// File name of the plugin.
    /// </summary>
    private readonly string _fileNameOfThePlugin = @"OpcUaServer.Connection.dll";

    /// <summary>
    /// Instance of the OPC UA server application.
    /// </summary>
    private ApplicationInstance? _application;

    /// <summary>
    /// Instance of the OPC UA server.
    /// </summary>
    private OpcUaServer? _opcUaServer;

    /// <summary>
    /// Connection to the data source
    /// </summary>
    private IOpcUaServerConnection? _plugin;

    /// <summary>
    /// Parameters of the OpcUaServer
    /// </summary>
    private IOpcUaSettings? _configurationService;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger? _logger;

    /// <summary>
    /// LoggerFactory
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    // Map with the ReferenceId as key and IOpcUaServerDataTypeConfiguration and PropertyState as value
    private Dictionary<ulong, (IOpcUaServerDataTypeConfiguration, PropertyState)> PropertyStateMap { get; set; } = new Dictionary<ulong, (IOpcUaServerDataTypeConfiguration, PropertyState)>();

    /// <summary>
    /// Fired when plugin status changed
    /// </summary>
    public event EventHandler<int>? OnPluginStatusChanged;

    // Captures the current status fo the plugin
    private int _pluginStatus;

    /// <summary>
    /// Constructor.
    /// </summary>
    public OpcUaServerManager(ILoggerFactory factory)
    {
      _loggerFactory = factory;
      _logger = _loggerFactory.CreateLogger<OpcUaServerManager>();
      AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    /// <summary>
    /// This method is called when the we need to load an assembly.
    /// </summary>
    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources"))
      {
        return null;
      }

      var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var name = args.Name.Split(',')[0];
      
      if (name == null || path == null)
      {
        return null;
      }

      path = Path.Combine(path, $"{name}.dll");
      if (File.Exists(path))
      {
        return Assembly.LoadFile(path);
      }

      _logger?.LogError($"Can't load {path} assembly");
      return null;
    }

    private void StartOpcUaServer(IOpcUaServerConfiguration configuration)
    {
      _logger?.LogInformation("StartOpcUaServer 1");

      if (_application != null)
      {
        _application.Stop();
        _application = null;
      }


      _application = new ApplicationInstance();
      _application.ApplicationType = ApplicationType.Server;
      _application.ConfigSectionName = "OpcUaServer";

      _logger?.LogInformation("StartOpcUaServer 2");

      // load the application configuration.
      //_application.LoadApplicationConfiguration(false).Wait();
      var configFileName = Path.Combine(AppContext.BaseDirectory, "OpcUaServer.Config.xml");
      _application.LoadApplicationConfiguration(configFileName, false).Wait();

      SetConnectionParameters();

      _logger?.LogInformation("StartOpcUaServer 3");

      // check the application certificate.
      _application.CheckApplicationInstanceCertificate(false, 0).Wait();

      _logger?.LogInformation("StartOpcUaServer 4");

      // start the server.
      // Reset PropertyStateMap.
      PropertyStateMap = new Dictionary<ulong, (IOpcUaServerDataTypeConfiguration, PropertyState)>();
      _opcUaServer = new OpcUaServer(configuration, PropertyStateMap, _loggerFactory);
      _application.Start(_opcUaServer).Wait();

      _logger?.LogInformation("StartOpcUaServer 5");
    }

    protected void SetConnectionParameters()
    {
      if (null == _application)
        return;

      var newAddresses = new StringCollection();

      _application.ApplicationConfiguration.ServerConfiguration.BaseAddresses
        .ForEach(address =>
        {
          var position = address.IndexOf(":");
          if (position > 0)
          {
            position = address.IndexOf(":", position + 1);
            if (position > 0)
            {
              var newAddres = address.Substring(0, position + 1);

              if(newAddres.ToLower().Contains("http"))
                newAddres += _configurationService?.PortHttps + "/";
              else
                newAddres += _configurationService?.PortTcp + "/";

              newAddres += _configurationService?.BaseAddress;

              newAddresses.Add(newAddres);
            }
          }
        });
      

      _application.ApplicationConfiguration.ServerConfiguration.BaseAddresses = newAddresses;
    }

    /// <summary>
    /// The Start method initializes a plugin, sets up event handlers for configuration and data changes, and then enters an infinite loop.
    /// The loaded plugin subscribes to OnConfigurationChanged and OnDataChanged events. 
    /// On a configuration change, the method starts an OPC UA server with the current configuration. 
    /// On a data change, the method iterates through the current data point values, and if the value is found in the PropertyStateMap, updates the corresponding property's value. 
    /// It specifically checks and handles two data types: IOpcUaServerDataValueBool and IOpcUaServerDataValueFloat. 
    /// After setting up the event handlers, the method enters an infinite loop, pausing for a second in each iteration.
    /// </summary>
    public bool Start(XDocument configuration, IOpcUaSettings configurationService, string serviceName)
    {
      _configurationService = configurationService;
      _logger?.LogInformation("Before loading plugin");
      _plugin = LoadPlugin(configuration, serviceName, _configurationService.Culture);
      _logger?.LogInformation("After loading plugin");
      if (_plugin == null)
        return false;

      _logger?.LogInformation("Done loading plugin");

      _plugin.OnConfigurationChanged += (sender, e) =>
      {
        _logger?.LogInformation("OnConfigurationChanged: {0}", _plugin.CurrentConfiguration.DataTypeConfiguration.Count);
        StartOpcUaServer(_plugin.CurrentConfiguration);
      };

      _plugin.OnDataChanged += (sender, e) =>
      {
        _logger?.LogInformation("OnDataChanged: {0}", _plugin.CurrentDataPointValues.Count);
        foreach (var item in _plugin.CurrentDataPointValues)
        {
          if (PropertyStateMap.TryGetValue(item.ReferenceId, out (IOpcUaServerDataTypeConfiguration, PropertyState) propertyState))
          {
            if (item is IOpcUaServerDataValueBool) {
              var boolItem = item as IOpcUaServerDataValueBool;
              propertyState.Item2.Value = boolItem?.Value;
              propertyState.Item2.StatusCode = boolItem?.IsValueValid == true ? StatusCodes.Good : StatusCodes.Uncertain;
              // The sourceTimestamp shall be UTC time and should indicate the time of the last change of the value or statusCode.
              propertyState.Item2.Timestamp = DateTime.UtcNow;
              propertyState.Item2.ClearChangeMasks(_opcUaServer?.getSystemContext(), false); 
            }
            if (item is IOpcUaServerDataValueFloat)
            {
              var floatItem = item as IOpcUaServerDataValueFloat;
              propertyState.Item2.Value = floatItem?.Value;
              propertyState.Item2.StatusCode = floatItem?.IsValueValid == true ? StatusCodes.Good : StatusCodes.Uncertain;
              // The sourceTimestamp shall be UTC time and should indicate the time of the last change of the value or statusCode.
              propertyState.Item2.Timestamp = DateTime.UtcNow;
              propertyState.Item2.ClearChangeMasks(_opcUaServer?.getSystemContext(), false);
            }
            if (item is IOpcUaServerDataValueString)
            {
              var stringItem = item as IOpcUaServerDataValueString;
              propertyState.Item2.Value = stringItem?.Value;
              propertyState.Item2.StatusCode = stringItem?.IsValueValid == true ? StatusCodes.Good : StatusCodes.Uncertain;
              // The sourceTimestamp shall be UTC time and should indicate the time of the last change of the value or statusCode.
              propertyState.Item2.Timestamp = DateTime.UtcNow;
              propertyState.Item2.ClearChangeMasks(_opcUaServer?.getSystemContext(), false);
            }
          }
        }
      };

      _plugin.OnDisconnect += (sender, e) =>
      {
        _logger?.LogInformation("OnDisconnect: ");
      };

      _plugin.OnStatusChanged += (sender, e) =>
      {
        _pluginStatus = e;
        if(OnPluginStatusChanged != null)
          OnPluginStatusChanged(this, (int)_pluginStatus);
        _logger?.LogInformation("new status: {0}", e);
      };

      return true;
    }

    /// <summary>
    /// This method stops the plugin
    /// </summary>
    public bool Stop()
    {
      if (_plugin == null)
        return true;

      _plugin.OnConfigurationChanged -= (sender, e) => { };
      _plugin.OnDataChanged -= (sender, e) => { };
      _plugin = null;
      
      return true;
    }

    /// <summary>
    /// This method loads a plugin from a specified DLL file and attempts to create an instance of the OpcUaServerConnection class from it. 
    /// The plugin's assembly is loaded from the DLL, after which the method searches for the OpcUaServerConnection class. 
    /// If the class is found, an instance of it is created and initialized. 
    /// </summary>
    IOpcUaServerConnection? LoadPlugin(XDocument configuration, string serviceName, string cultureName)
    {
      var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var fullPath = Path.Combine(assemblyPath!, _fileNameOfThePlugin);

      // Load the assembly from the DLL file.
      //var assembly = Assembly.LoadFrom(_fileNameOfThePlugin);
      var assembly = Assembly.LoadFrom(fullPath);

      // Get the type of the OpcUaServerConnection class.
      var type = assembly.GetType("OpcUaServerConnection");

      if (type == null)
      {
        throw new Exception("Could not find the OpcUaServerConnection class.");
      }

      // Create an instance of the OpcUaServerConnection class.
      var instance = Activator.CreateInstance(type) as IOpcUaServerConnection;

      if (instance == null)
      {
        throw new Exception("Could not create an instance of the OpcUaServerConnection class.");
      }

      // Now you can use the instance.
      instance.Init(configuration, _loggerFactory, serviceName, cultureName);

      return instance;
    }
  
    public int GetPluginStatus()
    {
      return _pluginStatus;
    }

  }
}