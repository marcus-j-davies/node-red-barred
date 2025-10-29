namespace Barred_Client;

public class MenuOption
{
    public string action { get; set; }
    public bool scan { get; set; }
    public bool destructive { get; set; }
    public object context { get; set; }
}