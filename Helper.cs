using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace PlaywrightScript;

/// <summary>
/// Common helper for dealing with Playwright stuff
/// </summary>
internal static partial class Helper
{
    /// <summary>
    /// Where do we need to load auth state
    /// </summary>
    private static string StatePath { get; set; } = Path.Combine(".auth", "state.json");

    /// <summary>
    /// Use existing MS Edge for the browser, no need to install from playwright script
    /// </summary>
    /// <param name="playwright"></param>
    /// <returns>Existing MS Edge browser with GUI visible</returns>
    public static Task<IBrowser> LaunchEdge(this IPlaywright playwright)
    {
        var opt = new BrowserTypeLaunchOptions
        {
            Headless = false,
            Channel = "msedge",
            Timeout = 10_000
        };
        var msEdgePath = GetMsEdgePath();
        if (!string.IsNullOrWhiteSpace(msEdgePath))
        {
            opt.ExecutablePath = msEdgePath;
        }

        return playwright.Chromium.LaunchAsync(opt);
    }

    /// <summary>
    /// Get new page and load .auth/state.json when available in particular order:
    /// <list type="list">
    ///     <item>Current working directory</item>
    ///     <item>App binary directory</item>
    ///     <item>Project directory</item>
    /// </list>
    /// </summary>
    /// <param name="browser"></param>
    /// <returns></returns>
    public static async Task<IPage> GetPageAsync(this IBrowser browser)
    {
        var opt = new BrowserNewContextOptions();
        var statePath = GetStatePath();
        if (!string.IsNullOrEmpty(statePath))
        {
            opt.StorageStatePath = statePath;
            Console.WriteLine($"Loading state from {statePath}");
        }

        var context = await browser.NewContextAsync(opt);
        return await context.NewPageAsync();
    }

    private static string GetMsEdgePath()
    {
        string relativePath = Path.Combine("Microsoft", "Edge", "Application", "msedge.exe");
        string[] programFilesEnvs = ["ProgramFiles", "ProgramFiles(x86)"];
        foreach (var programEnv in programFilesEnvs)
        {
            var envPath = Environment.GetEnvironmentVariable(programEnv);
            if (string.IsNullOrWhiteSpace(envPath))
            {
                continue;
            }

            var fullPath = Path.Combine(envPath, relativePath);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return string.Empty;
    }

    private static string GetStatePath()
    {
        if (File.Exists(StatePath))
        {
            return StatePath;
        }

        var appBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, StatePath);
        if (File.Exists(appBinPath))
        {
            return appBinPath;
        }

        var parent = Directory.GetParent(Directory.GetCurrentDirectory());
        var projectFolderRegex = ProjectFolderPattern();
        while (parent is not null && projectFolderRegex.IsMatch(parent.Name))
        {
            parent = parent.Parent;
        }

        var projectPath = Path.Combine(parent?.FullName ?? string.Empty, StatePath);
        if (File.Exists(projectPath))
        {
            return projectPath;
        }

        return string.Empty;
    }

    [GeneratedRegex(@"(bin|debug|release|(net\d+(\.\d+)?))", RegexOptions.IgnoreCase, "id-ID")]
    private static partial Regex ProjectFolderPattern();
}