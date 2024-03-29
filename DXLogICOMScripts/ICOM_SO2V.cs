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
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-09-19
// Updated by James M1DST 2024-03-28

//
// NB. For educational purposes only. This functionality is now native to DXLog.
//

using IOComm;
using NAudio.Midi;

namespace DXLog.net
{
    public class IcomSO2V : IScriptClass
    {
        readonly bool Debug = false;
        FrmMain mainForm;

        readonly byte[] IcomDualWatchOn = { 0x07, 0xC1 };
        readonly byte[] IcomDualWatchOff = { 0x07, 0xC0 };
        readonly byte[] IcomSelectMain = { 0x07, 0xD0 };
        readonly byte[] IcomSelectSub = { 0x07, 0xD1 };
        readonly byte[] IcomSplitOff = { 0x0F, 0x00 };

        delegate void HandleListenStatusChangeCB(int newMode);

        bool tempStereoAudio;
        int lastFocus;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            var radio1 = main.COMMainProvider.RadioObject(1);
            mainForm = main;
            var modeIsSo2V = (mainForm.ContestDataProvider.OPTechnique == ContestData.Technique.SO2V);

            if (modeIsSo2V)
            {
                // Initialize temporary stereo mode to DXLog's stereo mode to support temporary toggle
                // At start up, radio 1 is always focused and stereo audio is disabled
                tempStereoAudio = false;
                lastFocus = 1;

                main.ContestDataProvider.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);

                // Only initialize radio if present and ICOM 
                if (radio1 != null)
                    if (radio1.IsICOM())
                    {
                        // Initialize radio to DW off, Split off and Main VFO focused
                        radio1.SendCustomCommand(IcomDualWatchOff);
                        radio1.SendCustomCommand(IcomSelectMain);
                        radio1.SendCustomCommand(IcomSplitOff);
                    }
            }
        }

        public void Deinitialize() { }

        // Toggle dual watch when radio 1 is focused in SO2V. Typically mapped to a key in upper left corner of keyboard.
        public void Main(FrmMain mainForm, ContestData cdata, COMMain comMain, MidiEvent midiEvent)
        {
            var focusedRadio = cdata.FocusedRadio;
            var radio1 = comMain.RadioObject(focusedRadio);
            var radio1Present = radio1 != null;

            if ((focusedRadio == 1) && cdata.OPTechnique == ContestData.Technique.SO2V)
            { // If VFO A focused, SO2V and radio present
                tempStereoAudio = !tempStereoAudio;
                mainForm.SetMainStatusText(tempStereoAudio ? "Both receivers." : "Main receiver only.");
                if (radio1Present)
                    radio1.SendCustomCommand(tempStereoAudio ? IcomDualWatchOn : IcomDualWatchOff);
            }
        }

        // Event handler invoked when switching between VFO (SO2V) in DXLog.net
        void HandleFocusChange()
        {
            var radio1 = mainForm.COMMainProvider.RadioObject(1);
            var focusedRadio = mainForm.ContestDataProvider.FocusedRadio;
            var stereoAudio = mainForm.ListenStatusMode == COMMain.ListenMode.R1R2;
            var radio1Present = radio1 != null;

            if (focusedRadio != lastFocus) // Only active in SO2V and with ICOM. Ignore false invokes.
            {
                // Set temporary stereo mode to DXLog's stereo mode to support temporary toggle
                tempStereoAudio = stereoAudio; 
                lastFocus = focusedRadio;

                if (radio1Present)
                {
                    radio1.SendCustomCommand(focusedRadio == 1 ? IcomSelectMain : IcomSelectSub);
                    radio1.SendCustomCommand(stereoAudio || (focusedRadio == 2) ? IcomDualWatchOn : IcomDualWatchOff);
                }

                if (Debug) mainForm.SetMainStatusText($"IcomSO2V: Listenmode {mainForm.ListenStatusMode}. Focus is Radio #{focusedRadio}");
                else
                    mainForm.SetMainStatusText("Focus on " + ((focusedRadio == 1) ? "main" : "sub") + " VFO. " +
                        (stereoAudio || (focusedRadio == 2) ? "Both receivers" : "Main receiver only"));
            }
        }
    }
}
