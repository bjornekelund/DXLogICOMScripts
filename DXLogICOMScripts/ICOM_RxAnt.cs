// ICOM 7851/7610 Receive antenna toggling.
// By Björn Ekelund SM7IUN sm7iun@sm7iun.se 2022-07-08

using System;
using IOComm;
using NAudio.Midi;

namespace DXLog.net
{
    public class IcomRxAnt : IScriptClass
    {
        readonly bool StatusText = true;

        bool[] RxAntEnabled = new bool[2];
        byte[] CIVcmd = { 0x12, 0x00, 0x00 };

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
            // Disable Rx antenna on both radios to create a known starting state
            SetIcomRxAnt(1, false, main);
            SetIcomRxAnt(2, false, main);
        }

        // Executes as DXLog.net close down
        public void Deinitialize() 
        { 
        }

        // Executes at macro invocation
        public void Main(FrmMain mainForm, ContestData cdata, COMMain comMain, MidiEvent midiEvent)
        {
            // Toggle state of Rx antenna for currently focused radio
            SetIcomRxAnt(cdata.FocusedRadio, !RxAntEnabled[cdata.FocusedRadio - 1], mainForm);
        }

        private void SetIcomRxAnt(int radio, bool enabled, FrmMain main)
        {
            var radioObject = radio == 1 ? main.COMMainProvider._radio1Object : main.COMMainProvider._radio2Object;

            if (radioObject != null)
            {
                RxAntEnabled[radio - 1] = enabled;
                CIVcmd[2] = (byte)(enabled ? 0x01 : 0x00);
                radioObject.SendCustomCommand(CIVcmd);

                if (StatusText)
                {
                    main.SetMainStatusText($"IcomRxAnt: Rx antenna {(enabled ? "enabled" : "disabled")} on radio {radio} with command {BitConverter.ToString(CIVcmd)}.");
                }
            }
            else
            {
                if (StatusText)
                {
                    main.SetMainStatusText($"IcomRxAnt: Radio {radio} is not available.");
                }
            }
        }
    }
}
