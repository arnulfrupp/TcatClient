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

namespace TcatCli // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static TlsSession tlsSession = null;
        static TlvStreamWatcher tlvStreamWatcher = null;


        private static void OnTlvReceived(object sender, TlvStreamWatcher.TlvAvailableEventArgs e)
        {
            if (e.Tlv.Type == TcatTlv.TcatTlvType.Application)
            {
                Console.WriteLine(Encoding.Default.GetString(e.Tlv.Data));
            }
            else if (e.Tlv.Type == TcatTlv.TcatTlvType.Response)
            {
                Console.WriteLine("Respose code: " + e.Tlv.Data[0].ToString());
            }
            else
            {
                Console.WriteLine("Alert: Unexpected TCAT TLV", "OK");
            }
    }

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("TcatCli");
            Console.WriteLine("=======");

            BleThreadDevice device = new();
            string scanFilter;

            if(args.Length > 0) scanFilter = args[0];
            else scanFilter = "Thread BLE";

            if(await device.Scan(scanFilter, 15) == false) return 1;

            tlsSession = new(device);
            if(await tlsSession.Connect() == false) return 1;
        
            Console.WriteLine("### Authenticated ###" + "\n");

            tlvStreamWatcher = new TlvStreamWatcher(tlsSession.GetTlsStream());
            tlvStreamWatcher.TlvAvailable += OnTlvReceived;

            TcatTlv tlv = new(TcatTlv.TcatTlvType.Application, Encoding.ASCII.GetBytes("Hallo Welt"));
            byte[] tlvBytes = tlv.GetBytes();  

            if (!tlsSession.GetTlsStream().IsAuthenticated) return 1;

            tlsSession.GetTlsStream().Write(tlvBytes, 0, tlvBytes.Length);

            await Task.Delay(3000);

            tlvStreamWatcher.Detach();
            await tlsSession.Disconnect();
            Console.WriteLine("Closed");
                    
            return 0;       
         
        }       
    }
}