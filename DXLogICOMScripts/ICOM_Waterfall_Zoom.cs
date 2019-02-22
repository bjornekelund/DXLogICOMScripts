//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM waterfall/spectrum display management.
// Mapped to suitable key for creating a +/- WaterfallWidth fixed spectrum display  
// Does not support dual spectrum display and thus only considers VFO A.
// Works for all supported operating "techniques" SO1R, SO2R and SO2V.
// Only active for ICOM radio but does not verify radio is Waterfall capable.
// Waterfall capability is defined in readonly variable WaterfallCapable[].
// By Björn Ekelund SM7IUN bjorn@ekelund.nu 2019-01-30

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomWaterfallZoom : ScriptClass
    {
        ContestData cdata;
        FrmMain frmMain;

        // Defines which of the radio's three edge sets is manipulated by script 
        static readonly byte UsedEdgeSet = 3; // which scope edge should be manipulated

        // Predefined ICOM CI-V commands
        static readonly byte[] IcomSetFixedMode = new byte[] { 0x27, 0x14, 0x0, 0x1 };
        static readonly byte[] IcomSetEdgeSet = new byte[] { 0x27, 0x16, 0x0, UsedEdgeSet };
        static byte[] IcomSetEdges = new byte[] { 0x27, 0x1E, 0x00, UsedEdgeSet, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static byte[] IcomSetRefLevel = new byte[6] { 0x27, 0x19, 0x00, 0x00, 0x00, 0x00 };
        static readonly double WaterfallWidth = 20.0; // Width of zoomed waterfall

        static readonly bool debug = false;

        // Waterfall capability of radio 1 and radio 2, only meaningful in SO2R
        static readonly bool[] WaterfallCapable = new bool[] {
            true, // Radio 1
            true  // Radio 2
        };

        // Extend to 75 to avoid crash with 4m capable radios
        static readonly int[,] RefLevel = new int[2,75]; 

        // Maps actual MHz to radio's scope edge set on ICOM 7800, 785x, 7300 and 7610
        static readonly int[] RadioEdgeSet = new int[] { 1, 2, 3, 3, 3, 3, 4, 4, 5, 5, 5, 6,
            6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11,
            11, 11, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12 };

        public void Initialize(FrmMain main)
        {
            RefLevel[0, 1] = -3; //160m Radio 1
            RefLevel[0, 3] = -11; // 80m Radio 1
            RefLevel[0, 5] = 1; // 60m Radio 1
            RefLevel[0, 7] = -6; // 40m Radio 1
            RefLevel[0, 10] = 2; // 30m Radio 1
            RefLevel[0, 14] = 2; // 20m Radio 1
            RefLevel[0, 18] = -2; // 17m Radio 1
            RefLevel[0, 21] = -5; // 15m Radio 1
            RefLevel[0, 24] = 0; // 12m Radio 1
            RefLevel[0, 28] = 0; // 10m Radio 1
            RefLevel[0, 29] = 0; // 10m Radio 1
            RefLevel[0, 50] = 2; //  6m Radio 1

            RefLevel[1, 1] = -3; //160m Radio 2
            RefLevel[1, 3] = -11; // 80m Radio 2
            RefLevel[1, 5] = 1; // 60m Radio 2
            RefLevel[1, 7] = -6; // 40m Radio 2
            RefLevel[1, 10] = 2; // 30m Radio 2
            RefLevel[1, 14] = 2; // 20m Radio 2
            RefLevel[1, 18] = -2; // 17m Radio 2
            RefLevel[1, 21] = -5; // 15m Radio 2
            RefLevel[1, 24] = 0; // 12m Radio 2
            RefLevel[1, 28] = 0; // 10m Radio 2
            RefLevel[1, 29] = 0; // 10m Radio 2
            RefLevel[1, 50] = 2; //  6m Radio 2
        }

        public void Deinitialize() {}

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            CATCommon usedRadio;
            int lowerEdge, upperEdge, refLevel, absRefLevel, megaHertz;
            int RadioNumber = cdata.FocusedRadio;

            // Radio index is radio number - 1 for SO2R, otherwise 0 which represents radio 1
            int usedRadioIndex = ((cdata.OPTechnique == 1) || (cdata.OPTechnique == 2)) ? RadioNumber - 1 : 0;

            // If on VFO B in SO2V, do nothing and return
            if ((cdata.OPTechnique == 4) && (RadioNumber == 2)) 
            return;

            // If not waterfall capable, do nothing and return
            if (!WaterfallCapable[usedRadioIndex]) 
            return;

            // only do when we know we have a valid radio index
            usedRadio = main.COMMainProvider.RadioObject(usedRadioIndex + 1);

            // If no CAT capable radio present
            if (usedRadio == null) 
            {
                main.SetMainStatusText(string.Format("IcomWaterfallZoom: Radio {0} is not available.", usedRadioIndex + 1));
                return;
            }

            megaHertz = (int)(usedRadio.VFOAFrequency / 1000.0);

            lowerEdge = (int)(usedRadio.VFOAFrequency + 0.5 - WaterfallWidth / 2.0);
            upperEdge = (int)(lowerEdge + WaterfallWidth);

            refLevel = RefLevel[usedRadioIndex, megaHertz];
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
                main.SetMainStatusText(
                    string.Format("IcomWaterfallZoom: Radio # {0} Edge {1} Low {2} High {3} Ref {4} Commands: [{5}] [{6}]",
                    usedRadioIndex + 1, UsedEdgeSet, lowerEdge, upperEdge, refLevel, BitConverter.ToString(IcomSetEdges),
                    BitConverter.ToString(IcomSetRefLevel)));

            usedRadio.SendCustomCommand(IcomSetFixedMode); // Set fixed mode
            usedRadio.SendCustomCommand(IcomSetEdgeSet); // Select edge set EdgeSet
            usedRadio.SendCustomCommand(IcomSetEdges); // set up waterfall edges
            usedRadio.SendCustomCommand(IcomSetRefLevel); // set scope ref level
        }
    }
}

