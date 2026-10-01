using System;
using System.Reflection;
using System.Runtime.Loader;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Core.Models.XAMLTheme;
using ClassIsland.LiquidGlass.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClassIsland.LiquidGlass;

/// <summary>ClassIsland 2.x plugin entry point for the adaptive liquid-glass theme.</summary>
public sealed class LiquidGlassPlugin : PluginBase
{
    private static readonly Assembly SelfAssembly = typeof(LiquidGlassPlugin).Assembly;

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        EnsureAssemblyResolvable();
        services.AddXamlTheme(
            new Uri("avares://ClassIsland.LiquidGlass/Themes/Styles.axaml"),
            new ThemeManifest
            {
                Id = "ceshi.liquidglass",
                Name = "液态玻璃",
                Description = "根据当前明暗主题自适应亮度与前景对比度的液态玻璃效果。",
                Author = "Ceshi",
                Version = "1.0.0",
                Url = "https://github.com/ClassIsland/ClassIsland",
            });
        services.AddSettingsPage<LiquidGlassSettingsPage>();
    }

    private static void EnsureAssemblyResolvable()
    {
        var selfName = SelfAssembly.GetName().Name;
        AssemblyLoadContext.Default.Resolving += (_, requested) =>
            requested.Name == selfName ? SelfAssembly : null;
    }
}
