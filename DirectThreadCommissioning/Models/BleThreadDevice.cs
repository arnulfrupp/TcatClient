using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;



namespace DirectThreadCommissioning.Models
{
    internal abstract class BleThreadDevice : INotifyPropertyChanged
    {

        public abstract Task<Stream> Connect(); 
        public abstract Task Disconnect(); 

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseProperChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

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
    }
}
