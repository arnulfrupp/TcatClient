using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using TcatCli.Models;
using DirectThreadCommissioning.Models;

namespace TcatCli.Models
{
    internal class TlsSession
    {
        private BleStream bleStream = null;
        private SslStream sslStream = null;
        private BuildInCertificate theCert = null;
        private BleThreadDevice bleThreadDevice = null;

        public TlsSession(BleThreadDevice aBleThreadDevice)
        {
            bleThreadDevice = aBleThreadDevice;
            theCert = new();
        }

        public SslStream GetTlsStream()
        {
            return sslStream;
        }

        public async Task Disconnect()
        {
            await bleStream.Disconnect();
        }

        private X509Certificate OnLocalCertificateSelection(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            if(theCert.GetInstallerCert() == null) throw new NullReferenceException();
            return theCert.GetInstallerCert();
        }

        public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool result = false;
            X509Certificate2 theDeviceCert = new X509Certificate2(certificate);

            if(theCert.GetCaCert() == null) return false;

            Console.WriteLine("Device: X509 Subject: " + certificate.Subject);
            Console.WriteLine("Device: X509 Issuer: " + certificate.Issuer);

            if (sslPolicyErrors == SslPolicyErrors.None) Console.WriteLine("Check against operating system trust store: No Policy errors");
            else if(sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) Console.WriteLine("Check against operating system trust store: Policy error (RemoteCertificateChainErrors)");
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch) Console.WriteLine("Check against operating system trust store: Policy error (RemoteCertificateNameMismatch)");
            else if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable) Console.WriteLine("Check against operating system trust store: Policy error (RemoteCertificateNotAvailable)");
            else Console.WriteLine("Check against operating system trust store: Policy error (unknown)");
        
            // Certificate successfully validated against a CA certificate in the operating system trust store (not expected)?
            if(sslPolicyErrors == SslPolicyErrors.None) return true;


            // Create certificate chain 
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            chain.ChainPolicy.CustomTrustStore.Add(theCert.GetCaCert());
            // chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            // chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            result = chain.Build(theDeviceCert);

            Console.WriteLine("Check against Vendor CA certificate successful: " + result.ToString());

            return result;
        }

        public async Task<bool> Connect()
        {
            bleStream = new(bleThreadDevice.ThreadDevice);

            if(theCert.GetInstallerCert() == null || bleStream == null) return false;

            if (await bleStream.ConnectAsync() == false)
            {
                Console.WriteLine("BLE connection error");
                return false;
            }

            Console.WriteLine("BLE connection successful");

            sslStream = new SslStream(bleStream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), new LocalCertificateSelectionCallback(OnLocalCertificateSelection));

            try
            {
                // As server....
                // await sslStream.AuthenticateAsServerAsync(theInstallerCert, true, false);

                // As client ....
                X509CertificateCollection theInstallerCerts = new X509CertificateCollection();
                theInstallerCerts.Add(theCert.GetInstallerCert());
                await sslStream.AuthenticateAsClientAsync("", theInstallerCerts, false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Authentication Error: " + e.Message);
                if (e.InnerException != null) Console.WriteLine("InnerException: " + e.InnerException.Message);
                return false;
            }

            return true;
        }
    }
}