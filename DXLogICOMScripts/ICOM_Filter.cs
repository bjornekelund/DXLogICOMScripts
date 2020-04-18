//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM 7610 Filter cycling. Typically mapped to Alt-' for 
// muscle memory compatibility with N1MM. 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-07-08

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomFilter : ScriptClass
    {
        readonly bool Debug = false;

        int currentFilter;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            // Choose and set the "middle" filter at start up
            currentFilter = 2; 
            SetIcomFilter(currentFilter, main);
        }

        // Executes as DXLog.net close down
        public void Deinitialize() { } 

        // Step through filters, Main is mapped to a key, typically not a shifted 
        // key to allow rapid multiple presses
        // The value of currentfilter steps through 1,2,3,1,2,3,1...
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            currentFilter = (currentFilter % 3) + 1;
            SetIcomFilter(currentFilter, main);
        }

        private void SetIcomFilter(int filter, FrmMain main)
        {
            byte mode;
            bool modeIsSO2V = main.ContestDataProvider.OPTechnique == ContestData.Technique.SO2V;
            int focusedRadio = main.ContestDataProvider.FocusedRadio;

            // Physical radio is #1 in SO2V, otherwised the focused radio
            int physicalRadio = modeIsSO2V ? 1 : focusedRadio;
            CATCommon radio = main.COMMainProvider.RadioObject(physicalRadio);

            // Act on currently selected VFO. In SO2V, the selected "radio" defines which VFO
            byte vfo = (byte)(((focusedRadio == 2) && modeIsSO2V) ? 0x01 : 0x00);

            // If there is no radio or if it is not ICOM, do nothing
            if (radio == null || !radio.IsICOM())
                return;

            // The set filter CAT command requires mode information
            // Exits and does nothing if current mode is not supported
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
                default:
                    return;
            }

            // Always disable APF when switching filter
            byte[] IcomDisableAPF = { 0x16, 0x32, 0x00 };
            radio.SendCustomCommand(IcomDisableAPF);

            // Switch filter
            byte[] IcomSetModeFilter = { 0x26, vfo, mode, 0x00, (byte)filter };
            radio.SendCustomCommand(IcomSetModeFilter);

            if (Debug)
                main.SetMainStatusText(String.Format("IcomFilter: VFO {0} changed to FIL{1}. CI-V command: [{2}]. ",
                    (vfo == 0) ? "A" : "B", filter, BitConverter.ToString(IcomSetModeFilter)));
            else
                main.SetMainStatusText(String.Format("VFO {0} changed to FIL{1}.",
                    (vfo == 0) ? "A" : "B", filter));
        }
    }
}
