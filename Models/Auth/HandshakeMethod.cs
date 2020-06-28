namespace PinkUmbrella.Models.Auth
{
    public enum HandshakeMethod
    {
        Default = -1,
        ManualCodeMachine = 0,
        ManualCodeHuman = 1,
        Link = 2,
        Email = 3,
        Text = 4,
        QRCode = 5,
        BlueTooth = 6,
        HttpRequest = 7,
    }
}