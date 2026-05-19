using System;
using System.IO;

namespace QuickCopy.Models;

internal sealed class QuickCopySettings
{
    public string PasteListPath { get; init; } = "${HOME}/prog/dotfiles/user/rofi/rofi-paste-list.json";

    public string LegacyScriptPath { get; init; } = $"/home/{Environment.UserName}/scripts/jsonpaste-win-wsl";

    public string WslExecutable { get; init; } = DefaultWslExecutable();

    public string Win32YankPath { get; init; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "bin",
        "win32yank.exe");

    public string Distribution { get; init; } = string.Empty;

    public string ShellInit { get; init; } = "source ~/.zprofile >/dev/null 2>&1";

    private static string DefaultWslExecutable()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32",
            "wsl.exe");
    }
}
