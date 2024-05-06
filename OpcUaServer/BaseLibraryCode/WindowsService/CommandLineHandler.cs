// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.

using BaseLibraryCode.WindowsService.Net.Utilities;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.CommandLine;
using System.Diagnostics;
using System.IO;


namespace BaseLibraryCode.WindowsService.Net
{
  public class CommandLineHandler
  {
  
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns>true if the service should be started, false if the args have been correctly processed and the servcie
    /// doesn't need to be started</returns>
    public static async Task<bool> ProcessArgs(string[] args)
    {

      bool bStartServiceAfterwards = true;

      if (WindowsServiceHelpers.IsWindowsService())
        return bStartServiceAfterwards;

      // define commandline options
      var installOption = new Option<bool>(new[] { "--install", "-i", "/i" }, "Resources.ServiceInstall");
      var uninstallOption = new Option<bool>(new[] { "--uninstall", "-u", "/u" }, "Resources.ServiceUninstall");
      var managerOption = new Option<bool>(new[] { "--manager", "-m", "/m" }, "Resources.ServiceManager");
      var debugOption = new Option<bool>(new[] { "--debug", "-d", "/d" }, "Resources.ServiceDebug");

      var cmd = new RootCommand
      {
        installOption,
        uninstallOption,
        managerOption,
        debugOption
      };

      // setup commandline handler
      cmd.SetHandler(async (bool install, bool uninstall, bool manager, bool debug) =>
      {
        if (install)
        {
          try
          {
            bStartServiceAfterwards = false;
            Installer.InstallService(BaseServiceAppEnvironment.ServiceName!, BaseServiceAppEnvironment.ServiceDescription!);
          }
          catch (InstallerException instEx)
          {
            // Use hard-coded text to avoid DB call
            MessageBox.Show(instEx.Message, "Install Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            throw;
          }
        }
        else if (uninstall)
        {
          try
          {
            bStartServiceAfterwards = false;
            Installer.UninstallService(BaseServiceAppEnvironment.ServiceName!);
          }
          catch (InstallerException instEx)
          {
            MessageBox.Show(instEx.Message, "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            throw;
          }
        }
        else if (manager)
        {
          try
          {
            bStartServiceAfterwards = false;
            var hostingExe = System.Reflection.Assembly.GetEntryAssembly();
            string managerName = hostingExe!.GetName().Name + "Manager.exe";

            ProcessStartInfo psi = new ProcessStartInfo
            {
              CreateNoWindow = true,
              UseShellExecute = false,

#if DEBUG
              WorkingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
#endif
              FileName = managerName
            };

            var process = Process.Start(psi);
            if (process == null)
            {
              MessageBox.Show("Manager could not be started", "Manager Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
              return;
            }              

            await process.WaitForExitAsync();
            process.Dispose();
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            MessageBox.Show("Manager could not be started", "Manager Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
        }
        else if (debug)
        {
          bStartServiceAfterwards = true;
        }
      }, installOption, uninstallOption, managerOption, debugOption);


      try
      {
        await cmd.InvokeAsync(args);
      }
      catch (Exception ex)
      {
        //  Any exceptions thrown in InvokeAsync are already handled/swallowed
        // https://github.com/dotnet/command-line-api/issues/796
        Console.WriteLine(ex);
      }

      return bStartServiceAfterwards;
    }


  }


}
