using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using InTheHand.Bluetooth;

namespace TcatMaui.Models
{
    internal class BleThreadDevice_Portable : BleThreadDevice
    {
        static private bool BleAvailability = false;
        
        public BluetoothDevice OperatingSystemBleDevice { get; set; }

        public override Task<Stream> Connect()
        {
            return null;
        } 

        public override Task Disconnect()
        {
            return Task.CompletedTask;
        }

        static async internal Task<bool> GetBleAvailabilityAsync()
        {
            try
            {
                BleAvailability = await Bluetooth.GetAvailabilityAsync();
            }
            catch (Exception)
            {
                BleAvailability = false;
            }

            return BleAvailability;
        }
    }
}
