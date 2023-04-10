using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using InTheHand.Bluetooth;
using DirectThreadCommissioning.Models;


namespace DirectThreadCommissioning.Views;

[QueryProperty(nameof(SelectedDevice), "SelectedDevice")]
public partial class TerminalPage : ContentPage
{
    BleStream bleStream = null;
    SslStream sslStream = null;
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
        byte[] buf = new byte[20];
        int len;

        //edtTerminal.Text += "--> OnAppearing()" + "\n";

        bleStream = new BleStream(selectedDevice);
        string theInstallerCertPem = "";
        string theInstallerCertPrivKeyPem = "";
        string theCaCertPem = "";

        // Use build in certificates for testing       
        
        theInstallerCertPem += "-----BEGIN CERTIFICATE-----\n";
        theInstallerCertPem += "MIIBoDCCAUegAwIBAgIEESIzATAKBggqhkjOPQQDAjBaMQswCQYDVQQGEwJERTER\n";
        theInstallerCertPem += "MA8GA1UEBxMIR2FyY2hpbmcxDDAKBgNVBAsTA1NUQTERMA8GA1UEChMITXlWZW5k\n";
        theInstallerCertPem += "b3IxFzAVBgNVBAMTDm9wdG90cm9uaWMuY29tMB4XDTIzMDMyNDIzMTkxNFoXDTI0\n";
        theInstallerCertPem += "MDMyNDIzMTkxNFowVzELMAkGA1UEBhMCREUxETAPBgNVBAcTCEdhcmNoaW5nMQww\n";
        theInstallerCertPem += "CgYDVQQLEwNTVEExETAPBgNVBAoTCE15VmVuZG9yMRQwEgYDVQQDEwtKb2huIE1p\n";
        theInstallerCertPem += "bGxlcjBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABOOXQ/Y9pLcC4e/SpAm0xcDG\n";
        theInstallerCertPem += "ekBqyQOLcWesxOiyJzQLC2bBSsmlJqFtBu7grejvRv2GgulL2VWPyH1ODCGJ6KQw\n";
        theInstallerCertPem += "CgYIKoZIzj0EAwIDRwAwRAIgdIrDtE3IkTv4CP5TjFX0hbXsXvspfvmI81j5NvOQ\n";
        theInstallerCertPem += "H/ACIGtWqguQR7hlvv/3l7T86th/xC4IxmYH6i8rAL7tjR4w\n";
        theInstallerCertPem += "-----END CERTIFICATE-----\n";

        theInstallerCertPrivKeyPem += "-----BEGIN EC PRIVATE KEY-----\n";
        theInstallerCertPrivKeyPem += "MHcCAQEEIA/Xvs1wT7+GGp+/+hj8nrAJyMVgFMkxf6mswP6zKrovoAoGCCqGSM49\n";
        theInstallerCertPrivKeyPem += "AwEHoUQDQgAE45dD9j2ktwLh79KkCbTFwMZ6QGrJA4txZ6zE6LInNAsLZsFKyaUm\n";
        theInstallerCertPrivKeyPem += "oW0G7uCt6O9G/YaC6UvZVY/IfU4MIYnopA==\n";
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

                edtTerminal.Text += "### Authenticated (TEST) ###" + "\n";

                while ((len = await sslStream.ReadAsync(buf, 0, 20)) > 0)
                {
                    edtTerminal.Text += Encoding.Default.GetString(buf, 0, len);
                }

                edtTerminal.Text += "\nTERMINATED\n";
            }
        }
    }

    override protected void OnDisappearing()
    {
        //edtTerminal.Text += "--> OnDisappearing()" + "\n";
        //bleStream.Disconnect();
    }

    private async void btnClose_Clicked(object sender, EventArgs e)
    {
        sslStream.Close();
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
        if (sslStream == null) return;

        byte[] buf = Encoding.ASCII.GetBytes(entInput.Text + "\r\n");

        sslStream.Write(buf, 0, buf.Length);
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