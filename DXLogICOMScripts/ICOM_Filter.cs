//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM 7610 Filter cycling. Typically mapped to Alt-' for 
// muscle memory compatibility with N1MM. 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-05-15

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomFilter : ScriptClass
    {
        readonly bool Debug = false;
        ContestData cdata;
        FrmMain mainForm;

        int currentFilter;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            cdata = main.ContestDataProvider;
            mainForm = main;

            currentFilter = 2; // "Middle" filter

            SetIcomFilter(currentFilter);
        }

        public void Deinitialize() { } // Do nothing at DXLog.net close down

        // Step through filters, Main is mapped to a key, typically not a shifted 
        // key to allow rapid multiple presses
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            currentFilter = (currentFilter % 3) + 1;

            SetIcomFilter(currentFilter);
        }

        private void SetIcomFilter(int filter)
        {
            byte[] IcomSetModeFilter = { 0x26, 0x00, 0x00, 0x00, 0x00 };
            byte[] IcomDisableAPF = { 0x16, 0x32, 0x00 };

            bool modeIsSO2V = cdata.OPTechnique == ContestData.Technique.SO2V;
            int focusedRadio = cdata.FocusedRadio;
            int physicalRadio = modeIsSO2V ? 1 : focusedRadio;
            CATCommon radio = mainForm.COMMainProvider.RadioObject(physicalRadio);
            int vfo, mode = 0;

            if ((radio == null) || (!radio.IsICOM()))
                return;

            vfo = ((focusedRadio == 2) && modeIsSO2V) ? 0x01 : 0x00;

            // Only works for modes listed below 
            switch ((vfo == 0) ? radio.VFOAMode : radio.VFOBMode)
            {
                case "LSB":
                    mode = 0x00;
                    break;
                case "USB":
                    mode = 0x01;
                    break;
                case "AM":
                    mode = 0x02;
                    break;
                case "CW":
                    mode = 0x03;
                    break;
                case "RTTY":
                    mode = 0x04;
                    break;
                case "FM":
                    mode = 0x05;
                    break;
            }

            IcomSetModeFilter[1] = (byte)vfo;
            IcomSetModeFilter[2] = (byte)mode;
            IcomSetModeFilter[4] = (byte)filter;

            radio.SendCustomCommand(IcomDisableAPF);
            radio.SendCustomCommand(IcomSetModeFilter);

            if (Debug)
                mainForm.SetMainStatusText(String.Format("IcomFilter: VFO {0} changed to FIL{1}. Command: [{2}]. ",
                    (vfo == 0) ? "A" : "B", filter, BitConverter.ToString(IcomSetModeFilter)));
            else
                mainForm.SetMainStatusText(String.Format("VFO {0} changed to FIL{1}.",
                    (vfo == 0) ? "A" : "B", filter));
        }
    }
}
