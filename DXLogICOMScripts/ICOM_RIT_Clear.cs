//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM RIT clear and disable, typically mapped to Shift-Left and/or Shift-Delete
// Disables RIT on the currently focused radio.
// By Björn Ekelund SM7IUN bjorn@ekelund.nu 2019-01-31

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomRitClear : ScriptClass
    {
        readonly bool Debug = false;
        readonly byte[] IcomDisableRit = new byte[] { 0x21, 0x01, 0x00 }; // CI-V Command to disable RIT
        readonly byte[] IcomSetRit = new byte[] { 0x21, 0x00, 0x00, 0x00, 0x00}; // CI-V Command to clear RIT

        public void Initialize(FrmMain main) { }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            CATCommon radioObject;
            bool modeIsSO2V = (cdata.OPTechnique == 4);

            if (modeIsSO2V) // if SO2V reset RIT on physical radio 1 regardless if VFO A or B are focused
                radioObject = comMain.RadioObject(1);
            else
                radioObject = comMain.RadioObject(cdata.FocusedRadio);


            if (radioObject == null)
            {
                main.SetMainStatusText(String.Format("IcomRitClear: Radio {0} is not available.", cdata.FocusedRadio));
                return;
            }

            // if S&P, do nothing
            if (cdata.RadioModePrimary == 1) return; 

            radioObject.RitOffSet = 0;

            if (Debug) main.SetMainStatusText(String.Format("IcomRitClear: Commands: [{0}] [{1}]. New RIT offset = {2}",
                BitConverter.ToString(IcomDisableRit), BitConverter.ToString(IcomSetRit), radioObject.RitOffSet));

            radioObject.SendCustomCommand(IcomSetRit);
            radioObject.SendCustomCommand(IcomDisableRit);
        }
    }
}

