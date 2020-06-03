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

            mainForm.KeyDown += new KeyEventHandler(HandleKeyPress);
            mainForm.KeyUp += new KeyEventHandler(HandleKeyRelease);
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain) { }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey && !Sending)
            {
                cdata.TXOnRadio = cdata.FocusedRadio;
                mainForm.SetMainStatusText(string.Format("Transmitting on radio {0}.", cdata.TXOnRadio));
                mainForm.COMMainProvider.SetPTTOn(cdata.TXOnRadio, false);
                Sending = true;
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