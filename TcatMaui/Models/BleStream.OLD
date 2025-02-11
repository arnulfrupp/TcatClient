using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;


namespace TcatMaui.Models
{

    // [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]

    internal class BleStream : Stream
    {
        private Semaphore readSemaphore;
        private Queue<byte> readQueue;
        private bool connected = false;
        private bool inRead = false;
        private int maxPacketSize;

        private IDevice bleDevice;
        private ICharacteristic writeChar = null;
        private ICharacteristic notifyChar = null;

        public string log = "I 2022-09-15T15:14:00Z\n";
        public int log_line = 0;
        public string log_out = "I 2022-09-15T15:14:00Z\n";
        public int log_line_out = 0;

        public BleStream(IDevice aDevice)
        {
            bleDevice = aDevice;
            maxPacketSize = 20;     // Minimum GATT MTU - 3 byte GATT overhead

            readQueue = new Queue<byte>();
            readSemaphore = new Semaphore(0, 1);
        }


        // Overriden methods from Stream class
        // ===================================

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush() { return; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int availableBytes;

            if (inRead) throw new InvalidOperationException("simultaneous read requests not supported");
            inRead = true;

            if (!connected || count == 0) return 0;

            lock (readQueue) availableBytes = readQueue.Count;

            while (availableBytes == 0 && connected == true)
            {
                readSemaphore.WaitOne();
                lock (readQueue) availableBytes = readQueue.Count;
            }

            lock (readQueue)
            {
                if (readQueue.Count < count) count = readQueue.Count;

                for (int i = 0; i < count; i++)
                {
                    buffer[offset + i] = readQueue.Dequeue();
                }
            }

            inRead = false;

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                log_out += log_line_out.ToString("x6") + " ";

                for (int j = 0; j < 16 && i < count; j++, i++)
                {
                    log_out += buffer[offset + i].ToString("x2") + " ";
                    log_line_out++;
                }

                log_out += "\n";
            }


            while (count > 0)
            {
                int packetSize = count > maxPacketSize ? maxPacketSize : count;

                Byte[] bytes = new Byte[packetSize];

                if (writeChar == null) return;

                for (int i = 0; i < packetSize; i++) bytes[i] = buffer[offset + i];

                writeChar.WriteAsync(bytes);
                count -= packetSize;
                offset += packetSize;
            }
        }

        public override void Close()
        {
            Disconnect();
            base.Close();
        }


        // BleStream specific methods
        // ==========================

        public async Task<bool> ConnectAsync()
        {
            IService primaryService;
            //IReadOnlyList<ICharacteristic> allChar;
            Guid servicGuid = Guid.Parse("0000FFFB-0000-1000-8000-00805F9B34FB");
            Guid txCharGuid = Guid.Parse("7fddf61f-280a-4773-b448-ba1b8fe0dd69");
            Guid rxCharGuid = Guid.Parse("6bd10d8b-85a7-4e5a-ba2d-c83558a5f220");

            if (bleDevice == null) return false;
            CrossBluetoothLE.Current.Adapter.DeviceDisconnected += OnGattServerDisconnected;

            await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(bleDevice);

            //var services = await bleDevice.GetServicesAsync();

            primaryService = await bleDevice.GetServiceAsync(servicGuid);
            if (primaryService == null) return false;

            //allChar = await primaryService.GetCharacteristicsAsync();

            writeChar = await primaryService.GetCharacteristicAsync(rxCharGuid);
            if (writeChar == null) return false;

            notifyChar = await primaryService.GetCharacteristicAsync(txCharGuid);
            if (notifyChar == null) return false;

            notifyChar.ValueUpdated += OnCharacteristicValueChanged;
            await notifyChar.StartUpdatesAsync();

            connected = true;

            return true;
        }

    
        private async void Disconnect()
        {
            if (bleDevice == null) return;

            if (notifyChar != null)
            {
                notifyChar.ValueUpdated -= OnCharacteristicValueChanged;
            }

            CrossBluetoothLE.Current.Adapter.DeviceDisconnected -= OnGattServerDisconnected;
            await CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(bleDevice);

            connected = false;
            readSemaphore.WaitOne(0);
            readSemaphore.Release();
        }


        // Callback methods
        // ================

        private void OnGattServerDisconnected(object sender, DeviceEventArgs e)
        {
            connected = false;
            readSemaphore.WaitOne(0);
            readSemaphore.Release();
        }

        private void OnCharacteristicValueChanged(object sender, CharacteristicUpdatedEventArgs e)
        {
            log += log_line.ToString("x6") + " ";
            log_line += e.Characteristic.Value.Length;

            foreach (byte b in e.Characteristic.Value)
            {
                log += b.ToString("x2") + " ";
            }

            log += "\n";


            lock (readQueue)
            {
                foreach (byte b in e.Characteristic.Value)
                {
                    readQueue.Enqueue(b);
                }

                readSemaphore.WaitOne(0);
                readSemaphore.Release();
            }
        }
    }
}
