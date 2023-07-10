using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;

using TcatCli.Models;


namespace TcatCli.Models
{

    // [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]

    internal class BleStream : Stream
    {
        private Semaphore readSemaphore;
        private Queue<byte> readQueue;
        private bool connected = false;
        private int maxPacketSize;

        // Sync check
        private bool inRead = false;
        private bool inWrite = false;
        private bool inOnCharacteristicValueChanged = false;
        private Device bleDevice;
        private GattCharacteristic writeChar = null;
        private GattCharacteristic notifyChar = null;

        public StringBuilder log_out = new(); 
        public int log_byte_count = 0;
        private int log_direction = 0;              // 0=start, 1=out, -1=in

        public const int initialMtuSize = 23;       // ATT_MTU
        public const int gattOverhead = 3;          // Three bytes overhead
        public const int initialMaxPacketSize = initialMtuSize - gattOverhead;
        

        public BleStream(Device aDevice)
        {
            bleDevice = aDevice;
           
            maxPacketSize = initialMaxPacketSize;

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

            if (inRead)
            {
                log_out.Append("Simultaneous read requests not supported\n");
                throw new InvalidOperationException("simultaneous read requests not supported");
            }

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

        public override async void Write(byte[] buffer, int offset, int count)
        {
            DateTime dateTime = DateTime.Now;
            Dictionary<string, object> options = new();
            
            // Set options for frite without response
            options.Add("type", "command");

            // Update maximum packet size
            maxPacketSize = 0; // Todo: How to get MTU size in Linux

            if (maxPacketSize < initialMaxPacketSize) maxPacketSize = initialMaxPacketSize;

            if (inWrite)
            {
                log_out.Append("Simultaneous write requests not supported\n");
                throw new InvalidOperationException("simultaneous write requests not supported");
            }

            inWrite = true;

            if (log_direction != 1)   // outbound
            {
                log_direction = 1;
                log_byte_count = 0;
                log_out.Append("O ");
                log_out.Append(DateTime.Now.ToUniversalTime().ToString("o"));
                log_out.Append("\n");
            }

            while (count > 0)
            {
                int packetSize = count > maxPacketSize ? maxPacketSize : count;

                Byte[] bytes = new Byte[packetSize];

                if (writeChar == null)
                {
                    log_out.Append("writeChar == null\n");
                    inWrite = false;
                    return;
                }

                log_out.Append(log_byte_count.ToString("x6"));
                log_out.Append(" ");

                for (int i = 0; i < packetSize; i++)
                {
                    bytes[i] = buffer[offset + i];
                    log_out.Append(buffer[offset + i].ToString("x2"));
                    log_out.Append(" ");
                    log_byte_count++;
                }

                log_out.Append("\n");

                await writeChar.WriteValueAsync(bytes, options);

                count -= packetSize;
                offset += packetSize;
            }

            inWrite = false;
        }

        public override void Close()
        {
            base.Close();
        }


        // BleStream specific methods
        // ==========================

        public async Task<bool> ConnectAsync()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(5);

            string serviceGuid = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
            string rxCharGuid = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";
            string txCharGuid = "6E400003-B5A3-F393-E0A9-E50E24DCCA9E";

            if (bleDevice == null) return false;

            // bleDevice.Connected += OnGattServerConnected;
            bleDevice.Disconnected += OnGattServerDisconnected;
            // bleDevice.ServicesResolved += OnGattServerServicesResolved;

            await bleDevice.ConnectAsync();
            await bleDevice.WaitForPropertyValueAsync("Connected", value: true, timeout);
            await bleDevice.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);

            IGattService1 service = await bleDevice.GetServiceAsync(serviceGuid);

            if(service == null) 
            {
                await bleDevice.DisconnectAsync();
                return false;
            }

            writeChar = await service.GetCharacteristicAsync(rxCharGuid);
            notifyChar = await service.GetCharacteristicAsync(txCharGuid);
           
            if(writeChar == null || notifyChar  == null) 
            {
                await bleDevice.DisconnectAsync();
                return false;
            }

            notifyChar.Value += OnCharacteristicValueChanged;
            await notifyChar.StartNotifyAsync();     

            // Peer interface responds "Warning, notification not enabled on the selected attribute" if not 
            // waiting >200ms before the first request is sent.
            // TODO: Find way how to wait for notifications to be available.
            await Task.Delay(500);

            connected = true;

            return true;
        }


        public async Task Disconnect()
        {
            Close();

            if (notifyChar != null)
            {
                notifyChar.Value -= OnCharacteristicValueChanged;
                await notifyChar.StopNotifyAsync();
                notifyChar = null;
            }
            
            bleDevice.Disconnected -= OnGattServerDisconnected;
         
            await bleDevice.DisconnectAsync();

            notifyChar = null;
            writeChar = null;
            connected = false;
            readSemaphore.WaitOne(0);
            readSemaphore.Release();
        }


        // Callback methods
        // ================

        private Task OnGattServerDisconnected(Device sender, BlueZEventArgs eventArgs)
        {
            Close();
            connected = false;
            readSemaphore.WaitOne(0);
            readSemaphore.Release();

            return Task.CompletedTask;
        }

        private Task OnCharacteristicValueChanged(GattCharacteristic characteristic, GattCharacteristicValueEventArgs e)
        {
            DateTime dateTime = DateTime.UtcNow;

            if (inOnCharacteristicValueChanged)
            {
                log_out.Append("simultaneous OnCharacteristicValueChanged requests not supported\n");
                throw new InvalidOperationException("simultaneous OnCharacteristicValueChanged requests not supported");
            }

            inOnCharacteristicValueChanged = true;

            if (log_direction != -1)   // inbound
            {
                log_direction = -1;
                log_byte_count = 0;
                log_out.Append("I ");
                log_out.Append(DateTime.Now.ToUniversalTime().ToString("o"));
                log_out.Append("\n");
            }

            log_out.Append(log_byte_count.ToString("x6"));
            log_out.Append(" ");

            foreach (byte b in e.Value)
            {
                log_out.Append(b.ToString("x2"));
                log_out.Append(" ");
                log_byte_count++;
            }

            log_out.Append("\n");

            lock (readQueue)
            {
                foreach (byte b in e.Value)
                {
                    readQueue.Enqueue(b);
                }

                readSemaphore.WaitOne(0);
                readSemaphore.Release();
            }

            inOnCharacteristicValueChanged = false;

            return Task.CompletedTask;
        }
    }
}
