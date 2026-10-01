# ClassIsland 液态玻璃主题插件

本仓库已改为与 ClassIsland 2.x 插件相同的 **Avalonia + `manifest.yml`** 结构；不再使用先前错误的 WPF 控件项目。

## 给普通用户：取得可安装文件

本仓库的 GitHub Actions 工作流会生成唯一需要下载的文件：

```text
ClassIsland.LiquidGlass-1.0.0.cipx
```

在 GitHub 的 **Actions → Package ClassIsland Liquid Glass → 最新成功任务 → Artifacts** 下载它。然后在 ClassIsland 的插件管理页导入 `.cipx`，并在主题列表选择“液态玻璃”。无需解压、无需修改任何代码。

## 插件内容

- `manifest.yml` 使用 ClassIsland API `2.0.0.0`，并把 `ClassIsland.LiquidGlass.dll` 声明为入口程序集；
- `LiquidGlassPlugin` 注册 XAML 主题与设置页；
- `Themes/Styles.axaml` 使用 Avalonia `ThemeDictionaries`：浅色主题自动选择深色玻璃与深色文字，深色主题自动选择亮色玻璃与浅色文字；
- `.cipx` 打包时只包含插件本身及清单资源，避免重复携带 ClassIsland 与 Avalonia 的运行时程序集。

## 本地构建（仅开发者）

将 `ClassIsland.LiquidGlass` 放在 ClassIsland 源码仓库的 `Extensions` 下，再执行：

```powershell
dotnet build Extensions/LiquidGlass/ClassIsland.LiquidGlass.csproj -c Release
Compress-Archive -Path Extensions/LiquidGlass/bin/Release/net8.0/* -DestinationPath ClassIsland.LiquidGlass-1.0.0.zip
Rename-Item ClassIsland.LiquidGlass-1.0.0.zip ClassIsland.LiquidGlass-1.0.0.cipx
```
