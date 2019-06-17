//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V helper script to toggle permanent dual watch on and off. 
// Mapped to Ctrl-Alt-S (AltGr-S) so that it executes together with the 
// built-in listen mode toggle. 
// Only active for ICOM radio but does not verify radio is SO2V capable
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-04-16

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomSO2VDW : ScriptClass
    {
        readonly bool Debug = false;
        ContestData cdata;
        FrmMain mainForm;

        readonly byte[] IcomDualWatchOn = { 0x07, 0xC1 };
        readonly byte[] IcomDualWatchOff = { 0x07, 0xC0 };
        readonly string statusMessage = "Focus on {0} VFO. {1}.";

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            cdata = main.ContestDataProvider;
            mainForm = main;
        }

        public void Deinitialize() { } 

        // Toggle permanent dual watch, execution of Main is mapped to same key as built-in toggle Ctrl-Alt-S = AltGr-S.
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            CATCommon radio1 = comMain.RadioObject(1);
            int focusedRadio = cdata.FocusedRadio;
            // ListenStatusMode: 0=Radio 1, 1=Radio 2 toggle, 2=Radio 2, 3=Both
            bool stereoAudio = (mainForm.ListenStatusMode != 0);
            bool modeIsSo2V = (cdata.OPTechnique == ContestData.Technique.SO2V);
            bool radio1Present = (radio1 != null);


            if (focusedRadio == 1)
            {
                mainForm.SetMainStatusText(String.Format(statusMessage, focusedRadio == 1 ? "Main" : "Sub", stereoAudio ? "Main Receiver" : "Dual Watch"));
                if (radio1Present && modeIsSo2V)
                    if (radio1.IsICOM())
                        radio1.SendCustomCommand(stereoAudio ? IcomDualWatchOff : IcomDualWatchOn);
                //mainForm.ContinueScript = true;
            }

        }
    }
}
