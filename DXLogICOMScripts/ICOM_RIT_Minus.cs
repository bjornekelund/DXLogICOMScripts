//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll
//INCLUDE_ASSEMBLY IOComm.dll

// ICOM RIT/Frequency change typically mapped to Shift-Up/Down
// For N1MM style operation, map Shift-Up to frequency down and vice versa.
// Tested on IC-7610. Intended for all operating techniques
// Radio 1 focused with RUN in SO1R, SO2R, or SO2V = RIT on radio 1
// Radio 1 focused with S&P in SO1R, SO2R, or SO2V = adjust VFO A on radio 1 
// Radio 2 focused with RUN in SO2R = RIT on radio 2
// Radio 2 focused with S&P in SO2R = adjust VFO A on radio 2 
// Radio 2 focused with RUN in SO2V = No action
// Radio 2 focused with S&P in SO2V = adjust VFO B on radio 1 

// By Björn Ekelund SM7IUN bjorn@ekelund.nu 2019-01-31

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomRitMinus : ScriptClass
    {
        readonly bool Debug = false;
        readonly int FrequencyStep = -20; // Hertz 
        readonly byte[] IcomEnableRit = new byte[] { 0x21, 0x01, 0x01 }; // CI-V Command to enable RIT
        byte[] IcomSetRit = new byte[] { 0x21, 0x00, 0x00, 0x00, 0x00 }; // CI-V Command to set RIT value

        public void Initialize(FrmMain main) { }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            CATCommon radio1 = comMain._radio1Object; // All commands are sent to radio 1 for SO2V and SO1R
            CATCommon radio2 = comMain._radio2Object; // Only used for SO2R 
            int focusedRadio = cdata.FocusedRadio;
            bool modeIsSO1R = (cdata.OPTechnique == 0);
            bool modeIsSO2R = ((cdata.OPTechnique == 1) || (cdata.OPTechnique == 2));
            bool modeIsSO2V = (cdata.OPTechnique == 4);
            bool radio1modeIsRun = (cdata.RadioModePrimary == 0);
            bool radio2modeIsRun = (cdata.RadioModeSecondary == 0);
            int currentRit;
            int currentAbsRit;

            if (radio1 == null)
            {
                main.SetMainStatusText("IcomRitMinus: Radio 1 is not available.");
                return;
            }

            if ((radio2 == null) && modeIsSO2R)
            {
                main.SetMainStatusText("IcomRitMinus: Radio 2 is not available.");
                return;
            }

            if (focusedRadio == 1)
            {
                if (radio1modeIsRun)
                { // If radio 1 is focused and RUN, do RIT regardless of operating technique
                    currentRit = radio1.RitOffSet += FrequencyStep;
                    currentAbsRit = Math.Abs(currentRit);

                    IcomSetRit[2] = (byte)((((currentAbsRit / 10) % 10) << 4) + (currentAbsRit % 10));
                    IcomSetRit[3] = (byte)((((currentAbsRit / 1000) % 10) << 4) + ((currentAbsRit / 100) % 10));
                    IcomSetRit[4] = (currentRit < 0) ? (byte)1 : (byte)0;

                    radio1.SendCustomCommand(IcomEnableRit);
                    radio1.SendCustomCommand(IcomSetRit);

                    if (Debug) main.SetMainStatusText(String.Format("IcomRitMinus: Radio 1: RIT offset = {0}", radio1.RitOffSet));
                }
                else
                { // If radio 1 is focused and S&P, do frequency adjustment regardless of op technique
                    radio1.SetVFOAFrequency(cdata.Radio1_FreqA + FrequencyStep / 1000.0);
                    if (Debug) main.SetMainStatusText(String.Format("IcomRitMinus: Radio 1: VFO A: {0:F2}", cdata.Radio1_FreqA));
                }
            }
            else // radio 2 is focused
            {
                if (radio2modeIsRun && modeIsSO2R)
                { /// If radio 2 is focused and SO2R and RUN, do RIT on radio 2 
                    currentRit = radio2.RitOffSet += FrequencyStep;
                    currentAbsRit = Math.Abs(currentRit);

                    IcomSetRit[2] = (byte)((((currentAbsRit / 10) % 10) << 4) + (currentAbsRit % 10));
                    IcomSetRit[3] = (byte)((((currentAbsRit / 1000) % 10) << 4) + ((currentAbsRit / 100) % 10));
                    IcomSetRit[4] = (currentRit < 0) ? (byte)1 : (byte)0;

                    radio2.SendCustomCommand(IcomEnableRit);
                    radio2.SendCustomCommand(IcomSetRit);

                    if (Debug) main.SetMainStatusText(String.Format("IcomRitMinus: Radio2: RIT offset = {0}", radio2.RitOffSet));
                }
                else if (!radio2modeIsRun)
                { // if radio 2 is focused and S&P
                    if (modeIsSO2R)
                    { // if radio 2 is focused and S&P and technique is SO2R, adjust VFO A on radio 2
                        radio2.SetVFOAFrequency(cdata.Radio2_FreqA + FrequencyStep / 1000.0);
                        if (Debug) main.SetMainStatusText(String.Format("IcomRitMinus: Radio 2: VFO A: {0:F2}", cdata.Radio2_FreqA));
                    }
                    else
                    { // if radio 2 is focused and S&P and technique is SO1R or SO2V, adjust VFO B on radio 1
                        radio1.SetVFOBFrequency(cdata.Radio1_FreqB + FrequencyStep / 1000.0);
                        if (Debug) main.SetMainStatusText(String.Format("IcomRitMinus: Radio 1: VFO B: {0:F2}", cdata.Radio1_FreqB));
                    }
                }
            }
        }
    }
}
