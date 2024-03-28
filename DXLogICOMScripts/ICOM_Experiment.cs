// ICOM Experiment script for exploration of DXLog's inner workings
// By Björn Ekelund SM7IUN sm7iun@ssa.se 

using System.Windows.Forms;
using IOComm;
using NAudio.Midi;

namespace DXLog.net
{
    public class Experiment : IScriptClass
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

        public void Main(FrmMain mainForm, ContestData cdata, COMMain comMain, MidiEvent midiEvent)
        {
            //byte[] msg1 = new byte[] { 0x17, (byte)'A', (byte)'B', (byte)' ', (byte)'A', (byte)' ', (byte)'B' };
            //byte[] msg2 = new byte[] { 0x17, (byte)'A', (byte)'B', (byte)' ', (byte)'A', (byte)' ', (byte)'B' };
            var msg1 = new byte[] { 0x17, (byte)'A', (byte)'B' };
            var msg2 = new byte[] { 0x17, (byte)'C', (byte)'D' };

            var radio1 = mainForm.COMMainProvider.RadioObject(1);
            radio1.SendCustomCommand(msg1);
            radio1.SendCustomCommand(msg2);

            //main.SetMainStatusText(String.Format("Experiment: Speed={0} toggleSplit={1}", ICOMspeed, !toggleSplit));
            //main.SetMainStatusText(String.Format("Experiment: Callsign = {0}", mainForm.CurrentEntryLine.ActualQSO.Callsign));
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.RControlKey)
                mainForm.SetMainStatusText($"Experiment: Key {e.KeyCode} pressed");
        }

        private void HandleKeyRelease(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.RControlKey)
                mainForm.SetMainStatusText($"Experiment: Key {e.KeyCode} released");
        }
    }
}