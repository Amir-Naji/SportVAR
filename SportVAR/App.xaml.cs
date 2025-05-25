using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SportVAR.Services;

namespace SportVAR;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDispatcher>(new WpfDispatcher(Current.Dispatcher));
        services.AddTransient<ICameraService, CameraService>();
        services.AddTransient<IVideoRecorder>(provider =>
                                                      new VideoRecorder( 1280, 720));
        
        services.AddTransient<IPreviewService, PreviewService>();
        services.AddTransient<ICameraListService, CameraListService>();
        services.AddSingleton<Func<IVideoRecorder>>(provider => provider.GetRequiredService<IVideoRecorder>);
        services.AddSingleton<MainWindow>();
    }
}