//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM Experiment script for exploration of DXLog's inner workings
// By Björn Ekelund SM7IUN sm7iun@ssa.se 

using System;
using System.Text;
using System.Windows.Forms;
using IOComm;
using CWKeyer;

namespace DXLog.net
{
    public class Experiment : ScriptClass
    {
        ContestData cdata;
        FrmMain mainForm;

        readonly byte[] IcomSplitOff = { 0x0F, 0x00 };
        readonly byte[] IcomSplitOn = { 0x0F, 0x01 };
        static byte[] IcomSetSpeed = new byte[4] { 0x14, 0x0C, 0x00, 0x00 };
        bool toggleSplit;

        public void Initialize(FrmMain main)
        {
            cdata = main.ContestDataProvider;
            mainForm = main;
            toggleSplit = false;
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            CATCommon radio1 = mainForm.COMMainProvider.RadioObject(1);
            int focusedRadio = mainForm.ContestDataProvider.FocusedRadio;
            int ICOMspeed;

            if (radio1 == null)
            {
                mainForm.SetMainStatusText("Experiment: Radio 1 is not available.");
                return;
            }

            //if (toggleSplit)
            //    radio1.SendCustomCommand(IcomSplitOn);
            //else
            //    radio1.SendCustomCommand(IcomSplitOff);

            toggleSplit = !toggleSplit;

            // Update keyer speed
            ICOMspeed = (255 * (mainForm._cwKeyer.CWSpeed(focusedRadio) - 6)) / (48 - 6); // ICOM scales 6-48 WPM onto 0-255
            IcomSetSpeed[2] = (byte)((ICOMspeed / 100) % 10);
            IcomSetSpeed[3] = (byte)((((ICOMspeed / 10) % 10) << 4) + (ICOMspeed % 10));
            radio1.SendCustomCommand(IcomSetSpeed);

            main.SetMainStatusText(String.Format("Experiment: Speed={0} toggleSplit={1}", ICOMspeed, !toggleSplit));

        }
    }
}