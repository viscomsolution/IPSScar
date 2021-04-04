using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGMTcs
{
    class TGMTlog
    {
        string m_source;

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TGMTlog(string source)
        {
            m_source = source;

            if (!EventLog.SourceExists(m_source))
                EventLog.CreateEventSource(m_source, "Application");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void WriteEventLog(string message, EventLogEntryType type, int id = 0)
        { 
            try
            {
                EventLog.WriteEntry(m_source, message, type, id);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
