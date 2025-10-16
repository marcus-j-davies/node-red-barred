namespace Barred_Client;

public class Invitiation
{
    public string StackEndpoint { get; set; }
    public string Namespace { get; set; }
    public string StackVersion { get; set; }
    public string Department { get; set; }
    public string ClientID { get; set; }
    public string ClientLabel { get; set; }
    public Theme Theme { get; set; }
}

public class Theme
{
    public string Color { get; set; }
    public string LogoURL { get; set; }
}