//----------------------------------------------------------------------------------------------------------------------
// AMMANN GROUP CH-4900 LANGENTHAL                   © 2023 Alle Rechte vorbehalten
//
// Vervielfältigung, Veröffentlichung oder Weitergabe dieses Dokuments oder Teilen daraus sind, zu welchem Zweck und
// in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die Ammann-Group nicht gestattet.
//
// Ersteller der Datei:   Michael Serzhan
//----------------------------------------------------------------------------------------------------------------------

using OpcUaServer.Properties;
using System.Diagnostics;
using Serilog;
using System.IO;
using BaseLibraryCode.WindowsService.Net;


namespace OpcUaServer
{
  public class Program
  {
    private static readonly string assemblyName = typeof(Program).Assembly.GetName().Name!;
    public static readonly string ComaDataPath = GetComaDataPath();
    // private static IHost? DefaultHost;

    private static async Task<int> Main(string[] args)
    {
      ConfigureLogger();

      IHost host = Host.CreateDefaultBuilder(args)
          .ConfigureServices(services =>
          {
            BackgroundServiceBase.ConfigureBaseServices(services);
            services.AddHostedService<WindowsBackgroundService>();
          })
          .UseWindowsService().Build();

      // Exit with an error
      if (host == null)
        return await Task.Run(() => 1);

      Debug.WriteLine("Server 1");
      Log.Information("OpcUaServer begins");

      AppEnvironment.ServiceProvider = host.Services;

      BaseServiceAppEnvironment.ServiceName = Settings.Default.ServiceName;
      BaseServiceAppEnvironment.ServiceDescription = Settings.Default.ServiceDescription;
      BaseServiceAppEnvironment.ServiceProvider = host.Services;

      bool bStartServiceAfterwards = await CommandLineHandler.ProcessArgs(args);
      if (!bStartServiceAfterwards)
        return 0;

      await host.RunAsync();

      // exit without an error
      return 0;
    }

    private static void ConfigureLogger()
    {
      var config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();
  
      Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config)
        .WriteTo.RollingFile(Path.Combine(ComaDataPath, "Logs", Settings.Default.ServiceName, $"{Settings.Default.ServiceName}.log")) // default: max. 31 files
        .CreateLogger();
    }


    private static string GetComaDataPath()
    {
      return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                          "Ammann", "ComaData");
    }

    //static void HandleException(object sender, UnhandledExceptionEventArgs e)
    //{
    //  Debug.WriteLine($". \n {e.ExceptionObject}");
    //}

  }


}
