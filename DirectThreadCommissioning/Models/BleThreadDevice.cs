using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using InTheHand.Bluetooth;


namespace DirectThreadCommissioning.Models
{
    internal class BleThreadDevice : INotifyPropertyChanged
    {
        static private bool BleAvailability = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseProperChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public BluetoothDevice BluetoothDevice { get; set; }
        public string Name { get; set; }

        public short rssi;

        public short Rssi
        {
            get { return rssi; }
            set
            {
                if (rssi != value)
                {
                    rssi = value;
                    RaiseProperChanged();
                }
            }
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
