//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V VFO focus and audio management, event driven but also mapped 
// to key for stereo/main toggling when Radio 1 (main receiver in SO2V) is 
// selected. Key used is typically § (on EU keyboard) or `(on US keyboard) 
// to maintain muscle-memory compatibility with N1MM.
// Tested on IC-7610 but should work on all modern dual receiver ICOM radios.
// Use Ctrl-Alt-S/AltGr-S to toggle between permanent dual receive and 
// dual receive only when sub receiver is focused. Since pressing Ctrl-Alt-S 
// does not trigger an event, any change in audio mode will not come 
// into force until the next focus switch. 
// Only active for ICOM radio but does not verify radio is SO2V capable
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
        readonly string statusMessage = "Focus on {0} VFO. {1}.";

        delegate void HandleListenStatusChangeCB(int newMode);

        bool tempStereoAudio;
        int lastFocus;

        // Executes at DXLog.net start 

        public void Initialize(FrmMain main)
        {
            CATCommon radio1 = main.COMMainProvider.RadioObject(1);
            cdata = main.ContestDataProvider;
            mainForm = main;

            // Initialize temporary stereo mode to DXLog's stereo mode to support temporary toggle
            // At start up, radio 1 is always focused and stereo audio is disabled
            tempStereoAudio = (mainForm.ListenStatusMode != 0);
            lastFocus = 1;

            // Only initialize radio if present and ICOM 
            cdata.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);

            if (radio1 != null)
                if (radio1.IsICOM()) {
                    // Initialize radio to DW off, Split off and Main VFO focused
                    radio1.SendCustomCommand(IcomDualWatchOff); 
                    radio1.SendCustomCommand(IcomSelectMain);
                    radio1.SendCustomCommand(IcomSplitOff);
                }
        }

        public void Deinitialize() { }

        // Toggle dual watch when radio 1 is focused in SO2V. Typically mapped to a key in upper left corner of keyboard.

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            CATCommon radio1 = comMain.RadioObject(focusedRadio);
            bool radio1Present = (radio1 != null);

            if ((focusedRadio == 1) && cdata.OPTechnique == ContestData.Technique.SO2V)
            { // If VFO A focused, SO2V and radio present
                tempStereoAudio = !tempStereoAudio;
                mainForm.SetMainStatusText(String.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", tempStereoAudio ? "Dual Watch" : "Main Receiver"));
                if (radio1Present)
                    radio1.SendCustomCommand(tempStereoAudio ? IcomDualWatchOn : IcomDualWatchOff);
            }
        }

        // Event handler invoked when switching between radios (SO2R) or VFO (SO2V) in DXLog.net

        private void HandleFocusChange()
        {
            CATCommon radio1 = mainForm.COMMainProvider.RadioObject(1);
            int focusedRadio = cdata.FocusedRadio;
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            int listenMode = mainForm.ListenStatusMode;
            bool stereoAudio = (listenMode != 0);
            bool modeIsSo2V = (cdata.OPTechnique == ContestData.Technique.SO2V);
            string audioStatus;
            bool noRadio = radio1 == null;

            if (modeIsSo2V && focusedRadio != lastFocus) // Only active in SO2V and with ICOM. Ignore false triggers.
            {
                tempStereoAudio = stereoAudio; // Set temporary stereo mode to DXLog's stereo mode to support temporary toggle
                lastFocus = focusedRadio;

                if (!noRadio)
                {
                    radio1.SendCustomCommand(focusedRadio == 1 ? IcomSelectMain : IcomSelectSub);
                    radio1.SendCustomCommand(stereoAudio || (focusedRadio == 2) ? IcomDualWatchOn : IcomDualWatchOff);
                }

                audioStatus = stereoAudio || (focusedRadio == 2) ? "Dual Watch" : "Main Receiver";

                if (Debug) mainForm.SetMainStatusText(String.Format("IcomSO2V: Listenmode {0}. Focus is Radio #{1}, {2}.",
                    listenMode, focusedRadio, audioStatus));
                else
                    mainForm.SetMainStatusText(String.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", audioStatus));

            }
        }
    }
}
