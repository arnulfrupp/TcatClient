using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;

using TcatMaui.Models; 

namespace TcatCli.Models
{
    internal class BleThreadDevice_BlueZ : BleThreadDevice
    {
        BleStream bleStream = null;
        public Device OperatingSystemBleDevice { get; set; }

        public BleThreadDevice_BlueZ()
        {
            OperatingSystemBleDevice = null;    
        }

        public override async Task<Stream> Connect()
        {
            bleStream = new BleStream(OperatingSystemBleDevice);

            if (await bleStream.ConnectAsync() == false)
            {
                Console.WriteLine("BLE connection error");
                return null;
            }

            return bleStream;
        } 

        public override async Task Disconnect()
        {
            if (bleStream == null) return;
            
            await bleStream.Disconnect();
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
                var deviceProperties = await device.GetAllAsync();
                Console.WriteLine($"{deviceProperties.Alias} (Address: {deviceProperties.Address}, RSSI: {deviceProperties.RSSI})");

                if(deviceProperties.Alias.Contains(aScanFilter, StringComparison.OrdinalIgnoreCase) ||
                   deviceProperties.Address.Equals(aScanFilter, StringComparison.OrdinalIgnoreCase))
                {
                    OperatingSystemBleDevice = device;
                }
            }
                
            IDisposable  d = await adapter.WatchDevicesAddedAsync(async device =>
            {
                var deviceProperties = await device.GetAllAsync();
                Console.WriteLine($"{deviceProperties.Alias} (Address: {deviceProperties.Address}, RSSI: {deviceProperties.RSSI})");

                if(deviceProperties.Alias.Contains(aScanFilter, StringComparison.OrdinalIgnoreCase) ||
                   deviceProperties.Address.Equals(aScanFilter, StringComparison.OrdinalIgnoreCase))
                {
                    OperatingSystemBleDevice = device;
                }

            });

            await adapter.StartDiscoveryAsync();

            while (OperatingSystemBleDevice == null &&  --aScanSeconds > 0)
            {
                await Task.Delay(1000);
            }

            await adapter.StopDiscoveryAsync();
            d.Dispose();

            if(OperatingSystemBleDevice == null)
            {
                Console.WriteLine("No Thread Device found");
                return false;   
            }

            return true;
        }
    }
}