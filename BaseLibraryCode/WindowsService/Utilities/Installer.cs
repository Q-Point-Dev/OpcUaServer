using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace BaseLibraryCode.WindowsService.Net.Utilities
{
  /// <summary>
  /// 
  /// </summary>
  public static class Installer
  {
    #region public methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="servicename"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    /// <exception cref="InstallerException"></exception>
    public static int InstallService(string servicename, string description)
    {
      // returncode
      int rc = 1;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
      string executableName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name!;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

      if (IsInstalled(servicename))
      {
        throw new InstallerException("Service is already installed");
      }

      try
      {
        var binPath = $"{Path.Combine(AppContext.BaseDirectory, executableName)}.exe";
        rc = ExecuteScUtility($@"create ""{servicename}"" type= own start= auto displayname= ""Ammann {servicename}"" binpath= ""{binPath}""");
        if (rc == 0)
          rc &= ExecuteScUtility($@"description ""{servicename}"" ""{description}""");
      }
      catch (Exception ex)
      {
        throw new InstallerException(ex);
      }
      return rc;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="servicename"></param>
    /// <returns></returns>
    /// <exception cref="InstallerException"></exception>
    public static int UninstallService(string servicename)
    {
      // returncode
      int rc = 1;

      if (!IsInstalled(servicename))
      {
        throw new InstallerException("Service is not installed");
      }

      try
      {
        rc = ExecuteScUtility($@"delete {servicename}");
      }
      catch (Exception ex)
      {
        throw new InstallerException(ex);
      }
      return rc;
    }



    #endregion

    #region private methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="servicename"></param>
    /// <returns></returns>
    private static bool IsInstalled(string servicename)
    {
      using var registryKey =
        Registry.LocalMachine.OpenSubKey(@$"System\CurrentControlSet\Services\{servicename}");
      registryKey?.Close();

      return registryKey != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private static int ExecuteScUtility(string arguments)
    {
      var process = Process.Start(new ProcessStartInfo
      {
        CreateNoWindow = true,
        UseShellExecute = true,
        FileName = "sc.exe",
        Arguments = arguments,
        Verb = "runas"
      });

      process?.WaitForExit();

      var exitCode = process?.ExitCode;
      process?.Dispose();
      return exitCode ?? 0;
    }
    #endregion
  }


  /// <summary>
  /// 
  /// </summary>
  [Serializable]
  class InstallerException : Exception
  {
    public InstallerException() { }

    public InstallerException(string reason)
        : base(String.Format("InstallerException: {0}", reason))
    {

    }

    public InstallerException(Exception innerException)
        : base("InstallerException, see inner exception for details", innerException)
    {

    }
  }
}
