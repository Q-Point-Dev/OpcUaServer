/*
 * @license
 * © 2023 Ammann-Group Switzerland. All rights reserved
 * Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
 * purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.
 */



using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Gds;
using Opc.Ua.Server;
using OpcUaServer.Infrastructure;


namespace OpcUaServer.Domain
{
  /// <summary>
  /// A node manager for a server that exposes several variables.
  /// </summary>
  public class OpcUaNodeManager : CustomNodeManager2
  {
    IOpcUaServerConfiguration? configuration;

    Dictionary<ulong, (IOpcUaServerDataTypeConfiguration, PropertyState)>? PropertyStateMap;

    private Dictionary<string, BaseObjectState> ExistingNodes { get; set; }

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger? _logger;

    /// <summary>
    /// LoggerFactory
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;


    #region Constructors
    /// <summary>
    /// Initializes the node manager.
    /// </summary>
    public OpcUaNodeManager(IServerInternal server, ApplicationConfiguration configuration, IOpcUaServerConfiguration? config,
      Dictionary<ulong, (IOpcUaServerDataTypeConfiguration, PropertyState)>? propertyStateMap, ILoggerFactory loggerFactory
      )
        :
            base(server, configuration, OpcUaNamespaces.Empty)
    {
      SystemContext.NodeIdFactory = this;
      this.configuration = config;
      PropertyStateMap = propertyStateMap;
      _loggerFactory = loggerFactory;
      _logger = _loggerFactory.CreateLogger<OpcUaNodeManager>();

      ExistingNodes = new Dictionary<string, BaseObjectState>();

      // get the configuration for the node manager.
      m_configuration = configuration.ParseExtension<OpcUaServerConfiguration>();

      // use suitable defaults if no configuration exists.
      if (m_configuration == null)
      {
        m_configuration = new OpcUaServerConfiguration();
      }
    }
    #endregion

    #region IDisposable Members
    /// <summary>
    /// An overrideable version of the Dispose.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // TBD
      }
    }
    #endregion

    #region INodeIdFactory Members
    /// <summary>
    /// Creates the NodeId for the specified node.
    /// </summary>
    public override NodeId New(ISystemContext context, NodeState node)
    {
      return node.NodeId;
    }
    #endregion

    #region INodeManager Members
    /// <summary>
    /// Does any initialization required before the address space can be used.
    /// </summary>
    /// <remarks>
    /// The externalReferences is an out parameter that allows the node manager to link to nodes
    /// in other node managers. For example, the 'Objects' node is managed by the CoreNodeManager and
    /// should have a reference to the root folder node(s) exposed by this node manager.  
    /// </remarks>
    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
      lock (Lock)
      {
        if (configuration == null)
        {
          throw new ArgumentNullException(nameof(configuration));
        }

        ExistingNodes = new Dictionary<string, BaseObjectState>();
        CreateNodes(configuration, externalReferences);
      }
    }





    /// <summary>
    /// Frees any resources allocated for the address space.
    /// </summary>
    public override void DeleteAddressSpace()
    {
      lock (Lock)
      {
        // TBD
      }
    }

    /// <summary>
    /// Returns a unique handle for the node.
    /// </summary>
    protected override NodeHandle? GetManagerHandle(ServerSystemContext context, NodeId nodeId, IDictionary<NodeId, NodeState> cache)
    {
      lock (Lock)
      {
        // quickly exclude nodes that are not in the namespace. 
        if (!IsNodeIdInNamespace(nodeId))
        {
          return null;
        }

        NodeState? node = null;

        if (!PredefinedNodes.TryGetValue(nodeId, out node))
        {
          return null;
        }

        NodeHandle handle = new NodeHandle();

        handle.NodeId = nodeId;
        handle.Node = node;
        handle.Validated = true;

        return handle;
      }
    }

    /// <summary>
    /// Verifies that the specified node exists.
    /// </summary>
    protected override NodeState? ValidateNode(
        ServerSystemContext context,
        NodeHandle handle,
        IDictionary<NodeId, NodeState> cache)
    {
      // not valid if no root.
      if (handle == null)
      {
        return null;
      }

      // check if previously validated.
      if (handle.Validated)
      {
        return handle.Node;
      }

      // TBD

      return null;
    }

    private NodeId ConvertDataType(DataType dataType)
    {
      switch (dataType)
      {
        case DataType.Boolean:
          return Opc.Ua.DataTypeIds.Boolean;
        case DataType.Float:
          return Opc.Ua.DataTypeIds.Float;
        case DataType.String:
          return Opc.Ua.DataTypeIds.String;
        default:
          throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
      }
    }

    /// <summary>
    /// Get the initial value for a variable based on its data type
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private object InitalValueByDataType(DataType dataType)
    {
      switch (dataType)
      {
        case DataType.Boolean:
          return false;
        case DataType.Float:
          return 0.0;
        case DataType.String:
          return "";
        default:
          throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
      }
    }

    public void CreateNodes(IOpcUaServerConfiguration serverConfiguration, IDictionary<NodeId, IList<IReference>> externalReferences)
    {
      uint counter = 1;
      foreach (var dataTypeConfiguration in serverConfiguration.DataTypeConfiguration)
      {
        CreateNodeFromDataTypeConfiguration(dataTypeConfiguration, externalReferences, ref counter);
      }
    }

    private void CreateNodeFromDataTypeConfiguration(IOpcUaServerDataTypeConfiguration dataTypeConfiguration, IDictionary<NodeId, IList<IReference>> externalReferences, ref uint counter)
    {
      var pathNames = dataTypeConfiguration.PathName.Split('.');
      var pathLocalzedNames = dataTypeConfiguration.PathNameLocalized.Split('.');
      BaseObjectState? parentNode = null;

      string currentPath = string.Empty;
      for (var i = 0; i < pathNames.Length; i++)
      {
        string pathNameLocalized = string.Empty;
        var pathName = pathNames[i];
        if (string.IsNullOrEmpty(currentPath))
        {
          currentPath = pathName;
        }
        else
        {
          currentPath = $"{currentPath}.{pathName}";
        }

        if (pathLocalzedNames.Length == pathNames.Length)
        {
          pathNameLocalized = pathLocalzedNames[i];
        } else
        {
          pathNameLocalized = pathName;
        }

        // If this is the last path name, it's a property
        if (i == pathNames.Length - 1)
        {
          if (parentNode == null)
            parentNode = GetOrCreateNode(null, "Default", "Default", ref counter, externalReferences, "Default");

          CreateEndPointProperty(dataTypeConfiguration, externalReferences, ref counter, ref parentNode, pathName, pathNameLocalized);
        }
        else // It's a node
        {
          BaseObjectState? node = GetOrCreateNode(parentNode, currentPath, pathName, ref counter, externalReferences, pathNameLocalized);
          parentNode = node;
        }
      }
      // Add predefined node
      AddPredefinedNode(SystemContext, parentNode);
    }

    private void CreateEndPointProperty(IOpcUaServerDataTypeConfiguration dataTypeConfiguration, IDictionary<NodeId, IList<IReference>> externalReferences,
      ref uint counter, ref BaseObjectState parentNode, string pathName, string pathNameLocalized)
    {
      // Check input Parameter and throw exception if null
      if (dataTypeConfiguration == null) throw new ArgumentNullException(nameof(dataTypeConfiguration));
      if (pathName == null) throw new ArgumentNullException(nameof(pathName));
      if (parentNode == null) throw new ArgumentNullException(nameof(parentNode));
        
      var property = new PropertyState(parentNode)
      {
        NodeId = new NodeId(dataTypeConfiguration.PathName, NamespaceIndex),
        BrowseName = new QualifiedName(pathName),
        DisplayName = pathNameLocalized,
        DataType = ConvertDataType(dataTypeConfiguration.DataType),
        TypeDefinitionId = VariableTypeIds.PropertyType,
        ReferenceTypeId = ReferenceTypeIds.HasProperty,
        Description = dataTypeConfiguration.Description,  
        ValueRank = ValueRanks.Scalar,
        Value = InitalValueByDataType(dataTypeConfiguration.DataType)
      };

      StringBuilder stringBuilder = new StringBuilder();
      if (!string.IsNullOrEmpty(dataTypeConfiguration.Unit))
      {
        stringBuilder.Append("Unit: ");
        stringBuilder.Append(dataTypeConfiguration.Unit);
      }
      if (dataTypeConfiguration.Minimum > -3.3E+38)
      {
        if (stringBuilder.Length > 0)
        {
          stringBuilder.Append("; ");
        }
        stringBuilder.Append("Min: ");
        stringBuilder.Append(dataTypeConfiguration.Minimum);
        if (!string.IsNullOrEmpty(dataTypeConfiguration.Unit))
          stringBuilder.Append(dataTypeConfiguration.Unit);
      }

      if (dataTypeConfiguration.Maximum > -3.3E+38)
      {
        if (stringBuilder.Length > 0)
        {
          stringBuilder.Append("; ");
        }
        stringBuilder.Append("Max: ");
        stringBuilder.Append(dataTypeConfiguration.Maximum);
        if (!string.IsNullOrEmpty(dataTypeConfiguration.Unit))
          stringBuilder.Append(dataTypeConfiguration.Unit);
      }

      if (!string.IsNullOrEmpty(dataTypeConfiguration.Description))
      {
        if (stringBuilder.Length > 0)
        {
          stringBuilder.Append("; ");
        }
        stringBuilder.Append(dataTypeConfiguration.Description);
      }

      property.Description = stringBuilder.ToString();

      _logger!.LogInformation($"Creating EndPoint {property.NodeId} - {property.BrowseName} ({property.DisplayName}) [{property.DataType}]");
      counter++;
      parentNode.AddChild(property);
      if (PropertyStateMap != null) {
        PropertyStateMap.TryAdd(dataTypeConfiguration.ReferenceId, (dataTypeConfiguration, property));
      }

      // Todo: ist dies notwendig?
      // 
      // ReferenceTypeState referenceType = new ReferenceTypeState();
      // 
      // referenceType.NodeId = new NodeId(counter, NamespaceIndex);
      // counter++;
      // referenceType.BrowseName = new QualifiedName("IsTriggerSource", NamespaceIndex);
      // referenceType.DisplayName = referenceType.BrowseName.Name;
      // referenceType.InverseName = new LocalizedText("IsSourceOfTrigger");
      // referenceType.SuperTypeId = ReferenceTypeIds.NonHierarchicalReferences;
      // 
      // property.AddReference(referenceType.NodeId, false, ObjectIds.Server);
      // 
      // IList<IReference> references = null;
      // if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references))
      // {
      //   externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
      // }
      // references.Add(new NodeStateReference(referenceType.NodeId, true, property.NodeId));
      // 
      // // save in dictionary. 
      // AddPredefinedNode(SystemContext, referenceType);

    }

    private BaseObjectState GetOrCreateNode(BaseObjectState? parentNode, string currentPath, string pathName, ref uint counter,
      IDictionary<NodeId, IList<IReference>> externalReferences, string pathNameLocalized)
    {
      if (!ExistingNodes.TryGetValue(currentPath, out var node))
      {
        Debug.WriteLine($"Creating node {currentPath} {parentNode?.BrowseName.Name}");
        BaseObjectState nodeObject = new BaseObjectState(parentNode);
        uint nodeId = counter;
        nodeObject.NodeId = new NodeId(nodeId, NamespaceIndex);
        counter++;
        nodeObject.BrowseName = new QualifiedName(pathName, NamespaceIndex);
        nodeObject.DisplayName = pathNameLocalized;
        nodeObject.TypeDefinitionId = Opc.Ua.ObjectTypeIds.FolderType;
        _logger!.LogInformation($"Creating node {currentPath} {nodeObject.NodeId} ({nodeObject.BrowseName})");

        nodeObject.AddReference(ReferenceTypeIds.Organizes, true, Opc.Ua.ObjectIds.ObjectsFolder);
        if (parentNode == null)
        {
          AddPredefinedNode(SystemContext, nodeObject);

          IList<IReference>? references = null;
          if (!externalReferences.TryGetValue(Opc.Ua.ObjectIds.ObjectsFolder, out references))
          {
            externalReferences[Opc.Ua.ObjectIds.ObjectsFolder] = references = new List<IReference>();
          }
          references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, nodeObject.NodeId));
        }


        // Add to dictionary for reusability
        ExistingNodes.Add(currentPath, nodeObject);
        node = nodeObject;

        if (parentNode != null)
        {
          parentNode.AddChild(node);
        }
      } else
      {
        Debug.WriteLine($"Node {currentPath} already exists");
      }

      return node;
    }

    #endregion

    #region Overridden Methods
    #endregion

    #region Private Fields
    private OpcUaServerConfiguration m_configuration;
    #endregion
  }

}
