//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM 7851/7610 Receive antenna I/O toggling.
// By Björn Ekelund SM7IUN sm7iun@sm7iun.se 2023-11-22

using System;
using IOComm;

namespace DXLog.net
{
    public class IcomRxAntIO : ScriptClass
    {
        readonly bool StatusText = true;

        bool[] RxAntEnabled = new bool[2];
        byte[] CIVcmd = { 0x16, 0x53, 0x00 };

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
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            // Toggle state of Rx antenna for currently focused radio
            SetIcomRxAnt(cdata.FocusedRadio, !RxAntEnabled[cdata.FocusedRadio - 1], main);
        }

        private void SetIcomRxAnt(int radio, bool enabled, FrmMain main)
        {
            CATCommon radioobject = radio == 1 ? main.COMMainProvider._radio1Object : main.COMMainProvider._radio2Object;

            if (radioobject != null)
            {
                RxAntEnabled[radio - 1] = enabled;
                CIVcmd[2] = (byte)(enabled ? 0x01 : 0x02);
                radioobject.SendCustomCommand(CIVcmd);

                if (StatusText)
                {
                    main.SetMainStatusText(string.Format("IcomRxAntIO: Rx antenna I/O {0} on radio {1} with command {2}.", enabled ? "B" : "A", radio, BitConverter.ToString(CIVcmd)));
                }
            }
            else
            {
                if (StatusText)
                {
                    main.SetMainStatusText(string.Format("IcomRxAntIO: Radio {0} is not available.", radio));
                }
            }
        }
    }
}
