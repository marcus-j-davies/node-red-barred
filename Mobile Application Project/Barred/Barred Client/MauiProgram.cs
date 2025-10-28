using Microsoft.Extensions.Logging;
using BarcodeScanning;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio;

namespace Barred_Client;

public static class MauiProgram
{
    public static readonly string _RequiredStackVersion = "^1.0.0";
    public static Invitiation _Enrollment;

    public static Color ThemeColor => Color.FromArgb(_Enrollment.Theme.Color);
    public static string Group => _Enrollment.Group;
    public static string UserLabel => _Enrollment.ClientLabel;
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeScanning()
            .AddAudio()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        Routing.RegisterRoute("Enrol",typeof(Enrol));
        Routing.RegisterRoute("Scanner",typeof(Scanner));

        return builder.Build();
    }
}