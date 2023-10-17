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

        // Old 
        theInstallerCertPem += "-----BEGIN CERTIFICATE-----\n";
        theInstallerCertPem += "MIIByzCCAXCgAwIBAgIEAQIDBDAKBggqhkjOPQQDAjBaMQswCQYDVQQGEwJERTER\n";
        theInstallerCertPem += "MA8GA1UEBxMIR2FyY2hpbmcxDDAKBgNVBAsTA1NUQTERMA8GA1UEChMITXlWZW5k\n";
        theInstallerCertPem += "b3IxFzAVBgNVBAMTDm9wdG90cm9uaWMuY29tMB4XDTIzMDgwODEwNDQxN1oXDTI0\n";
        theInstallerCertPem += "MDgwODEwNDQxN1owaDELMAkGA1UEBhMCREUxEDAOBgNVBAgTB015U3RhdGUxDzAN\n";
        theInstallerCertPem += "BgNVBAcTBk15Q2l0eTEPMA0GA1UECxMGTXlVbml0MREwDwYDVQQKEwhNeVZlbmRv\n";
        theInstallerCertPem += "cjESMBAGA1UEAxMJVW5saW1pdGVkMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE\n";
        theInstallerCertPem += "fKo0DDeur8qo7Kc4bryLR263pjFcbFbpT2p7RXq4o5WwZBTmFD7yXuhYrAeh4iHZ\n";
        theInstallerCertPem += "ZVs1cEd7vgQy5VQit1yUgqMWMBQwEgYJKwYBBAGC3yoDBAUBAQEBATAKBggqhkjO\n";
        theInstallerCertPem += "PQQDAgNJADBGAiEA+dsy0iLDWFOw0cYN4TnrPTqAptp0k1CQJuqGxQ7skY0CIQCV\n";
        theInstallerCertPem += "NmilVUrWHdBYW2NLUTfJoIp/iolpwgs2i3R1PsbpfQ==\n";
        theInstallerCertPem += "-----END CERTIFICATE-----\n";

        theInstallerCertPrivKeyPem += "-----BEGIN EC PRIVATE KEY-----\n";
        theInstallerCertPrivKeyPem += "MHcCAQEEIDy7T9D9jo1/Bf+ajcWo9DPuDGhL+0Js3c5YYCZ/aakPoAoGCCqGSM49\n";
        theInstallerCertPrivKeyPem += "AwEHoUQDQgAEfKo0DDeur8qo7Kc4bryLR263pjFcbFbpT2p7RXq4o5WwZBTmFD7y\n";
        theInstallerCertPrivKeyPem += "XuhYrAeh4iHZZVs1cEd7vgQy5VQit1yUgg==\n";
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

        // OpenThread Referenz
        /*
        theInstallerCertPem += "-----BEGIN CERTIFICATE-----\n";
        theInstallerCertPem += "MIIB7DCCAZGgAwIBAgIEAQIDBDAKBggqhkjOPQQDAjBvMQswCQYDVQQGEwJYWDEQ\n";
        theInstallerCertPem += "MA4GA1UECBMHTXlTdGF0ZTEPMA0GA1UEBxMGTXlDaXR5MQ8wDQYDVQQLEwZNeVVu\n";
        theInstallerCertPem += "aXQxETAPBgNVBAoTCE15VmVuZG9yMRkwFwYDVQQDExB3d3cubXl2ZW5kb3IuY29t\n";
        theInstallerCertPem += "MB4XDTIzMTAxNjEwMzgyN1oXDTI1MTAxNjEwMzgyN1owdDELMAkGA1UEBhMCWFgx\n";
        theInstallerCertPem += "EDAOBgNVBAgTB015U3RhdGUxDzANBgNVBAcTBk15Q2l0eTEPMA0GA1UECxMGTXlV\n";
        theInstallerCertPem += "bml0MREwDwYDVQQKEwhNeVZlbmRvcjEeMBwGA1UEAxMVR2l2ZW5OYW1lIEZhbWls\n";
        theInstallerCertPem += "aXlOYW1lMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEUb+XQqxo00qRkhpEEVea\n";
        theInstallerCertPem += "IK7SE9oPH2wg0o/oVSN2uQeFgAK25mTHABIcC6YoSX7j6YsvT0t05C8hbsEshz5C\n";
        theInstallerCertPem += "U6MWMBQwEgYJKwYBBAGC3yoDBAUBAQEBATAKBggqhkjOPQQDAgNJADBGAiEA3IP7\n";
        theInstallerCertPem += "K139L48a5hgK2xYlTlo4nGCeXnVjvyZBngFjrE4CIQCx8eo/XK85tGPxsPpD03m2\n";
        theInstallerCertPem += "93MhOtdDcJhpNnzYcI+OwQ==\n";
        theInstallerCertPem += "-----END CERTIFICATE-----\n";

        theInstallerCertPrivKeyPem += "-----BEGIN EC PRIVATE KEY-----\n";
        theInstallerCertPrivKeyPem += "MHcCAQEEIPHYBsvz+VYNrR/sjCRJml6rvkP/VQzNxK1IXhxzeVzJoAoGCCqGSM49\n";
        theInstallerCertPrivKeyPem += "AwEHoUQDQgAEUb+XQqxo00qRkhpEEVeaIK7SE9oPH2wg0o/oVSN2uQeFgAK25mTH\n";
        theInstallerCertPrivKeyPem += "ABIcC6YoSX7j6YsvT0t05C8hbsEshz5CUw==\n";
        theInstallerCertPrivKeyPem += "-----END EC PRIVATE KEY-----\n";

        theCaCertPem += "-----BEGIN CERTIFICATE-----\n";
        theCaCertPem += "MIICCDCCAa2gAwIBAgIJAIKxygBXoH+5MAoGCCqGSM49BAMCMG8xCzAJBgNVBAYT\n";
        theCaCertPem += "AlhYMRAwDgYDVQQIEwdNeVN0YXRlMQ8wDQYDVQQHEwZNeUNpdHkxDzANBgNVBAsT\n";
        theCaCertPem += "Bk15VW5pdDERMA8GA1UEChMITXlWZW5kb3IxGTAXBgNVBAMTEHd3dy5teXZlbmRv\n";
        theCaCertPem += "ci5jb20wHhcNMjMxMDE2MTAzMzE1WhcNMjYxMDE2MTAzMzE1WjBvMQswCQYDVQQG\n";
        theCaCertPem += "EwJYWDEQMA4GA1UECBMHTXlTdGF0ZTEPMA0GA1UEBxMGTXlDaXR5MQ8wDQYDVQQL\n";
        theCaCertPem += "EwZNeVVuaXQxETAPBgNVBAoTCE15VmVuZG9yMRkwFwYDVQQDExB3d3cubXl2ZW5k\n";
        theCaCertPem += "b3IuY29tMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEWdyzPAXGKeZY94OhHAWX\n";
        theCaCertPem += "HzJfQIjGSyaOzlgL9OEFw2SoUDncLKPGwfPAUSfuMyEkzszNDM0HHkBsDLqu4n25\n";
        theCaCertPem += "/6MyMDAwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQU4EynoSw9eDKZEVPkums2\n";
        theCaCertPem += "IWLAJCowCgYIKoZIzj0EAwIDSQAwRgIhAMYGGL9xShyE6P9wEU+MAYF6W3CzdrwV\n";
        theCaCertPem += "kuerX1encIH2AiEA5rq490NUobM1Au43roxJq1T6Z43LscPVbGZfULD1Jq0=\n";
        theCaCertPem += "-----END CERTIFICATE-----\n";
        */

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
            if (e.Tlv.Type == TcatTlvType.SendApplicationData)
            {
                edtTerminal.Text += "Application data: " + Encoding.Default.GetString(e.Tlv.Data) + "\r\n";
            }
            else if (e.Tlv.Type == TcatTlvType.ResponseWithPayload)
            {
                edtTerminal.Text += "Response payload: " + Encoding.Default.GetString(e.Tlv.Data) + "\r\n";
            }
            else if (e.Tlv.Type == TcatTlvType.ResponseWithStatus)
            {
                edtTerminal.Text += "Response code: " + e.Tlv.Data[0].ToString() + "\r\n";
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
        TcatTlv tlv = new(TcatTlv.TcatTlvType.SendApplicationData, Encoding.ASCII.GetBytes(entInput.Text));
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

        TcatTlv tlv = new(TcatTlvType.SetActiveOperationalDataset, dataset);
        byte[] tlvBytes = tlv.GetBytes();

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }

    private void btnNetworkkey_Clicked(object sender, EventArgs e)
    {
        byte[] networkkey = new byte[16] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
        TcatTlv tlv = new(25, 0xF997, networkkey);
        byte[] tlvBytes = tlv.GetBytes();

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }
    private void btnDecommission_Clicked(object sender, EventArgs e)
    {
        byte[] dataset = new byte[] { };

        TcatTlv tlv = new(TcatTlvType.SetActiveOperationalDataset, dataset);
        byte[] tlvBytes = tlv.GetBytes();

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }

    private void btnThreadOn_Clicked(object sender, EventArgs e)
    {
        TcatTlv tlv = new(TcatTlvType.StartThreadInterface);
        byte[] tlvBytes = tlv.GetBytes();

        if (sslStream == null) return;
        if (!sslStream.IsAuthenticated) return;

        sslStream.Write(tlvBytes, 0, tlvBytes.Length);
    }

    private void btnThreadOff_Clicked(object sender, EventArgs e)
    {
        TcatTlv tlv = new(TcatTlvType.StopThreadInterface);
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

    private void btnDeCommission_Clicked(object sender, EventArgs e)
    {

    }
}