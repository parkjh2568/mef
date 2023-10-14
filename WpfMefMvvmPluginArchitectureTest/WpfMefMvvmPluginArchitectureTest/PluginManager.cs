﻿using WpfPackInterface;

namespace WpfMefMvvmPluginArchitectureTest;

/// <summary>
/// IServiceProvider를 이용하면 생성자 주입을 사용할 수 있지만, 새로 re-load 되는 plugin 들에 대해서 IServiceCollection을 계속해서 BuildServiceProvider 해줘야 한다.
/// 따라서 plugin에 대해서는 IServiceProvider를 사용하지 않고, IServiceCollection으로만 사용해서 GetService를 통해 필요한 서비스를 가져오도록 한다.
/// </summary>
public sealed class PluginManager
{
    public PluginManager()
    {
        string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

        if (!Directory.Exists(pluginPath))
            Directory.CreateDirectory(pluginPath);

        catalog = new DirectoryCatalog("Plugins");
        container = new CompositionContainer(catalog);
        container.ComposeParts(this);

        pluginServiceCollection = new ServiceCollection();
        pluginServiceCollection.AddSingleton(catalog);
        pluginServiceCollection.AddSingleton(container);
    }

    private IServiceCollection pluginServiceCollection;
    private readonly DirectoryCatalog catalog;
    private readonly CompositionContainer container;

    [ImportMany(typeof(IPlugin), AllowRecomposition = true)]
    private IEnumerable<IPlugin>? Plugins { get; } // ImportMany를 통해 동적으로 생성되는 IEnumerable 이기 때문에 set이 필요없다.

    public IEnumerable<IPlugin>? BuildPluginServiceProvider()
    {
        if (Plugins is null)
            return default;

        catalog.Refresh();

        foreach (IPlugin plugin in Plugins)
        {
            if (pluginServiceCollection.Any(s => s.ServiceType == typeof(IPlugin))) continue;
            plugin.RegisterServices(pluginServiceCollection);
        }

        return Plugins;
    }
}
