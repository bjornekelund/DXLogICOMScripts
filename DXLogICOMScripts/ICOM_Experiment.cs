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