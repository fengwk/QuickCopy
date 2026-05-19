using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using QuickCopy.Models;
using QuickCopy.Services;

namespace QuickCopy.Commands;

internal sealed partial class CopyQuickCopyEntryCommand : InvokableCommand
{
    private readonly QuickCopyBackend _backend;
    private readonly int _index;
    private readonly QuickCopyEntry _entry;
    private readonly string _label;

    public CopyQuickCopyEntryCommand(QuickCopyBackend backend, int index, QuickCopyEntry entry, string label, bool hasShell)
    {
        _backend = backend;
        _index = index;
        _entry = entry;
        _label = label;

        Name = hasShell ? "Run and copy" : "Copy";
        Icon = new(hasShell ? "\uE756" : "\uE8C8");
    }

    public override CommandResult Invoke()
    {
        _ = Task.Run(() =>
        {
            try
            {
                _backend.CopyEntry(_index, _entry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"QuickCopy failed for {_label}: {ex}");
            }
        });

        return CommandResult.Dismiss();
    }
}
