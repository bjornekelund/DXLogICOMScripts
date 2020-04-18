//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// Yaesu FTDX101D antenna cycling. 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2020-04-18

using System;
using IOComm;

namespace DXLog.net
{
    public class YaesuAntenna : ScriptClass
    {
        int currentAntenna;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            // Choose and set the first antenna at start up
            currentAntenna = 1;
            SetYaesuAntenna(currentAntenna, main);
        }

        // Executes as DXLog.net close down
        public void Deinitialize() { }

        // Step through Antennas, Main is mapped to a key, typically not a shifted 
        // key to allow rapid multiple presses
        // The value of currentAntenna steps through 1,2,3,1,2,3,1...
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            currentAntenna = (currentAntenna % 3) + 1;
            SetYaesuAntenna(currentAntenna, main);
        }

        private void SetYaesuAntenna(int antenna, FrmMain main)
        {
            bool modeIsSO2V = main.ContestDataProvider.OPTechnique == ContestData.Technique.SO2V;
            int focusedRadio = main.ContestDataProvider.FocusedRadio;

            // Physical radio is #1 in SO2V, otherwised the focused radio
            int physicalRadio = modeIsSO2V ? 1 : focusedRadio;

            // Act on currently selected VFO unless SO2V where the selected "radio" defines which VFO
            string avfo = ((focusedRadio == 2) && modeIsSO2V) ? "B" : main.ContestDataProvider.FocusedRadioActiveVFO;
            string vfo = avfo == "A" ? "0" : "1";

            main.COMMainProvider.RadioObject(physicalRadio).SendCustomCommand("AN" + vfo + antenna.ToString() + ";");

            main.SetMainStatusText(string.Format("{0} antenna switched to #{1}.", vfo == "0" ? "Main" : "Sub", currentAntenna));
        }
    }
}
