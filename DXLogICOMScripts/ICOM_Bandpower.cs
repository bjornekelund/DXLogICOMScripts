//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM per band output power for IC-785x, IC-7300 and IC-7610
// Sets output power to a safe level at band changes.
// Invoked automatically at band changes made either on radio or in DXLog
// but can also be mapped to a key for restoring level e.g. after manual adjustments
// Only active for ICOM radios.
// Please note that cross-band operation in SO2V with PA requires fast reacting band 
// switching, CI-V based PA band switching may not be sufficiently fast.
// By Björn Ekelund SM7IUN bjorn@ekelund.nu 2019-01-29

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomBandPower : ScriptClass
    {
        ContestData cdata;
        FrmMain frmMain;

        static readonly bool Debug = false;
        static int EventCount = 0; // To work around an anomaly in how band change events are raised at startup

        // Predefined CI-V command strings
        static byte[] IcomSetPower = new byte[] { 0x14, 0x0A, 0x00, 0x00 };

        // Waterfall edges and per mode/band segment ref levels 
        // _scopedge[Radionumber-1, i, modeindex, lower/upper/ref level]
        // converted at initialization to ScopeEdge[Radionumber-1, megaHertz, modeindex, lower/upper/ref level]
        // Mode is CW, Digital, Phone, Band = 0, 1, 2, 3
        static readonly int[,] TxPower = new int[2, 75];
        static readonly int[,,] _txpower = new int[2, 12, 2] {
          // Radio 1 - Percent of full output power
          {{ 1800, 1 }, // 160m
           { 3500, 23 }, // 80m
           { 5351, 1 }, // 60m
           { 7000, 28 }, // 40m
           { 10100, 9 }, // 30m
           { 14000, 23 }, // 20m
           { 18086, 23 }, // 17m
           { 21000, 25 }, // 15m
           { 24890, 23 }, // 12m
           { 28000, 23 }, // 10m
           { 50000, 8 }, // 6m
           { 70000, 8 }}, // 4m
          // Radio 2 - Percent of full output power
          {{ 1800, 1 }, // 160m
           { 3500, 23 }, // 80m
           { 5351, 1 }, // 60m
           { 7000, 28 }, // 40m
           { 10100, 9 }, // 30m
           { 14000, 23 }, // 20m
           { 18086, 23 }, // 17m
           { 21000, 25 }, // 15m
           { 24890, 23 }, // 12m
           { 28000, 23 }, // 10m
           { 50000, 8 }, // 6m
           { 70000, 8 }} // 4m
        };

        public void Initialize(FrmMain main)
        {
            int radioIndex, bandIndex, megaHertz;
            CATCommon radio1 = main.COMMainProvider.RadioObject(1);

            cdata = main.ContestDataProvider;
            frmMain = main;

            if (radio1 != null)                 // Only test if radio is ICOM if radio exist 
                if (radio1.IsICOM())
                    cdata.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);

            // Create look-up table based on actual frequency for simple look-up
            for (radioIndex = 0; radioIndex < 2; radioIndex++)
                for (bandIndex = 0; bandIndex < 12; bandIndex++)
                {
                    megaHertz = (int)(_txpower[radioIndex, bandIndex, 0] / 1000.0);
                    TxPower[radioIndex, megaHertz] = _txpower[radioIndex, bandIndex, 1];
                }

            cdata.ActiveRadioBandChanged += new ContestData.ActiveRadioBandChange(HandleBandChange);
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            HandleBandChange(frmMain.ContestDataProvider.FocusedRadio);
        }

        private void HandleBandChange(int RadioNumber)
        {
            CATCommon usedRadio;
            int megaHertz, usedPower;

            // Special arrangement to avoid confusing the power control logic in SO2V. 
            // During start up DXLog raises one band change event per radio.
            // The second event for radio 2 confuses the logic in SO2V. 
            // Since it is called last, it sets the power level on radio 1 using 
            // the band of VFO B, in spite of VFO A being selected in DXLog. 
            if (cdata.OPTechnique == 4)
            {
                EventCount = (EventCount < 3) ? EventCount + 1 : 3;
                if (EventCount == 2)  
                    return;
            }

            // usedRadio index is radio number - 1 for SO2R, otherwise 0 which represents radio 1
            int usedRadioIndex = ((cdata.OPTechnique == 1) || (cdata.OPTechnique == 2)) ? RadioNumber - 1 : 0;

            usedRadio = frmMain.COMMainProvider.RadioObject(usedRadioIndex + 1);

            if (usedRadio == null) // No CAT capable radio present
            {
                frmMain.SetMainStatusText(string.Format("IcomBandPower: Radio {0} is not available.", usedRadioIndex + 1));
                return;
            }

            if (!usedRadio.IsICOM()) // if radio is not ICOM, do nothing and return
                return;

            if ((RadioNumber == 1) || (RadioNumber == 2)) // If a regular band change 
            {
                if ((cdata.OPTechnique == 4) && (RadioNumber == 2)) // In SO2V, logical radio 2 is physical radio 1 VFO B
                    megaHertz = (int)(usedRadio.VFOBFrequency / 1000.0);
                else
                    megaHertz = (int)(usedRadio.VFOAFrequency / 1000.0);

                usedPower = (int)((255.0f * TxPower[usedRadioIndex, megaHertz]) / 100.0f + 0.99f); // Weird ICOM mapping of percent to binary
                if (usedPower > 255) usedPower = 255;

                IcomSetPower[2] = (byte)((usedPower / 100) % 10);
                IcomSetPower[3] = (byte)((((usedPower / 10) % 10) << 4) + (usedPower % 10));
                usedRadio.SendCustomCommand(IcomSetPower); // Set power

                if (Debug)
                    frmMain.SetMainStatusText(
                        string.Format("IcomBandPower: Band Change: Controlled radio #{0} Focused Radio #{1} megaHertz {2} Power {3}% UsedPower {4} Command: [{5}]",
                        usedRadioIndex + 1, RadioNumber, megaHertz, TxPower[usedRadioIndex, megaHertz],
                        usedPower, BitConverter.ToString(IcomSetPower)));
            }
        }

        private void HandleFocusChange()
        {
            CATCommon usedRadio;
            int focusedRadio = cdata.FocusedRadio;
            int megaHertz, usedPower;

            // Only act if SO2V. No need to verify radio as ICOM since handler is
            // not active otherwise. In SO2V only physical radio 1 is used. 
            // Logical radio 1 is VFO A, logical radio 2 is VFO B
            if (cdata.OPTechnique == 4) 
            {
                usedRadio = frmMain.COMMainProvider.RadioObject(1);

                if (usedRadio == null)
                {
                    frmMain.SetMainStatusText("IcomBandPower: Radio #1 is not available.");
                    return;
                }

                // Only force power level if focus change means band change to leave
                // manual adjustments untouched in same-band operation
                if ((int)(usedRadio.VFOAFrequency / 1000.0) != (int)(usedRadio.VFOBFrequency / 1000.0))
                {
                    if (focusedRadio == 2) // Logical radio 2 is physical radio 1 VFO B
                        megaHertz = (int)(usedRadio.VFOBFrequency / 1000.0);
                    else
                        megaHertz = (int)(usedRadio.VFOAFrequency / 1000.0);

                    usedPower = (int)((255.0f * TxPower[0, megaHertz]) / 100.0f + 0.99f); // Weird ICOM mapping of percent to binary

                    if (usedPower > 255) usedPower = 255;

                    IcomSetPower[2] = (byte)((usedPower / 100) % 10);
                    IcomSetPower[3] = (byte)((((usedPower / 10) % 10) << 4) + (usedPower % 10));
                    usedRadio.SendCustomCommand(IcomSetPower); // Set power

                    if (Debug)
                        frmMain.SetMainStatusText(
                            string.Format("IcomBandPower: Focus change: Phyiscal radio #1 Logical radio #{0} megaHertz {1} Power {2}% UsedPower {3} Command: [{4}]",
                            cdata.FocusedRadio, megaHertz, TxPower[0, megaHertz], usedPower, BitConverter.ToString(IcomSetPower)));
                }
                else
                {
                    if (Debug)
                        frmMain.SetMainStatusText(
                        string.Format("IcomBandPower: Focus change: Phyiscal radio #1 Logical radio #{0} No band change.",
                        cdata.FocusedRadio));

                }
            }
        }
    }
}

