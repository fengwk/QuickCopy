using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using QuickCopy.Models;

namespace QuickCopy.Services;

internal sealed class QuickCopyBackend
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly UnicodeEncoding Utf16LeWithoutBom = new(false, false);

    private readonly QuickCopySettings _settings = new();
    private bool? _legacyScriptExists;

    public IReadOnlyList<QuickCopyEntry> LoadEntries()
    {
        EnsureSettings();
        var payload = RunWslCaptureOutput($"cat \"{_settings.PasteListPath}\"");
        var entries = JsonSerializer.Deserialize(
            payload,
            QuickCopyJsonSerializerContext.Default.QuickCopyEntryArray);
        return entries ?? [];
    }

    public void CopyEntry(int index, QuickCopyEntry entry)
    {
        EnsureSettings();

        if (!string.IsNullOrWhiteSpace(entry.Content))
        {
            CopyToClipboard(entry.Content);
            return;
        }

        if (string.IsNullOrWhiteSpace(entry.Shell))
        {
            CopyToClipboard(entry.Display ?? string.Empty);
            return;
        }

        if (LegacyScriptExists())
        {
            RunWslWithoutOutput($"\"{_settings.LegacyScriptPath}\" copy {index}");
            return;
        }

        var content = ResolveContent(entry);
        CopyToClipboard(content);
    }

    private string ResolveContent(QuickCopyEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.Content))
        {
            return entry.Content;
        }

        if (!string.IsNullOrWhiteSpace(entry.Shell))
        {
            return RunWslCaptureOutput(entry.Shell).TrimEnd('\r', '\n');
        }

        return entry.Display ?? string.Empty;
    }

    private void EnsureSettings()
    {
        if (!File.Exists(_settings.WslExecutable))
        {
            throw new FileNotFoundException("wsl.exe was not found.", _settings.WslExecutable);
        }
    }

    private bool LegacyScriptExists()
    {
        if (_legacyScriptExists.HasValue)
        {
            return _legacyScriptExists.Value;
        }

        try
        {
            RunWslWithoutOutput($"test -f \"{_settings.LegacyScriptPath}\"");
            _legacyScriptExists = true;
        }
        catch
        {
            _legacyScriptExists = false;
        }

        return _legacyScriptExists.Value;
    }

    private string RunWslCaptureOutput(string command)
    {
        var wrappedCommand = string.IsNullOrWhiteSpace(_settings.ShellInit)
            ? command
            : $"{_settings.ShellInit}; {command}";

        ProcessStartInfo startInfo = new()
        {
            FileName = _settings.WslExecutable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
        };

        if (!string.IsNullOrWhiteSpace(_settings.Distribution))
        {
            startInfo.ArgumentList.Add("-d");
            startInfo.ArgumentList.Add(_settings.Distribution);
        }

        startInfo.ArgumentList.Add("zsh");
        startInfo.ArgumentList.Add("-lc");
        startInfo.ArgumentList.Add(wrappedCommand);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start wsl.exe.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(stderr)
                    ? "WSL command failed."
                    : stderr.Trim());
        }

        return stdout;
    }

    private void RunWslWithoutOutput(string command)
    {
        _ = RunWslCaptureOutput(command);
    }

    private static void CopyToClipboard(string content)
    {
        if (TryCopyToWin32Yank(content))
        {
            return;
        }

        var clipPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "clip.exe");

        ProcessStartInfo startInfo = new()
        {
            FileName = clipPath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            StandardInputEncoding = Utf16LeWithoutBom,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start clip.exe.");

        process.StandardInput.Write(content);
        process.StandardInput.Close();

        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(stderr)
                    ? "clip.exe failed."
                    : stderr.Trim());
        }
    }

    private static bool TryCopyToWin32Yank(string content)
    {
        var yankPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "bin",
            "win32yank.exe");

        if (!File.Exists(yankPath))
        {
            return false;
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = yankPath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            StandardInputEncoding = Utf8WithoutBom,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("-i");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start win32yank.exe.");

        process.StandardInput.Write(content);
        process.StandardInput.Close();

        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(stderr)
                    ? "win32yank.exe failed."
                    : stderr.Trim());
        }

        return true;
    }
}
