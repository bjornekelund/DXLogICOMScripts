//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll
//INCLUDE_ASSEMBLY CWKeyer.dll

// ICOM Synchronization of built-in keyer with DXLog.net.
// Event driven, not mapped to any key.
// Works for up to two radios in all operating scenarios including SO2V. 
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
        ContestData cdata;
        int lastFocus;

        static byte[] IcomSetSpeed = new byte[4] { 0x14, 0x0C, 0x00, 0x00 };

	    public void Initialize(FrmMain main)
	    {
            CATCommon radio1 = main.COMMainProvider.RadioObject(1);
            cdata = main.ContestDataProvider;
            mainForm = main;
            lastFocus = 1;

            if ((radio1 != null) && (mainForm._cwKeyer != null))
                if (radio1.IsICOM())
                {
                    // Subscribe to CW speed change event
                    mainForm._cwKeyer.CWSpeedChange += new CWKey.CWSpeedChangeDelegate(HandleCWSpeedChange);
                    // Subscribe to radio focus change event
                    cdata.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);
                }
        }

        public void Deinitialize() { }

        // No key is mapped to this script
        public void Main(FrmMain main, ContestData cdata, COMMain comMain) { }

        // Executes every time DXLog.net keyer speed is changed 
        private void HandleCWSpeedChange(int radioNumber, int newSpeed)
        {
            CATCommon radioObject;
            bool modeIsSO2V = (mainForm.ContestDataProvider.OPTechnique == ContestData.Technique.SO2V);
            int physicalRadio, ICOMspeed;

            // If SO2V, physical radio is always #1
            physicalRadio = modeIsSO2V ? 1 : radioNumber;

            radioObject = mainForm.COMMainProvider.RadioObject(physicalRadio);

            if (radioObject == null)
            {
                mainForm.SetMainStatusText(String.Format("IcomSpeedSynch: Radio {0} is not available!", physicalRadio));
                return;
            }

            // Update radio's keyer speed
            ICOMspeed = (255 * (mainForm._cwKeyer.CWSpeed(radioNumber) - 6)) / (48 - 6); // ICOM scales 6-48 WPM onto 0-255
            IcomSetSpeed[2] = (byte)((ICOMspeed / 100) % 10);
            IcomSetSpeed[3] = (byte)((((ICOMspeed / 10) % 10) << 4) + (ICOMspeed % 10));
            radioObject.SendCustomCommand(IcomSetSpeed);

            if (Debug) mainForm.SetMainStatusText(String.Format(
                "IcomSpeedSynch: {0}. Command: [{1}]. Radio {2} CW speed changed to {3} wpm! R1speed = {4} R2speed = {5}.",
                ICOMspeed, BitConverter.ToString(IcomSetSpeed), radioNumber, mainForm._cwKeyer.CWSpeed(radioNumber), mainForm._cwKeyer.CWSpeed(1), mainForm._cwKeyer.CWSpeed(2))
                );
        }

        // Event handler invoked when switching between radios (in SO2R) or VFO (in SO1R and SO2V) in DXLog.net
        private void HandleFocusChange()
        {
            CATCommon radio1 = mainForm.COMMainProvider.RadioObject(1);
            int focusedRadio = mainForm.ContestDataProvider.FocusedRadio;
            bool modeIsSo2V = (mainForm.ContestDataProvider.OPTechnique == ContestData.Technique.SO2V);

            if (radio1 == null)
            {
                mainForm.SetMainStatusText("IcomSpeedSynch: Radio 1 is not available.");
                return;
            }

            if (modeIsSo2V && (focusedRadio != lastFocus)) // Only active in SO2V and with ICOM. Ignores redundant events.
            {
                lastFocus = focusedRadio;

                // Update radio's keyer speed
                int ICOMspeed = (255 * (mainForm._cwKeyer.CWSpeed(focusedRadio) - 6)) / (48 - 6); // ICOM scales 6-48 WPM onto 0-255
                IcomSetSpeed[2] = (byte)((ICOMspeed / 100) % 10);
                IcomSetSpeed[3] = (byte)((((ICOMspeed / 10) % 10) << 4) + (ICOMspeed % 10));
                radio1.SendCustomCommand(IcomSetSpeed);

                if (Debug) mainForm.SetMainStatusText(
                    String.Format("IcomSpeedSynch: {0}. Command: [{1}]. Radio {2} CW speed changed to {3} wpm!",
                    ICOMspeed, BitConverter.ToString(IcomSetSpeed), focusedRadio, mainForm._cwKeyer.CWSpeed(focusedRadio))
                    );
            }
        }
    }
}