// © 2022 Ammann-Group Switzerland. All rights reserved.
// Changes to or the duplication, publication or transmission of this entire document or parts of it, for whatever
// purpose and in whatever form, is not permitted without express written authorization from the Ammann Group.



using BaseLibraryCode.WindowsService.Net.Api.Models;


namespace BaseLibraryCode.WindowsService.Net.Api
{
  public class GetJournalEntries
  {
    public List<JournalEntry> GetEntries(int topOf, string progSourceName)
    {
      List<JournalEntry> lst = new List<JournalEntry>();

      /* Not called in OpcUaServer to avoid DB call*/
      return lst;
    }





  }











}
