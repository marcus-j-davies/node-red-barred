using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using BarcodeScanning;
using Semver;
using Plugin.Maui.Audio;

namespace Barred_Client;

public partial class Enrol : ContentPage
{
    
    public Enrol()
    {
        InitializeComponent();
    }

    protected override void OnDisappearing()
    {
        Scanner.CameraEnabled = false;
        Scanner.PauseScanning = true;
        base.OnDisappearing();
    }

    private async void Backup(object? sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopAsync();
    }

    private async void CameraView_OnOnDetectionFinished(object? sender, OnDetectionFinishedEventArg e)
    {
        if (e.BarcodeResults.Any())
        {
            Scanner.PauseScanning = true;
            try
            {
                byte[] CompressedBytes = Convert.FromBase64String(e.BarcodeResults.First().RawValue);
                using (MemoryStream compressedStream = new MemoryStream(CompressedBytes))
                {
                    using (GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    {
                        using (MemoryStream resultStream = new MemoryStream())
                        {
                            gzipStream.CopyTo(resultStream);
                            string JSONs  = System.Text.Encoding.UTF8.GetString(resultStream.ToArray());
                            Invitiation I = Newtonsoft.Json.JsonConvert.DeserializeObject<Invitiation>(JSONs);

                            if (I != null)
                            {
                                if (!Semver.SemVersion.Parse(I.StackVersion).SatisfiesNpm(MauiProgram._RequiredStackVersion))
                                {
                                    await DisplayAlert("Error", "Sorry, The BARRED stack version is not supported in this Client version.", "OK");
                                    Scanner.PauseScanning = false;
                                }
                                else
                                {
                                    Microsoft.Maui.Storage.Preferences.Set("Enrollment",JSONs);
                                    MauiProgram._Enrollment = I;
                                    await Shell.Current.GoToAsync("Scanner");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Error", "Sorry, The scanned QR code, does not seem to be an Invitiation.", "OK");
                                Scanner.PauseScanning = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Sorry, The scanned QR code, does not seem to be an Invitiation.", "OK");
                Scanner.PauseScanning = false;
            }

        }
       
    }
}