using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;

namespace TcatCli.Models
{
    internal class BleThreadDevice
    {
        public Device ThreadDevice { get; set; }

        public BleThreadDevice()
        {
            ThreadDevice = null;    
        }

        private async Task<string> GetDeviceDescriptionAsync(IDevice1 device)
        {
            var deviceProperties = await device.GetAllAsync();
            return $"{deviceProperties.Alias} (Address: {deviceProperties.Address}, RSSI: {deviceProperties.RSSI})";
        }

        public async Task<bool> Scan(String aScanFilter, int aScanSeconds)
        {
            IAdapter1 adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();

            if(adapter == null) 
            {
                Console.WriteLine("No Bluetooth adapter found");
                return false;
            }

            var adapterPath = adapter.ObjectPath.ToString();
            var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/") + 1);
            Console.WriteLine($"Using Bluetooth adapter {adapterName}");
            Console.WriteLine($"Adapter's full path:    {adapterPath}"); 

            Console.WriteLine($"\nScanning for {aScanSeconds} seconds...");

            var devices = await adapter.GetDevicesAsync();
            foreach (var device in devices)
            {
                string deviceDescription = await GetDeviceDescriptionAsync(device);
                Console.WriteLine($"{deviceDescription}");

                if(deviceDescription.Contains(aScanFilter))
                {
                    ThreadDevice = device;
                }
            }
                
            IDisposable  d = await adapter.WatchDevicesAddedAsync(async device =>
            {
                // Write a message when we detect new devices during the scan.
                string deviceDescription = await GetDeviceDescriptionAsync(device);
                Console.WriteLine($"{deviceDescription}");
                
                if(deviceDescription.Contains(aScanFilter))
                {
                    ThreadDevice = device;
                }

            });

            await adapter.StartDiscoveryAsync();

            while (ThreadDevice == null &&  --aScanSeconds > 0)
            {
                await Task.Delay(1000);
            }

            await adapter.StopDiscoveryAsync();
            d.Dispose();

            if(ThreadDevice == null)
            {
                Console.WriteLine("No Thread Device found");
                return false;   
            }

            return true;
        }
    }
}