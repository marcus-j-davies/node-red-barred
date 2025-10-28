using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barred_Client;

public partial class Menu : CommunityToolkit.Maui.Views.Popup
{
    public VerticalStackLayout _ContentPH;

    
    public Menu(string MenuTitle)
    {
        InitializeComponent();
        _ContentPH = ContentPH;
        Title.Text =  MenuTitle;
    }
    
    private void Button_Cancel(object? sender, EventArgs e)
    {
        Close(false);
    }
}