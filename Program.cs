using System.Text.RegularExpressions;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ProtocolInterceptor
{

    class Program
    {
        static Random rnd = new Random();
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

            if (!Regex.IsMatch(args[0], "^[0-9a-z-]+://.+$"))
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
            switch (action)
            {
                case "dismiss":
                    break;
                default:
                case "copy":
                    CopyURL(parsed.Get("url"));
                    break;
            }
        }

        private static void CopyURL(string url)
        {
            var t = new Thread(() =>
            {
                Clipboard.SetText(url);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }
}