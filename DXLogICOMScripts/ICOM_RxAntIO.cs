// ICOM 7851/7610 Receive antenna I/O toggling.
// By Björn Ekelund SM7IUN sm7iun@sm7iun.se 2023-11-22

using System;
using IOComm;
using NAudio.Midi;

namespace DXLog.net
{
    public class IcomRxAntIO : IScriptClass
    {
        readonly bool StatusText = true;

        int[] RxAntIOstatus = new int[2];
        string[] RxAntIOstatusString = { "OFF", "A", "B" };
        byte[] CIVcmd = { 0x16, 0x53, 0x00 };

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            // Disable Rx Antenna I/O on both radios to create a known starting state
            SetIcomRxAntIO(1, 0x00, main);
            SetIcomRxAntIO(2, 0x00, main);
        }

        // Executes as DXLog.net close down
        public void Deinitialize() 
        { 
        }

        // Executes at macro invocation
        public void Main(FrmMain mainForm, ContestData cdata, COMMain comMain, MidiEvent midiEvent)
        {
            // Step state Rx antenna I/O for currently focused radio
            int newstate = (RxAntIOstatus[cdata.FocusedRadio - 1] + 1) % 3;
            SetIcomRxAntIO(cdata.FocusedRadio, newstate, mainForm);
        }

        private void SetIcomRxAntIO(int radio, int state, FrmMain main)
        {
            var radioObject = radio == 1 ? main.COMMainProvider._radio1Object : main.COMMainProvider._radio2Object;

            if (radioObject != null)
            {
                    RxAntIOstatus[radio - 1] = state;
                CIVcmd[2] = (byte)state;
                radioObject.SendCustomCommand(CIVcmd);

                if (StatusText)
                {
                    main.SetMainStatusText($"IcomRxAntIO: Rx antenna I/O {RxAntIOstatusString[state]} on radio {radio} with command {BitConverter.ToString(CIVcmd)}.");
                }
            }
            else
            {
                if (StatusText)
                {
                    main.SetMainStatusText($"IcomRxAntIO: Radio {radio} is not available.");
                }
            }
        }
    }
}
