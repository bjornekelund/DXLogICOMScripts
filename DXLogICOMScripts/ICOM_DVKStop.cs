//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// Trigger internal DVK on modern ICOM radios
// Typically used in macro sequences using the $!SCRIPTNAME syntax
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-05-03

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomRunDVKStop : ScriptClass
    {
        readonly bool Debug = false;
        const byte DVK = 0;
        readonly byte[] IcomDVKCommand = new byte[] { 0x28, 0x00, DVK }; // CI-V Command to trigger DVK

        public void Initialize(FrmMain main) { }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            CATCommon radioObject;
            bool modeIsSO2V = (cdata.OPTechnique == 4);

            if (modeIsSO2V) // if SO2V send command to radio 1 regardless if VFO A or B are focused
                radioObject = comMain.RadioObject(1);
            else
                radioObject = comMain.RadioObject(cdata.FocusedRadio);

            if (radioObject == null)
            {
                main.SetMainStatusText(String.Format("IcomRunDVK: Radio {0} is not available.",
                    cdata.FocusedRadio));
                return;
            }

            if (Debug) main.SetMainStatusText(String.Format("IcomRunDVK: {0}",
                BitConverter.ToString(IcomDVKCommand)));

            radioObject.SendCustomCommand(IcomDVKCommand);
        }
    }
}

