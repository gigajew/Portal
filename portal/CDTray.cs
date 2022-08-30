using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace portal
{
    internal class CDTray
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        public static extern int mciSendStringA(string lpstrCommand, string lpstrReturnString,
                                    int uReturnLength, int hwndCallback);
        public static void Open()
        {
            string returnString = "";
            var cdDrives = DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.CDRom);
            foreach(var drive in cdDrives )
            {
                var formattedName = drive.Name.Substring(0, 1);
                Console.WriteLine(formattedName);

                mciSendStringA("open " + formattedName + ": type CDaudio alias drive" + formattedName,
                 returnString, 0, 0);
                mciSendStringA("set drive" + formattedName + " door open", returnString, 0, 0);
            }
            
        }
        public static void Close()
        {
            string returnString = "";
            var cdDrives = DriveInfo.GetDrives().Where(drive => drive.DriveType == DriveType.CDRom);
            foreach (var drive in cdDrives)
            {
                var formattedName = drive.Name.Substring(0, 1);
                Console.WriteLine(formattedName);

                mciSendStringA("open " + formattedName + ": type CDaudio alias drive" + formattedName,
                 returnString, 0, 0);
                mciSendStringA("set drive" + formattedName + " door closed", returnString, 0, 0);
            }
        }
    }
}
