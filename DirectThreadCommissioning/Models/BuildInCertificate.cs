using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DirectThreadCommissioning.Models
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