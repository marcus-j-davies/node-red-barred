namespace Barred_Client;

public class BarcodeResponse
{
    public string status { get; set; }
    public object payload { get; set; }
    public string payloadType { get; set; }
    public string barcode { get; set; }
    public string title { get; set; }
}