namespace Barred_Client;
using CommunityToolkit.Maui.Views;

public partial class MainPage : ContentPage
{
    

    public MainPage()
    {
        InitializeComponent();
        
        if (Microsoft.Maui.Storage.Preferences.ContainsKey("Enrollment"))
        {
            MauiProgram._Enrollment = Newtonsoft.Json.JsonConvert.DeserializeObject<Invitiation>(Microsoft.Maui.Storage.Preferences.Get("Enrollment","{}"));
            Shell.Current.GoToAsync("Scanner");
        }
       
    }

    private async void StartEnrollemnt(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Enrol");
    }

   
}