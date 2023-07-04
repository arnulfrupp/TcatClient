using DirectThreadCommissioning.Models;
using InTheHand.Bluetooth;

namespace TcatCli
{
    internal class Program
    {
        internal BluetoothLEScan? bleScan = null;

        static async Task<bool> WaitForBleAdapter()
        {
            bool bleAvailabe = false;
            int n = 5;

            // Request user permission on startup
            while (!bleAvailabe && n-- > 0)
            {
                bleAvailabe = await BleThreadDevice.GetBleAvailabilityAsync();

                if (!bleAvailabe)
                {
                    Console.WriteLine("Waiting for Bluetooth adapter");
                    Thread.Sleep(1000);
                }
            }

            if (!bleAvailabe)
            {
                Console.WriteLine("Bluetooth adapter not found");
                return false;
            }

            Console.WriteLine("Blutooth adapter found");

            return true;
        }

        private static async Task<bool> TestDeviceDiscovery()
        {
            var discoveredDevices = await Bluetooth.ScanForDevicesAsync();
            Console.WriteLine($"found {discoveredDevices?.Count} devices");
            return discoveredDevices?.Count > 0;
        }

        static async Task Main(string[] args)
        {
            BluetoothLEScanOptions BleScanOptions = new BluetoothLEScanOptions();

            //if (!await WaitForBleAdapter()) return;

            var discoveryTask = TestDeviceDiscovery();
            discoveryTask.Wait();

        }
    }
}