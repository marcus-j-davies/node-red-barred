using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barred_Client;

public partial class Create : CommunityToolkit.Maui.Views.Popup
{
    public VerticalStackLayout _ContentPH;
    public Create()
    {
        InitializeComponent();
        _ContentPH = ContentPH;
    }

    private void Button_Submit(object? sender, EventArgs e)
    {
        Close(true);
    }

    private void Button_Cancel(object? sender, EventArgs e)
    {
        Close(false);
    }
}


