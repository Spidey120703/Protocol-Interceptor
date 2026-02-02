using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Protocol_Interceptor
{
    /// <summary>
    /// Win32 原生库
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/ns-shlobj_core-openasinfo">OPENASINFO (shlobj_core.h) - Win32 apps | Microsoft Learn</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shopenwithdialog">SHOpenWithDialog function (shlobj_core.h) - Win32 apps | Microsoft Learn</seealso>
    public class Win32Native
    {

        [Flags]
        public enum OPEN_AS_INFO_FLAGS
        {
            /// <summary>
            /// Enable the "always use this program" checkbox. If not passed, it will be disabled.
            /// </summary>
            OAIF_ALLOW_REGISTRATION = 0x00000001,

            /// <summary>
            /// Do the registration after the user hits the <b>OK</b> button.
            /// </summary>
            OAIF_REGISTER_EXT = 0x00000002,

            /// <summary>
            /// Execute file after registering.
            /// </summary>
            OAIF_EXEC = 0x00000004,

            /// <summary>
            /// Force the <b>Always use this program</b> checkbox to be checked. Typically, you won't use the OAIF_ALLOW_REGISTRATION flag when you pass this value.
            /// </summary>
            OAIF_FORCE_REGISTRATION = 0x00000008,

            /// <summary>
            /// <b>Introduced in Windows Vista</b>. Hide the <b>Always use this program</b> checkbox. If this flag is specified, the OAIF_ALLOW_REGISTRATION and OAIF_FORCE_REGISTRATION flags will be ignored.
            /// </summary>
            OAIF_HIDE_REGISTRATION = 0x00000020,

            /// <summary>
            /// <b>Introduced in Windows Vista</b>. The value for the extension that is passed is actually a protocol, so the <b>Open With</b> dialog box should show applications that are registered as capable of handling that protocol.
            /// </summary>
            OAIF_URL_PROTOCOL = 0x00000040,

            /// <summary>
            /// <b>Introduced in Windows 8</b>. The location pointed to by the <i>pcszFile</i> parameter is given as a URI.
            /// </summary>
            OAIF_FILE_IS_URI = 0x00000080,
        }

        /// <summary>
        /// Stores information for the SHOpenWithDialog function.
        /// <code>
        /// typedef struct _openasinfo {
        ///   LPCWSTR pcszFile;
        ///   LPCWSTR pcszClass;
        ///   OPEN_AS_INFO_FLAGS oaifInFlags;
        /// } OPENASINFO, *POPENASINFO;
        /// </code>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OPENASINFO
        {
            /// <summary>
            /// A pointer to the file name.
            /// </summary>
            public string pcszFile;

            /// <summary>
            /// A pointer to the file type description. Set this parameter to <b>NULL</b> to use the file name extension of <b>pcszFile</b>.
            /// </summary>
            public string pcszClass;

            /// <summary>
            /// The characteristics of the SHOpenWithDialog dialog box. One or more of the following values.
            /// </summary>
            public OPEN_AS_INFO_FLAGS oaifInFlags;
        }

        /// <summary>
        /// Displays the <b>Open With</b> dialog box.
        /// <code>
        /// SHSTDAPI SHOpenWithDialog(
        ///   [in, optional] HWND             hwndParent,
        ///   [in]           const OPENASINFO *poainfo
        /// );
        /// </code>
        /// </summary>
        /// <param name="hwndParent">The handle of the parent window. This value can be <b>NULL</b>.</param>
        /// <param name="poainfo">A pointer to an OPENASINFO structure, which specifies the contents of the resulting dialog.</param>
        /// <returns>If this function succeeds, it returns <b>S_OK</b>. Otherwise, it returns an <b>HRESULT</b> error code.</returns>
        [DllImport("shell32.dll", EntryPoint = "SHOpenWithDialog", CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1401:P/Invokes 应该是不可见的", Justification = "<挂起>")]
        public static extern int SHOpenWithDialog(IntPtr hwndParent, ref OPENASINFO poainfo);
    }

    class Program
    {
        static readonly Random rnd = new();
        const string Group = "protocol-interceptor";

        static void Main(string[] args)
        {
            if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
            {
                using var wait = new ManualResetEventSlim(false);
                ToastNotificationManagerCompat.OnActivated += e =>
                {
                    ToastNotificationManagerCompat_OnActivated(e);
                    wait.Set();
                };

                wait.Wait(TimeSpan.FromSeconds(1));
                return;
            }

            if (args.Length != 1)
            {
                return;
            }

            if (!Regex.IsMatch(args[0], "^[0-9a-z][0-9a-z-]*://.+$"))
            {
                return;
            }

            ShowToastContent(args[0]);
        }

        private static void ShowToastContent(string url)
        {
            int n = rnd.Next(0x0000, 0xffff) & 0xffff;
            string tag = "protocol-interceptor-toast-" + n.ToString("x4");

            new ToastContentBuilder()
                .AddText(url)
                .AddArgument("tag", tag)
                .AddArgument("url", url)

                .AddButton(new ToastButton()
                    .SetContent("复制")
                    .AddArgument("action", "copy"))

                .AddButton(new ToastButton()
                    .SetContent("忽略")
                    .AddArgument("action", "dismiss"))

                .Show(toast =>
                {
                    toast.ExpirationTime = DateTime.Now.AddMinutes(10);
                    toast.Tag = tag;
                    toast.Group = Group;
                });
        }

        private static void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            var parsed = ToastArguments.Parse(e.Argument);
            var tag = parsed.Get("tag");
            ToastNotificationManagerCompat.History.Remove(tag, Group);

            parsed.TryGetValue("action", out var action);

            var url = parsed.Get("url");
            switch (action)
            {
                case "dismiss":
                    break;
                case "copy":
                    CopyURL(url);
                    break;
                default:
                    OpenAs(url);
                    break;
            }
        }

        private static void OpenAs(string url)
        {
            // 等同于：C:\> rundll32.exe shell32.dll,OpenAs_RunDLL {url}
            var info = new Win32Native.OPENASINFO
            {
                pcszFile = url,
                oaifInFlags = Win32Native.OPEN_AS_INFO_FLAGS.OAIF_URL_PROTOCOL | Win32Native.OPEN_AS_INFO_FLAGS.OAIF_EXEC
            };
            _ = Win32Native.SHOpenWithDialog(IntPtr.Zero, ref info);
        }

        private static void CopyURL(string url)
        {
            var t = new Thread(() =>
            {
                Clipboard.SetText(url);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}