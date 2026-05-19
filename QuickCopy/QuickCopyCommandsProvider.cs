// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace QuickCopy;

public partial class QuickCopyCommandsProvider : CommandProvider
{
    private static readonly IconInfo QuickCopyIcon = new("\uE8C8");

    private readonly ICommandItem[] _commands;

    public QuickCopyCommandsProvider()
    {
        DisplayName = "QuickCopy";
        Icon = QuickCopyIcon;
        _commands = [
            new CommandItem(new QuickCopyPage())
            {
                Title = "Open QuickCopy",
                Subtitle = "Browse snippets",
                Icon = QuickCopyIcon,
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}
