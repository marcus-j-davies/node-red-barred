using System.Globalization;
using BarcodeScanning;
using Plugin.Maui.Audio;
using SocketIOClient;
using CommunityToolkit.Maui.Views;


namespace Barred_Client;



public partial class Scanner : ContentPage
{
    private SocketIOClient.SocketIO SOK;
    private IAudioManager AM;
    private IAudioPlayer AM_OK;
    private IAudioPlayer AM_ERROR;
    private IAudioPlayer AM_PROMPT;
    private TaskCompletionSource<OnDetectionFinishedEventArg>? _scanWaiter;

    public Scanner(IAudioManager AudioManager)
    {
        InitializeComponent();
        AM = AudioManager;
        SetupAudio();
        SetupConnection();
    }
    
    protected override void OnDisappearing()
    {
        ScannerEl.CameraEnabled = false;
        ScannerEl.PauseScanning = true;
        SOK.DisconnectAsync();
        SOK.Dispose();
        SOK = null;
        base.OnDisappearing();
    }

    private async void SetupAudio()
    {
        AM_OK = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("OK.mp3"));
        AM_ERROR = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ERROR.mp3"));
        AM_PROMPT = AM.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("PROMPT.mp3"));
    }

    private void PlayAudio(IAudioPlayer Player)
    {
        if (AudioSwitch.IsToggled)
        {
            Player.Play();
        }
    }

    private void EnableCamera(bool enable)
    {
        MainThread.BeginInvokeOnMainThread(() => { ScannerEl.CameraEnabled = enable; });
    }

    private void RenderPayload(object Obj)
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

    private async void SetupConnection()
    {
        RenderPayload("Connecting to Stack...");
        SocketIOOptions Ops = new SocketIOOptions();
        Ops.Path = MauiProgram._Enrollment.Namespace;
        Ops.Reconnection = true;
        Ops.ReconnectionAttempts = 30;
        Ops.ReconnectionDelay = 2000;
        Dictionary<string, object> Auth = new Dictionary<string, object>
        {
            { "id", MauiProgram._Enrollment.ClientID },
        };
        Ops.Auth = Auth;

        SOK = new SocketIOClient.SocketIO(MauiProgram._Enrollment.StackEndpoint, Ops);
        SOK.OnError += (sender, s) =>
        {
            PlayAudio(AM_ERROR);
            RenderPayload($"ERROR: {s}");
        };
        SOK.OnDisconnected += (sender, s) =>
        {
            PlayAudio(AM_ERROR);
            RenderPayload("Lost connection to the Stack");
        };
        SOK.OnConnected += (sender, args) =>
        {
            PlayAudio(AM_OK);
            RenderPayload("Scanner Ready!");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                IMG_Icon.Source = new UriImageSource
                {
                    Uri = new Uri(MauiProgram._Enrollment.Theme.IconURL),
                    CachingEnabled = false
                };
            });
        };
        SOK.On("BARRED.Item", (E) =>
        {
            PlayAudio(AM_PROMPT);

            Item I = E.GetValue<Item>();
            switch (I.payloadType)
            {
                case "object":
                    I.payload =  Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(I.payload.ToString());
                    break;

                default:
                    I.payload = I.payload.ToString();
                    break;
            }

            RenderPayload(I.payload);
        });

        await SOK.ConnectAsync(CancellationToken.None);
    }

    private IView GetControl(object value, string key)
    {
        
        switch (value)
        {
            case "string":
            case "number":
                Entry En = new Entry();
                En.BindingContext = key;
                En.BackgroundColor = Colors.LightGrey;
                En.PlaceholderColor = Colors.Gray;
                En.Keyboard = value.Equals("number") ? Keyboard.Numeric : Keyboard.Text;
                return En;
            
            case "ml_string":
                Editor Ed = new Editor();
                Ed.BindingContext = key;
                Ed.BackgroundColor = Colors.LightGrey;
                Ed.PlaceholderColor = Colors.Gray;
                Ed.Keyboard = Keyboard.Text;
                return Ed;
            
            case "date":
                DatePicker Dp = new DatePicker();
                Dp.BindingContext = key;
                Dp.BackgroundColor = Colors.LightGrey;
                return Dp;
            
            case "time":
                TimePicker Tm = new TimePicker();
                Tm.BindingContext = key;
                Tm.BackgroundColor = Colors.LightGrey;
                return Tm;
            
            default:
                Entry De = new Entry();
                De.IsReadOnly = true;
                De.Text = value.ToString();
                De.BindingContext = key;
                De.BackgroundColor = Colors.LightGrey;
                De.PlaceholderColor = Colors.Gray;
                De.Keyboard = Int64.TryParse(value.ToString(), out _) ? Keyboard.Numeric : Keyboard.Text;
                return De;
        }
    }

    private async Task HandleInfoResponseAsync(Dictionary<string, object> layout)
    {
        EnableCamera(false);
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            Information I = new Information();

            foreach (string key in layout.Keys)
            {
                object value = layout[key];
                
                Label L = new Label();
                L.Text = key;

                I._ContentPH.Add(new VerticalStackLayout
                {
                    Spacing = 8.0,
                    Children = {L, GetControl(value, key) }
                });
            }

            var popupResult = await this.ShowPopupAsync(I);
            if (!((bool)popupResult))
            {
                EnableCamera(true);
                return;
            }

            List<Entry> entries = I._ContentPH.GetDescendantsOfType<Entry>().ToList();
            List<Editor> editors = I._ContentPH.GetDescendantsOfType<Editor>().ToList();
            List<DatePicker> datePickers = I._ContentPH.GetDescendantsOfType<DatePicker>().ToList();
            List<TimePicker> timePickers = I._ContentPH.GetDescendantsOfType<TimePicker>().ToList();

            Dictionary<string, object> itemPayload = new Dictionary<string, object>();
            long unixTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            itemPayload.Add("timestamp", unixTimeMillis);

            Dictionary<string, object> scanner = new Dictionary<string, object>
            {
                { "id", MauiProgram._Enrollment.ClientID },
                { "name", MauiProgram._Enrollment.ClientLabel },
                { "appVersion", AppInfo.Current.Version.ToString() }
            };
            itemPayload.Add("scanner", scanner);

            Dictionary<string, object> item = new Dictionary<string, object>();
            foreach (Entry e in entries)
            {
                if (e.BindingContext != null && !string.IsNullOrEmpty(e.Text))
                {
                    if(e.Keyboard == Keyboard.Numeric) item.Add(e.BindingContext.ToString(), Int64.Parse(e.Text));
                    if(e.Keyboard == Keyboard.Text) item.Add(e.BindingContext.ToString(), e.Text);
                }
            }

            foreach (Editor ed in editors)
            {
                if (ed.BindingContext != null && !string.IsNullOrEmpty(ed.Text)) item.Add(ed.BindingContext.ToString(), ed.Text);
            }
            
            foreach (DatePicker dp in datePickers)
            {
                if (dp.BindingContext != null) item.Add(dp.BindingContext.ToString(),new DateTimeOffset(dp.Date.ToUniversalTime()).ToUnixTimeMilliseconds());
            }
            
            foreach (TimePicker tm in timePickers)
            {
                if (tm.BindingContext != null) item.Add(tm.BindingContext.ToString(),tm.Time.ToString());
            }

            itemPayload.Add("item", item);

            await SOK.EmitAsync("BARRED.Item", itemPayload);
            EnableCamera(true);
            RenderPayload($"Item Data Sent submitted.");
        });
    }

    private void HandleRootStackResponse(SocketIOResponse response)
    {
        BarcodeResponse Res = response.GetValue<BarcodeResponse>(0);
        string Status = Res.status;
        string PayloadType = Res.payloadType;
        string Title = Res.title;
        object Payload = Res.payload;

        if (Status == "ERROR") PlayAudio(AM_ERROR);
        else if (Status == "OK") PlayAudio(AM_OK);
        else if (Status == "INFO") PlayAudio(AM_PROMPT);
        else if (Status == "MENU") PlayAudio(AM_PROMPT);

        if (Status == "INFO")
        {
            Dictionary<string, object> Layout = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Payload.ToString());
            _ = HandleInfoResponseAsync(Layout);
        }
        else if (Status == "MENU")
        {
            Dictionary<string, MenuOption> MenuCollection = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, MenuOption>>(Payload.ToString());

            Menu M = new Menu(Title);
            foreach (string key in MenuCollection.Keys)
            {
                Button BT = new Button();
                BT.Text = key;
                BT.WidthRequest = 250;
                BT.BindingContext = MenuCollection[key];
                BT.Clicked += (sender, e) =>
                {
                    M.CloseAsync().ContinueWith((t) =>
                    {
                        Button_Menu(sender, e);
                    });
                    
                    
                };
                BT.BackgroundColor = MenuCollection[key].destructive ? Colors.Red : MauiProgram.ThemeColor;
                BT.Text = key;
                
                M._ContentPH.Add(new VerticalStackLayout
                {
                    Spacing = 8.0,
                    Children = {BT}
                });
            }

            MainThread.InvokeOnMainThreadAsync(() => { this.ShowPopupAsync(M); });
        }
        else
        {
            switch (PayloadType)
            {
                case "object":
                    Payload = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Payload.ToString());
                    break;
                default:
                    Payload = Payload.ToString();
                    break;
            }

            RenderPayload(Payload);
        }
    }
    
    private void ScannerEl_OnOnDetectionFinished(object? sender, OnDetectionFinishedEventArg e)
    {
        if (SOK.Connected)
        {
            if (e.BarcodeResults.Any())
            {
                ScannerEl.PauseScanning = true;

                if (_scanWaiter != null)
                {
                    _scanWaiter.TrySetResult(e);
                    _scanWaiter = null;
                    new Task(() =>
                    {
                        Thread.Sleep(MauiProgram._Enrollment.ScanRate);
                        ScannerEl.PauseScanning = false;
                    }).Start();
                    return;
                }

                RenderPayload("Sending...");

                Dictionary<string, object> Payload = new Dictionary<string, object>();
                long unixTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                Payload.Add("timestamp", unixTimeMillis);

                Dictionary<string, object> Barcode = new Dictionary<string, object>
                {
                    { "barcode", e.BarcodeResults.First().RawValue },
                    { "symbology", e.BarcodeResults.First().BarcodeFormat.ToString() }
                };
                Payload.Add("barcode", Barcode);

                Dictionary<string, object> Scanner = new Dictionary<string, object>
                {
                    { "id", MauiProgram._Enrollment.ClientID },
                    { "name", MauiProgram._Enrollment.ClientLabel },
                    { "appVersion", AppInfo.Current.Version.ToString() }
                };
                Payload.Add("scanner", Scanner);

                Action<SocketIOResponse> Callback = (response) => HandleRootStackResponse(response);
                SOK.EmitAsync("BARRED.Barcode", Callback, Payload).ContinueWith((t) => { RenderPayload($"Sent: {e.BarcodeResults.First().RawValue}"); });

                new Task(() =>
                {
                    Thread.Sleep(MauiProgram._Enrollment.ScanRate);
                    ScannerEl.PauseScanning = false;
                }).Start();
            }
        }
    }

    private async void Button_Menu(object? sender, EventArgs e)
    {
        if (SOK.Connected)
        {
            OnDetectionFinishedEventArg? scanResult = null;
            MenuOption MO = (MenuOption)((Button)sender).BindingContext;

            if (MO.scan)
            {
                PlayAudio(AM_PROMPT);
                RenderPayload($"Action requires a scan: please perform item scan...");
                _scanWaiter = new TaskCompletionSource<OnDetectionFinishedEventArg>();
                scanResult = await _scanWaiter.Task;
                _scanWaiter = null;
            }

            Dictionary<string, object> Payload = new Dictionary<string, object>();
            long unixTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Payload.Add("timestamp", unixTimeMillis);

            Dictionary<string, object> Action = new Dictionary<string, object>
            {
                { "name", MO.action},
                { "context", MO.context ?? null},
            };
            if (scanResult != null)
            {
                Action.Add("barcode", scanResult.BarcodeResults.First().RawValue);
            }

            Payload.Add("action", Action);

            Dictionary<string, object> scanner = new Dictionary<string, object>
            {
                { "id", MauiProgram._Enrollment.ClientID },
                { "name", MauiProgram._Enrollment.ClientLabel },
                { "appVersion", AppInfo.Current.Version.ToString() }
            };
            Payload.Add("scanner", scanner);

            Action<SocketIOResponse> Callback = (response) => HandleRootStackResponse(response);
            SOK.EmitAsync("BARRED.Action", Callback, Payload).ContinueWith((t) => { RenderPayload($"Action Sent"); });
        }

    }

    private async void Button_Delete(object? sender, EventArgs e)
    {
        bool Yes = await DisplayAlert("Delete Registration?", "Are you sure you wish to delete the registration? Doing so will put the scanner in a state of 'Un-enrolled'", "Yes", "No");
        if (Yes)
        {
            Microsoft.Maui.Storage.Preferences.Remove("Enrollment");
            await Shell.Current.Navigation.PopToRootAsync();
        }
    }
}