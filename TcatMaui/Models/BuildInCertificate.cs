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