using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace AudioTray
{
    internal enum EDataFlow
    {
        ERender = 0,
        ECapture = 1,
        EAll = 2
    }

    internal enum ERole
    {
        EConsole = 0,
        EMultimedia = 1,
        ECommunications = 2
    }

    [Flags]
    internal enum DeviceState
    {
        Active = 0x00000001,
        Disabled = 0x00000002,
        NotPresent = 0x00000004,
        Unplugged = 0x00000008,
        All = 0x0000000F
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PropertyKey
    {
        public Guid fmtid;
        public int pid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PropVariant
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr p;
        public int p2;
    }

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(EDataFlow dataFlow, DeviceState dwStateMask, out IMMDeviceCollection ppDevices);

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);

        [PreserveSig]
        int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);

        [PreserveSig]
        int RegisterEndpointNotificationCallback(IntPtr pClient);

        [PreserveSig]
        int UnregisterEndpointNotificationCallback(IntPtr pClient);
    }

    [ComImport]
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount(out uint pcDevices);

        [PreserveSig]
        int Item(uint nDevice, out IMMDevice ppDevice);
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        [PreserveSig]
        int OpenPropertyStore(int stgmAccess, out IPropertyStore ppProperties);

        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

        [PreserveSig]
        int GetState(out DeviceState pdwState);
    }

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out uint cProps);

        [PreserveSig]
        int GetAt(uint iProp, out PropertyKey pkey);

        [PreserveSig]
        int GetValue(ref PropertyKey key, out PropVariant pv);

        [PreserveSig]
        int SetValue(ref PropertyKey key, ref PropVariant propvar);

        [PreserveSig]
        int Commit();
    }

    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        [PreserveSig] int RegisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int UnregisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int GetChannelCount(out uint pnChannelCount);
        [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);
        [PreserveSig] int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        [PreserveSig] int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        [PreserveSig] int SetMute(bool bMute, ref Guid pguidEventContext);
        [PreserveSig] int GetMute(out bool pbMute);
        [PreserveSig] int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        [PreserveSig] int VolumeStepUp(ref Guid pguidEventContext);
        [PreserveSig] int VolumeStepDown(ref Guid pguidEventContext);
        [PreserveSig] int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        [PreserveSig] int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    [ComImport]
    [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfig
    {
        [PreserveSig] int GetMixFormat();
        [PreserveSig] int GetDeviceFormat();
        [PreserveSig] int ResetDeviceFormat();
        [PreserveSig] int SetDeviceFormat();
        [PreserveSig] int GetProcessingPeriod();
        [PreserveSig] int SetProcessingPeriod();
        [PreserveSig] int GetShareMode();
        [PreserveSig] int SetShareMode();
        [PreserveSig] int GetPropertyValue();
        [PreserveSig] int SetPropertyValue();
        [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, ERole role);
        [PreserveSig] int SetEndpointVisibility();
    }

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator
    {
    }

    [ComImport]
    [Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    internal class PolicyConfigClient
    {
    }

    internal sealed class AudioDevice
    {
        public string Id;
        public string Name;
    }

    internal static class AudioManager
    {
        private const int ClsctxAll = 23;

        private static PropertyKey friendlyNameKey = new PropertyKey
        {
            fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"),
            pid = 14
        };

        public static List<AudioDevice> GetRenderDevices()
        {
            IMMDeviceCollection collection;
            Marshal.ThrowExceptionForHR(Enumerator().EnumAudioEndpoints(EDataFlow.ERender, DeviceState.Active, out collection));

            uint count;
            Marshal.ThrowExceptionForHR(collection.GetCount(out count));

            var devices = new List<AudioDevice>();
            for (uint i = 0; i < count; i++)
            {
                IMMDevice device = null;
                try
                {
                    Marshal.ThrowExceptionForHR(collection.Item(i, out device));
                    string id;
                    Marshal.ThrowExceptionForHR(device.GetId(out id));
                    devices.Add(new AudioDevice { Id = id, Name = GetFriendlyName(device) });
                }
                finally
                {
                    Release(device);
                }
            }

            Release(collection);
            return devices;
        }

        public static string GetDefaultRenderDeviceId()
        {
            IMMDevice device = null;
            try
            {
                Marshal.ThrowExceptionForHR(Enumerator().GetDefaultAudioEndpoint(EDataFlow.ERender, ERole.EMultimedia, out device));
                string id;
                Marshal.ThrowExceptionForHR(device.GetId(out id));
                return id;
            }
            finally
            {
                Release(device);
            }
        }

        public static void SetDefaultRenderDevice(string deviceId)
        {
            var config = (IPolicyConfig)new PolicyConfigClient();
            try
            {
                Marshal.ThrowExceptionForHR(config.SetDefaultEndpoint(deviceId, ERole.EConsole));
                Marshal.ThrowExceptionForHR(config.SetDefaultEndpoint(deviceId, ERole.EMultimedia));
                Marshal.ThrowExceptionForHR(config.SetDefaultEndpoint(deviceId, ERole.ECommunications));
            }
            finally
            {
                Release(config);
            }
        }

        public static float GetDefaultRenderVolume()
        {
            IMMDevice device = null;
            object endpoint = null;
            try
            {
                Marshal.ThrowExceptionForHR(Enumerator().GetDefaultAudioEndpoint(EDataFlow.ERender, ERole.EMultimedia, out device));
                var iid = typeof(IAudioEndpointVolume).GUID;
                Marshal.ThrowExceptionForHR(device.Activate(ref iid, ClsctxAll, IntPtr.Zero, out endpoint));
                var volume = (IAudioEndpointVolume)endpoint;
                float scalar;
                Marshal.ThrowExceptionForHR(volume.GetMasterVolumeLevelScalar(out scalar));
                return scalar;
            }
            finally
            {
                Release(endpoint);
                Release(device);
            }
        }

        public static bool GetDefaultRenderMute()
        {
            IMMDevice device = null;
            object endpoint = null;
            try
            {
                Marshal.ThrowExceptionForHR(Enumerator().GetDefaultAudioEndpoint(EDataFlow.ERender, ERole.EMultimedia, out device));
                var iid = typeof(IAudioEndpointVolume).GUID;
                Marshal.ThrowExceptionForHR(device.Activate(ref iid, ClsctxAll, IntPtr.Zero, out endpoint));
                var volume = (IAudioEndpointVolume)endpoint;
                bool muted;
                Marshal.ThrowExceptionForHR(volume.GetMute(out muted));
                return muted;
            }
            finally
            {
                Release(endpoint);
                Release(device);
            }
        }

        private static IMMDeviceEnumerator Enumerator()
        {
            return (IMMDeviceEnumerator)new MMDeviceEnumerator();
        }

        private static string GetFriendlyName(IMMDevice device)
        {
            IPropertyStore store = null;
            try
            {
                Marshal.ThrowExceptionForHR(device.OpenPropertyStore(0, out store));
                PropVariant value;
                Marshal.ThrowExceptionForHR(store.GetValue(ref friendlyNameKey, out value));
                try
                {
                    var name = Marshal.PtrToStringUni(value.p);
                    return string.IsNullOrWhiteSpace(name) ? "Unknown audio device" : name;
                }
                finally
                {
                    PropVariantClear(ref value);
                }
            }
            finally
            {
                Release(store);
            }
        }

        private static void Release(object item)
        {
            if (item != null && Marshal.IsComObject(item))
            {
                Marshal.ReleaseComObject(item);
            }
        }

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);
    }

    [XmlRoot("AudioTrayConfig")]
    public sealed class AppConfig
    {
        public string CommandName { get; set; }
        public uint HotkeyModifiers { get; set; }
        public uint HotkeyKey { get; set; }
        public bool DarkMode { get; set; }
        public List<string> CycleDeviceIds { get; set; }
        public List<DeviceMark> Marks { get; set; }

        public AppConfig()
        {
            CommandName = "Default command";
            HotkeyModifiers = HotkeyWindow.ModAlt | HotkeyWindow.ModControl;
            HotkeyKey = HotkeyWindow.VkA;
            DarkMode = true;
            CycleDeviceIds = new List<string>();
            Marks = new List<DeviceMark>();
        }
    }

    public sealed class DeviceMark
    {
        [XmlAttribute]
        public string DeviceId { get; set; }

        [XmlAttribute]
        public string DeviceName { get; set; }

        [XmlAttribute]
        public string ColorName { get; set; }
    }

    internal sealed class HotkeyWindow : NativeWindow, IDisposable
    {
        private const int WmHotkey = 0x0312;
        public const uint ModAlt = 0x0001;
        public const uint ModControl = 0x0002;
        public const uint ModShift = 0x0004;
        public const uint ModWin = 0x0008;
        public const uint VkA = 0x41;
        private const int HotkeyId = 9142;
        private bool registered;

        public event EventHandler HotkeyPressed;

        public HotkeyWindow(uint modifiers, uint key)
        {
            CreateHandle(new CreateParams());
            Register(modifiers, key);
        }

        public bool Register(uint modifiers, uint key)
        {
            if (registered)
            {
                UnregisterHotKey(Handle, HotkeyId);
                registered = false;
            }

            registered = RegisterHotKey(Handle, HotkeyId, modifiers, key);
            return registered;
        }

        public void Dispose()
        {
            if (registered)
            {
                UnregisterHotKey(Handle, HotkeyId);
            }

            DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId && HotkeyPressed != null)
            {
                HotkeyPressed(this, EventArgs.Empty);
            }

            base.WndProc(ref m);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    internal sealed class TrayAppContext : ApplicationContext
    {
        private readonly string configPath;
        private readonly NotifyIcon notifyIcon;
        private readonly Timer timer;
        private readonly HotkeyWindow hotkeyWindow;
        private readonly Dictionary<string, Color> knownColors;
        private readonly AppConfig config;

        private List<AudioDevice> devices;
        private string currentDeviceId;
        private SettingsForm settingsForm;
        private Icon currentIcon;
        private Bitmap currentBitmap;
        private IntPtr currentIconHandle;
        private string currentIconKey;

        public TrayAppContext()
        {
            configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audiotray.config.xml");
            knownColors = new Dictionary<string, Color>
            {
                { "Blue", Color.FromArgb(0, 122, 255) },
                { "Green", Color.FromArgb(34, 197, 94) },
                { "Red", Color.FromArgb(239, 68, 68) },
                { "Yellow", Color.FromArgb(245, 158, 11) },
                { "Purple", Color.FromArgb(168, 85, 247) }
            };

            config = LoadConfig(configPath);
            devices = new List<AudioDevice>();

            notifyIcon = new NotifyIcon { Icon = AppIcon.Load(), Visible = true };
            hotkeyWindow = new HotkeyWindow(config.HotkeyModifiers, config.HotkeyKey);
            hotkeyWindow.HotkeyPressed += delegate { SwitchNextDevice(); };

            timer = new Timer { Interval = 2000 };
            timer.Tick += delegate { RefreshTick(); };
            timer.Start();

            RefreshDevices();
            RebuildMenu();
            UpdateTrayIcon();
            notifyIcon.ShowBalloonTip(1000, "AudioTray", "\u5df2\u542f\u52a8\u3002\u53cc\u51fb\u6258\u76d8\u56fe\u6807\u6253\u5f00\u8bbe\u7f6e\u3002", ToolTipIcon.Info);
            notifyIcon.DoubleClick += delegate { ShowSettings(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Stop();
                timer.Dispose();
                hotkeyWindow.Dispose();
                if (settingsForm != null)
                {
                    settingsForm.Dispose();
                    settingsForm = null;
                }

                if (notifyIcon.ContextMenuStrip != null)
                {
                    notifyIcon.ContextMenuStrip.Dispose();
                }

                notifyIcon.Visible = false;
                DisposeIconState();
                notifyIcon.Dispose();
            }

            base.Dispose(disposing);
        }

        private static AppConfig LoadConfig(string path)
        {
            if (!File.Exists(path))
            {
                return new AppConfig();
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    return NormalizeConfig((AppConfig)new XmlSerializer(typeof(AppConfig)).Deserialize(stream));
                }
            }
            catch
            {
                return new AppConfig();
            }
        }

        private static AppConfig NormalizeConfig(AppConfig loaded)
        {
            var normalized = loaded ?? new AppConfig();
            if (normalized.CycleDeviceIds == null)
            {
                normalized.CycleDeviceIds = new List<string>();
            }

            if (normalized.Marks == null)
            {
                normalized.Marks = new List<DeviceMark>();
            }

            return normalized;
        }

        private void SaveConfig()
        {
            using (var stream = File.Create(configPath))
            {
                new XmlSerializer(typeof(AppConfig)).Serialize(stream, config);
            }
        }

        private void RefreshTick()
        {
            try
            {
                var oldId = currentDeviceId;
                RefreshDevices();
            if (!string.Equals(oldId, currentDeviceId, StringComparison.Ordinal))
            {
                RebuildMenu();
            }

                UpdateTrayIcon();
            }
            catch
            {
                SetNotifyText("AudioTray: \u65e0\u6cd5\u8bfb\u53d6\u97f3\u9891\u8bbe\u5907");
            }
        }

        private void RefreshDevices()
        {
            devices = AudioManager.GetRenderDevices();
            currentDeviceId = AudioManager.GetDefaultRenderDeviceId();

            var activeIds = new HashSet<string>(devices.Select(d => d.Id), StringComparer.Ordinal);
            config.CycleDeviceIds = config.CycleDeviceIds.Where(activeIds.Contains).Distinct().ToList();
            if (RefreshDeviceMarkIds())
            {
                SaveConfig();
            }
        }

        private bool RefreshDeviceMarkIds()
        {
            var changed = false;
            foreach (var device in devices)
            {
                var exactMark = config.Marks.FirstOrDefault(m => string.Equals(m.DeviceId, device.Id, StringComparison.Ordinal));
                if (exactMark != null)
                {
                    if (!string.Equals(exactMark.DeviceName, device.Name, StringComparison.Ordinal))
                    {
                        exactMark.DeviceName = device.Name;
                        changed = true;
                    }

                    continue;
                }

                var nameMark = config.Marks.FirstOrDefault(m => string.Equals(m.DeviceName, device.Name, StringComparison.Ordinal));
                if (nameMark != null)
                {
                    nameMark.DeviceId = device.Id;
                    changed = true;
                }
            }

            return changed;
        }

        private void RebuildMenu()
        {
            RefreshDevices();
            var menu = CreateThemedContextMenu();
            menu.Items.Add(new ToolStripMenuItem("\u5f53\u524d\u8bbe\u5907: " + GetDeviceName(currentDeviceId)) { Enabled = false, AutoSize = false, Height = 34 });
            menu.Items.Add(new ToolStripSeparator());

            var openItem = new ToolStripMenuItem("\u6253\u5f00\u8bbe\u7f6e") { AutoSize = false, Height = 34 };
            openItem.Click += delegate { ShowSettings(); };
            menu.Items.Add(openItem);

            var switchItem = new ToolStripMenuItem("\u5207\u6362\u5230\u4e0b\u4e00\u4e2a\u8bbe\u5907 (" + FormatHotkey(config.HotkeyModifiers, config.HotkeyKey) + ")") { AutoSize = false, Height = 34 };
            switchItem.Click += delegate { SwitchNextDevice(); };
            menu.Items.Add(switchItem);

            menu.Items.Add(new ToolStripSeparator());

            foreach (var device in devices)
            {
                var deviceId = device.Id;
                var deviceItem = new ToolStripMenuItem(device.Name)
                {
                    Checked = string.Equals(device.Id, currentDeviceId, StringComparison.Ordinal),
                    AutoSize = false,
                    Height = 34
                };
                deviceItem.Click += delegate { SwitchToDevice(deviceId); };

                var cycleItem = new ToolStripMenuItem("\u52a0\u5165\u5feb\u6377\u5207\u6362")
                {
                    Checked = config.CycleDeviceIds.Contains(deviceId),
                    AutoSize = false,
                    Height = 32
                };
                cycleItem.Click += delegate
                {
                    SetCycleEnabled(deviceId, !cycleItem.Checked);
                    RebuildMenu();
                };
                deviceItem.DropDownItems.Add(cycleItem);
                deviceItem.DropDownItems.Add(new ToolStripSeparator());

                foreach (var colorName in knownColors.Keys)
                {
                    var markItem = new ToolStripMenuItem("\u6807\u8bb0\u4e3a" + GetColorDisplayName(colorName))
                    {
                        Checked = string.Equals(GetMark(deviceId), colorName, StringComparison.Ordinal),
                        AutoSize = false,
                        Height = 32
                    };
                    var localColorName = colorName;
                    markItem.Click += delegate
                    {
                        SetDeviceMark(deviceId, localColorName);
                        RebuildMenu();
                    };
                    deviceItem.DropDownItems.Add(markItem);
                }

                var clearItem = new ToolStripMenuItem("\u6e05\u9664\u989c\u8272\u6807\u8bb0") { AutoSize = false, Height = 32 };
                clearItem.Click += delegate
                {
                    SetDeviceMark(deviceId, null);
                    RebuildMenu();
                };
                deviceItem.DropDownItems.Add(clearItem);
                ApplyMenuTheme(deviceItem.DropDownItems);
                menu.Items.Add(deviceItem);
            }

            menu.Items.Add(new ToolStripSeparator());
            var refreshItem = new ToolStripMenuItem("\u5237\u65b0\u8bbe\u5907") { AutoSize = false, Height = 34 };
            refreshItem.Click += delegate
            {
                RebuildMenu();
                UpdateTrayIcon();
                if (settingsForm != null)
                {
                    settingsForm.RefreshFromContext();
                }
            };
            menu.Items.Add(refreshItem);

            var exitItem = new ToolStripMenuItem("\u9000\u51fa") { AutoSize = false, Height = 34 };
            exitItem.Click += delegate { ExitThread(); };
            menu.Items.Add(exitItem);
            ApplyMenuTheme(menu.Items);

            var oldMenu = notifyIcon.ContextMenuStrip;
            notifyIcon.ContextMenuStrip = menu;
            if (oldMenu != null)
            {
                oldMenu.Dispose();
            }
        }

        private ContextMenuStrip CreateThemedContextMenu()
        {
            var dark = config.DarkMode;
            var background = dark ? Color.FromArgb(24, 26, 31) : Color.White;
            var text = dark ? Color.White : Color.FromArgb(30, 30, 30);
            var hover = dark ? Color.FromArgb(38, 43, 52) : Color.FromArgb(237, 242, 248);
            var border = dark ? Color.FromArgb(72, 80, 94) : Color.FromArgb(205, 213, 224);
            return new ContextMenuStrip
            {
                BackColor = background,
                ForeColor = text,
                Renderer = CreateMenuRenderer(),
                ShowCheckMargin = false,
                ShowImageMargin = false,
                Padding = new Padding(8, 8, 8, 8)
            };
        }

        private void ApplyMenuTheme(ToolStripItemCollection items)
        {
            var dark = config.DarkMode;
            var background = dark ? Color.FromArgb(24, 26, 31) : Color.White;
            var text = dark ? Color.FromArgb(245, 247, 250) : Color.FromArgb(30, 30, 30);
            foreach (ToolStripItem item in items)
            {
                item.AutoSize = true;
                item.BackColor = background;
                item.ForeColor = text;
                item.Margin = Padding.Empty;
                item.Padding = new Padding(8, 0, 8, 0);
                var menuItem = item as ToolStripMenuItem;
                if (menuItem != null && menuItem.DropDownItems.Count > 0)
                {
                    menuItem.DropDown.BackColor = background;
                    menuItem.DropDown.ForeColor = text;
                    menuItem.DropDown.Renderer = CreateMenuRenderer();
                    var dropDownMenu = menuItem.DropDown as ToolStripDropDownMenu;
                    if (dropDownMenu != null)
                    {
                        dropDownMenu.ShowCheckMargin = false;
                        dropDownMenu.ShowImageMargin = false;
                    }
                    menuItem.DropDown.Padding = new Padding(8, 8, 8, 8);
                    ApplyMenuTheme(menuItem.DropDownItems);
                }
            }
        }

        private ToolStripRenderer CreateMenuRenderer()
        {
            var dark = config.DarkMode;
            var background = dark ? Color.FromArgb(24, 26, 31) : Color.White;
            var hover = dark ? Color.FromArgb(38, 43, 52) : Color.FromArgb(237, 242, 248);
            var border = dark ? Color.FromArgb(72, 80, 94) : Color.FromArgb(205, 213, 224);
            var text = dark ? Color.FromArgb(245, 247, 250) : Color.FromArgb(24, 26, 31);
            var muted = dark ? Color.FromArgb(132, 141, 154) : Color.FromArgb(88, 96, 106);
            return new ThemedMenuRenderer(background, hover, border, Color.FromArgb(78, 198, 255), text, muted);
        }

        private void SetCycleEnabled(string deviceId, bool enabled)
        {
            config.CycleDeviceIds.RemoveAll(id => string.Equals(id, deviceId, StringComparison.Ordinal));
            if (enabled)
            {
                config.CycleDeviceIds.Add(deviceId);
            }

            SaveConfig();
            if (settingsForm != null)
            {
                settingsForm.RefreshFromContext();
            }
        }

        private void SetDeviceMark(string deviceId, string colorName)
        {
            var deviceName = GetKnownDeviceName(deviceId);
            config.Marks.RemoveAll(mark =>
                string.Equals(mark.DeviceId, deviceId, StringComparison.Ordinal) ||
                (!string.IsNullOrWhiteSpace(deviceName) && string.Equals(mark.DeviceName, deviceName, StringComparison.Ordinal)));
            if (!string.IsNullOrWhiteSpace(colorName))
            {
                config.Marks.Add(new DeviceMark { DeviceId = deviceId, DeviceName = deviceName, ColorName = colorName });
            }

            SaveConfig();
            UpdateTrayIcon();
            if (settingsForm != null)
            {
                settingsForm.RefreshFromContext();
            }
        }

        private void SwitchNextDevice()
        {
            RefreshDevices();
            var ids = config.CycleDeviceIds.Count > 0
                ? config.CycleDeviceIds.ToList()
                : devices.Select(d => d.Id).ToList();

            if (ids.Count == 0)
            {
                return;
            }

            var currentIndex = ids.FindIndex(id => string.Equals(id, currentDeviceId, StringComparison.Ordinal));
            var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % ids.Count;
            SwitchToDevice(ids[nextIndex]);
        }

        private void SwitchToDevice(string deviceId)
        {
            if (string.Equals(deviceId, currentDeviceId, StringComparison.Ordinal))
            {
                return;
            }

            AudioManager.SetDefaultRenderDevice(deviceId);
            System.Threading.Thread.Sleep(150);
            RefreshDevices();
            RebuildMenu();
            UpdateTrayIcon();
            if (settingsForm != null)
            {
                settingsForm.RefreshCurrentDeviceStateFromContext();
            }
        }

        private string GetDeviceName(string deviceId)
        {
            var deviceName = GetKnownDeviceName(deviceId);
            return deviceName == null ? "Unknown audio device" : deviceName;
        }

        private string GetKnownDeviceName(string deviceId)
        {
            var device = devices.FirstOrDefault(d => string.Equals(d.Id, deviceId, StringComparison.Ordinal));
            return device == null ? null : device.Name;
        }

        private string GetMark(string deviceId)
        {
            var mark = config.Marks.FirstOrDefault(m => string.Equals(m.DeviceId, deviceId, StringComparison.Ordinal));
            if (mark == null)
            {
                var deviceName = GetKnownDeviceName(deviceId);
                if (!string.IsNullOrWhiteSpace(deviceName))
                {
                    mark = config.Marks.FirstOrDefault(m => string.Equals(m.DeviceName, deviceName, StringComparison.Ordinal));
                }
            }

            return mark == null ? null : mark.ColorName;
        }

        private Color GetCurrentColor()
        {
            var mark = GetMark(currentDeviceId);
            Color color;
            return mark != null && knownColors.TryGetValue(mark, out color) ? color : Color.FromArgb(107, 114, 128);
        }

        private void UpdateTrayIcon()
        {
            var volume = SafeGetVolume();
            var muted = SafeGetMute();
            var volumePercent = Math.Max(0, Math.Min(99, (int)Math.Round(volume * 100)));
            var color = GetCurrentColor();
            var nextIconKey = string.Format("{0}-{1}-{2}-{3}-{4}-{5}", color.R, color.G, color.B, color.A, volumePercent, muted);

            if (!string.Equals(currentIconKey, nextIconKey, StringComparison.Ordinal))
            {
                var oldIcon = currentIcon;
                var oldBitmap = currentBitmap;
                var oldHandle = currentIconHandle;

                var nextBitmap = new Bitmap(32, 32);
                var foregroundColor = GetReadableForegroundColor(color);
                var shadowColor = foregroundColor == Color.White ? Color.FromArgb(150, 0, 0, 0) : Color.FromArgb(120, 255, 255, 255);
                using (var graphics = Graphics.FromImage(nextBitmap))
                using (var backgroundPath = UiDrawing.RoundedRectangle(new Rectangle(1, 1, 30, 30), 7))
                using (var backgroundBrush = new SolidBrush(color))
                using (var borderPen = new Pen(Color.FromArgb(120, 30, 30, 30), 1))
                using (var textBrush = new SolidBrush(foregroundColor))
                using (var shadowBrush = new SolidBrush(shadowColor))
                using (var muteShadowPen = new Pen(shadowColor, 3))
                using (var mutePen = new Pen(foregroundColor, 3))
                using (var font = new Font("Segoe UI", volumePercent >= 10 ? 21F : 25F, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    graphics.Clear(Color.Transparent);
                    graphics.FillPath(backgroundBrush, backgroundPath);
                    graphics.DrawPath(borderPen, backgroundPath);

                    if (muted)
                    {
                        graphics.DrawEllipse(muteShadowPen, 8, 7, 16, 16);
                        graphics.DrawLine(muteShadowPen, 10, 23, 25, 8);
                        graphics.DrawEllipse(mutePen, 7, 6, 16, 16);
                        graphics.DrawLine(mutePen, 9, 22, 24, 7);
                    }
                    else
                    {
                        var textRect = new RectangleF(-1, 1, 32, 29);
                        var displayText = volumePercent.ToString();
                        graphics.DrawString(displayText, font, shadowBrush, new RectangleF(0, 2, 32, 29), format);
                        graphics.DrawString(displayText, font, textBrush, textRect, format);
                    }
                }

                var nextHandle = nextBitmap.GetHicon();
                var nextIcon = Icon.FromHandle(nextHandle);
                currentBitmap = nextBitmap;
                currentIconHandle = nextHandle;
                currentIcon = nextIcon;
                notifyIcon.Icon = currentIcon;
                currentIconKey = nextIconKey;

                DisposeIconObjects(oldIcon, oldBitmap, oldHandle);
            }

            SetNotifyText(string.Format("AudioTray: {0} ({1})", GetDeviceName(currentDeviceId), muted ? "\u5df2\u9759\u97f3" : volumePercent + "%"));
        }

        private static Color GetReadableForegroundColor(Color background)
        {
            var luminance = (background.R * 299 + background.G * 587 + background.B * 114) / 1000;
            return luminance > 155 ? Color.FromArgb(30, 30, 30) : Color.White;
        }

        private static float SafeGetVolume()
        {
            try
            {
                return AudioManager.GetDefaultRenderVolume();
            }
            catch
            {
                return 0;
            }
        }

        private static bool SafeGetMute()
        {
            try
            {
                return AudioManager.GetDefaultRenderMute();
            }
            catch
            {
                return false;
            }
        }

        private void SetNotifyText(string text)
        {
            notifyIcon.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
        }

        private void DisposeIconState()
        {
            notifyIcon.Icon = null;
            DisposeIconObjects(currentIcon, currentBitmap, currentIconHandle);
            currentIcon = null;
            currentBitmap = null;
            currentIconHandle = IntPtr.Zero;
            currentIconKey = null;
        }

        private void DisposeIconObjects(Icon icon, Bitmap bitmap, IntPtr handle)
        {
            if (icon != null)
            {
                icon.Dispose();
            }

            if (bitmap != null)
            {
                bitmap.Dispose();
            }

            if (handle != IntPtr.Zero)
            {
                DestroyIcon(handle);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public void ShowSettings()
        {
            if (settingsForm == null || settingsForm.IsDisposed)
            {
                settingsForm = new SettingsForm(this);
            }

            settingsForm.RefreshFromContext();
            settingsForm.Show();
            settingsForm.WindowState = FormWindowState.Normal;
            settingsForm.Activate();
        }

        public IReadOnlyList<AudioDevice> Devices
        {
            get { return devices; }
        }

        public string CurrentDeviceId
        {
            get { return currentDeviceId; }
        }

        public AppConfig Config
        {
            get { return config; }
        }

        public IReadOnlyDictionary<string, Color> KnownColors
        {
            get { return knownColors; }
        }

        public void RefreshAll()
        {
            RefreshDevices();
            RebuildMenu();
            UpdateTrayIcon();
            if (settingsForm != null)
            {
                settingsForm.RefreshFromContext();
            }
        }

        public void ToggleCycleDevice(string deviceId, bool enabled)
        {
            SetCycleEnabled(deviceId, enabled);
            RebuildMenu();
        }

        public void ApplyDeviceMark(string deviceId, string colorName)
        {
            SetDeviceMark(deviceId, colorName);
            RebuildMenu();
        }

        public void TestSwitch()
        {
            SwitchNextDevice();
        }

        public void SelectDevice(string deviceId)
        {
            SwitchToDevice(deviceId);
        }

        public void UpdateCommandName(string commandName)
        {
            config.CommandName = string.IsNullOrWhiteSpace(commandName) ? "Default command" : commandName.Trim();
            SaveConfig();
        }

        public void SetDarkMode(bool enabled)
        {
            config.DarkMode = enabled;
            SaveConfig();
            if (settingsForm != null)
            {
                settingsForm.ApplyTheme();
                settingsForm.RefreshFromContext();
            }
        }

        public bool IsAutoStartEnabled()
        {
            return StartupManager.IsEnabled();
        }

        public void SetAutoStart(bool enabled)
        {
            StartupManager.SetEnabled(enabled);
        }

        public bool UpdateHotkey(uint modifiers, uint key)
        {
            if (key == 0)
            {
                return false;
            }

            config.HotkeyModifiers = modifiers;
            config.HotkeyKey = key;
            SaveConfig();
            var ok = hotkeyWindow.Register(modifiers, key);
            RebuildMenu();
            return ok;
        }

        public static string FormatHotkey(uint modifiers, uint key)
        {
            var parts = new List<string>();
            if ((modifiers & HotkeyWindow.ModControl) != 0) parts.Add("Ctrl");
            if ((modifiers & HotkeyWindow.ModAlt) != 0) parts.Add("Alt");
            if ((modifiers & HotkeyWindow.ModShift) != 0) parts.Add("Shift");
            if ((modifiers & HotkeyWindow.ModWin) != 0) parts.Add("Win");
            parts.Add(((Keys)key).ToString());
            return string.Join(" + ", parts);
        }

        private static string GetColorDisplayName(string colorName)
        {
            switch (colorName)
            {
                case "Blue": return "\u84dd\u8272";
                case "Green": return "\u7eff\u8272";
                case "Red": return "\u7ea2\u8272";
                case "Yellow": return "\u9ec4\u8272";
                case "Purple": return "\u7d2b\u8272";
                default: return colorName;
            }
        }
    }

    internal static class UiDrawing
    {
        public static GraphicsPath RoundedRectangle(Rectangle rect, int radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class SettingsForm : Form
    {
        private readonly TrayAppContext context;
        private readonly Label hotkeyBox;
        private readonly Button editHotkeyButton;
        private readonly TopTabBar topTabs;
        private readonly TableLayoutPanel mainPage;
        private readonly TableLayoutPanel settingsPage;
        private readonly FlowLayoutPanel deviceList;
        private readonly FlowLayoutPanel settingsList;
        private bool editingHotkey;
        private bool refreshing;

        public SettingsForm(TrayAppContext context)
        {
            this.context = context;

            Text = "AudioTray";
            Icon = AppIcon.Load();
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(700, 500);
            Size = new Size(760, 540);
            BackColor = Color.FromArgb(18, 19, 22);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += HotkeyBoxOnKeyDown;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 19, 22),
                ColumnCount = 1,
                RowCount = 2
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var topBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 18, 28, 10),
                BackColor = Color.FromArgb(18, 19, 22),
                ColumnCount = 2,
                RowCount = 1
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            root.Controls.Add(topBar, 0, 0);

            topTabs = new TopTabBar
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BackColor = Color.FromArgb(18, 19, 22),
                ForeColor = Color.FromArgb(176, 184, 196),
                ActiveColor = Color.FromArgb(78, 198, 255),
                TextColor = Color.FromArgb(176, 184, 196),
                ActiveTextColor = Color.White
            };
            topTabs.SelectedIndexChanged += delegate
            {
                if (topTabs.SelectedIndex == 0)
                {
                    ShowMainPage();
                }
                else
                {
                    ShowSettingsPage();
                }
            };
            topBar.Controls.Add(topTabs, 1, 0);

            mainPage = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 0, 28, 24),
                BackColor = Color.FromArgb(18, 19, 22),
                ColumnCount = 1,
                RowCount = 1
            };
            mainPage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainPage.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(mainPage, 0, 1);

            editHotkeyButton = MakeButton("\u4fee\u6539", 10.5F);
            editHotkeyButton.Click += delegate
            {
                editingHotkey = true;
                hotkeyBox.Text = "\u8bf7\u6309\u4e0b\u5feb\u6377\u952e...";
                Focus();
            };
            hotkeyBox = MakeHotkeyDisplay();

            deviceList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(18, 19, 22),
                Padding = new Padding(0, 16, 0, 0)
            };
            deviceList.Resize += delegate { ResizeDeviceRows(); };
            mainPage.Controls.Add(deviceList, 0, 0);

            settingsPage = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 0, 28, 24),
                BackColor = Color.FromArgb(18, 19, 22),
                ColumnCount = 1,
                RowCount = 1,
                Visible = false
            };
            settingsPage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            settingsPage.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(settingsPage, 0, 1);

            settingsList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(18, 19, 22),
                Padding = new Padding(0, 16, 0, 0)
            };
            settingsList.Resize += delegate { ResizeSettingsRows(); };
            settingsPage.Controls.Add(settingsList, 0, 0);

            Shown += delegate
            {
                editingHotkey = false;
                RefreshHotkeyText();
                ActiveControl = deviceList;
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            WindowChrome.SetDarkTitleBar(Handle, context.Config.DarkMode);
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            var dark = context.Config.DarkMode;
            var window = dark ? Color.FromArgb(18, 19, 22) : Color.FromArgb(246, 247, 249);
            var surface = dark ? Color.FromArgb(18, 19, 22) : Color.FromArgb(246, 247, 249);
            var panel = dark ? Color.FromArgb(31, 33, 38) : Color.White;
            var input = dark ? Color.FromArgb(24, 26, 31) : Color.White;
            var text = dark ? Color.FromArgb(245, 247, 250) : Color.FromArgb(24, 26, 31);
            var secondary = dark ? Color.FromArgb(176, 184, 196) : Color.FromArgb(88, 96, 106);
            var button = dark ? Color.FromArgb(38, 42, 49) : Color.FromArgb(232, 236, 242);
            var buttonHover = dark ? Color.FromArgb(48, 54, 63) : Color.FromArgb(220, 226, 235);

            BackColor = window;
            ForeColor = text;
            WindowChrome.SetDarkTitleBar(Handle, dark);
            ApplyThemeToControls(Controls, surface, panel, input, text, secondary, button, buttonHover);
        }

        private void ApplyThemeToControls(Control.ControlCollection controls, Color surface, Color panel, Color input, Color text, Color secondary, Color button, Color buttonHover)
        {
            foreach (Control control in controls)
            {
                if (control is TableLayoutPanel || control is FlowLayoutPanel)
                {
                    control.BackColor = surface;
                }
                else if (control is DeviceRowPanel)
                {
                    var row = (DeviceRowPanel)control;
                    row.BackColor = panel;
                    row.BorderColor = context.Config.DarkMode ? Color.FromArgb(45, 50, 58) : Color.FromArgb(220, 225, 232);
                    row.CurrentBorderColor = Color.FromArgb(78, 198, 255);
                    foreach (Control child in control.Controls)
                    {
                        child.BackColor = panel;
                    }
                }
                else if (control is Button)
                {
                    control.BackColor = button;
                    control.ForeColor = text;
                    var buttonControl = (Button)control;
                    buttonControl.FlatAppearance.MouseOverBackColor = buttonHover;
                    buttonControl.FlatAppearance.MouseDownBackColor = buttonHover;
                    var roundedButton = control as RoundedButton;
                    if (roundedButton != null)
                    {
                        var activeNav = control.Tag as string == "activeNav";
                        roundedButton.SurfaceColor = activeNav ? Color.FromArgb(28, 78, 105) : button;
                        roundedButton.HoverColor = activeNav ? Color.FromArgb(35, 91, 122) : buttonHover;
                        roundedButton.BorderColor = activeNav ? Color.FromArgb(78, 198, 255) : (context.Config.DarkMode ? Color.FromArgb(66, 73, 84) : Color.FromArgb(204, 211, 222));
                        roundedButton.TextColor = activeNav || context.Config.DarkMode ? Color.White : text;
                    }
                }
                else if (control is TopTabBar)
                {
                    var tabBar = (TopTabBar)control;
                    tabBar.BackColor = surface;
                    tabBar.TextColor = secondary;
                    tabBar.ActiveTextColor = text;
                    tabBar.ActiveColor = Color.FromArgb(78, 198, 255);
                    tabBar.Invalidate();
                }
                else if (control is ColorDotButton)
                {
                    var colorButton = (ColorDotButton)control;
                    colorButton.BackColor = panel;
                    colorButton.SurfaceColor = input;
                    colorButton.BorderColor = context.Config.DarkMode ? Color.FromArgb(66, 73, 84) : Color.FromArgb(204, 211, 222);
                    colorButton.Invalidate();
                }
                else if (control == hotkeyBox)
                {
                    control.BackColor = input;
                    control.ForeColor = text;
                    var hotkeyDisplay = control as RoundedLabel;
                    if (hotkeyDisplay != null)
                    {
                        hotkeyDisplay.SurfaceColor = input;
                        hotkeyDisplay.BorderColor = context.Config.DarkMode ? Color.FromArgb(58, 65, 76) : Color.FromArgb(205, 213, 224);
                        hotkeyDisplay.TextColor = text;
                    }
                }
                else if (control is Label)
                {
                    control.ForeColor = control.Tag as string == "secondary" ? secondary : text;
                    var multilineLabel = control as MultilineCenterLabel;
                    if (multilineLabel != null)
                    {
                        multilineLabel.TextColor = control.ForeColor;
                        multilineLabel.Invalidate();
                    }
                }

                ApplyThemeToControls(control.Controls, surface, panel, input, text, secondary, button, buttonHover);
            }
        }

        private void ShowMainPage()
        {
            topTabs.SelectedIndex = 0;
            settingsPage.Visible = false;
            mainPage.Visible = true;
            mainPage.BringToFront();
            ActiveControl = deviceList;
        }

        private void ShowSettingsPage()
        {
            topTabs.SelectedIndex = 1;
            RefreshSettingsPage();
            mainPage.Visible = false;
            settingsPage.Visible = true;
            settingsPage.BringToFront();
            ApplyTheme();
            ActiveControl = settingsList;
        }

        private void RefreshSettingsPage()
        {
            if (settingsList == null) return;

            settingsList.SuspendLayout();
            try
            {
                settingsList.Controls.Clear();
                RefreshHotkeyText();
                settingsList.Controls.Add(MakeHotkeySettingRow());
                settingsList.Controls.Add(MakeSettingRow("\u6df1\u8272\u6a21\u5f0f", context.Config.DarkMode, delegate(bool enabled)
                {
                    context.SetDarkMode(enabled);
                    RefreshSettingsPage();
                }));
                settingsList.Controls.Add(MakeSettingRow("\u5f00\u673a\u81ea\u52a8\u542f\u52a8", context.IsAutoStartEnabled(), delegate(bool enabled)
                {
                    context.SetAutoStart(enabled);
                    RefreshSettingsPage();
                }));
                ResizeSettingsRows();
            }
            finally
            {
                settingsList.ResumeLayout();
            }
        }

        public void RefreshFromContext()
        {
            if (IsDisposed) return;

            refreshing = true;
            try
            {
                RefreshHotkeyText();
                deviceList.SuspendLayout();
                deviceList.Controls.Clear();
                foreach (var device in context.Devices)
                {
                    deviceList.Controls.Add(MakeDeviceRow(device));
                }
                ResizeDeviceRows();
                if (settingsPage.Visible)
                {
                    RefreshSettingsPage();
                }
                ApplyTheme();
            }
            finally
            {
                deviceList.ResumeLayout();
                refreshing = false;
            }
        }

        public void RefreshCurrentDeviceStateFromContext()
        {
            if (IsDisposed) return;

            var foundCurrent = false;
            foreach (Control control in deviceList.Controls)
            {
                var row = control as DeviceRowPanel;
                if (row == null)
                {
                    continue;
                }

                var deviceId = row.Tag as string;
                var isCurrent = string.Equals(deviceId, context.CurrentDeviceId, StringComparison.Ordinal);
                foundCurrent = foundCurrent || isCurrent;
                if (row.IsCurrent != isCurrent)
                {
                    row.IsCurrent = isCurrent;
                    row.Invalidate();
                }
            }

            if (!foundCurrent)
            {
                RefreshFromContext();
            }
        }

        private Control MakeDeviceRow(AudioDevice device)
        {
            var row = new DeviceRowPanel
            {
                Width = Math.Max(560, deviceList.ClientSize.Width - 28),
                Height = 104,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.FromArgb(31, 33, 38),
                BorderColor = Color.FromArgb(45, 50, 58),
                CurrentBorderColor = Color.FromArgb(78, 198, 255),
                IsCurrent = string.Equals(device.Id, context.CurrentDeviceId, StringComparison.Ordinal)
            };
            row.Tag = device.Id;
            row.Cursor = Cursors.Hand;

            var selected = context.Config.CycleDeviceIds.Contains(device.Id);
            var check = new CheckTile
            {
                Checked = selected,
                Location = new Point(22, 36),
                Size = new Size(32, 32)
            };
            check.CheckedChanged += delegate
            {
                if (!refreshing)
                {
                    context.ToggleCycleDevice(device.Id, check.Checked);
                }
            };
            row.Controls.Add(check);

            var title = new MultilineCenterLabel
            {
                Text = device.Name,
                AutoSize = false,
                Location = new Point(78, 20),
                Size = new Size(Math.Max(180, row.Width - 166), 64),
                ForeColor = Color.FromArgb(245, 247, 250),
                BackColor = Color.FromArgb(31, 33, 38),
                Font = new Font("Microsoft YaHei UI", 9.4F, FontStyle.Bold, GraphicsUnit.Point),
                TextColor = Color.FromArgb(245, 247, 250)
            };
            row.Controls.Add(title);

            EventHandler selectDevice = delegate { context.SelectDevice(device.Id); };
            row.Click += selectDevice;
            title.Click += selectDevice;
            title.Cursor = Cursors.Hand;

            var colorButton = new ColorDotButton
            {
                Size = new Size(46, 46),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(row.Width - 66, 29),
                DotColor = GetDeviceColor(device.Id),
                ToolTipText = "\u9009\u62e9\u989c\u8272"
            };
            colorButton.Click += delegate { ShowColorMenu(colorButton, device.Id); };
            row.Controls.Add(colorButton);
            row.Resize += delegate
            {
                colorButton.Location = new Point(row.Width - 66, 29);
                title.Width = Math.Max(180, row.Width - 166);
                title.Height = 64;
            };

            return row;
        }

        private Control MakeHotkeySettingRow()
        {
            var row = new DeviceRowPanel
            {
                Width = Math.Max(560, settingsList.ClientSize.Width - 28),
                Height = 92,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.FromArgb(31, 33, 38),
                BorderColor = Color.FromArgb(45, 50, 58),
                CurrentBorderColor = Color.FromArgb(78, 198, 255),
                IsCurrent = false
            };

            var title = new Label
            {
                Text = "\u5feb\u6377\u952e",
                AutoSize = false,
                Location = new Point(24, 32),
                Size = new Size(96, 28),
                ForeColor = Color.FromArgb(245, 247, 250),
                BackColor = Color.FromArgb(31, 33, 38),
                Font = new Font("Microsoft YaHei UI", 9.8F, FontStyle.Bold, GraphicsUnit.Point)
            };
            row.Controls.Add(title);

            hotkeyBox.Dock = DockStyle.None;
            hotkeyBox.Location = new Point(128, 25);
            hotkeyBox.Size = new Size(Math.Max(180, row.Width - 250), 42);
            row.Controls.Add(hotkeyBox);

            editHotkeyButton.Dock = DockStyle.None;
            editHotkeyButton.Location = new Point(row.Width - 98, 25);
            editHotkeyButton.Size = new Size(74, 42);
            row.Controls.Add(editHotkeyButton);

            row.Resize += delegate
            {
                hotkeyBox.Size = new Size(Math.Max(180, row.Width - 250), 42);
                editHotkeyButton.Location = new Point(row.Width - 98, 25);
            };

            return row;
        }

        private Control MakeSettingRow(string titleText, bool selected, Action<bool> apply)
        {
            var row = new DeviceRowPanel
            {
                Width = Math.Max(560, settingsList.ClientSize.Width - 28),
                Height = 92,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.FromArgb(31, 33, 38),
                BorderColor = Color.FromArgb(45, 50, 58),
                CurrentBorderColor = Color.FromArgb(78, 198, 255),
                IsCurrent = false
            };

            var title = new Label
            {
                Text = titleText,
                AutoSize = false,
                Location = new Point(24, 32),
                Size = new Size(420, 28),
                ForeColor = Color.FromArgb(245, 247, 250),
                BackColor = Color.FromArgb(31, 33, 38),
                Font = new Font("Microsoft YaHei UI", 9.8F, FontStyle.Bold, GraphicsUnit.Point)
            };
            row.Controls.Add(title);

            var check = new CheckTile
            {
                Checked = selected,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(row.Width - 58, 30),
                Size = new Size(32, 32)
            };
            check.CheckedChanged += delegate
            {
                if (!refreshing)
                {
                    apply(check.Checked);
                }
            };
            row.Controls.Add(check);

            EventHandler toggleSetting = delegate { check.Checked = !check.Checked; };
            row.Click += toggleSetting;
            title.Click += toggleSetting;
            row.Cursor = Cursors.Hand;
            title.Cursor = Cursors.Hand;

            row.Resize += delegate
            {
                check.Location = new Point(row.Width - 58, 30);
                title.Width = Math.Max(220, row.Width - 110);
            };

            return row;
        }

        private void ShowColorMenu(Control anchor, string deviceId)
        {
            var dark = context.Config.DarkMode;
            var current = GetDeviceMark(deviceId);
            var choices = new List<ColorChoice>
            {
                new ColorChoice(null, "\u65e0", Color.FromArgb(120, 120, 120))
            };

            foreach (var colorName in context.KnownColors.Keys)
            {
                choices.Add(new ColorChoice(colorName, GetColorDisplayName(colorName), context.KnownColors[colorName]));
            }

            var dropDown = new ToolStripDropDown
            {
                AutoSize = false,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                BackColor = Color.Transparent
            };

            var palette = new ColorPalettePanel(choices, current, dark)
            {
                Size = new Size(176, 236)
            };
            palette.ColorSelected += delegate(object sender, ColorSelectedEventArgs e)
            {
                context.ApplyDeviceMark(deviceId, e.ColorName);
                dropDown.Close(ToolStripDropDownCloseReason.ItemClicked);
            };

            var host = new ToolStripControlHost(palette)
            {
                AutoSize = false,
                Size = palette.Size,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            dropDown.Items.Add(host);
            dropDown.Size = palette.Size;
            dropDown.Show(anchor, new Point(anchor.Width - palette.Width, anchor.Height + 6));
        }

        private static string GetColorDisplayName(string colorName)
        {
            switch (colorName)
            {
                case "Blue": return "\u84dd\u8272";
                case "Green": return "\u7eff\u8272";
                case "Red": return "\u7ea2\u8272";
                case "Yellow": return "\u9ec4\u8272";
                case "Purple": return "\u7d2b\u8272";
                default: return colorName;
            }
        }

        private string GetDeviceMark(string deviceId)
        {
            var mark = context.Config.Marks.FirstOrDefault(m => string.Equals(m.DeviceId, deviceId, StringComparison.Ordinal));
            if (mark == null)
            {
                var device = context.Devices.FirstOrDefault(d => string.Equals(d.Id, deviceId, StringComparison.Ordinal));
                if (device != null)
                {
                    mark = context.Config.Marks.FirstOrDefault(m => string.Equals(m.DeviceName, device.Name, StringComparison.Ordinal));
                }
            }

            return mark == null ? null : mark.ColorName;
        }

        private Color GetDeviceColor(string deviceId)
        {
            var mark = GetDeviceMark(deviceId);
            Color color;
            return mark != null && context.KnownColors.TryGetValue(mark, out color) ? color : Color.FromArgb(120, 120, 120);
        }

        private static string GuessDeviceKind(string name)
        {
            if (name.IndexOf("head", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("\u8033", StringComparison.OrdinalIgnoreCase) >= 0) return "\u8033\u673a";
            if (name.IndexOf("speaker", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("\u626c\u58f0\u5668", StringComparison.OrdinalIgnoreCase) >= 0) return "\u626c\u58f0\u5668";
            return "\u64ad\u653e\u8bbe\u5907";
        }

        private void HotkeyBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            if (!editingHotkey)
            {
                return;
            }

            var key = e.KeyCode;
            if (key == Keys.ControlKey || key == Keys.Menu || key == Keys.ShiftKey || key == Keys.LWin || key == Keys.RWin) return;

            uint modifiers = 0;
            if (e.Control) modifiers |= HotkeyWindow.ModControl;
            if (e.Alt) modifiers |= HotkeyWindow.ModAlt;
            if (e.Shift) modifiers |= HotkeyWindow.ModShift;

            var ok = context.UpdateHotkey(modifiers, (uint)key);
            editingHotkey = false;
            RefreshHotkeyText();
            if (!ok)
            {
                MessageBox.Show(this, "\u8fd9\u4e2a\u5feb\u6377\u952e\u6ce8\u518c\u5931\u8d25\uff0c\u53ef\u80fd\u5df2\u88ab\u5176\u4ed6\u7a0b\u5e8f\u5360\u7528\u3002", "AudioTray", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshHotkeyText()
        {
            hotkeyBox.Text = TrayAppContext.FormatHotkey(context.Config.HotkeyModifiers, context.Config.HotkeyKey);
        }

        private void ResizeDeviceRows()
        {
            foreach (Control control in deviceList.Controls)
            {
                control.Width = Math.Max(560, deviceList.ClientSize.Width - 28);
            }
        }

        private void ResizeSettingsRows()
        {
            if (settingsList == null) return;

            foreach (Control control in settingsList.Controls)
            {
                control.Width = Math.Max(560, settingsList.ClientSize.Width - 28);
            }
        }

        private Label MakeFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(176, 184, 196),
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Tag = "secondary"
            };
        }

        private TextBox MakeTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(49, 49, 49),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
                Margin = new Padding(0, 8, 10, 8)
            };
        }

        private Label MakeHotkeyDisplay()
        {
            return new RoundedLabel
            {
                Dock = DockStyle.None,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(24, 26, 31),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 12, 0),
                SurfaceColor = Color.FromArgb(24, 26, 31),
                BorderColor = Color.FromArgb(58, 65, 76),
                TextColor = Color.White
            };
        }

        private Button MakeButton(string text, float fontSize)
        {
            var button = new RoundedButton
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(38, 42, 49),
                ForeColor = Color.White,
                Margin = new Padding(0, 7, 6, 7),
                Font = new Font("Microsoft YaHei UI", fontSize, FontStyle.Regular, GraphicsUnit.Point),
                SurfaceColor = Color.FromArgb(38, 42, 49),
                HoverColor = Color.FromArgb(48, 54, 63),
                BorderColor = Color.FromArgb(66, 73, 84),
                TextColor = Color.White
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(56, 56, 56);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(56, 56, 56);
            return button;
        }
    }

    internal sealed class RoundedLabel : Label
    {
        public Color SurfaceColor { get; set; }
        public Color BorderColor { get; set; }
        public Color TextColor { get; set; }

        public RoundedLabel()
        {
            DoubleBuffered = true;
            SurfaceColor = Color.FromArgb(24, 26, 31);
            BorderColor = Color.FromArgb(58, 65, 76);
            TextColor = Color.White;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var parentColor = Parent == null ? BackColor : Parent.BackColor;
            e.Graphics.Clear(parentColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = UiDrawing.RoundedRectangle(rect, 7))
            using (var surface = new SolidBrush(SurfaceColor))
            using (var border = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(surface, path);
                e.Graphics.DrawPath(border, path);
            }

            var textRect = new Rectangle(Padding.Left, 0, Math.Max(0, Width - Padding.Left - Padding.Right), Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, TextColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }
    }

    internal sealed class MultilineCenterLabel : Label
    {
        public Color TextColor { get; set; }

        public MultilineCenterLabel()
        {
            DoubleBuffered = true;
            TextColor = Color.White;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var parentColor = Parent == null ? BackColor : Parent.BackColor;
            e.Graphics.Clear(parentColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var singleLineFlags = TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
            var measuredSingleLine = TextRenderer.MeasureText(e.Graphics, Text, Font, new Size(int.MaxValue, Height), singleLineFlags);
            var wraps = measuredSingleLine.Width > Width;
            var flags = wraps
                ? TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding
                : singleLineFlags;
            var proposed = wraps ? new Size(Math.Max(1, Width), Math.Max(1, Height)) : new Size(Math.Max(1, Width), Height);
            var measured = TextRenderer.MeasureText(e.Graphics, Text, Font, proposed, flags);
            var y = Math.Max(0, (Height - measured.Height) / 2);
            var rect = new Rectangle(0, y, Width, Height - y);
            TextRenderer.DrawText(e.Graphics, Text, Font, rect, TextColor, flags);
        }
    }

    internal sealed class RoundedButton : Button
    {
        private bool hovered;
        private bool pressed;

        public Color SurfaceColor { get; set; }
        public Color HoverColor { get; set; }
        public Color BorderColor { get; set; }
        public Color TextColor { get; set; }

        public RoundedButton()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            SurfaceColor = Color.FromArgb(38, 42, 49);
            HoverColor = Color.FromArgb(48, 54, 63);
            BorderColor = Color.FromArgb(66, 73, 84);
            TextColor = Color.White;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovered = false;
            pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var fill = hovered || pressed ? HoverColor : SurfaceColor;
            var textOffset = pressed ? 1 : 0;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = UiDrawing.RoundedRectangle(rect, 7))
            using (var brush = new SolidBrush(fill))
            using (var border = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }

            var textRect = new Rectangle(textOffset, textOffset, Width, Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, TextColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            var parentColor = Parent == null ? BackColor : Parent.BackColor;
            pevent.Graphics.Clear(parentColor);
        }
    }

    internal sealed class TopTabBar : Control
    {
        private int selectedIndex;
        private readonly string[] labels = { "\u8bbe\u5907", "\u8bbe\u7f6e" };

        public event EventHandler SelectedIndexChanged;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                var next = Math.Max(0, Math.Min(labels.Length - 1, value));
                if (selectedIndex == next)
                {
                    return;
                }

                selectedIndex = next;
                Invalidate();
                if (SelectedIndexChanged != null)
                {
                    SelectedIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public Color TextColor { get; set; }
        public Color ActiveTextColor { get; set; }
        public Color ActiveColor { get; set; }

        public TopTabBar()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            TextColor = Color.FromArgb(176, 184, 196);
            ActiveTextColor = Color.White;
            ActiveColor = Color.FromArgb(78, 198, 255);
            Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            SelectedIndex = e.X < Width / 2 ? 0 : 1;
            base.OnMouseClick(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var tabWidth = Width / labels.Length;
            for (var i = 0; i < labels.Length; i++)
            {
                var rect = new Rectangle(i * tabWidth, 0, tabWidth, Height - 1);
                var textRect = new Rectangle(rect.Left + 10, 0, Math.Max(0, rect.Width - 20), Math.Max(0, Height - 10));
                var active = i == selectedIndex;
                TextRenderer.DrawText(
                    e.Graphics,
                    labels[i],
                    Font,
                    textRect,
                    active ? ActiveTextColor : TextColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                if (active)
                {
                    using (var pen = new Pen(ActiveColor, 2))
                    {
                        var y = Height - 3;
                        e.Graphics.DrawLine(pen, rect.Left + 26, y, rect.Right - 26, y);
                    }
                }
            }
        }
    }

    internal sealed class DeviceRowPanel : Panel
    {
        public bool IsCurrent { get; set; }
        public Color BorderColor { get; set; }
        public Color CurrentBorderColor { get; set; }

        public DeviceRowPanel()
        {
            DoubleBuffered = true;
            BorderColor = Color.FromArgb(45, 50, 58);
            CurrentBorderColor = Color.FromArgb(78, 198, 255);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var parentColor = Parent == null ? BackColor : Parent.BackColor;
            e.Graphics.Clear(parentColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = UiDrawing.RoundedRectangle(rect, 8))
            using (var brush = new SolidBrush(BackColor))
            using (var border = new Pen(IsCurrent ? CurrentBorderColor : BorderColor, IsCurrent ? 1.4F : 1F))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }

            base.OnPaint(e);
        }
    }

    internal sealed class CheckTile : Control
    {
        private bool isChecked;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value)
                {
                    return;
                }

                isChecked = value;
                Invalidate();
                if (CheckedChanged != null)
                {
                    CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public CheckTile()
        {
            BackColor = Color.FromArgb(48, 48, 48);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var parentColor = Parent == null ? BackColor : Parent.BackColor;
            e.Graphics.Clear(parentColor);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var fill = Checked ? Color.FromArgb(78, 198, 255) : BackColor;
            var border = Checked ? Color.FromArgb(78, 198, 255) : Color.FromArgb(94, 103, 116);
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border, 2))
            using (var path = UiDrawing.RoundedRectangle(new Rectangle(1, 1, Width - 3, Height - 3), 6))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }

            if (Checked)
            {
                using (var pen = new Pen(Color.FromArgb(12, 28, 36), 2))
                {
                    e.Graphics.DrawLines(pen, new[]
                    {
                        new Point(7, 16),
                        new Point(13, 22),
                        new Point(23, 9)
                    });
                }
            }
        }
    }

    internal sealed class ColorDotButton : Control
    {
        private readonly ToolTip toolTip = new ToolTip();
        private bool hovered;
        public Color DotColor { get; set; }
        public Color SurfaceColor { get; set; }
        public Color BorderColor { get; set; }
        public string ToolTipText { get { return toolTip.GetToolTip(this); } set { toolTip.SetToolTip(this, value); } }
        public ColorDotButton()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            BackColor = Color.FromArgb(48, 48, 48);
            SurfaceColor = Color.FromArgb(58, 58, 58);
            BorderColor = Color.FromArgb(78, 78, 78);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var borderColor = hovered ? Color.FromArgb(78, 198, 255) : BorderColor;
            using (var background = new SolidBrush(SurfaceColor))
            using (var border = new Pen(borderColor, 1))
            using (var dotBrush = new SolidBrush(DotColor))
            using (var outer = UiDrawing.RoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), 8))
            using (var inner = UiDrawing.RoundedRectangle(new Rectangle(11, 11, Width - 22, Height - 22), 5))
            {
                e.Graphics.FillPath(background, outer);
                e.Graphics.DrawPath(border, outer);
                e.Graphics.FillPath(dotBrush, inner);
            }
        }
    }

    internal sealed class ColorChoice
    {
        public readonly string Name;
        public readonly string DisplayName;
        public readonly Color Color;

        public ColorChoice(string name, string displayName, Color color)
        {
            Name = name;
            DisplayName = displayName;
            Color = color;
        }
    }

    internal sealed class ColorSelectedEventArgs : EventArgs
    {
        public string ColorName { get; private set; }

        public ColorSelectedEventArgs(string colorName)
        {
            ColorName = colorName;
        }
    }

    internal sealed class ColorPalettePanel : Control
    {
        private const int RowHeight = 34;
        private const int TopPadding = 12;
        private const int SidePadding = 10;
        private readonly IList<ColorChoice> choices;
        private readonly string selectedName;
        private readonly bool dark;
        private int hoverIndex = -1;

        public event EventHandler<ColorSelectedEventArgs> ColorSelected;

        public ColorPalettePanel(IList<ColorChoice> choices, string selectedName, bool dark)
        {
            this.choices = choices;
            this.selectedName = selectedName;
            this.dark = dark;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = dark ? Color.FromArgb(24, 26, 31) : Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var nextHover = GetIndexAt(e.Location);
            if (hoverIndex != nextHover)
            {
                hoverIndex = nextHover;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hoverIndex = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            var index = GetIndexAt(e.Location);
            if (index >= 0 && index < choices.Count && ColorSelected != null)
            {
                ColorSelected(this, new ColorSelectedEventArgs(choices[index].Name));
            }

            base.OnMouseClick(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Transparent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var surface = dark ? Color.FromArgb(30, 33, 39) : Color.White;
            var borderColor = dark ? Color.FromArgb(72, 80, 94) : Color.FromArgb(205, 213, 224);
            var textColor = dark ? Color.FromArgb(245, 247, 250) : Color.FromArgb(24, 26, 31);
            var mutedText = dark ? Color.FromArgb(176, 184, 196) : Color.FromArgb(88, 96, 106);
            var hover = dark ? Color.FromArgb(42, 48, 58) : Color.FromArgb(237, 242, 248);
            var selected = dark ? Color.FromArgb(28, 78, 105) : Color.FromArgb(222, 243, 255);

            using (var path = UiDrawing.RoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), 8))
            using (var brush = new SolidBrush(surface))
            using (var border = new Pen(borderColor, 1))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }

            for (var i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                var row = GetRowRect(i);
                var isSelected = string.Equals(choice.Name, selectedName, StringComparison.Ordinal) ||
                    (choice.Name == null && string.IsNullOrWhiteSpace(selectedName));

                if (isSelected || i == hoverIndex)
                {
                    using (var rowPath = UiDrawing.RoundedRectangle(row, 6))
                    using (var rowBrush = new SolidBrush(isSelected ? selected : hover))
                    {
                        e.Graphics.FillPath(rowBrush, rowPath);
                    }
                }

                DrawSwatch(e.Graphics, choice, row, isSelected, borderColor, textColor);

                var labelRect = new Rectangle(row.Left + 38, row.Top, row.Width - 70, row.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    choice.DisplayName,
                    Font,
                    labelRect,
                    choice.Name == null ? mutedText : textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                if (isSelected)
                {
                    using (var pen = new Pen(dark ? Color.White : Color.FromArgb(24, 26, 31), 2))
                    {
                        var x = row.Right - 24;
                        var y = row.Top + 15;
                        e.Graphics.DrawLines(pen, new[]
                        {
                            new Point(x, y + 4),
                            new Point(x + 4, y + 8),
                            new Point(x + 12, y)
                        });
                    }
                }
            }
        }

        private void DrawSwatch(Graphics graphics, ColorChoice choice, Rectangle row, bool selected, Color borderColor, Color textColor)
        {
            var swatch = new Rectangle(row.Left + 12, row.Top + 8, 18, 18);
            if (choice.Name == null)
            {
                using (var pen = new Pen(selected ? textColor : choice.Color, 2))
                {
                    graphics.DrawEllipse(pen, swatch);
                    graphics.DrawLine(pen, swatch.Left + 5, swatch.Bottom - 5, swatch.Right - 5, swatch.Top + 5);
                }
                return;
            }

            using (var brush = new SolidBrush(choice.Color))
            using (var border = new Pen(selected ? Color.White : borderColor, selected ? 2 : 1))
            using (var path = UiDrawing.RoundedRectangle(swatch, 5))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(border, path);
            }
        }

        private int GetIndexAt(Point point)
        {
            var index = (point.Y - TopPadding) / RowHeight;
            if (point.X < SidePadding || point.X > Width - SidePadding || index < 0 || index >= choices.Count)
            {
                return -1;
            }

            return index;
        }

        private Rectangle GetRowRect(int index)
        {
            return new Rectangle(SidePadding, TopPadding + index * RowHeight, Width - SidePadding * 2, RowHeight - 3);
        }
    }

    internal sealed class ThemedMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color background;
        private readonly Color hover;
        private readonly Color border;
        private readonly Color accent;
        private readonly Color text;
        private readonly Color mutedText;

        public ThemedMenuRenderer(Color background, Color hover, Color border, Color accent, Color text, Color mutedText) : base(new ThemedMenuColors(background, hover, border))
        {
            this.background = background;
            this.hover = hover;
            this.border = border;
            this.accent = accent;
            this.text = text;
            this.mutedText = mutedText;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var rect = new Rectangle(4, 1, e.Item.Width - 8, e.Item.Height - 2);
            if (e.Item.Selected || (e.Item as ToolStripMenuItem) != null && ((ToolStripMenuItem)e.Item).Checked)
            {
                using (var path = UiDrawing.RoundedRectangle(rect, 6))
                using (var brush = new SolidBrush(e.Item.Selected ? hover : Color.FromArgb(32, accent)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                }
            }

            var item = e.Item as ToolStripMenuItem;
            if (item != null && item.Checked)
            {
                using (var brush = new SolidBrush(accent))
                {
                    e.Graphics.FillRectangle(brush, 4, 7, 3, e.Item.Height - 14);
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            var item = e.Item as ToolStripMenuItem;
            e.TextColor = e.Item.Enabled
                ? (item != null && item.Checked ? Color.White : text)
                : mutedText;
            e.TextRectangle = new Rectangle(e.TextRectangle.Left + 6, e.TextRectangle.Top, e.TextRectangle.Width - 6, e.TextRectangle.Height);
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = e.Item.Enabled ? mutedText : Color.FromArgb(90, mutedText);
            base.OnRenderArrow(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using (var pen = new Pen(border))
            {
                var y = e.Item.Height / 2;
                e.Graphics.DrawLine(pen, 8, y, e.Item.Width - 8, y);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (var pen = new Pen(border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
        }
    }

    internal sealed class ThemedMenuColors : ProfessionalColorTable
    {
        private readonly Color background;
        private readonly Color hover;
        private readonly Color border;

        public ThemedMenuColors(Color background, Color hover, Color border)
        {
            this.background = background;
            this.hover = hover;
            this.border = border;
        }

        public override Color ImageMarginGradientBegin { get { return background; } }
        public override Color ImageMarginGradientMiddle { get { return background; } }
        public override Color ImageMarginGradientEnd { get { return background; } }
        public override Color MenuItemSelected { get { return hover; } }
        public override Color MenuBorder { get { return border; } }
    }

    internal static class WindowChrome
    {
        public static void SetDarkTitleBar(IntPtr handle, bool dark)
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }

            var enabled = dark ? 1 : 0;
            if (DwmSetWindowAttribute(handle, 20, ref enabled, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(handle, 19, ref enabled, sizeof(int));
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    }

    internal static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "AudioTray";

        public static bool IsEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                var value = key == null ? null : key.GetValue(AppName) as string;
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null)
                {
                    return;
                }

                if (enabled)
                {
                    key.SetValue(AppName, "\"" + Application.ExecutablePath + "\"");
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }
    }

    internal static class AppIcon
    {
        public static Icon Load()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDir, "AudioTray.ico"),
                Path.Combine(baseDir, "build", "AudioTray.ico"),
                Path.Combine(baseDir, "..", "build", "AudioTray.ico")
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    var path = Path.GetFullPath(candidate);
                    if (File.Exists(path))
                    {
                        return new Icon(path);
                    }
                }
                catch
                {
                }
            }

            try
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                return null;
            }
        }
    }

    internal static class Dpi
    {
        public static void Enable()
        {
            try
            {
                SetProcessDPIAware();
            }
            catch
            {
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Dpi.Enable();

            if (args.Any(arg => string.Equals(arg, "--self-test", StringComparison.OrdinalIgnoreCase)))
            {
                var devices = AudioManager.GetRenderDevices();
                var currentId = AudioManager.GetDefaultRenderDeviceId();
                var current = devices.FirstOrDefault(d => string.Equals(d.Id, currentId, StringComparison.Ordinal));
                Console.WriteLine("Current: {0} ({1}%)", current == null ? "Unknown audio device" : current.Name, Math.Round(AudioManager.GetDefaultRenderVolume() * 100));
                foreach (var device in devices)
                {
                    Console.WriteLine("Device: {0}", device.Name);
                }

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayAppContext());
        }
    }
}

