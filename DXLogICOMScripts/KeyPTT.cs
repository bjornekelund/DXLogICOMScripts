//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// Keyboard PTT experiment by Björn Ekelund SM7IUN sm7iun@ssa.se 

using System.Windows.Forms;
using IOComm;

namespace DXLog.net
{
    public class KeyPTT : ScriptClass
    {
        ContestData cdata;
        FrmMain mainForm;
        bool Sending = false;

        public void Initialize(FrmMain main)
        {
            cdata = main.ContestDataProvider;
            mainForm = main;

            //mainForm.KeyDown += new KeyEventHandler(HandleKeyPress);
            //mainForm.KeyUp += new KeyEventHandler(HandleKeyRelease);
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain) 
        {
            cdata.TXOnRadio = cdata.FocusedRadio;
            if (!Sending)
            {
                Sending = true;
                mainForm.HandleTXRequestChange(Sending);
                mainForm.SetMainStatusText(string.Format("Transmitting on radio {0}.", cdata.TXOnRadio));
                mainForm.COMMainProvider.SetPTTOn(cdata.TXOnRadio, false);
            }
            else
            {
                Sending = false;
                mainForm.HandleTXRequestChange(Sending);
                mainForm.SetMainStatusText(string.Format("PTT off on radio {0}", cdata.TXOnRadio));
                mainForm.COMMainProvider.SetPTTOff(cdata.FocusedRadio);
            }
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                cdata.TXOnRadio = cdata.FocusedRadio;
                if (!Sending)
                {
                    mainForm.SetMainStatusText(string.Format("Transmitting on radio {0}.", cdata.TXOnRadio));
                    mainForm.COMMainProvider.SetPTTOn(cdata.TXOnRadio, false);
                    Sending = true;
                }
                else
                {
                    mainForm.SetMainStatusText(string.Format("PTT off on radio {0}", cdata.TXOnRadio));
                    mainForm.COMMainProvider.SetPTTOff(cdata.FocusedRadio);
                    Sending = false;
                }
            }
        }

        private void HandleKeyRelease(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                mainForm.SetMainStatusText(string.Format(""));
                mainForm.COMMainProvider.SetPTTOff(cdata.FocusedRadio);
                Sending = false;
            }
        }
    }
}