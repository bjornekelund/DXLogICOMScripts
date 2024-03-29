// ICOM SO2V helper script to toggle permanent dual watch on and off. 
// Mapped to Ctrl-Alt-S (AltGr-S) so that it executes together with the 
// built-in listen mode toggle. 
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
    public class IcomSO2VDW : IScriptClass
    {
        readonly byte[] IcomDualWatchOn = { 0x07, 0xC1 };
        readonly byte[] IcomDualWatchOff = { 0x07, 0xC0 };

        public void Initialize(FrmMain main) {}
        public void Deinitialize() { } 

        // Toggle permanent dual watch, execution of Main is mapped to same key as built-in toggle Ctrl-Alt-S = AltGr-S.
        public void Main(FrmMain mainForm, ContestData cdata, COMMain comMain, MidiEvent midiEvent)
        {
            var radio1 = comMain.RadioObject(1);
            var focusedRadio = cdata.FocusedRadio;
            var stereoAudio = mainForm.ListenStatusMode == COMMain.ListenMode.R1R2;
            var modeIsSo2V = cdata.OPTechnique == ContestData.Technique.SO2V;
            var radio1Present = radio1 != null;

            mainForm.SetMainStatusText("Sub receiver " + (stereoAudio ? "not " : "") + "permanently on.");

            if (focusedRadio == 1 && radio1Present && modeIsSo2V)
                if (radio1.IsICOM())
                    radio1.SendCustomCommand(stereoAudio ? IcomDualWatchOff : IcomDualWatchOn);

            // Also execute the normal operation of the key
            mainForm.ScriptContinue = true;
        }
    }
}
