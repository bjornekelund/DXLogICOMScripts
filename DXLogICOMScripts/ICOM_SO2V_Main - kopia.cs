//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM SO2V VFO focus and audio management 
// Companion to ICOM_SO2V_Main.cs which disables dual watch temporarily, typically mapped to top leftmost key on keyboard
// By Björn Ekelund SM7IUN bjorn@ekelund.nu 2018-12-11

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomSo2vMain : ScriptClass
    {
        readonly bool Debug = false;
        readonly byte[] IcomDualWatchOff = { 0x07, 0xC0 };

        public void Initialize(FrmMain main) { }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            CATCommon radio1 = comMain.RadioObject(focusedRadio);
            bool radio1Present = (radio1 != null);

            if ((focusedRadio == 1) && cdata.OPTechnique == 4) // Only active in SO2V
            {
                if (radio1Present) radio1.SendCustomCommand(IcomDualWatchOff);
                if (Debug) main.SetMainStatusText(String.Format("IcomSo2vMain: Focus is Radio #1, Main Receiver"));
            }
        }
    }
}