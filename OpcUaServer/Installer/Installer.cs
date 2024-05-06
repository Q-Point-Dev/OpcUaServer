//----------------------------------------------------------------------------------------------------------------------
// AMMANN GROUP CH-4900 LANGENTHAL                   © 2023 Alle Rechte vorbehalten
//
// Vervielfältigung, Veröffentlichung oder Weitergabe dieses Dokuments oder Teilen daraus sind, zu welchem Zweck und
// in welcher Form auch immer, ohne die ausdrückliche schriftliche Genehmigung durch die Ammann-Group nicht gestattet.
//
// Ersteller der Datei:   Michael Serzhan
//----------------------------------------------------------------------------------------------------------------------
using Microsoft.Win32;
using OpcUaServer.Properties;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace OpcUaServer;

public static class Installer
{
  #region public methods
  public static void InstallService(string serviceName, string description, string executableName)
  {
    var result = true;

    // Service is already installed
    if (IsInstalled(serviceName))
    {
      MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.ServiceAlreadyInstalled, serviceName), 
                      Resources.InstallService, 
                      MessageBoxButtons.OK, 
                      MessageBoxIcon.Information);
      return; 
    }

    try
    {
      var binaryPath = $"{Path.Combine(AppContext.BaseDirectory, executableName)}.exe";
      result = ExecuteScUtility($@"create ""{serviceName}"" type= own start= auto displayname= ""Ammann {serviceName}"" binpath= ""{binaryPath}""");
      if (!result)
        result = ExecuteScUtility($@"description ""{serviceName}"" ""{description}""");
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"install failed. {ex}");
    }
    
    MessageBox.Show(result
        ? string.Format(CultureInfo.CurrentCulture, Resources.ServiceInstalledSuccessfully, serviceName)
        : string.Format(CultureInfo.CurrentCulture, Resources.ServiceInstalledFailed, serviceName),
      Resources.InstallService, MessageBoxButtons.OK, MessageBoxIcon.Information);
  }

  public static void UninstallService(string serviceName)
  {
    var result  = true;

    // Service is not installed
    if (!IsInstalled(serviceName))
    {
      MessageBox.Show(string.Format(CultureInfo.CurrentCulture, Resources.ServiceNotInstalled, serviceName), 
                      Resources.InstallService, 
                      MessageBoxButtons.OK, 
                      MessageBoxIcon.Information);
      return; 
    }

    try
    {
      result = ExecuteScUtility($@"delete {serviceName}");
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"uninstall failed. {ex}");
    }

    MessageBox.Show(result ? string.Format(CultureInfo.CurrentCulture, Resources.ServiceUninstalledSuccessfully, serviceName)
                           : string.Format(CultureInfo.CurrentCulture, Resources.ServiceUninstalledFailed, serviceName),
                    Resources.UninstallService, MessageBoxButtons.OK, MessageBoxIcon.Information);
  }
  #endregion

  #region private methods
  private static bool IsInstalled(string servicename)
  {
    using var registryKey =
      Registry.LocalMachine.OpenSubKey(@$"System\CurrentControlSet\Services\{servicename}");
    registryKey?.Close();

    return registryKey != null;
  }

  private static bool ExecuteScUtility(string arguments)
  {
    var info = new ProcessStartInfo
    {
      CreateNoWindow = true,
      UseShellExecute = true,
      FileName = "sc.exe",
      Arguments = arguments,
      Verb = "runas"
    };

    var process = Process.Start(info);
    process?.WaitForExit();

    var exitCode = process?.ExitCode;
    process?.Dispose();

    Debug.WriteLine($"sc.exe ExitCode: {exitCode}");

    return exitCode == 0;
  }
  #endregion
}