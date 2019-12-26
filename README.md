# DXLog.net script kit for IC-7610/785x/7300

*These scripts are used entirely at your own risk. Since the scripts control hardware, 
including things like transmitter output power, they can cause 
hardware damage due to latent software defects or if wrongly configured or used.*

This is a script kit designed for enhancing [DXLog.net](http://dxlog.net) SO2V contest operation with
ICOM IC-7610 and IC-78xx but it also contains functions useful for IC-7300, IC-7600 and IC-7700. 
It is only verified on IC-7610 and IC-7300. Should you find any anomalies or defects when used with 
other radios, please email me or make a pull request.

Main features are:
1. Automatic focus shifting for radio's Main VFO knob in SO2V.
2. Automatic main/sub audio switching in SO2V. (Ctrl-Alt-S or AltGr-S toggles permanent stereo on/off.)
3. Automatic per-band output power level for safe PA operation. (Even supports cross-band SO2V if PA band switching is fast.) 
4. Automatic setting of edges and reference level for the waterfall/spectrum display based on band and operating mode. 
5. Single-key waterfall/spectrum display zoom. 

Note that if the utility [Icom Automagic](https://github.com/bjornekelund/ICOMautomagic) is used, 
items 3-5 are redundant and should not be enabled. 

Minor modifications of the scripts (e.g. changing power levels or band edges) can be 
done using any source code editor (*notepad++* is a good choice.)
Any more sophisticated modification and/or extension of the scripts will greatly 
benefit from using Visual Studio. The [community](https://visualstudio.microsoft.com/downloads)
version is free of charge for individuals and open-source contributors. 

Since the debugging facilities within *DXLog.net* are very limited, a more advanced SDK like Visual 
Studio is a great help.
The script kit is formatted for use in Visual Studio. Once cloned or 
downloaded/decompressed, just double-click on the file `DXLogICOMScripts.sln` 
to open the script kit in Visual Studio.

If the editor complains about syntax/usage (red wavy lines) make sure you have 
references to the *DXLog.net* `DLL` and `EXE` files correctly set up. 
You can add this in the *Solution Explorer* panel. If you see yellow warning 
triangles, re-enter/add the references by right-clicking "References" and 
selecting them in the *DXLog.net* binary folder, 
typically `C:\Program Files (x86)\DXLog.net`.

Since you may want to modify the scripts over time (setting scope edges, output power etc.), 
it is also a good idea to pin this project to Visual Studio's startup panel.

To install the scripts in *DXLog.net*, enter the scripts manager (Tools->Scripts manager),
add the scripts one by one, give them a name and assign a key those that need it.


| Script               | Suggested Name | Suggested Key                            |
|----------------------|----------------|------------------------------------------|
| ICOM_SO2V            | SO2V           | ` on english keyboard, § on Scandinavian | 
| ICOM_SO2V_DW         | TOGGLEDWH      | Ctrl-Alt-S                               |  
| ICOM_Speedsynch      | SPEED          | None                                     | 
| ICOM_Filter          | FILTER         | ' or Alt-'                               |
| ICOM_Bandpower*      | BANDPOWER      | None                                     |
| ICOM_Waterfall_Mode* | WFMODE         | Alt-U                                    | 
| ICOM_Waterfall_Zoom* | WFZOOM         | Alt-Z                                    |


\* Redundant and should not be enabled if the [ICOMautomagic](https://github.com/bjornekelund/ICOMautomagic) utility is used.

The following **Alt keys** are unassigned by default in *DXLog.net*: H, Q, U, X, and Z.

The following **Ctrl keys** are unassigned by default in *DXLog.net*: C, H, I, J, K, M, N, O, P, 
Q, R, U, V, X, and Y.

## Scripts description

**ICOM_SO2V** Automatic switching of VFO knob focus and audio paths for SO2V operation 
with ICOM IC-7610. It has not been tested but believed to be working also for IC-7800, 
7850 and 7851. (All feedback is welcome.) In "Normal listening mode", selecting the 
main VFO ("radio 1" in SO2V) will result in the main receiver in both ears. 
Selecting the sub VFO ("radio 2") will result in the main receiver in the left ear 
and the sub receiver in the right ear. In "permanent stereo" mode (toggled with Ctrl-Alt-S 
or AltGr-S) the main receiver will always be in the left ear and the sub receiver 
in the right ear, independent of focus. Since it is not possible to listen to only 
the sub receiver, it is recommended to Run on the main VFO and S&P on the sub VFO. 
To help with weak stations answering on Run, or to check a station you are waiting 
to work on radio 2, you can temporarily toggle between stereo and only main receiver 
using the key mapped to the script. To manage weak or hard to copy stations on S&P, 
press the main receiver's AF gain knob to temporarily mute it. 

**ICOM_SO2V_DW** Toggles "permanent stereo" on and off. Executes both the script 
(to immediately force dual watch on and off on the radio) and DXLog.net's built-in function
to toggle audio mode. Does nothing if radio 1 not is selected.

**ICOM_Speedsynch** Synchronizes the radio's internal keyer with *DXLog.net's* speed setting.
Acts silently in the background, needs no key mapping. Works for all operating techniques. 
By using both the speed change and focus change events it works also for SO2V where the 
two logical radios are the same physical radio. 

**ICOM_Filter** Cycles through the three filter settings, one step per invokation. 
Works for all operating techniques. 

**ICOM_Bandpower** Per band output power control to avoid overdriving a PA. 
Typically not used for low power/barefoot operation. Edit the power level table 
to set safe levels for each band and adjust level manually if required. 
Only invoked at band changes, so manual adjustments are not overwritten 
until the next band change.

**ICOM_Waterfall_Mode** Automatically sets the edges and reference level of the 
radio's waterfall/spectrum display based on frequency band and operating mode. 
Should be assigned to a key for quick restore after manual adjustments or zoom.
Only operates on VFO A but supports all operating modes; SO1R, SO2R and SO2V.
Only active for ICOM radios but does not actually poll the radio to determine 
if it is waterfall capable. For e.g. SO2R operation with two ICOM radios where 
only one is waterfall capable, modify the object `WaterfallCapable[]` accordingly.

**ICOM_Waterfall_Zoom** Script to quickly zoom the radio's built in panadapter to a 
narrow segment of the band (default is 20kHz, but easily modified in the script code) 
centered around the current frequency.

**ICOM_Experiment** A script solely for development purposes. No need to install. 

Please note that due to the design of *DXLog.net's* CAT queueing mechanics, 
performance may not be completely reliable at 19200bps with frequent polling (< 300ms). 
For the most reliable and responsive operation, communication via the USB interface 
at 115200bps is recommended.

For further information, see the source code or [www.sm7iun.se](https://sm7iun.se/contesting/dxlog-net/)
