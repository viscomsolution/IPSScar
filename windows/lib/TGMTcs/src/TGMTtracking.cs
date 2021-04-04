using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TGMTcs
{
    class TGMTtracking
    {
        static TGMTtracking m_instance;
        string m_host;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static TGMTtracking GetInstance()
        {
            if (m_instance == null)
                m_instance = new TGMTtracking();
            return m_instance;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        void Tracking()
        {
            if (TGMTonline.IsInternetAvailable(m_host))
            {
                string request = m_host + "?";

                request += "user=" + Environment.UserName;
                request += "&pc=" + Environment.MachineName;
                request += "&udid=" + TGMThardware.GetUDID();
                request += "&version=" + TGMTutil.GetVersion();

                string respond = TGMTonline.SendGETrequest(request);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Init(string host)
        {
            m_host = host;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SendUsage()
        {
            Thread t = new Thread(new ThreadStart(Tracking));
            t.IsBackground = true;
            t.Start();
        }
    }
}
