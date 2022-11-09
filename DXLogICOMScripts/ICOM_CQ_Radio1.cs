//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// Focus radio 1 and send CQ 
// By Björn Ekelund SM7IUN sm7iun@ssa.se 2020-04-15

//
// NB. For educational purposes only. This functionality is now native to DXLog.
//

using System;
using IOComm;
using System.Threading;

namespace DXLog.net
{
    public class IcomCQRadio1 : ScriptClass
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
        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            const int RUN = 0;
            const int radioSwitchingDelay = 25;

            if (cdata.OPTechnique == ContestData.Technique.SO2V)
            {
                if (cdata.FocusedRadio == 2)
                {
                    main.handleKeyCommand("CHANGE_ACTIVE_RADIO", null, string.Empty);
                }

                //Thread.Sleep(25);

                if (cdata.RadioModePrimary != RUN)
                {
                    main.handleKeyCommand("CHANGE_RUNSPMODE_PRIMARY", null, string.Empty);
                }

                Thread.Sleep(radioSwitchingDelay);

                main.SendKeyMessage(false, "F1");
            }
        }
    }
}
