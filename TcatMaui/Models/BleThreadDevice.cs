using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Plugin.BLE.Abstractions.Contracts;


namespace TcatMaui.Models
{
    internal class BleThreadDevice : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseProperChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public IDevice BluetoothDevice { get; set; }
        public string Name { get; set; }

        public int rssi;

        public int Rssi
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
    }
}
