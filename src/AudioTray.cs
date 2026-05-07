using System;
using System.Collections.Generic;
using System.Drawing;
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

            notifyIcon = new NotifyIcon { Visible = true };
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
                    return (AppConfig)new XmlSerializer(typeof(AppConfig)).Deserialize(stream);
                }
            }
            catch
            {
                return new AppConfig();
            }
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
            config.Marks = config.Marks.Where(m => activeIds.Contains(m.DeviceId)).ToList();
        }

        private void RebuildMenu()
        {
            RefreshDevices();
            var menu = CreateThemedContextMenu();
            menu.Items.Add(new ToolStripMenuItem("\u5f53\u524d\u8bbe\u5907: " + GetDeviceName(currentDeviceId)) { Enabled = false });
            menu.Items.Add(new ToolStripSeparator());

            var openItem = new ToolStripMenuItem("\u6253\u5f00\u8bbe\u7f6e");
            openItem.Click += delegate { ShowSettings(); };
            menu.Items.Add(openItem);

            var switchItem = new ToolStripMenuItem("\u5207\u6362\u5230\u4e0b\u4e00\u4e2a\u8bbe\u5907 (" + FormatHotkey(config.HotkeyModifiers, config.HotkeyKey) + ")");
            switchItem.Click += delegate { SwitchNextDevice(); };
            menu.Items.Add(switchItem);

            menu.Items.Add(new ToolStripSeparator());

            foreach (var device in devices)
            {
                var deviceId = device.Id;
                var deviceItem = new ToolStripMenuItem(device.Name)
                {
                    Checked = string.Equals(device.Id, currentDeviceId, StringComparison.Ordinal)
                };
                deviceItem.Click += delegate { SwitchToDevice(deviceId); };

                var cycleItem = new ToolStripMenuItem("\u52a0\u5165\u5feb\u6377\u5207\u6362")
                {
                    Checked = config.CycleDeviceIds.Contains(deviceId)
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
                        Checked = string.Equals(GetMark(deviceId), colorName, StringComparison.Ordinal)
                    };
                    var localColorName = colorName;
                    markItem.Click += delegate
                    {
                        SetDeviceMark(deviceId, localColorName);
                        RebuildMenu();
                    };
                    deviceItem.DropDownItems.Add(markItem);
                }

                var clearItem = new ToolStripMenuItem("\u6e05\u9664\u989c\u8272\u6807\u8bb0");
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
            var refreshItem = new ToolStripMenuItem("\u5237\u65b0\u8bbe\u5907");
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

            var exitItem = new ToolStripMenuItem("\u9000\u51fa");
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
            var background = dark ? Color.FromArgb(42, 42, 42) : Color.White;
            var text = dark ? Color.White : Color.FromArgb(30, 30, 30);
            var hover = dark ? Color.FromArgb(58, 58, 58) : Color.FromArgb(232, 232, 232);
            var border = dark ? Color.FromArgb(82, 82, 82) : Color.FromArgb(180, 180, 180);
            return new ContextMenuStrip
            {
                BackColor = background,
                ForeColor = text,
                Renderer = CreateMenuRenderer(),
                ShowImageMargin = true
            };
        }

        private void ApplyMenuTheme(ToolStripItemCollection items)
        {
            var dark = config.DarkMode;
            var background = dark ? Color.FromArgb(42, 42, 42) : Color.White;
            var text = dark ? Color.White : Color.FromArgb(30, 30, 30);
            foreach (ToolStripItem item in items)
            {
                item.BackColor = background;
                item.ForeColor = text;
                var menuItem = item as ToolStripMenuItem;
                if (menuItem != null && menuItem.DropDownItems.Count > 0)
                {
                    menuItem.DropDown.BackColor = background;
                    menuItem.DropDown.ForeColor = text;
                    menuItem.DropDown.Renderer = CreateMenuRenderer();
                    ApplyMenuTheme(menuItem.DropDownItems);
                }
            }
        }

        private ToolStripRenderer CreateMenuRenderer()
        {
            var dark = config.DarkMode;
            var background = dark ? Color.FromArgb(42, 42, 42) : Color.White;
            var hover = dark ? Color.FromArgb(58, 58, 58) : Color.FromArgb(232, 232, 232);
            var border = dark ? Color.FromArgb(82, 82, 82) : Color.FromArgb(180, 180, 180);
            return new ThemedMenuRenderer(background, hover, border);
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
            config.Marks.RemoveAll(mark => string.Equals(mark.DeviceId, deviceId, StringComparison.Ordinal));
            if (!string.IsNullOrWhiteSpace(colorName))
            {
                config.Marks.Add(new DeviceMark { DeviceId = deviceId, ColorName = colorName });
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
            AudioManager.SetDefaultRenderDevice(deviceId);
            System.Threading.Thread.Sleep(150);
            RefreshDevices();
            RebuildMenu();
            UpdateTrayIcon();
            if (settingsForm != null)
            {
                settingsForm.RefreshFromContext();
            }
        }

        private string GetDeviceName(string deviceId)
        {
            var device = devices.FirstOrDefault(d => string.Equals(d.Id, deviceId, StringComparison.Ordinal));
            return device == null ? "Unknown audio device" : device.Name;
        }

        private string GetMark(string deviceId)
        {
            var mark = config.Marks.FirstOrDefault(m => string.Equals(m.DeviceId, deviceId, StringComparison.Ordinal));
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
                using (var graphics = Graphics.FromImage(nextBitmap))
                using (var textBrush = new SolidBrush(Color.White))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                using (var dotBrush = new SolidBrush(color))
                using (var dotPen = new Pen(Color.FromArgb(30, 30, 30), 2))
                using (var muteShadowPen = new Pen(Color.FromArgb(150, 0, 0, 0), 3))
                using (var mutePen = new Pen(Color.White, 3))
                using (var font = new Font("Segoe UI", volumePercent >= 10 ? 21F : 25F, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    graphics.Clear(Color.Transparent);

                    if (muted)
                    {
                        graphics.DrawEllipse(muteShadowPen, 7, 5, 17, 17);
                        graphics.DrawEllipse(mutePen, 6, 4, 17, 17);
                    }
                    else
                    {
                        var textRect = new RectangleF(-1, -1, 32, 27);
                        var displayText = volumePercent.ToString();
                        graphics.DrawString(displayText, font, shadowBrush, new RectangleF(0, 0, 32, 27), format);
                        graphics.DrawString(displayText, font, textBrush, textRect, format);
                    }

                    graphics.FillEllipse(dotBrush, 21, 21, 10, 10);
                    graphics.DrawEllipse(dotPen, 21, 21, 10, 10);
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

    internal sealed class SettingsForm : Form
    {
        private readonly TrayAppContext context;
        private readonly Label hotkeyBox;
        private readonly Button editHotkeyButton;
        private readonly Button settingsButton;
        private readonly FlowLayoutPanel deviceList;
        private bool editingHotkey;
        private bool refreshing;

        public SettingsForm(TrayAppContext context)
        {
            this.context = context;

            Text = "AudioTray";
            Icon = AppIcon.Load();
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(640, 460);
            Size = new Size(680, 500);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += HotkeyBoxOnKeyDown;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                ColumnCount = 1,
                RowCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 20, 24, 20),
                BackColor = Color.FromArgb(35, 35, 35),
                ColumnCount = 4,
                RowCount = 2
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(content, 0, 0);

            content.Controls.Add(MakeFieldLabel("\u5feb\u6377\u952e"), 0, 0);
            hotkeyBox = MakeHotkeyDisplay();
            content.Controls.Add(hotkeyBox, 1, 0);

            editHotkeyButton = MakeButton("\u4fee\u6539", 10.5F);
            editHotkeyButton.Click += delegate
            {
                editingHotkey = true;
                hotkeyBox.Text = "\u8bf7\u6309\u4e0b\u5feb\u6377\u952e...";
                Focus();
            };
            content.Controls.Add(editHotkeyButton, 2, 0);

            settingsButton = MakeButton("\u8bbe\u7f6e", 10.5F);
            settingsButton.Click += delegate { ShowSettingsMenu(settingsButton); };
            content.Controls.Add(settingsButton, 3, 0);

            deviceList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(35, 35, 35),
                Padding = new Padding(0, 12, 0, 0)
            };
            deviceList.Resize += delegate { ResizeDeviceRows(); };
            content.SetColumnSpan(deviceList, 4);
            content.Controls.Add(deviceList, 0, 1);

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
            var window = dark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(245, 245, 245);
            var surface = dark ? Color.FromArgb(35, 35, 35) : Color.FromArgb(238, 238, 238);
            var panel = dark ? Color.FromArgb(48, 48, 48) : Color.White;
            var input = dark ? Color.FromArgb(49, 49, 49) : Color.White;
            var text = dark ? Color.White : Color.FromArgb(30, 30, 30);
            var secondary = dark ? Color.FromArgb(215, 215, 215) : Color.FromArgb(80, 80, 80);
            var button = dark ? Color.FromArgb(42, 42, 42) : Color.FromArgb(230, 230, 230);
            var buttonHover = dark ? Color.FromArgb(56, 56, 56) : Color.FromArgb(218, 218, 218);

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
                    control.BackColor = panel;
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
                }
                else if (control is ColorDotButton)
                {
                    var colorButton = (ColorDotButton)control;
                    colorButton.BackColor = panel;
                    colorButton.SurfaceColor = button;
                    colorButton.BorderColor = context.Config.DarkMode ? Color.FromArgb(78, 78, 78) : Color.FromArgb(196, 196, 196);
                    colorButton.Invalidate();
                }
                else if (control == hotkeyBox)
                {
                    control.BackColor = input;
                    control.ForeColor = text;
                }
                else if (control is Label)
                {
                    control.ForeColor = text;
                }

                ApplyThemeToControls(control.Controls, surface, panel, input, text, secondary, button, buttonHover);
            }
        }

        private void ShowSettingsMenu(Control anchor)
        {
            var dark = context.Config.DarkMode;
            var menu = new ContextMenuStrip
            {
                BackColor = dark ? Color.FromArgb(42, 42, 42) : Color.White,
                ForeColor = dark ? Color.White : Color.FromArgb(30, 30, 30),
                Renderer = dark
                    ? (ToolStripRenderer)new ThemedMenuRenderer(Color.FromArgb(42, 42, 42), Color.FromArgb(58, 58, 58), Color.FromArgb(82, 82, 82))
                    : new ThemedMenuRenderer(Color.White, Color.FromArgb(232, 232, 232), Color.FromArgb(180, 180, 180)),
                ShowImageMargin = true
            };
            var darkItem = MakeSettingMenuItem("\u6df1\u8272\u6a21\u5f0f", context.Config.DarkMode);
            darkItem.Click += delegate
            {
                context.SetDarkMode(!context.Config.DarkMode);
            };
            menu.Items.Add(darkItem);

            var startupItem = MakeSettingMenuItem("\u5f00\u673a\u81ea\u52a8\u542f\u52a8", context.IsAutoStartEnabled());
            startupItem.Click += delegate
            {
                context.SetAutoStart(!context.IsAutoStartEnabled());
            };
            menu.Items.Add(startupItem);

            menu.Show(anchor, new Point(anchor.Width - 220, anchor.Height + 4));
        }

        private ToolStripMenuItem MakeSettingMenuItem(string text, bool selected)
        {
            return new ToolStripMenuItem(text)
            {
                Checked = selected,
                ForeColor = context.Config.DarkMode ? Color.White : Color.FromArgb(30, 30, 30),
                BackColor = context.Config.DarkMode ? Color.FromArgb(42, 42, 42) : Color.White,
                AutoSize = false,
                Size = new Size(220, 34),
                Image = MakeCheckIcon(selected)
            };
        }

        private static Bitmap MakeCheckIcon(bool selected)
        {
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var border = new Pen(selected ? Color.FromArgb(78, 198, 255) : Color.FromArgb(135, 135, 135), 1))
            using (var fill = new SolidBrush(selected ? Color.FromArgb(78, 198, 255) : Color.Transparent))
            using (var tick = new Pen(Color.FromArgb(12, 28, 36), 2))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                graphics.FillRectangle(fill, 2, 2, 12, 12);
                graphics.DrawRectangle(border, 2, 2, 12, 12);
                if (selected)
                {
                    graphics.DrawLines(tick, new[]
                    {
                        new Point(5, 8),
                        new Point(8, 11),
                        new Point(12, 5)
                    });
                }
            }
            return bitmap;
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
                ApplyTheme();
            }
            finally
            {
                deviceList.ResumeLayout();
                refreshing = false;
            }
        }

        private Control MakeDeviceRow(AudioDevice device)
        {
            var row = new DeviceRowPanel
            {
                Width = Math.Max(560, deviceList.ClientSize.Width - 28),
                Height = 96,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.FromArgb(48, 48, 48),
                IsCurrent = string.Equals(device.Id, context.CurrentDeviceId, StringComparison.Ordinal)
            };

            var selected = context.Config.CycleDeviceIds.Contains(device.Id);
            var check = new CheckTile
            {
                Checked = selected,
                Location = new Point(22, 33),
                Size = new Size(30, 30)
            };
            check.CheckedChanged += delegate
            {
                if (!refreshing)
                {
                    context.ToggleCycleDevice(device.Id, check.Checked);
                }
            };
            row.Controls.Add(check);

            var title = new Label
            {
                Text = GuessDeviceKind(device.Name),
                AutoSize = false,
                Location = new Point(78, 18),
                Size = new Size(360, 28),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point)
            };
            row.Controls.Add(title);

            var subtitle = new Label
            {
                Text = device.Name,
                AutoSize = false,
                Location = new Point(78, 52),
                Size = new Size(360, 28),
                ForeColor = Color.FromArgb(215, 215, 215),
                BackColor = Color.FromArgb(48, 48, 48),
                Font = new Font("Segoe UI", 9.8F, FontStyle.Regular, GraphicsUnit.Point)
            };
            row.Controls.Add(subtitle);

            var colorButton = new ColorDotButton
            {
                Size = new Size(40, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(row.Width - 58, 28),
                DotColor = GetDeviceColor(device.Id),
                ToolTipText = "\u9009\u62e9\u989c\u8272"
            };
            colorButton.Click += delegate { ShowColorMenu(colorButton, device.Id); };
            row.Controls.Add(colorButton);
            row.Resize += delegate
            {
                colorButton.Location = new Point(row.Width - 58, 28);
                title.Width = Math.Max(180, row.Width - 158);
                subtitle.Width = Math.Max(180, row.Width - 158);
            };

            row.DoubleClick += delegate { context.TestSwitch(); };
            return row;
        }

        private void ShowColorMenu(Control anchor, string deviceId)
        {
            var dark = context.Config.DarkMode;
            var menuBack = dark ? Color.FromArgb(42, 42, 42) : Color.White;
            var menuText = dark ? Color.White : Color.FromArgb(30, 30, 30);
            var menuHover = dark ? Color.FromArgb(58, 58, 58) : Color.FromArgb(232, 232, 232);
            var menuBorder = dark ? Color.FromArgb(82, 82, 82) : Color.FromArgb(180, 180, 180);
            var menu = new ContextMenuStrip
            {
                BackColor = menuBack,
                ForeColor = menuText,
                ShowImageMargin = true,
                Renderer = new ThemedMenuRenderer(menuBack, menuHover, menuBorder)
            };
            var current = GetDeviceMark(deviceId);

            var noneItem = MakeColorMenuItem("\u65e0", Color.FromArgb(120, 120, 120), string.IsNullOrWhiteSpace(current));
            noneItem.Click += delegate { context.ApplyDeviceMark(deviceId, null); };
            menu.Items.Add(noneItem);
            menu.Items.Add(new ToolStripSeparator());

            foreach (var colorName in context.KnownColors.Keys)
            {
                var localName = colorName;
                var item = MakeColorMenuItem(GetColorDisplayName(colorName), context.KnownColors[colorName], string.Equals(current, colorName, StringComparison.Ordinal));
                item.Click += delegate { context.ApplyDeviceMark(deviceId, localName); };
                menu.Items.Add(item);
            }

            menu.Show(anchor, new Point(anchor.Width - 180, anchor.Height + 2));
        }

        private ToolStripMenuItem MakeColorMenuItem(string text, Color color, bool selected)
        {
            var dark = context.Config.DarkMode;
            var item = new ToolStripMenuItem(text)
            {
                Checked = selected,
                ForeColor = dark ? Color.White : Color.FromArgb(30, 30, 30),
                BackColor = dark ? Color.FromArgb(42, 42, 42) : Color.White,
                AutoSize = false,
                Size = new Size(180, 30),
                Image = MakeColorSwatch(color)
            };
            return item;
        }

        private static Bitmap MakeColorSwatch(Color color)
        {
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(Color.FromArgb(180, 180, 180), 1))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                graphics.FillEllipse(brush, 2, 2, 12, 12);
                graphics.DrawEllipse(pen, 2, 2, 12, 12);
            }
            return bitmap;
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
            return mark == null ? null : mark.ColorName;
        }

        private Color GetDeviceColor(string deviceId)
        {
            var mark = GetDeviceMark(deviceId);
            Color color;
            return mark != null && context.KnownColors.TryGetValue(mark, out color) ? color : Color.FromArgb(78, 198, 255);
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

        private Label MakeFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(225, 225, 225),
                Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point)
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
            return new Label
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(49, 49, 49),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
                Margin = new Padding(0, 8, 10, 8),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 8, 0)
            };
        }

        private Button MakeButton(string text, float fontSize)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(42, 42, 42),
                ForeColor = Color.White,
                Margin = new Padding(0, 8, 4, 8),
                Font = new Font("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Point)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(56, 56, 56);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(56, 56, 56);
            return button;
        }
    }

    internal sealed class DeviceRowPanel : Panel
    {
        public bool IsCurrent { get; set; }

        public DeviceRowPanel() { DoubleBuffered = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(BackColor)) e.Graphics.FillRectangle(brush, ClientRectangle);
            if (IsCurrent)
            {
                using (var brush = new SolidBrush(Color.FromArgb(0, 160, 20)))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, 5, Height);
                }
            }
            base.OnPaint(e);
        }
    }

    internal sealed class CheckTile : CheckBox
    {
        public CheckTile()
        {
            Appearance = Appearance.Button;
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.FromArgb(48, 48, 48);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            FlatAppearance.BorderSize = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var fill = Checked ? Color.FromArgb(78, 198, 255) : Color.FromArgb(48, 48, 48);
            var border = Checked ? Color.FromArgb(78, 198, 255) : Color.FromArgb(130, 130, 130);
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border, 2))
            {
                e.Graphics.FillRectangle(brush, 1, 1, Width - 2, Height - 2);
                e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
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
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var background = new SolidBrush(SurfaceColor))
            using (var border = new Pen(BorderColor, 1))
            using (var dotBrush = new SolidBrush(DotColor))
            {
                e.Graphics.FillRectangle(background, 7, 7, Width - 14, Height - 14);
                e.Graphics.DrawRectangle(border, 7, 7, Width - 15, Height - 15);
                e.Graphics.FillEllipse(dotBrush, Width / 2 - 7, Height / 2 - 7, 14, 14);
            }
        }
    }

    internal sealed class ThemedMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color background;
        private readonly Color hover;
        private readonly Color border;

        public ThemedMenuRenderer(Color background, Color hover, Color border) : base(new ThemedMenuColors(background, hover, border))
        {
            this.background = background;
            this.hover = hover;
            this.border = border;
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
            var color = e.Item.Selected ? hover : background;
            using (var brush = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
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

