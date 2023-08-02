using Microsoft.Extensions.Logging;

internal enum PlaywrightInstallStatus
{
    Installed,
    AlreadyInstalled,
    Failed
}

internal static class PlaywrightBootstrapper
{
    internal static string HomePath
    {
        get
        {
            var home = Environment.GetEnvironmentVariable("HOME_EXPANDED");

            return string.IsNullOrWhiteSpace(home) ?
                Path.Combine(Environment.ExpandEnvironmentVariables("%localappdata%"), "ms-playwright") :
                Path.Combine(home, "ms-playwright");
        }        
    }

    internal static string ChromiumExecutablePath
    {
        get
        {
            //find the path to chrome.exe in the HomePath
            var files = System.IO.Directory.EnumerateFileSystemEntries(
                PlaywrightBootstrapper.HomePath, 
                "chrome.exe", 
                System.IO.SearchOption.AllDirectories);

            return files.First();
        }
    }

    /// <summary>
    /// Returns true if Playwright was installed, false if it was already installed
    /// </summary>
    /// <returns></returns>
    internal static PlaywrightInstallStatus Run()
    {
        //azure functions running on azure, require the use of HOME_EXPANDED        
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HOME_EXPANDED")))
        {
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", PlaywrightBootstrapper.HomePath);
        }

        if (!Directory.Exists(PlaywrightBootstrapper.HomePath))
        {
            try
            {
                Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
                return PlaywrightInstallStatus.Installed;
            }
            catch
            {
                return PlaywrightInstallStatus.Failed;
            }            
        }

        return PlaywrightInstallStatus.AlreadyInstalled;
    }
}
