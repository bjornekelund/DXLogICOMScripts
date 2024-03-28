// ICOM per band output power for IC-785x, IC-7300 and IC-7610
// Sets output power to a safe level at band changes.
// Invoked automatically at band changes made either on radio or in DXLog
// but can also be mapped to a key for restoring level e.g. after manual adjustments
// Only active for ICOM radios.
// Please note that cross-band operation in SO2V with PA requires fast reacting band 
// switching, CI-V based PA band switching may not be sufficiently fast.
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-01-29

using System;
using IOComm;
using NAudio.Midi;

namespace DXLog.net
{
    public class IcomBandPower : IScriptClass
    {
        static readonly bool Debug = false;
        ContestData cdata;
        FrmMain mainForm;

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
            int radioIndex, bandIndex;
            var radio1 = main.COMMainProvider.RadioObject(1);

            cdata = main.ContestDataProvider;
            mainForm = main;

            if (radio1 != null)                 // Only test if radio is ICOM if radio exist 
                if (radio1.IsICOM())
                    cdata.FocusedRadioChanged += new ContestData.FocusedRadioChange(HandleFocusChange);

            // Create look-up table based on actual frequency for simple look-up
            for (radioIndex = 0; radioIndex < 2; radioIndex++)
                for (bandIndex = 0; bandIndex < 12; bandIndex++)
                {
                    var megaHertz = (int)(_txpower[radioIndex, bandIndex, 0] / 1000.0);
                    TxPower[radioIndex, megaHertz] = _txpower[radioIndex, bandIndex, 1];
                }

            cdata.ActiveRadioBandChanged += new ContestData.ActiveRadioBandChange(HandleBandChange);
        }

        public void Deinitialize() { }

        public void Main(FrmMain mainForm, ContestData cdata, COMMain comMain, MidiEvent midiEvent)
        {
            HandleBandChange(mainForm.ContestDataProvider.FocusedRadio);
        }

        private void HandleBandChange(int radioNumber)
        {
            // Special arrangement to avoid confusing the power control logic in SO2V. 
            // During start up DXLog raises one band change event per radio.
            // The second event for radio 2 confuses the logic in SO2V. 
            // Since it is called last, it sets the power level on radio 1 using 
            // the band of VFO B, in spite of VFO A being selected in DXLog. 
            if (cdata.OPTechnique == ContestData.Technique.SO2V)
            {
                EventCount = (EventCount < 3) ? EventCount + 1 : 3;
                if (EventCount == 2)  
                    return;
            }

            // usedRadio index is radio number - 1 for SO2R, otherwise 0 which represents radio 1
            var usedRadioIndex = ((cdata.OPTechnique == ContestData.Technique.SO2R) || (cdata.OPTechnique == ContestData.Technique.SO2R_ADV)) ? radioNumber - 1 : 0;

            var usedRadio = mainForm.COMMainProvider.RadioObject(usedRadioIndex + 1);

            if (usedRadio == null) // No CAT capable radio present
            {
                mainForm.SetMainStatusText($"IcomBandPower: Radio {usedRadioIndex + 1} is not available.");
                return;
            }

            if (!usedRadio.IsICOM()) // if radio is not ICOM, do nothing and return
                return;

            if ((radioNumber == 1) || (radioNumber == 2)) // If a regular band change 
            {
                int megaHertz;
                if ((cdata.OPTechnique == ContestData.Technique.SO2V) && (radioNumber == 2)) // In SO2V, logical radio 2 is physical radio 1 VFO B
                    megaHertz = (int)(usedRadio.VFOBFrequency / 1000.0);
                else
                    megaHertz = (int)(usedRadio.VFOAFrequency / 1000.0);

                var usedPower = (int)((255.0f * TxPower[usedRadioIndex, megaHertz]) / 100.0f + 0.99f);
                if (usedPower > 255) usedPower = 255;

                IcomSetPower[2] = (byte)((usedPower / 100) % 10);
                IcomSetPower[3] = (byte)((((usedPower / 10) % 10) << 4) + (usedPower % 10));
                usedRadio.SendCustomCommand(IcomSetPower); // Set power

                if (Debug)
                    mainForm.SetMainStatusText($"IcomBandPower: Band Change: Controlled radio #{usedRadioIndex + 1} Focused Radio #{radioNumber} megaHertz {megaHertz} Power {TxPower[usedRadioIndex, megaHertz]}% UsedPower {usedPower} Command: [{BitConverter.ToString(IcomSetPower)}]");
            }
        }

        private void HandleFocusChange()
        {
            var focusedRadio = cdata.FocusedRadio;

            // Only act if SO2V. No need to verify radio as ICOM since handler is
            // not active otherwise. In SO2V only physical radio 1 is used. 
            // Logical radio 1 is VFO A, logical radio 2 is VFO B
            if (cdata.OPTechnique == ContestData.Technique.SO2V) 
            {
                var usedRadio = mainForm.COMMainProvider.RadioObject(1);

                if (usedRadio == null)
                {
                    mainForm.SetMainStatusText("IcomBandPower: Radio #1 is not available.");
                    return;
                }

                // Only force power level if focus change means band change to leave
                // manual adjustments untouched in same-band operation
                if ((int)(usedRadio.VFOAFrequency / 1000.0) != (int)(usedRadio.VFOBFrequency / 1000.0))
                {
                    int megaHertz;
                    if (focusedRadio == 2) // Logical radio 2 is physical radio 1 VFO B
                        megaHertz = (int)(usedRadio.VFOBFrequency / 1000.0);
                    else
                        megaHertz = (int)(usedRadio.VFOAFrequency / 1000.0);

                    var usedPower = (int)((255.0f * TxPower[0, megaHertz]) / 100.0f + 0.99f);

                    if (usedPower > 255) usedPower = 255;

                    IcomSetPower[2] = (byte)((usedPower / 100) % 10);
                    IcomSetPower[3] = (byte)((((usedPower / 10) % 10) << 4) + (usedPower % 10));
                    usedRadio.SendCustomCommand(IcomSetPower); // Set power

                    if (Debug)
                        mainForm.SetMainStatusText($"IcomBandPower: Focus change: Physical radio #1 Logical radio #{cdata.FocusedRadio} megaHertz {megaHertz} Power {TxPower[0, megaHertz]}% UsedPower {usedPower} Command: [{BitConverter.ToString(IcomSetPower)}]");
                }
                else
                {
                    if (Debug)
                        mainForm.SetMainStatusText($"IcomBandPower: Focus change: Physical radio #1 Logical radio #{cdata.FocusedRadio} No band change.");

                }
            }
        }

    }
}

