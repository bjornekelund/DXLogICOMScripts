//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V VFO focus and audio management, event driven but also mapped 
// to key for stereo/main toggling when Radio 1 (main receiver in SO2V) is 
// selected. Key used is typically § (on EU keyboard) or `(on US keyboard) 
// to maintain muscle-memory compatibility with N1MM.
// Tested on IC-7610 but should work on most dual receiver ICOM radios.
// Use Ctrl-Alt-S/AltGr-S to toggle between permanent dual receive and 
// only dual only when sub receiver is focused. Since pPressing Ctrl-Alt-S 
// does not trigger an event, any change in audio mode will not come 
// into force until the next focus switch. 
// Only active for ICOM radio but does not verify radio is SO2V capable
// By Björn Ekelund SM7IUN bjorn@ekelund.nu 2019-01-31

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomSo2v : ScriptClass
    {
        ContestData cdata;
        FrmMain frmMain;

        readonly bool Debug = false;
        readonly byte[] IcomDualWatchOn = { 0x07, 0xC1 };
        readonly byte[] IcomDualWatchOff = { 0x07, 0xC0 };
        readonly byte[] IcomSelectMain = { 0x07, 0xD0 };
        readonly byte[] IcomSelectSub = { 0x07, 0xD1 };
        readonly byte[] IcomSplitOff = { 0x0F, 0x00 };
        bool tempStereoAudio;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            CATCommon radio1 = main.COMMainProvider.RadioObject(1);

            cdata = main.ContestDataProvider;
            frmMain = main;
            // Initialize temporary stereo mode to DXLog's stereo mode to support temporary toggle
            tempStereoAudio = (frmMain.ListenStatusMode != 0); 

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
                    if (radio1Present) radio1.SendCustomCommand(IcomDualWatchOff);
                    if (Debug) main.SetMainStatusText(String.Format("IcomSo2v: Focus is Radio #1, Main Receiver"));
                }
            else
                {
                    if (radio1Present) radio1.SendCustomCommand(IcomDualWatchOn);
                    if (Debug) main.SetMainStatusText(String.Format("IcomSo2v: Focus is Radio #1, Stereo"));
                }
            tempStereoAudio = !tempStereoAudio;
        }

        // Event handler invoked when switching between VFO in DXLog.net
        private void HandleFocusChange()
        {
            CATCommon radio1 = frmMain.COMMainProvider.RadioObject(1);
            int focusedRadio = frmMain.ContestDataProvider.FocusedRadio;
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            int listenMode = frmMain.ListenStatusMode;
            bool stereoAudio = (listenMode != 0);
            bool modeIsSo2V = (frmMain.ContestDataProvider.OPTechnique == 4);

            if (radio1 == null)
            {
                frmMain.SetMainStatusText("IcomSo2v: Radio 1 is not available.");
                return;
            }

            if (modeIsSo2V) // Only active in SO2V and with ICOM
            {
                tempStereoAudio = stereoAudio; // Set temporary stereo mode to DXLog's stereo mode to support temporary toggle
                if (focusedRadio == 1)
                    radio1.SendCustomCommand(IcomSelectMain);
                else
                    radio1.SendCustomCommand(IcomSelectSub);

                if (stereoAudio || (focusedRadio == 2))
                {
                    radio1.SendCustomCommand(IcomDualWatchOn);
                    if (Debug) frmMain.SetMainStatusText(String.Format("IcomSo2v: Listenmode {0}. Focus is Radio #{1}, Dual Watch", 
                        listenMode, focusedRadio));
                }
                else
                {
                    radio1.SendCustomCommand(IcomDualWatchOff);
                    if (Debug) frmMain.SetMainStatusText(String.Format("IcomSo2v: Listenmode {0}. Focus is Radio #{1}, Main Receiver",
                        listenMode, focusedRadio));
                }
            }
        }
    }
}