//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM waterfall/spectrum display management for IC-785x, IC-7300 and IC-7610.
// Sets waterfall/spectrum display range based on band and operating mode.
// Invoked automatically at band changes made either on radio or in DXLog.net
// but also via a mapped key for restoring range and ref level e.g. after 
// manual adjustments.
// Does not support dual spectrum display and only considers VFO A.
// Works for all supported operating "techniques" SO1R, SO2R and SO2V.
// Only active for ICOM radio but does not verify that radio is Waterfall capable.
// Waterfall capability is defined in readonly variable WaterfallCapable[].
// Separate frequency and ref levels for radio 1 and radio 2.
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2019-01-30

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomWaterfallMode : ScriptClass
    {
        static readonly bool debug = false;
        ContestData cdata;
        FrmMain mainForm;

        // Defines which of the radio's three edge sets is manipulated by script 
        static readonly byte UsedEdgeSet = 3;

        // Waterfall capability of radio 1 and radio 2
        static readonly bool[] WaterfallCapable = new bool[] {
            true, // Radio 1
            true  // Radio 2
        };

        // Predefined CI-V command strings
        static readonly byte[] IcomSetFixedMode = new byte[] { 0x27, 0x14, 0x0, 0x1 };
        static readonly byte[] IcomSetEdgeSet = new byte[] { 0x27, 0x16, 0x0, UsedEdgeSet };
        static byte[] IcomSetEdges = new byte[] { 0x27, 0x1E, 0x00, UsedEdgeSet, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static byte[] IcomSetRefLevel = new byte[6] { 0x27, 0x19, 0x00, 0x00, 0x00, 0x00 };

        // Waterfall edges and per mode/band segment ref levels 
        // _scopedge[Radionumber-1, i, modeindex, lower/upper/ref level]
        // converted at initialization to ScopeEdge[Radionumber-1, megaHertz, modeindex, lower/upper/ref level]
        // Mode is CW, Digital, Phone, Band = 0, 1, 2, 3
        static readonly int[,,,] ScopeEdge = new int[2, 75, 4, 3];
        static readonly int[,,,] _scopeedge = new int[2, 11, 4, 3] {
            // Radio 1
           {{{ 1810,  1840,  -6}, { 1840,  1860,  -6}, { 1840,  2000,  -6}, { 1800,  2000, -10}},
            {{ 3500,  3570, -14}, { 3570,  3600, -11}, { 3600,  3800, -17}, { 3500,  3800, -18}},
            {{ 5352,  5366,  -5}, { 5352,  5366,  -5}, { 5352,  5366,  -5}, { 5352,  5366,  -5}},
            {{ 7000,  7040,  -6}, { 7040,  7080,  -6}, { 7040,  7200, -14}, { 7000,  7200, -15}},
            {{10100, 10130,   4}, {10130, 10150,   4}, {10100, 10150,   4}, {10100, 10150,   4}},
            {{14000, 14070,  -2}, {14070, 14100,  -1}, {14100, 14350,  -4}, {14000, 14350,  -4}},
            {{18068, 18109,  -2}, {18089, 18109,  -2}, {18111, 18168,  -6}, {18068, 18168,  -9}},
            {{21000, 21070,  -3}, {21070, 21150,  -5}, {21150, 21450, -12}, {21000, 21450, -16}},
            {{24890, 24920,  -1}, {24910, 24932,  -1}, {24931, 24990,  -4}, {24890, 24990,  -7}},
            {{28000, 28070,  -4}, {28070, 28110,   0}, {28300, 28600,  -9}, {28000, 29000,   1}},
            {{50000, 50100,  -4}, {50300, 50350,   0}, {50100, 50500, -11}, {50000, 50500, -15}}},
           // Radio 2
           {{{ 1810, 1840,   -6}, { 1840,  1860,  -6}, { 1840,  2000,  -6}, { 1800,  2000, -10}},
            {{ 3500, 3570,  -14}, { 3570,  3600, -11}, { 3600,  3800, -17}, { 3500,  3800, -18}},
            {{ 5352, 5366,    0}, { 5352,  5366,   0}, { 5352,  5366,   0}, { 5352,  5366,   0}},
            {{ 7000, 7040,    0}, { 7040,  7080,   0}, { 7040,  7200,  -6}, { 7000,  7200,  -8}},
            {{10100, 10130,  10}, {10130, 10150,   6}, {10100, 10150,   4}, {10100, 10150,   4}},
            {{14000, 14070,   0}, {14070, 14100,  -1}, {14100, 14350,  -4}, {14000, 14350,  -6}},
            {{18068, 18109,  -2}, {18089, 18109,  -2}, {18111, 18168,  -6}, {18068, 18168,  -9}},
            {{21000, 21070,   0}, {21070, 21150,  -5}, {21150, 21450,  -6}, {21000, 21450,  -8}},
            {{24890, 24920,   5}, {24910, 24932,   3}, {24931, 24990,   3}, {24890, 24990,   0}},
            {{28000, 28070,  -2}, {28070, 28110,   0}, {28300, 28600,  -9}, {28000, 29000,   1}},
            {{50000, 50100,   0}, {50300, 50350,   0}, {50100, 50500, -11}, {50000, 50500, -15}}}
        };

        // Maps actual MHz to radio's scope edge set on ICOM 7800, 785x, 7300 and 7610
        static readonly int[] RadioEdgeSet = new int[]
            { 1, 2, 3, 3, 3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8,
                9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11,
                11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12 };

        public void Initialize(FrmMain main)
        {
            int radioIndex, bandIndex, modeIndex, i, megaHertz;

            cdata = main.ContestDataProvider;
            mainForm = main;

            // Create look-up table based on actual frequency for simple look-up
            for (radioIndex = 0; radioIndex < 2; radioIndex++)
                for (bandIndex = 0; bandIndex < 11; bandIndex++)
                {
                    megaHertz = (int)(_scopeedge[radioIndex, bandIndex, 0, 0] / 1000.0);
                    for (modeIndex = 0; modeIndex < 4; modeIndex++)
                        for (i = 0; i < 3; i++)
                            ScopeEdge[radioIndex, megaHertz, modeIndex, i] = _scopeedge[radioIndex, bandIndex, modeIndex, i];
                }
                            
            cdata.ActiveRadioBandChanged += new ContestData.ActiveRadioBandChange(HandleActiveRadioBandChanged);
        }

        public void Deinitialize() {}

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            HandleActiveRadioBandChanged(1);
        }

        public void HandleActiveRadioBandChanged(int RadioNumber)
        {
            CATCommon usedRadio;
            int modeIndex, lowerEdge, upperEdge, refLevel, absRefLevel, megaHertz; 

            // usedRadio index is radio number - 1 for SO2R, otherwise 0 which represents radio 1
            int usedRadioIndex = ((cdata.OPTechnique == ContestData.Technique.SO2R) || (cdata.OPTechnique == ContestData.Technique.SO2R_ADV)) ? RadioNumber - 1 : 0;

            if ((RadioNumber != 1) && (RadioNumber != 2)) // If just a focus change, do nothing and return
                return;

            // If changing band on VFO B in SO2V, do nothing and return
            if ((cdata.OPTechnique == ContestData.Technique.SO2V) && (RadioNumber == 2)) 
            return;

            // If selected radio is not waterfall capable, do nothing and return
            if (!WaterfallCapable[usedRadioIndex]) 
            return;

            usedRadio = mainForm.COMMainProvider.RadioObject(usedRadioIndex + 1); 

            if (usedRadio == null) // No CAT capable radio present
            {
                mainForm.SetMainStatusText(string.Format("IcomWaterfallMode: Radio {0} is not available.", 
                    usedRadioIndex + 1));
                return;
            }

            switch (usedRadio.VFOAMode)
            {
                case "CW": modeIndex = 0; break;
                case "RTTY": modeIndex = 1; break;
                case "USB":
                case "LSB":
                case "AM": modeIndex = 2; break;
                default: modeIndex = 3; break;
            }

            megaHertz = (int)(usedRadio.VFOAFrequency / 1000.0);
            lowerEdge = ScopeEdge[usedRadioIndex, megaHertz, modeIndex, 0];
            upperEdge = ScopeEdge[usedRadioIndex, megaHertz, modeIndex, 1];
            refLevel = ScopeEdge[usedRadioIndex, megaHertz, modeIndex, 2];
            absRefLevel = (refLevel >= 0) ? refLevel : -refLevel;

            IcomSetEdges[2] = (byte)(((RadioEdgeSet[megaHertz] / 10) << 4) + (RadioEdgeSet[megaHertz] % 10));
            IcomSetEdges[5] = (byte)(((lowerEdge % 10) << 4)); // 1kHz & 100Hz
            IcomSetEdges[6] = (byte)((((lowerEdge / 100) % 10) << 4) + ((lowerEdge / 10) % 10)); // 100kHz & 10kHz
            IcomSetEdges[7] = (byte)((((lowerEdge / 10000) % 10) << 4) + (lowerEdge / 1000) % 10); // 10MHz & 1MHz
            IcomSetEdges[10] = (byte)(((upperEdge % 10) << 4)); // 1kHz & 100Hz
            IcomSetEdges[11] = (byte)((((upperEdge / 100) % 10) << 4) + (upperEdge / 10) % 10); // 100kHz & 10kHz
            IcomSetEdges[12] = (byte)((((upperEdge / 10000) % 10) << 4) + (upperEdge / 1000) % 10); // 10MHz & 1MHz

            IcomSetRefLevel[3] = (byte)((absRefLevel / 10) * 16 + absRefLevel % 10);
            IcomSetRefLevel[5] = (refLevel >= 0) ? (byte)0 : (byte)1;

            if (debug)
                mainForm.SetMainStatusText(
                    string.Format("IcomWaterfallMode: Radio # {0} Modestring '{1}' Edge {2} Low {3} High {4} Ref {5} Commands: [{6}] [{7}]", 
                    usedRadioIndex + 1, usedRadio.VFOAMode, UsedEdgeSet, lowerEdge, upperEdge, refLevel, BitConverter.ToString(IcomSetEdges), 
                    BitConverter.ToString(IcomSetRefLevel)));

            usedRadio.SendCustomCommand(IcomSetFixedMode); // Set fixed mode
            usedRadio.SendCustomCommand(IcomSetEdgeSet); // Select edge set EdgeSet
            usedRadio.SendCustomCommand(IcomSetEdges); // set up waterfall edges
            usedRadio.SendCustomCommand(IcomSetRefLevel); // set scope ref level
        }
    }
}

