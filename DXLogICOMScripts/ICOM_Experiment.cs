//INCLUDE_ASSEMBLY System.dll
//INCLUDE_ASSEMBLY System.Windows.Forms.dll

// ICOM Experiment script for exploration of DXLog's inner workings
// By Björn Ekelund SM7IUN sm7iun@ssa.se 

using System;
using System.Text;
using System.Windows.Forms;
using IOComm;
using CWKeyer;

namespace DXLog.net
{
    public class Experiment : ScriptClass
    {
        ContestData cdata;
        FrmMain mainForm;

        public void Initialize(FrmMain main)
        {
            cdata = main.ContestDataProvider;
            mainForm = main;
        }

        public void Deinitialize() { }

        public void Main(FrmMain main, ContestData cdata, COMMain comMain)
        {
            int focusedRadio = cdata.FocusedRadio;
            CATCommon radio1 = comMain.RadioObject(focusedRadio);
            string message = "Unassigned";

            message = main.CurrentEntryLine.ActualQSO.Rcvd4;
            if (message == "")
                message = cdata.dalMain.ReadQSO(cdata.dalMain.MaxIDQSO()).Rcvd.Split(' ')[1];

            //message = cdata.dalHeader.CWMessage1;
            //comMain.SendCWSmart(1, Encoding.ASCII.GetBytes(message));
            //frmMain._cwKeyer.CWMessageStack.Enqueue(message);
            mainForm.SendCW(message, focusedRadio, true);

            main.SetMainStatusText(String.Format("Experiment! message = {0}", message));
        }
    }
}