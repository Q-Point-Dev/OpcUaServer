using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibraryCode.WindowsService.Net.Api.Models
{
  public class JournalEntry
  {
    public DateTime datetimeStamp { get; set; }
    public string category { get; set; }
    public string description { get; set; }

    public JournalEntry()
    {
      category = "";
      description = "";
    }
  }
}
