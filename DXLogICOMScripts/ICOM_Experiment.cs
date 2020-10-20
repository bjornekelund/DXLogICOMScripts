//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM Experiment script for exploration of DXLog's inner workings
// By Björn Ekelund SM7IUN sm7iun@ssa.se 

using System;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using IOComm;
using CWKeyer;
using DXLog.net;

namespace DXLog.net
{
    public class Experiment : ScriptClass
    {
        //ContestData cdata;
        FrmMain mainForm;


        public void Initialize(FrmMain main)
        {
            //cdata = main.ContestDataProvider;
            mainForm = main;

            mainForm.KeyDown += new KeyEventHandler(HandleKeyPress);
            mainForm.KeyUp += new KeyEventHandler(HandleKeyRelease);
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            //byte[] msg1 = new byte[] { 0x17, (byte)'A', (byte)'B', (byte)' ', (byte)'A', (byte)' ', (byte)'B' };
            //byte[] msg2 = new byte[] { 0x17, (byte)'A', (byte)'B', (byte)' ', (byte)'A', (byte)' ', (byte)'B' };
            byte[] msg1 = new byte[] { 0x17, (byte)'A', (byte)'B' };
            byte[] msg2 = new byte[] { 0x17, (byte)'C', (byte)'D' };

            CATCommon radio1 = mainForm.COMMainProvider.RadioObject(1);
            radio1.SendCustomCommand(msg1);
            radio1.SendCustomCommand(msg2);

            //main.SetMainStatusText(String.Format("Experiment: Speed={0} toggleSplit={1}", ICOMspeed, !toggleSplit));
            //main.SetMainStatusText(String.Format("Experiment: Callsign = {0}", mainForm.CurrentEntryLine.ActualQSO.Callsign));
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.RControlKey)
                mainForm.SetMainStatusText(String.Format("Experiment: Key {0} pressed", e.KeyCode));
        }

        private void HandleKeyRelease(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.RControlKey)
                mainForm.SetMainStatusText(String.Format("Experiment: Key {0} released", e.KeyCode));
        }
    }
}