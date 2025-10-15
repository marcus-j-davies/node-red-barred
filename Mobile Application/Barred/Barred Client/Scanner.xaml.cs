using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarcodeScanning;
using Plugin.Maui.Audio;

namespace Barred_Client;

public partial class Scanner : ContentPage
{
    private static SocketIOClient.SocketIO SOK;
    private static IAudioManager AM;
    private static IAudioPlayer AM_OK;
    private static IAudioPlayer AM_ERROR;
    private static IAudioPlayer AM_PROMPT;
    private static IAudioPlayer AM_WAIT;
    
    protected override void OnDisappearing()
    {
        ScannerEl.CameraEnabled = false;
        ScannerEl.PauseScanning = true;
        base.OnDisappearing();
    }
    
    private void Log(params object[]  Log)
    {
        Status.Text = $"[ {DateTime.Now.ToString("dd.MM.yyyy HH:mm")} ]{Environment.NewLine}{Environment.NewLine}";

        foreach (object V in Log)
        {
            Status.Text += $"{V}{Environment.NewLine}";
        }

    }
    
    public Scanner(IAudioManager AudioManager)
    {
        InitializeComponent();
        AM = AudioManager;
        SetupAudio();
        SetupConnection();
       
    }

    private async void SetupConnection()
    {
        Log("Connecting to Stack...");
        SOK = new SocketIOClient.SocketIO(MauiProgram._Enrollment.StackEndpoint);

        SOK.OnDisconnected += (sender, s) => { Log($"Lost connection to the Stack"); };
        SOK.OnConnected += (sender, args) =>
        {
            Log("Validating client...");
            SOK.EmitAsync("ClientAuth", MauiProgram._Enrollment.ClientID);
        };

        await SOK.ConnectAsync(CancellationToken.None);
    }

    private async void SetupAudio()
    {
        AM_OK = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("OK.mp3"));
        AM_ERROR = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ERROR.mp3"));
        AM_PROMPT = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("PROMPT.mp3"));
        //AM_WAIT = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("WAIT.mp3"));
        
    }

    private void ScannerEl_OnOnDetectionFinished(object? sender, OnDetectionFinishedEventArg e)
    {
        
        if(e.BarcodeResults.Any())
        {
            ScannerEl.PauseScanning = true;
            
            if (AudioSwitch.IsToggled)
            {
                AM_OK.Play();
            }
            
            Log($"Symbology: {e.BarcodeResults.First().BarcodeFormat.ToString()}",$"Value: {e.BarcodeResults.First().RawValue}");
            
            new Task(() =>
            {
                Thread.Sleep(500);
                ScannerEl.PauseScanning = false;
            }).Start();
        }
    }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        bool Yes = await DisplayAlert("Delete Registration?", "Are you sure, you wish to delete the registration? Doing so will put the scanner in a state of 'Un-enrolled'", "Yes", "No");
        if (Yes)
        {
            Microsoft.Maui.Storage.Preferences.Remove("Enrollment");
            await Shell.Current.Navigation.PopToRootAsync();
        }
    }
}