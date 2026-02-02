using Microsoft.Win32;
using System.Security;

namespace Protocol_Interceptor
{

    [Obsolete("已解决需求，无需此类协助")]
    public record Browser(string Name, string Icon, string Description, string ClassName)
    {
        public static Browser? FromRegistryKey(RegistryKey key, string protocol)
        {
            using RegistryKey? capabilities = key.OpenSubKey("Capabilities");
            if (capabilities == null) return null;

            using RegistryKey? associations = capabilities.OpenSubKey("URLAssociations");
            if (associations == null) return null;

            if (associations.GetValue(protocol) is not string className) return null;

            return new Browser(
                Name:           capabilities.GetValue("ApplicationName")        as string ?? "",
                Icon:           capabilities.GetValue("ApplicationIcon")        as string ?? "",
                Description:    capabilities.GetValue("ApplicationDescription") as string ?? "",
                ClassName:      className
            );
        }
    }

    [Obsolete("已解决需求，无需此类协助")]
    public class BrowserList
    {
        public record Item(Browser Broswer, string Command);

        private readonly List<Item> items = new();

        public IReadOnlyList<Item> Items => items.AsReadOnly();

        public void ExtendFromHKey(string protocol, RegistryKey root)
        {
            using RegistryKey? classes = root.OpenSubKey(@"SOFTWARE\Classes");
            if (classes == null) return;

            using RegistryKey? clients = root.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            if (clients == null) return;

            foreach (string name in clients.GetSubKeyNames())
            {
                using RegistryKey? key = clients.OpenSubKey(name);
                if (key == null) continue;

                var browser = Browser.FromRegistryKey(key, protocol);
                if (browser == null) continue;

                RegistryKey? classCommand = classes.OpenSubKey(browser.ClassName + @"\shell\open\command");
                if (classCommand == null) continue;

                if (classCommand.GetValue(null) is not string command) continue;

                items.Add(new Item(browser, command));
            }
        }
    }

    [Obsolete("已通过其他方案解决需求，此方法实现过于复杂，需要定制UI，但未来可能会有一定作用，暂时不删")]
    public class BrowserHelper
    {
        public static void Main()
        {
            BrowserList browserList = new();
            try
            {
                browserList.ExtendFromHKey("http", Registry.CurrentUser);
                browserList.ExtendFromHKey("http", Registry.LocalMachine);

                foreach (BrowserList.Item item in browserList.Items)
                {
                    Console.WriteLine(item.Broswer.Name + " " + item.Command);
                }
            } catch (SystemException ex) when (ex is ArgumentNullException || ex is ObjectDisposedException || ex is SecurityException) {
            }
        }
    }
}
