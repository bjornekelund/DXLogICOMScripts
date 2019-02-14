# DXLog.net script kit for IC-7610 
 
 ## Requires DXLog.net 2.3.18 which is still not released
 ## Custom ICOM CAT commands is broken in the current stable 2.3.17

These scripts are used entirely at your own risk. Since the scripts control hardware, 
including things like transmitter output power, they can cause 
hardware damage due to latent software defects or if wrongly configured or used.

The script kit is written for and verified on IC-7610 but will likely work for all 
waterfall capable ICOM radios in the 7000 family.
Minor modifications of the scripts (e.g. changing power levels or band edges) can be 
done using any source code editor (*notepad++* is a good choice.)

Any more sophisticated modification and/or extension of the scripts will greatly 
benefit from using Visual Studio. The community version is free of charge for 
individuals and open-source contributors. visualstudio.microsoft.com/downloads

Since the debugging facilities within *DXLog.net* are very limited, a more 
advanced SDK like Visual Studio is a great help.
The script kit is formatted for use in Visual Studio. Once cloned or 
downloaded/decompressed, just double-click on the file *DXLogICOMScripts.sln*.

If the editor complains about syntax/usage (red wavy lines) make sure you have 
references to the DXLog DLL and EXE files correctly set up. You can add this in 
the *Solution Explorer* panel. If you see yellow warning triangles, re-enter/add 
the references by right-clicking "References" and selecting them in the 
DXLog.net binary folder, typically *C:\Program Files (x86)\DXLog.net*.

Since you may want to modify the scripts over time (setting output power etc.), 
it is also a good idea to pin this project to Visual Studio's startup panel.

To install the scripts in DXLog, enter the scripts manager (Tools->Scripts manager)
Add the scripts one by one, give them a good name and assign a key those that need it.


| Script              | Suggested Name | Suggested Key                            |
|---------------------|----------------|------------------------------------------|
| ICOM_Bandpower      | ICOMBANDPOWER  | -                                        |
| ICOM_RIT_Clear      | ICOMRITCLEAR   | Shift-Left or Shift-Delete               | 
| ICOM_RIT_Minus      | ICOMRITMINUS   | Shift-Up                                 | 
| ICOM_RIT_Plus       | ICOMRITPLUS    | Shift-Down                               | 
| ICOM_SO2V           | ICOMSO2V       | ` on english keyboard, § on Scandinavian | 
| ICOM_Speedsynch     | ICOMSPEED      | -                                        | 
| ICOM_Waterfall_Mode | ICOMWFMODE     | Alt-U                                    | 
| ICOM_Waterfall_Zoom | ICOMWFZOOM     | Alt-Z                                    | 

The following **Alt keys** are unassigned by default in *DXLog.net*: H, Q, U, X, and Z.

The following **Ctrl keys** are unassigned by default in *DXLog.net*: C, H, I, J, K, M, N, O, P, 
Q, R, U, V, X, and Y.

The choice of **Shift-Up** vs. **Shift-down** is based on personal preferences. 
Some prefer them to correspond to the up/down movement in the band map, others 
prefer them to correspond to up/down in pitch. 
Remember that pitch depends on whether you use USB or LSB for CW. 
The proposed keys above is consistent with movement in the bandmap, where the 
frequency increases downwards. 

An very valuable feature in DXLog.net is the possibility to invoke scripts 
from key macros. The syntax is *$!SCRIPTNAME* where *SCRIPTNAME* is the name that 
you assign to the script in the Tools->Scripts Manager menu. 
It is highly recommended to use this to reset the RIT in the F1/CQ macro. 
Simply add *$!ICOMRITCLEAR* to the end of the F1 macro definition. 
Additionally, some prefer to do this also in the run F3/TU-Log macro. 

## Scripts description

**ICOM_Bandpower** Per band output power control to avoid overdriving a PA. 
Typically not used for low power/barefoot operation. Edit the power level table 
to set safe levels for each band and adjust level manually if required. 
Only ivoked at band changes. 

**ICOM_RIT_Plus** **ICOM_RIT_Minus** **ICOM_RIT_Clear** Three scripts for 
RIT (during Run) and frequency (during S&P) adjustment 
using shifted arrow keys. Plus and minus refers to actual frequency change. 

**ICOM_SO2V** Automatic switching of VFO knob focus and audio paths for SO2V operation 
with ICOM IC-7610. It has not been tested but believed to be working also for IC-7800, 
7850 and 7851. (All feedback is welcome.) In "Normal listening mode", selecting the 
main VFO ("radio 1" in SO2V) will result in the main receiver in both ears. 
Selecting the sub VFO ("radio 2") will result in the main receiver in the left ear 
and the sub receiver in the right ear. In "stereo" mode (toggled with Ctrl-Alt-S 
or AltGr-S) the main receiver will always be in the left ear and the sub receiver 
in the right ear, independent of focus. Since it is not possible to listen to only 
the sub receiver, it is recommended to Run on the main VFO and S&P on the sub VFO. 
To help with weak stations answering on Run, or to check a station you are waiting 
to work on radio 2, you can temporarily toggle between stereo and only main receiver 
using the key mapped to the script. To manage weak or hard to copy stations on S&P, 
press the main receiver's AF gain knob to temporarily mute it. 

**ICOM_Speedsynch** Synchronizes the radio's internal keyer with *DXLog.net's* speed setting.
Acts silently in the background, needs no key mapping. Works also for SO2R. Since radio 1 and 
radio 2 are the same physical radio in SO2V, and the script event is only raised at speed 
changes, it must unfortunately ignore the speed of radio 2 in SO2V. 
Should this be an actual problem, you can redesign **ICOM_SO2V** to also update the 
radio's keyer speed setting at focus changes. Each logical radio's current speed is 
available in the object `mainForm._cwKeyer.CWSpeed(int radioNumber)`

**ICOM_Waterfall_Mode** Automatically sets the edges and reference level of the 
radio's waterfall/spectrum display based on frequency band and operating mode. 
Should be assigned to a key for quick restore after manual adjustments or zoom.
Only operates on VFO A but supports all operating modes; SO1R, SO2R and SO2V.
Only active for ICOM radios but does not actually poll the radio to determine 
if it is waterfall capable. For e.g. SO2R operation with two ICOM radios where 
only one is waterfall capable, modify the object *WaterfallCapable[]* accordingly.

**ICOM_Waterfall_Zoom** Script to quickly zoom the radio's built in panadapter to a 
narrow segment of the band (default is 20kHz, but easily modified in the script code) 
centered around the current frequency.

**ICOM_Experiment** A script solely for development purposes. No need to install. 

Please note that due to the design of *DXLog.net's* CAT queueing mechanics, 
performance may not be completely reliable at 19200bps with frequent polling (< 300ms). 
For the most reliable and responsive operation, the communication via the USB interface 
at 115200bps is recommended.

For additional informatin, see the source code or www.sm7iun.se
