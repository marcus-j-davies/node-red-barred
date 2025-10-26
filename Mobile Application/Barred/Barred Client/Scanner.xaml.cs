using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BarcodeScanning;
using Plugin.Maui.Audio;
using SocketIOClient;
using CommunityToolkit.Maui.Views;


namespace Barred_Client;

public partial class Scanner : ContentPage
{
    private static SocketIOClient.SocketIO SOK;
    private static IAudioManager AM;
    private static IAudioPlayer AM_OK;
    private static IAudioPlayer AM_ERROR;
    private static IAudioPlayer AM_PROMPT;

    protected override void OnDisappearing()
    {
        ScannerEl.CameraEnabled = false;
        ScannerEl.PauseScanning = true;
        base.OnDisappearing();
    }

    private void EnableCamera(bool enable)
    {
        MainThread.BeginInvokeOnMainThread(() => { ScannerEl.CameraEnabled = enable; });
    }

    private void ProcessResult(object Obj)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Status.FormattedText.Spans.Clear();
            Status.FormattedText.Spans.Add(new Span
            {
                FontAttributes = FontAttributes.Bold,
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
            });
            Status.FormattedText.Spans.Add(new Span
            {
                Text = "\n--------------------------\n\n"
            });

            if (typeof(string) == Obj.GetType())
            {
                Status.FormattedText.Spans.Add(new Span
                {
                    Text = Obj.ToString()
                });
            }
            else
            {
                Dictionary<string, object> Props = (Dictionary<string, object>)Obj;
                int Pad = Props.Keys.Max((K) => K.Length) + 2;
                foreach (string KEY in Props.Keys)
                {
                    Status.FormattedText.Spans.Add(new Span
                    {
                        FontFamily = "Courier New",
                        FontAttributes = FontAttributes.Bold,
                        Text = $"{KEY}: ".PadRight(Pad, ' ')
                    });
                    Status.FormattedText.Spans.Add(new Span
                    {
                        FontFamily = "Courier New",
                        Text = Props[KEY].ToString()
                    });
                    Status.FormattedText.Spans.Add(new Span
                    {
                        Text = "\n"
                    });
                }
            }
        });
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
        ProcessResult("Connecting to Stack...");
        SocketIOOptions Ops = new SocketIOOptions();
        Ops.Path = MauiProgram._Enrollment.Namespace;
        Ops.Reconnection = true;
        Ops.ReconnectionAttempts = 30;
        Ops.ReconnectionDelay = 2000;
        Dictionary<string, object> Auth = new Dictionary<string, object>();
        Auth.Add("id", MauiProgram._Enrollment.ClientID);
        Ops.Auth = Auth;

        SOK = new SocketIOClient.SocketIO(MauiProgram._Enrollment.StackEndpoint, Ops);
        SOK.OnError += (sender, s) =>
        {
            AM_ERROR.Play();
            ProcessResult($"ERROR: {s}");
        };
        SOK.OnDisconnected += (sender, s) =>
        {
            AM_ERROR.Play();
            ProcessResult("Lost connection to the Stack");
        };
        SOK.OnConnected += (sender, args) =>
        {
            AM_OK.Play();
            ProcessResult("Scanner Ready!");
        };
        SOK.On("BARRED.Prompt", (E) =>
        {
            AM_PROMPT.Play();
            Prompt IN = E.GetValue<Prompt>();

            switch (IN.payloadType)
            {
                case "object":
                    IN.payload =
                        Newtonsoft.Json.JsonConvert
                            .DeserializeObject<Dictionary<string, object>>(IN.payload.ToString());
                    break;

                default:
                    IN.payload = IN.payload.ToString();
                    break;
            }

            ProcessResult(IN.payload);
        });

        await SOK.ConnectAsync(CancellationToken.None);
    }

    private async void SetupAudio()
    {
        AM_OK = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("OK.mp3"));
        AM_ERROR = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ERROR.mp3"));
        AM_PROMPT = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("PROMPT.mp3"));
    }

    private void ScannerEl_OnOnDetectionFinished(object? sender, OnDetectionFinishedEventArg e)
    {
        if (SOK.Connected)
        {
            if (e.BarcodeResults.Any())
            {
                ScannerEl.PauseScanning = true;

                ProcessResult("Sending...");

                Dictionary<string, object> Payload = new Dictionary<string, object>();
                long unixTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                Payload.Add("timestamp", unixTimeMillis);

                Dictionary<string, object> Barcode = new Dictionary<string, object>();
                Barcode.Add("barcode", e.BarcodeResults.First().RawValue);
                Barcode.Add("symbology", e.BarcodeResults.First().BarcodeFormat.ToString());
                Payload.Add("barcode", Barcode);

                Dictionary<string, object> Scanner = new Dictionary<string, object>();
                Scanner.Add("id", MauiProgram._Enrollment.ClientID);
                Scanner.Add("name", MauiProgram._Enrollment.ClientLabel);
                Scanner.Add("appVersion", AppInfo.Current.Version.ToString());
                Payload.Add("scanner", Scanner);

                Action<SocketIOResponse> Callback = async (response) =>
                {
                    BarcodeResponse Res = response.GetValue<BarcodeResponse>(0);
                    string Status = Res.status;
                    string PayloadType = Res.payloadType;
                    object Payload = Res.payload;

                    switch (PayloadType)
                    {
                        case "object":
                            Payload =
                                Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                    Payload.ToString());
                            break;

                        default:
                            Payload = Payload.ToString();
                            break;
                    }

                    if (AudioSwitch.IsToggled)
                    {
                        if (Status == "ERROR") AM_ERROR.Play();
                        else if (Status == "OK") AM_OK.Play();
                        else if (Status == "CREATE") AM_PROMPT.Play();
                    }

                    if (Status == "CREATE")
                    {
                        EnableCamera(false);
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            Create C = new Create();
                            Dictionary<string, object> Layout = (Dictionary<string, object>)Payload;
                            foreach (string KEY in Layout.Keys)
                            {
                                if (Layout[KEY].Equals("string") || Layout[KEY].Equals("number"))
                                {
                                    C._ContentPH.Add(new VerticalStackLayout
                                    {
                                        Spacing = 8.0,
                                        Children =
                                        {
                                            new Entry
                                            {
                                                IsReadOnly = KEY.StartsWith("_"),
                                                ClassId = KEY.StartsWith("_") ? KEY.Substring(1) : KEY,
                                                Text = KEY.StartsWith("_") ? e.BarcodeResults.First().RawValue : "",
                                                BackgroundColor = Colors.LightGrey, PlaceholderColor = Colors.Gray,
                                                Placeholder = KEY,
                                                Keyboard = Layout[KEY].Equals("number")
                                                    ? Keyboard.Numeric
                                                    : Keyboard.Text
                                            },
                                        }
                                    });
                                }
                                else if (Layout[KEY].Equals("ml_string"))
                                {
                                    C._ContentPH.Add(new VerticalStackLayout
                                    {
                                        Spacing = 8.0,
                                        Children =
                                        {
                                            new Editor
                                            {
                                                IsReadOnly = KEY.StartsWith("_"),
                                                ClassId = KEY.StartsWith("_") ? KEY.Substring(1) : KEY,
                                                Text = KEY.StartsWith("_") ? e.BarcodeResults.First().RawValue : "",
                                                BackgroundColor = Colors.LightGrey, PlaceholderColor = Colors.Gray,
                                                Placeholder = KEY,
                                                Keyboard = Keyboard.Text
                                            },
                                        }
                                    });
                                }

                            }

                            var R = await this.ShowPopupAsync(C);
                            if (!((bool)R))
                            {
                                EnableCamera(true);
                                return;
                            }

                            List<Entry> Entries = C._ContentPH.GetDescendantsOfType<Entry>().ToList();
                            List<Editor> Editors = C._ContentPH.GetDescendantsOfType<Editor>().ToList();

                            Dictionary<string, object> ItemPL = new Dictionary<string, object>();
                            long unixTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            ItemPL.Add("timestamp", unixTimeMillis);

                            Dictionary<string, object> Scanner = new Dictionary<string, object>();
                            Scanner.Add("id", MauiProgram._Enrollment.ClientID);
                            Scanner.Add("name", MauiProgram._Enrollment.ClientLabel);
                            Scanner.Add("appVersion", AppInfo.Current.Version.ToString());
                            ItemPL.Add("scanner", Scanner);

                            Dictionary<string, object> Item = new Dictionary<string, object>();
                            foreach (Entry E in Entries)
                            {
                                if (E.ClassId != null && E.Text.Length > 0)
                                {
                                    Item.Add(E.ClassId, E.Keyboard == Keyboard.Numeric ? int.Parse(E.Text) : E.Text);
                                }
                            }

                            foreach (Editor E in Editors)
                            {
                                if (E.ClassId != null && E.Text.Length > 0)
                                {
                                    Item.Add(E.ClassId, E.Text);
                                }
                            }

                            ItemPL.Add("item", Item);

                            SOK.EmitAsync("BARRED.Item", ItemPL).ContinueWith((t) =>
                            {
                                EnableCamera(true);
                                ProcessResult($"Item Data Sent for Barcode, scan again to confirm if required.");
                            });
                        });
                    }
                    else
                    {
                        ProcessResult(Payload);
                    }
                };

                SOK.EmitAsync("BARRED.Barcode", Callback, Payload).ContinueWith((t) =>
                {
                    ProcessResult($"Sent: {e.BarcodeResults.First().RawValue}");
                });

                new Task(() =>
                {
                    Thread.Sleep(MauiProgram._Enrollment.ScanRate);
                    ScannerEl.PauseScanning = false;
                }).Start();
            }
        }
    }


    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        bool Yes = await DisplayAlert("Delete Registration?",
            "Are you sure, you wish to delete the registration? Doing so will put the scanner in a state of 'Un-enrolled'",
            "Yes", "No");
        if (Yes)
        {
            Microsoft.Maui.Storage.Preferences.Remove("Enrollment");
            await Shell.Current.Navigation.PopToRootAsync();
        }
    }
}