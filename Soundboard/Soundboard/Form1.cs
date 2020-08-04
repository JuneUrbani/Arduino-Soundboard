using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Soundboard
{
    public partial class Form1 : Form
    {
        SoundProgram prog1;

        List<String> invalidPrograms = new List<String>()
        {
            "Idle",
            "System",
            "Registry",
            "smss",
            "csrss",
            "wininit",
            "services",
            "lsass",
            "svchost",
            "WUDFHost",
            "fontdrvhost",
            "SynTPEnhService",
            "NVDisplay.Container",
            "Memory Compression",
            "igfxCUIService",
            "audiodg",
            "wlanext",
            "spoolsv",
            "conhost",
            "PresentationFontCache",
            "IntelCpHDCPSvc",
            "Everything",
            "ibtsiva",
            "KillerAnalyticsService",
            "NahimicService",
            "RtkAudUService64",
            "LMIGuardianSvc",
            "MSIService",
            "MsiTrueColorService",
            "nvcontainer",
            "OfficeClickToRun",
            "OriginWebHelperService",
            "MsMpEng",
            "KillerNetworkService",
            "hamachi-2",
            "IntelCpHeciSvc",
            "xTendUtilityService",
            "KSPSService",
            "KNDBWMService",
            "KSPS",
            "xTendUtility",
            "KNDBWM",
            "WmiPrvSE",
            "NisSrv",
            "SearchIndexer",
            "dllhost",
            "SecurityHealthService",
            "IAStorDataMgrSvc",
            "jhi_service",
            "LMS",
            "SgrmBroker",
            "winlogon",
            "dwm",
            "ctfmon",
            "sihost",
            "igfxEM",
            "taskhostw",
            "SynTPEnh",
            "explorer",
            "SynTPHelper",
            "StartMenuExperienceHost",
            "RuntimeBroker",
            "NahimicSvc64",
            "SearchUI",
            "NahimicSvc32",
            "YourPhone",
            "SettingSyncHost",
            "NVIDIA Web Helper",
            "ApplicationFrameHost",
            "CompPkgSrv",
            "nvsphelper64",
            "SecurityHealthSystray",
            "NVIDIA Share",
            "MsiTrueColor",
            "MsiTrueColorHelper",
            "unsecapp",
            "SCM",
            "plugin-container",
            "igfxext",
            "SteelSeriesEngine3",
            "hamachi-2-ui",
            "Dragon Center",
            "IAStorIcon",
            "Microsoft.Photos",
            "LockApp",
            "devenv",
            "PerfWatson2",
            "Microsoft.ServiceHub.Controller",
            "ServiceHub.IdentityHost",
            "ServiceHub.VSDetouredHost",
            "ServiceHub.SettingsHost",
            "ServiceHub.Host.CLR.x86",
            "ServiceHub.RoslynCodeAnalysisService32",
            "ServiceHub.ThreadedWaitDialog",
            "ServiceHub.TestWindowStoreHost",
            "ServiceHub.DataWarehouseHost",
            "WindowsInternal.ComposableShell.Experiences.TextInput.InputApp",
            "init",
            "wslhost",
            "WinStore.App",
            "SystemSettings",
            "MSBuild",
            "VBCSCompiler",
            "ScriptedSandbox64",
            "WinFormsSurface",
            "TrustedInstaller",
            "TiWorker",
            "SearchProtocolHost",
            "SearchFilterHost",
            "msvsmon",
            "StandardCollector.Service"
        };

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public Form1()
        {
            AllocConsole();

            InitializeComponent();
            updateBoxes();
            new Thread(() => 
            {
                Thread.CurrentThread.IsBackground = true; 
                /* run your code here */ 
                while(true)
                {
                    VolumeMixer.SetApplicationVolume(prog1.pID, 50.0f);
                }
            }).Start();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            prog1 = comboBox1.SelectedItem as SoundProgram;
        }

        public void populateBox(ComboBox comboBox)
        {
            var dataSource = new List<SoundProgram>();
            foreach (var process in Process.GetProcesses())
            {
                bool containsItem = invalidPrograms.Any(item => item == process.ProcessName);
                if(!containsItem) 
                {
                    dataSource.Add(new SoundProgram() { Name = process.ProcessName, pID = process.Id });
                }
            }

            dataSource.Sort((x, y) => string.Compare(x.Name, y.Name));

            comboBox.DataSource = dataSource.Distinct().ToList();
            comboBox.DisplayMember = "Name";
            comboBox.ValueMember = "pID";

            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            updateBoxes();
        }

        private void updateBoxes() 
        {
            populateBox(this.comboBox1);
            populateBox(this.comboBox2);
        }
    }
}

public class SoundProgram
{
    public string Name { get; set; }
    public int pID { get; set; }

    public override bool Equals (object obj)
    {

        var item = obj as SoundProgram;

        if (item == null)
        {
            return false;
        }

        return this.Name == item.Name;
    }
    public override int GetHashCode()
    {
        return this.Name.GetHashCode();
    }
}

public class VolumeMixer
{
    public static float? GetApplicationVolume(int pid)
    {
        ISimpleAudioVolume volume = GetVolumeObject(pid);
        if (volume == null)
            return null;

        float level;
        volume.GetMasterVolume(out level);
        Marshal.ReleaseComObject(volume);
        return level * 100;
    }

    public static bool? GetApplicationMute(int pid)
    {
        ISimpleAudioVolume volume = GetVolumeObject(pid);
        if (volume == null)
            return null;

        bool mute;
        volume.GetMute(out mute);
        Marshal.ReleaseComObject(volume);
        return mute;
    }

    public static void SetApplicationVolume(int pid, float level)
    {
        ISimpleAudioVolume volume = GetVolumeObject(pid);
        if (volume == null)
            return;

        Guid guid = Guid.Empty;
        volume.SetMasterVolume(level / 100, ref guid);
        Marshal.ReleaseComObject(volume);
    }

    public static void SetApplicationMute(int pid, bool mute)
    {
        ISimpleAudioVolume volume = GetVolumeObject(pid);
        if (volume == null)
            return;

        Guid guid = Guid.Empty;
        volume.SetMute(mute, ref guid);
        Marshal.ReleaseComObject(volume);
    }

    private static ISimpleAudioVolume GetVolumeObject(int pid)
    {
        // get the speakers (1st render + multimedia) device
        IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
        IMMDevice speakers;
        deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

        // activate the session manager. we need the enumerator
        Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
        object o;
        speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
        IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

        // enumerate sessions for on this device
        IAudioSessionEnumerator sessionEnumerator;
        mgr.GetSessionEnumerator(out sessionEnumerator);
        int count;
        sessionEnumerator.GetCount(out count);

        // search for an audio session with the required name
        // NOTE: we could also use the process id instead of the app name (with IAudioSessionControl2)
        ISimpleAudioVolume volumeControl = null;
        for (int i = 0; i < count; i++)
        {
            IAudioSessionControl2 ctl;
            sessionEnumerator.GetSession(i, out ctl);
            int cpid;
            ctl.GetProcessId(out cpid);

            if (cpid == pid)
            {
                volumeControl = ctl as ISimpleAudioVolume;
                break;
            }
            Marshal.ReleaseComObject(ctl);
        }
        Marshal.ReleaseComObject(sessionEnumerator);
        Marshal.ReleaseComObject(mgr);
        Marshal.ReleaseComObject(speakers);
        Marshal.ReleaseComObject(deviceEnumerator);
        return volumeControl;
    }
}

[ComImport]
[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
internal class MMDeviceEnumerator
{
}

internal enum EDataFlow
{
    eRender,
    eCapture,
    eAll,
    EDataFlow_enum_count
}

internal enum ERole
{
    eConsole,
    eMultimedia,
    eCommunications,
    ERole_enum_count
}

[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceEnumerator
{
    int NotImpl1();

    [PreserveSig]
    int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

    // the rest is not implemented
}

[Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDevice
{
    [PreserveSig]
    int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

    // the rest is not implemented
}

[Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioSessionManager2
{
    int NotImpl1();
    int NotImpl2();

    [PreserveSig]
    int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

    // the rest is not implemented
}

[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioSessionEnumerator
{
    [PreserveSig]
    int GetCount(out int SessionCount);

    [PreserveSig]
    int GetSession(int SessionCount, out IAudioSessionControl2 Session);
}

[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISimpleAudioVolume
{
    [PreserveSig]
    int SetMasterVolume(float fLevel, ref Guid EventContext);

    [PreserveSig]
    int GetMasterVolume(out float pfLevel);

    [PreserveSig]
    int SetMute(bool bMute, ref Guid EventContext);

    [PreserveSig]
    int GetMute(out bool pbMute);
}

[Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioSessionControl2
{
    // IAudioSessionControl
    [PreserveSig]
    int NotImpl0();

    [PreserveSig]
    int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

    [PreserveSig]
    int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

    [PreserveSig]
    int GetGroupingParam(out Guid pRetVal);

    [PreserveSig]
    int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

    [PreserveSig]
    int NotImpl1();

    [PreserveSig]
    int NotImpl2();

    // IAudioSessionControl2
    [PreserveSig]
    int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int GetProcessId(out int pRetVal);

    [PreserveSig]
    int IsSystemSoundsSession();

    [PreserveSig]
    int SetDuckingPreference(bool optOut);
}