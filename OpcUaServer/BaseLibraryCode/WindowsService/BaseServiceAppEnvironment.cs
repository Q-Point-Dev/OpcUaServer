// © 2023 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.


using Microsoft.Extensions.Logging;


namespace BaseLibraryCode.WindowsService.Net
{

  public static class BaseServiceAppEnvironment
  {
    /// <summary>
    /// 
    /// </summary>
    public static string? ServiceName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static string? ServiceDescription { get; set; }

    /// <summary>
    /// Host service provider
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; set; }

  }

  public class ApplicationLogging
  {
    private static ILoggerFactory? _Factory;

    public static void ConfigureLogger(ILoggerFactory factory)
    {
      // factory.AddDebug(LogLevel.None).AddStackify();
      // factory.AddFile("logFileFromHelper.log"); //serilog file extension
    }

    public static ILoggerFactory LoggerFactory
    {
      get
      {
        if (_Factory == null)
        {
          _Factory = new LoggerFactory();
          ConfigureLogger(_Factory);
        }
        return _Factory;
      }
      set { _Factory = value; }
    }
    public static ILogger CreateLogger(string name) => LoggerFactory.CreateLogger(name);
  }

}

