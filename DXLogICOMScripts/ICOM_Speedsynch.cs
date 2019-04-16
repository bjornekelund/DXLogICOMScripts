//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll
//INCLUDE_ASSEMBLY CWKeyer.dll

// ICOM Synchronization of built-in keyer with DXLog.net.
// Event driven, not mapped to any key.
// Works for up to two radios in all operating scenarios with the exception 
// of VFO B in SO2V where the speed is ignored.
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-01-31

using System;
using CWKeyer;
using IOComm;

namespace DXLog.net
{
    public class IcomSpeedSync : ScriptClass
    {
        readonly bool Debug = false;
        FrmMain mainForm;
        static byte[] IcomSetSpeed = new byte[4] { 0x14, 0x0C, 0x00, 0x00 };

	    public void Initialize(FrmMain main)
	    {
            mainForm = main;

            // Subscribe to speed change event
            if (mainForm._cwKeyer != null)
                mainForm._cwKeyer.CWSpeedChange += new CWKey.CWSpeedChangeDelegate(HandleCWSpeedChange);
	    }

	    public void Deinitialize() { }

        // No key is mapped to this script
        public void Main(FrmMain main, ContestData cdata, COMMain comMain) { }

        // Executes every time DXLog.net keyer speed is changed 
        private void HandleCWSpeedChange(int radioNumber, int newSpeed)
        {
            CATCommon radioObject;
            bool modeIsSO2V = (mainForm.ContestDataProvider.OPTechnique == 4);
            int physicalRadio, ICOMspeed;

            if (modeIsSO2V) // If radio 2 is selected in SO2V, this is physical radio 1
                physicalRadio = 1;
            else
                physicalRadio = radioNumber;

            radioObject = mainForm.COMMainProvider.RadioObject(physicalRadio);

            if (radioObject == null)
            {
                mainForm.SetMainStatusText(String.Format("IcomSpeedSynch: Radio {0} is not available!", physicalRadio));
                return;
            }

            ICOMspeed = (255 * (mainForm._cwKeyer.CWSpeed(radioNumber) - 6)) / (48 - 6); // ICOM scales 6-48 WPM onto 0-255
            IcomSetSpeed[2] = (byte)((ICOMspeed / 100) % 10);
            IcomSetSpeed[3] = (byte)((((ICOMspeed / 10) % 10) << 4) + (ICOMspeed % 10));
            radioObject.SendCustomCommand(IcomSetSpeed);

            if (Debug) mainForm.SetMainStatusText(String.Format("IcomSpeedSynch: {0}. Command: [{1}]. Radio {2} CW speed changed to {3} wpm! R1speed = {4} R2speed = {5}.",
                ICOMspeed, BitConverter.ToString(IcomSetSpeed), radioNumber, mainForm._cwKeyer.CWSpeed(radioNumber), mainForm._cwKeyer.CWSpeed(1), mainForm._cwKeyer.CWSpeed(2)));
        }

    }
}