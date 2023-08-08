using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using TcatCli.Models;
using TcatMaui.Models;
using static TcatMaui.Models.TcatTlv;

namespace TcatCli
{
    internal class Program
    {
        static TlsSession tlsSession = null;
        static TlvStreamWatcher tlvStreamWatcher = null;


        private static void OnTlvReceived(object sender, TlvStreamWatcher.TlvAvailableEventArgs e)
        {
            if (e.Tlv.Type == TcatTlvType.SendApplicationData)
            {
                Console.WriteLine("Application data: " + Encoding.Default.GetString(e.Tlv.Data));
            }
            else if (e.Tlv.Type == TcatTlvType.ResponseWithPayload)
            {
                Console.WriteLine("Response payload: " + Encoding.Default.GetString(e.Tlv.Data));
            }
            else if (e.Tlv.Type == TcatTlvType.ResponseWithStatus)
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
            BleThreadDevice_BlueZ bleDevice = new();
            string scanFilter;
            string input = "";
            TcatTlv tlv = null;

            byte[] dataset = new byte[]   { 0x0e, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x12, 0x35, 0x06, 0x00, 0x04,
                                            0x00, 0x1f, 0xff, 0xe0, 0x02, 0x08, 0xef, 0x13, 0x98, 0xc2, 0xfd, 0x50, 0x4b, 0x67, 0x07, 0x08, 0xfd, 0x35, 0x34,
                                            0x41, 0x33, 0xd1, 0xd7, 0x3e, 0x05, 0x10, 0xfd, 0xa7, 0xc7, 0x71, 0xa2, 0x72, 0x02, 0xe2, 0x32, 0xec, 0xd0, 0x4c,
                                            0xf9, 0x34, 0xf4, 0x76, 0x03, 0x0f, 0x4f, 0x70, 0x65, 0x6e, 0x54, 0x68, 0x72, 0x65, 0x61, 0x64, 0x2d, 0x63, 0x36,
                                            0x34, 0x65, 0x01, 0x02, 0xc6, 0x4e, 0x04, 0x10, 0x5e, 0x9b, 0x9b, 0x36, 0x0f, 0x80, 0xb8, 0x8b, 0xe2, 0x60, 0x3f,
                                            0xb0, 0x13, 0x5c, 0x8d, 0x65, 0x0c, 0x04, 0x02, 0xa0, 0xf7, 0xf8 };

            Console.WriteLine("TcatCli");
            Console.WriteLine("=======");
            Console.WriteLine("");
            Console.WriteLine("Usage: TcatCli <deviceAddress>|<deviceNameSubstring>");

            if(args.Length > 0) scanFilter = args[0];
            else scanFilter = "Thread BLE";

            if(await bleDevice.Scan(scanFilter, 8) == false) return 1;

            tlsSession = new(await bleDevice.Connect());
            if(await tlsSession.Open() == false) return 1;
        
            Console.WriteLine("### Authenticated ###" + "\n");

            tlvStreamWatcher = new TlvStreamWatcher(tlsSession.GetTlsStream());
            tlvStreamWatcher.TlvAvailable += OnTlvReceived;

            while(input != "e")
            {
                Console.Write("'d' = dataset, 'n' = networkkey/panid/channel 'a' = application data, '0' = Thread On, '1'= Thread off, 'e'=exit: ");
                input = Console.ReadLine();
                Console.WriteLine("");

                switch(input)
                {
                    case "d":
                        tlv = new(TcatTlvType.SetActiveOperationalDataset, dataset);
                        tlsSession.Write(tlv.GetBytes());
                    break;

                    case "n":
                        byte[] networkkey = new byte[16] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };
                        tlv = new(25, 0xF997, networkkey);
                        tlsSession.Write(tlv.GetBytes());
                    break;

                    case "a":
                        tlv = new(TcatTlvType.SendApplicationData, Encoding.ASCII.GetBytes("Hello World"));
                        tlsSession.Write(tlv.GetBytes());
                    break;

                    case "0":
                        tlv = new(TcatTlvType.StopThreadInterface);
                        tlsSession.Write(tlv.GetBytes());
                    break;

                    case "1":
                        tlv = new(TcatTlvType.StartThreadInterface);
                        tlsSession.Write(tlv.GetBytes());
                    break;

                }

                if(input != "e") await Task.Delay(500);
            }

            Console.WriteLine("Disconnecting...");
            tlvStreamWatcher.Detach();
            tlsSession.Close();
            await bleDevice.Disconnect();
            Console.WriteLine("Disconnected");
                    
            return 0;       
         
        }       
    }
}