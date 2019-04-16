//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V VFO focus and audio management, event driven but also mapped 
// to key for stereo/main toggling when Radio 1 (main receiver in SO2V) is 
// selected. Key used is typically § (on EU keyboard) or `(on US keyboard) 
// to maintain muscle-memory compatibility with N1MM.
// Tested on IC-7610 but should work on all modern dual receiver ICOM radios.
// Use Ctrl-Alt-S/AltGr-S to toggle between permanent dual receive and 
// only dual only when sub receiver is focused. Since pressing Ctrl-Alt-S 
// does not trigger an event, any change in audio mode will not come 
// into force until the next focus switch. 
// Only active for ICOM radio but does not verify radio is SO2V capable
// Also synchronizes the radio's built-in keyer with the setting 
// in DXLog. 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-04-16

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomSO2V : ScriptClass
    {
        readonly bool Debug = false;
        ContestData cdata;
        FrmMain mainForm;

        readonly byte[] IcomDualWatchOn = { 0x07, 0xC1 };
        readonly byte[] IcomDualWatchOff = { 0x07, 0xC0 };
        readonly byte[] IcomSelectMain = { 0x07, 0xD0 };
        readonly byte[] IcomSelectSub = { 0x07, 0xD1 };
        readonly byte[] IcomSplitOff = { 0x0F, 0x00 };
        static byte[] IcomSetSpeed = new byte[4] { 0x14, 0x0C, 0x00, 0x00 };

        bool tempStereoAudio;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            CATCommon radio1 = main.COMMainProvider.RadioObject(1);
            cdata = main.ContestDataProvider;
            mainForm = main;

            // Initialize temporary stereo mode to DXLog's stereo mode to support temporary toggle
            tempStereoAudio = (mainForm.ListenStatusMode != 0); 

            // Only subscribe to event and initialize if radio is ICOM
            if (radio1 != null)
                if (radio1.IsICOM()) { // nesting if since method only valid if radio1 is != null
                    cdata.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);

                    // Initialize radio to DW off, Split off and Main VFO focused
                    radio1.SendCustomCommand(IcomDualWatchOff); 
                    radio1.SendCustomCommand(IcomSelectMain);
                    radio1.SendCustomCommand(IcomSplitOff);
                }
        }

        public void Deinitialize() { } // Do nothing at DXLog.net close down


        // Toggle dual watch, execution of Main is mapped to a key, typically in upper left corner of keyboard
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            CATCommon radio1 = comMain.RadioObject(focusedRadio);
            bool radio1Present = (radio1 != null);

            if ((focusedRadio == 1) && cdata.OPTechnique == 4) // Only active in SO2V
                if (tempStereoAudio)
                {
                    if (radio1Present)
                        radio1.SendCustomCommand(IcomDualWatchOff);
                    if (Debug)
                        main.SetMainStatusText(String.Format("IcomSO2V: Focus is Radio #1, Main Receiver"));
                }
            else
                {
                    if (radio1Present)
                        radio1.SendCustomCommand(IcomDualWatchOn);
                    if (Debug)
                        main.SetMainStatusText(String.Format("IcomSO2V: Focus is Radio #1, Stereo"));
                }
            tempStereoAudio = !tempStereoAudio;
        }

        // Event handler invoked when switching between radios (SO2R) or VFO (SO2V) in DXLog.net
        private void HandleFocusChange()
        {
            CATCommon radio1 = mainForm.COMMainProvider.RadioObject(1);
            int focusedRadio = mainForm.ContestDataProvider.FocusedRadio;
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            int listenMode = mainForm.ListenStatusMode;
            bool stereoAudio = (listenMode != 0);
            bool modeIsSo2V = (mainForm.ContestDataProvider.OPTechnique == 4);
            int ICOMspeed;

            if (radio1 == null)
            {
                mainForm.SetMainStatusText("IcomSO2V: Radio 1 is not available.");
                return;
            }

            if (modeIsSo2V) // Only active in SO2V and with ICOM
            {
                tempStereoAudio = stereoAudio; // Set temporary stereo mode to DXLog's stereo mode to support temporary toggle
                if (focusedRadio == 1)
                    radio1.SendCustomCommand(IcomSelectMain);
                else
                    radio1.SendCustomCommand(IcomSelectSub);

                // Update keyer speed
                ICOMspeed = (255 * (mainForm._cwKeyer.CWSpeed(focusedRadio) - 6)) / (48 - 6); // ICOM scales 6-48 WPM onto 0-255
                IcomSetSpeed[2] = (byte)((ICOMspeed / 100) % 10);
                IcomSetSpeed[3] = (byte)((((ICOMspeed / 10) % 10) << 4) + (ICOMspeed % 10));
                radio1.SendCustomCommand(IcomSetSpeed);

                if (stereoAudio || (focusedRadio == 2))
                {
                    radio1.SendCustomCommand(IcomDualWatchOn);
                    if (Debug) mainForm.SetMainStatusText(String.Format("IcomSO2V: Listenmode {0}. Focus is Radio #{1}, Dual Watch. Keyer speed = {2}", 
                        listenMode, focusedRadio, mainForm._cwKeyer.CWSpeed(focusedRadio)));
                }
                else
                {
                    radio1.SendCustomCommand(IcomDualWatchOff);
                    if (Debug) mainForm.SetMainStatusText(String.Format("IcomSO2V: Listenmode {0}. Focus is Radio #{1}, Main Receiver. Keyer speed = {2}",
                        listenMode, focusedRadio, mainForm._cwKeyer.CWSpeed(focusedRadio)));
                }

            }
        }
    }
}