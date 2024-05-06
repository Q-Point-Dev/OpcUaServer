// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.

using Microsoft.Extensions.Logging;

namespace BaseLibraryCode.WindowsService.Net.Utilities;

public static class ServiceFactory
{
  public static T? Create<T>(ILoggerFactory loggerFactory) where T : class
  {
    return Activator.CreateInstance(typeof(T), loggerFactory) as T;
  }
}
