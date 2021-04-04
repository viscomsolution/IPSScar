using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management; 


namespace TGMTcs
{
    public class TGMThardware
    {
        public static string GetCpuId()
        {
            //must add reference to System.Management
            ManagementObjectCollection mbsList = null;
            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select ProcessorID From Win32_processor");
            mbsList = mbs.Get();
            string id = "";
            foreach (ManagementObject mo in mbsList)
            {
                if (mo["ProcessorID"] != null)
                    id = mo["ProcessorID"].ToString();
            }
            return id;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetMainboardId()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            ManagementObjectCollection moc = mos.Get();
            string serial = "";
            foreach (ManagementObject mo in moc)
            {
                serial = (string)mo["SerialNumber"];
            }
            if (serial == "To be filled by O.E.M.")
                return "";
            return serial;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetPartitionId(string partition)
        {
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + partition + "\"");
            dsk.Get();
            string diskid = dsk["VolumeSerialNumber"].ToString();
            return diskid;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetMacAddress()
        {
            ManagementObjectSearcher objMOS = new ManagementObjectSearcher("Select MacAddress FROM Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMOS.Get();

            foreach (ManagementObject objMO in objMOC)
            {
                object tempMacAddrObj = objMO["MacAddress"];

                if (tempMacAddrObj != null) //Skip objects without a MACAddress
                {
                    return ((string)tempMacAddrObj).Replace(":", "");
                }
            }
            return "";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetUDID()
        {
            string udid = GetCpuId() + GetPartitionId("C:");
            string mainboardId = GetMainboardId();
            if (mainboardId != "" && mainboardId != "Default string" && mainboardId.Length > 5)
                udid += mainboardId;

            udid += GetMacAddress();
            udid.Replace(" ", "");

            int index = udid.Length / 2;
            udid = udid.Substring(index) + udid.Substring(0, index);
            udid = TGMTutil.RemoveSpecialCharacter(udid);
            udid = udid.ToLower();
            return udid;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string GetPCname()
        {
            return Environment.MachineName;
        }
    }
}
