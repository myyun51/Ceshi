# 液态玻璃 / Liquid Glass

这是一个 **ClassIsland 2.x 主题插件**。安装后，在 ClassIsland 的主题列表中选择“液态玻璃”即可启用；不需要编辑代码。

## 效果

- 深色主题：亮色半透明玻璃、白色高光边缘、浅色文字；
- 浅色主题：深色半透明玻璃、低调深色边缘、深色文字；
- 玻璃表面带有双色折射渐变和双层阴影，保持组件轮廓清晰。

这种根据实际明暗主题切换玻璃表面、边缘和文字对比度的方式，避免了固定白色磨砂层在浅色壁纸上发白或在深色壁纸上看不清文字的问题。

## 安装

1. 下载发布页中的 `ClassIsland.LiquidGlass-1.0.0.cipx` 文件。
2. 打开 ClassIsland 的插件管理页面，选择导入插件，并选择该文件。
3. 在 ClassIsland 的主题列表中选择“液态玻璃”。

## 给组件作者

在要使用玻璃样式的最外层 Avalonia `Border` 添加 `Classes="liquidGlass"` 即可；标题和正文分别添加 `liquidGlassTitle` 或 `liquidGlassText`。主题资源位于 `Themes/Styles.axaml`。
