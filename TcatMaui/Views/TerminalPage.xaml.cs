using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using InTheHand.Bluetooth;
using TcatMaui.Models;
using static TcatMaui.Models.TcatTlv;
using System.Data;

namespace TcatMaui.Views;

[QueryProperty(nameof(SelectedDevice), "SelectedDevice")]
public partial class TerminalPage : ContentPage
{
    BleStream bleStream = null;
    SslStream sslStream = null;
    TlvStreamWatcher tlvStreamWatcher = null;
    BluetoothDevice selectedDevice;

    static X509Certificate2 theCaCert = null;
    static X509Certificate2 theInstallerCert = null;

        
    public BluetoothDevice SelectedDevice
    {
        get => selectedDevice;
        set
        {
            selectedDevice = value;
            OnPropertyChanged();
        }
    }

    public TerminalPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private X509Certificate OnLocalCertificateSelection(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
    {
         return theInstallerCert;
    }

    public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool result = false;
        X509Certificate2 theDeviceCert = new X509Certificate2(certificate);
        // X509Chain theCaChain = null;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            edtTerminal.Text += "Device: X509 Subject: " + certificate.Subject + "\n";
            edtTerminal.Text += "Device: X509 Issuer: " + certificate.Issuer + "\n";

            if (sslPolicyErrors == SslPolicyErrors.None) edtTerminal.Text += "Check against operating system trust store: No Policy errors\n";
            else if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) edtTerminal.Text += "\nCheck against operating system trust store: Policy error (RemoteCertificateChainErrors)\n";
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch) edtTerminal.Text += "\nCheck against operating system trust store: Policy error (RemoteCertificateNameMismatch)\n";
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable) edtTerminal.Text += "\nCheck against operating system trust store: Policy error (RemoteCertificateNotAvailable)\n";
            else edtTerminal.Text += "Check against operating system trust store: Policy error (unknown)\n";
        });

        // Certificate successfully validated against a CA certificate in the operating system trust store (not expected)?
        if(sslPolicyErrors == SslPolicyErrors.None) return true;


        // Create certificate chain 
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.Add(theCaCert);
        // chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
        // chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        result = chain.Build(theDeviceCert);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            edtTerminal.Text += "Check against Vendor CA certificate successful: " + result.ToString() + "\n";
        });

        return result;
    }

    async protected Task<bool> Connect()
    {
        if (await bleStream.ConnectAsync() == false)
        {
            edtTerminal.Text += "BLE connection error\n";
            return false;
        }

        edtTerminal.Text += "BLE connection successful\n";

        // Android: https://github.com/dotnet/runtime/issues/74292
        sslStream = new SslStream(bleStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), new LocalCertificateSelectionCallback(OnLocalCertificateSelection));

        try
        {
            // As server....
            // await sslStream.AuthenticateAsServerAsync(theInstallerCert, true, false);

            // As client ....
            X509CertificateCollection theInstallerCerts = new X509CertificateCollection();
            theInstallerCerts.Add(theInstallerCert);
            await sslStream.AuthenticateAsClientAsync(null, theInstallerCerts, false);
        }
        catch (Exception e)
        {
            edtTerminal.Text += "Authentication Error: " + e.Message + "\n";
            if (e.InnerException != null) edtTerminal.Text += "InnerException: " + e.InnerException.Message + "\n";
            return false;
        }

        return true;
    }

    override async protected void OnAppearing()
    {
        //byte[] buf = new byte[20];
        //int len;

        //edtTerminal.Text += "--> OnAppearing()" + "\n";

        bleStream = new BleStream(selectedDevice);
        string theInstallerCertPem = "";
        string theInstallerCertPrivKeyPem = "";
        string theCaCertPem = "";

        // Use build in certificates for testing

        theInstallerCertPem += "-----BEGIN CERTIFICATE-----\n";
        theInstallerCertPem += "MIICCTCCAa6gAwIBAgIEESIzAjAKBggqhkjOPQQDAjBaMQswCQYDVQQGEwJERTER\n";
        theInstallerCertPem += "MA8GA1UEBxMIR2FyY2hpbmcxDDAKBgNVBAsTA1NUQTERMA8GA1UEChMITXlWZW5k\n";
        theInstallerCertPem += "b3IxFzAVBgNVBAMTDm9wdG90cm9uaWMuY29tMB4XDTIzMDUyMDIxMzMyOFoXDTI0\n";
        theInstallerCertPem += "MDUyMDIxMzMyOFowVzELMAkGA1UEBhMCREUxETAPBgNVBAcTCEdhcmNoaW5nMQww\n";
        theInstallerCertPem += "CgYDVQQLEwNTVEExETAPBgNVBAoTCE15VmVuZG9yMRQwEgYDVQQDEwtKb2huIE1p\n";
        theInstallerCertPem += "bGxlcjBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABPlCMHt0fXYwtOkoeYSpRRdQ\n";
        theInstallerCertPem += "Jt+aLABsdyO7yVabdCgus8g/q1sLM4XkW28qaYdXFEiJWKAPqu/WuwndewxPaSOj\n";
        theInstallerCertPem += "ZTBjMBIGCSsGAQQBgt8qAwQFgB8/AwMwGQYJKwYBBAGC3yoEBAxTb21lIE5ldHdv\n";
        theInstallerCertPem += "cmswFQYJKwYBBAGC3yoFBAgAESIzRFVmdzAbBgkrBgEEAYLfKgYEDlVua25vd24g\n";
        theInstallerCertPem += "RG9tYWluMAoGCCqGSM49BAMCA0kAMEYCIQDNScjM1JXXGBue/8WqTnb323xglRCY\n";
        theInstallerCertPem += "4CfO+32zITwWRwIhAOX5/vnMY4XESza4Nn1GZcIf35b/HE4D4mLdNuQN8+O8\n";
        theInstallerCertPem += "-----END CERTIFICATE-----\n";

        theInstallerCertPrivKeyPem += "-----BEGIN EC PRIVATE KEY-----\n";
        theInstallerCertPrivKeyPem += "MHcCAQEEIJvp3ToUOg/W2dzRhHc7vJRxbOhiClAk6zjJwZJNMVlkoAoGCCqGSM49\n";
        theInstallerCertPrivKeyPem += "AwEHoUQDQgAE+UIwe3R9djC06Sh5hKlFF1Am35osAGx3I7vJVpt0KC6zyD+rWwsz\n";
        theInstallerCertPrivKeyPem += "heRbbypph1cUSIlYoA+q79a7Cd17DE9pIw==\n";
        theInstallerCertPrivKeyPem += "-----END EC PRIVATE KEY-----\n";

        theCaCertPem += "-----BEGIN CERTIFICATE-----\n";
        theCaCertPem += "MIIB3TCCAYOgAwIBAgIJAIEkU9Kpk7sQMAoGCCqGSM49BAMCMFoxCzAJBgNVBAYT\n";
        theCaCertPem += "AkRFMREwDwYDVQQHEwhHYXJjaGluZzEMMAoGA1UECxMDU1RBMREwDwYDVQQKEwhN\n";
        theCaCertPem += "eVZlbmRvcjEXMBUGA1UEAxMOb3B0b3Ryb25pYy5jb20wHhcNMjMwMzI0MjMwODI2\n";
        theCaCertPem += "WhcNMjYwMzI0MjMwODI2WjBaMQswCQYDVQQGEwJERTERMA8GA1UEBxMIR2FyY2hp\n";
        theCaCertPem += "bmcxDDAKBgNVBAsTA1NUQTERMA8GA1UEChMITXlWZW5kb3IxFzAVBgNVBAMTDm9w\n";
        theCaCertPem += "dG90cm9uaWMuY29tMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEIZXjlDNlAxIV\n";
        theCaCertPem += "k19EVfeQRj755MWWlZnDhaZKbMPuuP+EML9zdIwWDeCleRP5tKq5fmWp0s81lRjr\n";
        theCaCertPem += "F2AwIs/TLaMyMDAwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUDf0KHNxzEy7q\n";
        theCaCertPem += "znA405Fx1lQsRLowCgYIKoZIzj0EAwIDSAAwRQIhAPDKNTxO8sLkns1y7ec2w2oR\n";
        theCaCertPem += "CYoQyDj2d498XeWYkSVuAiBz+GSRnTmdCFzQKfL8/ma7QaNdXihKYrWUdqvlynVV\n";
        theCaCertPem += "MQ==\n";
        theCaCertPem += "-----END CERTIFICATE-----\n";


        if (theInstallerCert == null || theCaCert == null)
        {
            ECDsa ecdsa = ECDsa.Create();
            X509Certificate2 tempCert = X509Certificate2.CreateFromPem(theInstallerCertPem);

            // CA Cert
            theCaCert = X509Certificate2.CreateFromPem(theCaCertPem);

            // Installer Cert
            ecdsa.ImportFromPem(theInstallerCertPrivKeyPem);
            theInstallerCert = tempCert.CopyWithPrivateKey(ecdsa);

            // Windows does not easily allow storing private keys in application memory. Importing Pkcs12 is a way to bypass this rule.  
            // see: https://github.com/dotnet/runtime/issues/23749
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                theInstallerCert = new X509Certificate2(theInstallerCert.Export(X509ContentType.Pkcs12));
            }
        }

        if (sslStream == null)
        {
            if (await Connect())
            {

                edtTerminal.Text += "### Authenticated (TEST-2) ###" + "\n";

                tlvStreamWatcher = new TlvStreamWatcher(sslStream);
                tlvStreamWatcher.TlvAvailable += OnTlvReceived;
            }
        }
    }

    override protected void OnDisappearing()
    {
        //edtTerminal.Text += "--> OnDisappearing()" + "\n";
    }

    protected override bool OnBackButtonPressed()
    {
        ClosingTasks();

        return base.OnBackButtonPressed();
    }

    private void ClosingTasks()
    {
        tlvStreamWatcher.Detach();
        sslStream.Close();
        tlvStreamWatcher = null;
        bleStream = null;
        sslStream = null;
    }

    void OnTlvReceived(object sender, TlvStreamWatcher.TlvAvailableEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (e.Tlv.Type == TcatTlvType.Application)
            {
                edtTerminal.Text += Encoding.Default.GetString(e.Tlv.Data) + "\r\n";
            }
            else if (e.Tlv.Type == TcatTlvType.Response)
            {
                edtTerminal.Text +=  "Respose code: " + e.Tlv.Data[0].ToString() + "\r\n";
            }
            else
            {
                await DisplayAlert("Alert", "Unexpected TCAT TLV", "OK");
            }
        });
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        ClosingTasks();
        await Shell.Current.GoToAsync("..");
    }

    private async void btnHexDump_Clicked(object sender, EventArgs e)
    {
        edtTerminal.Text = "";
        edtTerminal.Text += bleStream.log_out.ToString();
        await Clipboard.Default.SetTextAsync(bleStream.log_out.ToString());
    }

    private void btnEnter_Clicked(object sender, EventArgs e)
    {
        TcatTlv tlv = new(TcatTlv.TcatTlvType.Application, Encoding.ASCII.GetBytes(entInput.Text));
        byte[] tlvBytes = tlv.GetBytes();  

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }

    private void btnCommission_Clicked(object sender, EventArgs e)
    {
        //byte[] networkkey = new byte[16] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        //TcatTlv tlv = new(25, 0xF997, networkkey);
        //byte[] tlvBytes = tlv.GetBytes();

        byte[] dataset = new byte[] { 0x0e, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x12, 0x35, 0x06, 0x00, 0x04,
                                      0x00, 0x1f, 0xff, 0xe0, 0x02, 0x08, 0xef, 0x13, 0x98, 0xc2, 0xfd, 0x50, 0x4b, 0x67, 0x07, 0x08, 0xfd, 0x35, 0x34,
                                      0x41, 0x33, 0xd1, 0xd7, 0x3e, 0x05, 0x10, 0xfd, 0xa7, 0xc7, 0x71, 0xa2, 0x72, 0x02, 0xe2, 0x32, 0xec, 0xd0, 0x4c,
                                      0xf9, 0x34, 0xf4, 0x76, 0x03, 0x0f, 0x4f, 0x70, 0x65, 0x6e, 0x54, 0x68, 0x72, 0x65, 0x61, 0x64, 0x2d, 0x63, 0x36,
                                      0x34, 0x65, 0x01, 0x02, 0xc6, 0x4e, 0x04, 0x10, 0x5e, 0x9b, 0x9b, 0x36, 0x0f, 0x80, 0xb8, 0x8b, 0xe2, 0x60, 0x3f,
                                      0xb0, 0x13, 0x5c, 0x8d, 0x65, 0x0c, 0x04, 0x02, 0xa0, 0xf7, 0xf8 };

        TcatTlv tlv = new(TcatTlvType.ActiveDataset, dataset);
        byte[] tlvBytes = tlv.GetBytes();

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }

    private void btnThreadOn_Clicked(object sender, EventArgs e)
    {
        TcatTlv tlv = new(TcatCommand.ThreadStart);
        byte[] tlvBytes = tlv.GetBytes();

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }

    private void btnThreadOff_Clicked(object sender, EventArgs e)
    {
        TcatTlv tlv = new(TcatCommand.ThreadStop);
        byte[] tlvBytes = tlv.GetBytes();

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }

    private void entInput_Focused(object sender, FocusEventArgs e)
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            grdLayout.TranslateTo(0, -260);
        }
    }

    private void entInput_Unfocused(object sender, FocusEventArgs e)
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            grdLayout.TranslateTo(0, 0);
        }
    }
}