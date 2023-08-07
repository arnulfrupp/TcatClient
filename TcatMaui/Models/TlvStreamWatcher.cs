using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TcatMaui.Models
{
    internal class TlvStreamWatcher
    {
        public delegate void TlvAvailableEventHandler(object sender, TlvAvailableEventArgs e);

        public class TlvAvailableEventArgs : EventArgs
        {
            public TlvAvailableEventArgs(TcatTlv aTlv) : base()
            {
                Tlv = aTlv;
            }

            public TcatTlv Tlv { get; private set; }
        }

        private Stream stream;
        private readonly byte[] tlvHeaderBuffer = new byte[2];
        private int writeIndex = 0;
        private TcatTlv nextTlv = null;

        public TlvStreamWatcher(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            WatchNext();
        }

        public void Detach()
        {
            stream = null;
            nextTlv = null;
        }

        private void ReadDataCallback(IAsyncResult aResult)
        {
            if (stream == null) return;

            int bytesRead = stream.EndRead(aResult);

            writeIndex += bytesRead;

            if (writeIndex != nextTlv.Data.Length)
            {
                stream.BeginRead(nextTlv.Data, writeIndex, nextTlv.Data.Length - writeIndex, new AsyncCallback(ReadDataCallback), null);
            }
            else
            {
                TlvAvailable?.Invoke(this, new TlvAvailableEventArgs(nextTlv));
                
                nextTlv = null;
                WatchNext();
            }
        }

        private void ReadExtHeaderCallback(IAsyncResult aResult)
        {
            if (stream == null) return;

            int bytesRead = stream.EndRead(aResult);

            writeIndex += bytesRead;

            if (writeIndex != 2)
            {
                stream.BeginRead(tlvHeaderBuffer, writeIndex, 2 - writeIndex, new AsyncCallback(ReadExtHeaderCallback), null);
            }
            else
            {
                nextTlv.Data = new byte[tlvHeaderBuffer[0] >> 8 + tlvHeaderBuffer[1]];
                writeIndex = 0;

                if (nextTlv.Data.Length > 0)
                {
                    stream.BeginRead(nextTlv.Data, writeIndex, nextTlv.Data.Length, new AsyncCallback(ReadDataCallback), null);
                }
                else
                {
                    TlvAvailable?.Invoke(this, new TlvAvailableEventArgs(nextTlv));
                    nextTlv = null;
                    WatchNext();
                }
            }
        }

        private void ReadHeaderCallback(IAsyncResult aResult)
        {
            if (stream == null) return;

            int bytesRead = stream.EndRead(aResult);

            writeIndex += bytesRead;

            if (writeIndex != 2)
            {
                stream.BeginRead(tlvHeaderBuffer, writeIndex, 2 - writeIndex, new AsyncCallback(ReadHeaderCallback), null);
            }
            else
            {
                nextTlv = new();

                nextTlv.Type = (TcatTlv.TcatTlvType)tlvHeaderBuffer[0];

                if (tlvHeaderBuffer[1] != 255)
                {
                    nextTlv.Data = new byte[tlvHeaderBuffer[1]];
                    writeIndex = 0;

                    if (nextTlv.Data.Length > 0)
                    {
                        stream.BeginRead(nextTlv.Data, writeIndex, nextTlv.Data.Length, new AsyncCallback(ReadDataCallback), null);
                    }
                    else
                    {
                        TlvAvailable?.Invoke(this, new TlvAvailableEventArgs(nextTlv));
                        nextTlv = null;
                        WatchNext();
                    }
                }
                else
                {
                    writeIndex = 0;
                    stream.BeginRead(tlvHeaderBuffer, writeIndex, 2, new AsyncCallback(ReadExtHeaderCallback), null);
                }
            }
        }
        protected void WatchNext()
        {
            if (stream == null) return;
            writeIndex = 0;
            stream.BeginRead(tlvHeaderBuffer, writeIndex, 2, new AsyncCallback(ReadHeaderCallback), null);
        }

        public event TlvAvailableEventHandler TlvAvailable;
    }
}
