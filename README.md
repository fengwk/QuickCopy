# QuickCopy

QuickCopy 是一个 Windows Command Palette 扩展，用来把一份 WSL 中维护的 JSON 片段列表暴露到 Command Palette 里，并在选中后复制到 Windows 剪贴板。

## What

- 在 Command Palette 中提供 `QuickCopy` 扩展入口
- 从 WSL 里的 `rofi-paste-list.json` 读取条目
- 支持两类条目：
  - `content`：静态文本，直接复制
  - `shell`：在 WSL 中执行命令，再复制结果
- 选中条目后立即关闭面板，复制动作在后台继续执行
- 默认隐藏开始菜单应用项，只作为 Command Palette 扩展使用

## Runtime behavior

当前实现依赖以下运行环境：

- Windows 11 + PowerToys Command Palette
- `wsl.exe`
- WSL 中可用的 `zsh`
- WSL 中的唯一真相源：
  - `~/prog/dotfiles/user/rofi/rofi-paste-list.json`

默认配置写在：

- `QuickCopy/Models/QuickCopySettings.cs`

其中关键路径为：

- `PasteListPath = ${HOME}/prog/dotfiles/user/rofi/rofi-paste-list.json`
- `LegacyScriptPath = /home/<user>/scripts/jsonpaste-win-wsl`

如果 `LegacyScriptPath` 存在，shell 条目优先复用这个已验证可用的后端脚本；否则直接由扩展自行调用 `wsl.exe zsh -lc ...`。

## Data format

JSON 顶层必须是数组，单项支持：

```json
[
  {
    "display": "ark api key",
    "shell": "echo $ARK_API_KEY"
  },
  {
    "display": "test uid",
    "content": "6549746527"
  }
]
```

字段说明：

- `display`：列表展示名称
- `content`：静态复制内容
- `shell`：在 WSL 中执行的命令，输出会被复制

## Build

已验证可构建环境：

- Windows 上的 `dotnet` SDK（`10.0.300` 可用）
- 目标框架：`net9.0-windows10.0.26100.0`

命令行构建：

```powershell
dotnet build .\QuickCopy.sln -c Debug -p:Platform=x64
```

或在 Visual Studio 中：

- 打开 `QuickCopy.sln`
- 选择 `Debug | x64`
- 执行 `生成 -> 打包 QuickCopy`

## Package / Deploy

1. 生成 MSIX 包

```powershell
dotnet build .\QuickCopy\QuickCopy.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true
```

输出目录通常为：

- `QuickCopy/AppPackages/QuickCopy_0.0.1.0_x64_Debug_Test/`

2. 使用本机证书库中的开发证书签名

```powershell
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe" sign /fd SHA256 /sha1 <thumbprint> /s My ".\QuickCopy_0.0.1.0_x64_Debug.msix"
```

3. 安装 / 更新扩展

```powershell
Get-AppxPackage *QuickCopy* | Remove-AppxPackage
Add-AppxPackage ".\QuickCopy_0.0.1.0_x64_Debug.msix"
```

4. 在 Command Palette 中执行：

- `Reload`
- `Reload Command Palette extensions`

## Notes

- `Package.appxmanifest` 中已设置 `AppListEntry="none"`，安装后不应在开始菜单中作为普通应用入口出现
- 顶层扩展名为 `QuickCopy`，实际打开列表的命令显示为 `Open QuickCopy`
- 静态条目优先直接复制；shell 条目才会触发 WSL 调用

## Repository hygiene

仓库会忽略以下本地产物：

- `.vs/`
- `bin/`, `obj/`
- `AppPackages/`
- 本地证书与签名文件
- Visual Studio 缓存与升级日志
