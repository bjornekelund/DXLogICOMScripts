//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll
//INCLUDE_ASSEMBLY CWKeyer.dll

// ICOM Synchronization of built-in keyer with DXLog
// Event driven, not mapped to any key
// Works for up to two radios in all operating scenarios with the exception 
// of VFO B in SO2V where the speed is ignored
// By Björn Ekelund SM7IUN bjorn@ekelund.nu 2019-01-31

using System;
using System.Windows.Forms;
using CWKeyer;
using IOComm;

namespace DXLog.net
{
    public class IcomSpeedSync : ScriptClass
    {
        FrmMain mainForm;
        static byte[] IcomSetSpeed = new byte[4] { 0x14, 0x0C, 0x00, 0x00 };
        readonly bool Debug = false;

	    public void Initialize(FrmMain main)
	    {
            mainForm = main;
            if (mainForm._cwKeyer != null)
                mainForm._cwKeyer.CWSpeedChange += new CWKey.CWSpeedChangeDelegate(HandleCWSpeedChange);
	    }

	    public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain) { }

        private void HandleCWSpeedChange(int radioNumber, int newSpeed)
        {
            CATCommon radioObject = mainForm.COMMainProvider.RadioObject(radioNumber);
            bool radio2focused = (mainForm.ContestDataProvider.FocusedRadio == 2);
            bool modeIsSO2V = (mainForm.ContestDataProvider.OPTechnique == 4);

            if (radio2focused && modeIsSO2V) // If radio 2 is selected in SO2V, do nothing
                return;

            if (radioObject == null)
            {
                mainForm.SetMainStatusText(String.Format("IcomSpeedSynch: Radio {0} is not available!", radioNumber));
                return;
            }

            int ICOMspeed = (255 * (newSpeed - 6)) / (48 - 6); // ICOM scales 6-48 WPM onto 0-255

            IcomSetSpeed[2] = (byte) ((ICOMspeed / 100) % 10);
            IcomSetSpeed[3] = (byte) ((((ICOMspeed / 10) % 10) << 4) + (ICOMspeed % 10));

            radioObject.SendCustomCommand(IcomSetSpeed); 

            if (Debug) mainForm.SetMainStatusText(String.Format("IcomSpeedSynch: {0}. Command: [{1}]. Radio {2} CW speed changed to {3} wpm!", 
                ICOMspeed, BitConverter.ToString(IcomSetSpeed), radioNumber, newSpeed));
        }
    }
}