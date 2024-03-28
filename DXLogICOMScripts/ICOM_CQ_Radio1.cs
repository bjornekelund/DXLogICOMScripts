// Focus radio 1 and send CQ 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2020-04-15

//
// NB. For educational purposes only. This functionality is now native to DXLog.
//

using IOComm;
using System.Threading;
using NAudio.Midi;

namespace DXLog.net
{
    public class IcomCQRadio1 : IScriptClass
    {
        readonly bool Debug = false;

        // Executes at DXLog.net start 
        public void Initialize(FrmMain main)
        {
        }

        // Executes at DXLog.net close down
        public void Deinitialize() 
        { 
        } 

        // Switch to radio 1 if not selected. Then send CQ. 
        public void Main(FrmMain mainForm, ContestData cdata, COMMain comMain, MidiEvent midiEvent)
        {
            const int RUN = 0;
            const int radioSwitchingDelay = 25;

            if (cdata.OPTechnique == ContestData.Technique.SO2V)
            {
                if (cdata.FocusedRadio == 2)
                {
                    mainForm.handleKeyCommand("CHANGE_ACTIVE_RADIO", null, string.Empty);
                }

                //Thread.Sleep(25);

                if (cdata.RadioModePrimary != RUN)
                {
                    mainForm.handleKeyCommand("CHANGE_RUNSPMODE_PRIMARY", null, string.Empty);
                }

                Thread.Sleep(radioSwitchingDelay);

                mainForm.SendKeyMessage(false, "F1");
            }
        }
    }
}
