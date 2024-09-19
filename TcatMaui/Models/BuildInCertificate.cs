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
            /*
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
            */

            // OpenThread Referenz
            
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