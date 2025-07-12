using Microsoft.Extensions.DependencyInjection;
using SportVAR.Services;
using SportVAR.ViewModels;
using System.Windows;

namespace SportVAR;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<Window1>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainViewModel2>();
        services.AddSingleton<ViewModel3>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<Window1>();

        services.AddSingleton<IDispatcher>(new WpfDispatcher(Current.Dispatcher));
        services.AddTransient<ICameraService, CameraService>();
        services.AddTransient<IVideoRecorder>(provider =>
                                                      new VideoRecorder( 1280, 720));
        
        services.AddTransient<IPreviewService, PreviewService>();
        services.AddTransient<ICameraConfigurator, CameraConfigurator>();
        services.AddTransient<ICameraListService, CameraListService>();
        services.AddSingleton<Func<IVideoRecorder>>(provider => provider.GetRequiredService<IVideoRecorder>);
    }
}