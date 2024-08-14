using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TcatMaui.Models
{
    internal class BuildInCertificate
    {
        private string theInstallerCertPem = "";
        private string theInstallerCertPrivKeyPem = "";
        private string theCaCertPem = "";

        private X509Certificate2 theCaCert = null;
        private X509Certificate2 theInstallerCert = null;

        public BuildInCertificate()
        {
            theInstallerCertPem += "-----BEGIN CERTIFICATE-----\n";
            theInstallerCertPem += "MIIB8zCCAZigAwIBAgIBATAKBggqhkjOPQQDAjB2MQswCQYDVQQGEwJERTELMAkG\n";
            theInstallerCertPem += "A1UECBMCQlkxETAPBgNVBAcTCEdhcmNoaW5nMQswCQYDVQQLEwJEUzEVMBMGA1UE\n";
            theInstallerCertPem += "ChMMSW52ZW50cm9uaWNzMSMwIQYDVQQDExp3d3cuaW52ZW50cm9uaWNzZ2xvYmFs\n";
            theInstallerCertPem += "LmNvbTAgFw0yNDA4MTQwOTM2NDJaGA8yMTA3MTExNDA5MzY0MlowdTELMAkGA1UE\n";
            theInstallerCertPem += "BhMCREUxCzAJBgNVBAgTAkJZMREwDwYDVQQHEwhHYXJjaGluZzELMAkGA1UECxMC\n";
            theInstallerCertPem += "RFMxFTATBgNVBAoTDEludmVudHJvbmljczEiMCAGA1UEAxMZREFMSXAgQXBwIFRl\n";
            theInstallerCertPem += "c3Rpbmcgd2l0aCBRUjBZMBMGByqGSM49AgEGCCqGSM49AwEHA0IABMC5BcZ9sT2D\n";
            theInstallerCertPem += "cKtB+yMs8sUs14x3c3k+ps2afLVKsKbRxwD1r2JqxVzvIF8ckmZvcc2jq9BRSQBY\n";
            theInstallerCertPem += "9/gxyr1y88GjFjAUMBIGCSsGAQQBgt8qAwQFAQMhAwMwCgYIKoZIzj0EAwIDSQAw\n";
            theInstallerCertPem += "RgIhAOS+IVyD3tCQbHSqa1I2NhBHk9w+5+eryo5vpq78xKmMAiEAp2tgdMm564Mk\n";
            theInstallerCertPem += "bjnxi2z22W7xwJ/5citGr6rKUw47QUo=\n";
            theInstallerCertPem += "-----END CERTIFICATE-----\n";

            theInstallerCertPrivKeyPem += "-----BEGIN EC PRIVATE KEY-----\n";
            theInstallerCertPrivKeyPem += "MHcCAQEEIMDDNuHkWEuhdi9f+g/gjF2ErSS3E25Etwty0doU1zyaoAoGCCqGSM49\n";
            theInstallerCertPrivKeyPem += "AwEHoUQDQgAEwLkFxn2xPYNwq0H7IyzyxSzXjHdzeT6mzZp8tUqwptHHAPWvYmrF\n";
            theInstallerCertPrivKeyPem += "XO8gXxySZm9xzaOr0FFJAFj3+DHKvXLzwQ==\n";
            theInstallerCertPrivKeyPem += "-----END EC PRIVATE KEY-----\n";

            theCaCertPem += "-----BEGIN CERTIFICATE-----\n";
            theCaCertPem += "MIICFTCCAbygAwIBAgIIJ/sUSX8TaTkwCgYIKoZIzj0EAwIwdjELMAkGA1UEBhMC\n";
            theCaCertPem += "REUxCzAJBgNVBAgTAkJZMREwDwYDVQQHEwhHYXJjaGluZzELMAkGA1UECxMCRFMx\n";
            theCaCertPem += "FTATBgNVBAoTDEludmVudHJvbmljczEjMCEGA1UEAxMad3d3LmludmVudHJvbmlj\n";
            theCaCertPem += "c2dsb2JhbC5jb20wIBcNMjQwODE0MDkwNDIyWhgPMjEwNzEyMTQwOTA0MjJaMHYx\n";
            theCaCertPem += "CzAJBgNVBAYTAkRFMQswCQYDVQQIEwJCWTERMA8GA1UEBxMIR2FyY2hpbmcxCzAJ\n";
            theCaCertPem += "BgNVBAsTAkRTMRUwEwYDVQQKEwxJbnZlbnRyb25pY3MxIzAhBgNVBAMTGnd3dy5p\n";
            theCaCertPem += "bnZlbnRyb25pY3NnbG9iYWwuY29tMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE\n";
            theCaCertPem += "mc1809Ehgb0gY2XrGfH6C2Y/HbBRXXaPa2ZfIIQfsPF85sXdvN/rPOUibLH9tice\n";
            theCaCertPem += "UxInt/ML/849SuyfHeDctaMyMDAwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQU\n";
            theCaCertPem += "jdzsqNIB5ZFtAyFNPTnH0vPAwwUwCgYIKoZIzj0EAwIDRwAwRAIgIb1mYO4IUj5K\n";
            theCaCertPem += "ChnIQsSTYNILRSNryGbbdjTEMJ7cbVYCIE5cTWhwq0zcrHGRNgeGW39NQFFStIrF\n";
            theCaCertPem += "AxzyYGM/Omw+\n";
            theCaCertPem += "-----END CERTIFICATE-----\n";


            ECDsa ecdsa = ECDsa.Create();
            X509Certificate2 tempCert = X509Certificate2.CreateFromPem(GetInstallerCertPem());

            // CA Cert
            theCaCert = X509Certificate2.CreateFromPem(GetCaCertPem());

            // Installer Cert
            ecdsa.ImportFromPem(GetInstallerCertPrivKeyPem());
            theInstallerCert = tempCert.CopyWithPrivateKey(ecdsa);
        }
        
        public string GetInstallerCertPem()
        {
            return theInstallerCertPem;
        } 

        public string GetInstallerCertPrivKeyPem()
        {
            return theInstallerCertPrivKeyPem;
        } 

        public string GetCaCertPem()
        {
            return theCaCertPem; 
        } 

        public X509Certificate2 GetCaCert()
        {
            return theCaCert;
        }

        public X509Certificate2 GetInstallerCert()
        {
            return theInstallerCert;
        }
    }
}