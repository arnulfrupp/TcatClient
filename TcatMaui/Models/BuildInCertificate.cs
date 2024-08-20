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
            theInstallerCertPem += "MIIB5DCCAYqgAwIBAgIBATAKBggqhkjOPQQDAjB2MQswCQYDVQQGEwJERTELMAkG\n";
            theInstallerCertPem += "A1UECBMCQlkxETAPBgNVBAcTCEdhcmNoaW5nMQswCQYDVQQLEwJEUzEVMBMGA1UE\n";
            theInstallerCertPem += "ChMMSW52ZW50cm9uaWNzMSMwIQYDVQQDExp3d3cuaW52ZW50cm9uaWNzZ2xvYmFs\n";
            theInstallerCertPem += "LmNvbTAgFw0yNDA4MTQwOTM0MThaGA8yMTA3MTExNDA5MzQxOFowZzELMAkGA1UE\n";
            theInstallerCertPem += "BhMCREUxCzAJBgNVBAgTAkJZMREwDwYDVQQHEwhHYXJjaGluZzELMAkGA1UECxMC\n";
            theInstallerCertPem += "RFMxFTATBgNVBAoTDEludmVudHJvbmljczEUMBIGA1UEAxMLREFMSXAgQWRtaW4w\n";
            theInstallerCertPem += "WTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAAS0V4ZNEenCQxu5QwB/ME5nSl/K8Ack\n";
            theInstallerCertPem += "Ph0MOcyjzL5o0ssHIJbjoN1/fcX4SLcMSOI78tL0maypFVMAe4Yw41ceoxYwFDAS\n";
            theInstallerCertPem += "BgkrBgEEAYLfKgMEBQEBIQEBMAoGCCqGSM49BAMCA0gAMEUCIQD4m1YzZi8VFssv\n";
            theInstallerCertPem += "USjBwH1wEbnEeof82Du63LD4UwkZZgIgVtxQ4O9EU7AtMWxJoy5xbe/7KDoQdsit\n";
            theInstallerCertPem += "44ARewW1y0M=\n";
            theInstallerCertPem += "-----END CERTIFICATE-----\n";

            theInstallerCertPrivKeyPem += "-----BEGIN EC PRIVATE KEY-----\n";
            theInstallerCertPrivKeyPem += "MHcCAQEEIDCUZ28X4/Gide9cGukZuMW8z7v7TgzjSiTILqtq9CLToAoGCCqGSM49\n";
            theInstallerCertPrivKeyPem += "AwEHoUQDQgAEtFeGTRHpwkMbuUMAfzBOZ0pfyvAHJD4dDDnMo8y+aNLLByCW46Dd\n";
            theInstallerCertPrivKeyPem += "f33F+Ei3DEjiO/LS9JmsqRVTAHuGMONXHg==\n";
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