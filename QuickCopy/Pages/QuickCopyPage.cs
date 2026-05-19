// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using QuickCopy.Commands;
using QuickCopy.Models;
using QuickCopy.Services;
using System;
using System.Linq;

namespace QuickCopy;

internal sealed partial class QuickCopyPage : DynamicListPage
{
    private static readonly IconInfo QuickCopyIcon = new("\uE8C8");

    private readonly QuickCopyBackend _backend = new();

    private QuickCopyEntry[] _entries = [];
    private IListItem[] _items = [];
    private string? _loadError;
    private string _searchText = string.Empty;
    private bool _loaded;

    public QuickCopyPage()
    {
        Icon = QuickCopyIcon;
        Title = "QuickCopy";
        Name = "Browse";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        _searchText = newSearch;
        RebuildItems();
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        RebuildItems();
        return _items;
    }

    private void RebuildItems()
    {
        EnsureEntriesLoaded();

        if (!string.IsNullOrWhiteSpace(_loadError))
        {
            _items = [
                new ListItem(new NoOpCommand())
                {
                    Title = "Failed to load QuickCopy entries",
                    Subtitle = _loadError,
                },
            ];
            return;
        }

        var filteredEntries = _entries
            .Select((entry, index) => (entry, index))
            .Where(item => Matches(item.entry, _searchText))
            .Select(item => CreateListItem(item.index, item.entry))
            .Cast<IListItem>()
            .ToArray();

        _items = filteredEntries.Length > 0
            ? filteredEntries
            : [
                new ListItem(new NoOpCommand())
                {
                    Title = "No matching entries",
                    Subtitle = "Try a different keyword",
                },
            ];
    }

    private void EnsureEntriesLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        try
        {
            _entries = _backend.LoadEntries().ToArray();
            _loadError = null;
        }
        catch (Exception ex)
        {
            _entries = [];
            _loadError = ex.Message;
        }
    }

    private static bool Matches(QuickCopyEntry entry, string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        var needle = searchText.Trim().ToLowerInvariant();
        var haystack = entry.SearchText.ToLowerInvariant();

        return haystack.Contains(needle, StringComparison.Ordinal)
            || NormalizeForMatch(haystack).Contains(NormalizeForMatch(needle), StringComparison.Ordinal);
    }

    private static string NormalizeForMatch(string value)
    {
        return string.Concat(value.Where(ch => !char.IsWhiteSpace(ch) && ch != '_' && ch != '-' && ch != '|'));
    }

    private ListItem CreateListItem(int index, QuickCopyEntry entry)
    {
        return new(new CopyQuickCopyEntryCommand(_backend, index, entry, entry.Label, entry.HasShell))
        {
            Title = entry.Label,
            Subtitle = entry.Subtitle,
            Icon = new IconInfo(entry.HasShell ? "\uE756" : "\uE8C8"),
        };
    }
}
