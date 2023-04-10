﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DirectThreadCommissioning.Models
{
    internal class TcatTlv
    {
        public enum TcatTlvType
        {
            Command                 = 0,
            Status                  = 1,
            ActiveDataset           = 2,
        };

        public enum TcatCommand
        {
            StopThread              = 0,
            StartThread             = 1
        };

        public enum TcatStatus
        {
            Ok                      = 0,
            Error                   = 1
        };

        public enum MeshCopTlvType
        {
            Channel                 = 0,
            PanId                   = 1,
            ExtPanId                = 2,
            NetworkName             = 3,
            PKSc                    = 4,
            NetworkKey              = 5,
            NetworkKeySeqCounter    = 6,
            NetworkMeshLocalPrefix  = 7,
            SteeringData            = 8,
            BorderAgentLocator      = 9,
            CommissionerId          = 10,
            CommissionerSessionId   = 11,
            SecurityPolicy          = 12,
            GetTlv                  = 13,
            ActiveTimestamp         = 14,
            CommissionerUdpPort     = 15,
            StateTlv                = 16,
            JoinerDtlsEncapsulation = 17,
            JoinerUdpPort           = 18,
            JoinerIID               = 19,
            JoinerRouterLocator     = 20,
            JoinerROuterKEK         = 21,
            ProvisioningUrl         = 32,
            VendorName              = 33,
            VendorModel             = 34,
            VendorSwVersion         = 35,
            VendorData              = 36,
            VendorStackVersion      = 37,
            UdpEncapsulation        = 48,
            Ipv6Address             = 49,
            PendingTimestamp        = 51,
            DelayTimer              = 52,
            ChannelMask             = 53,
            Counter                 = 54,
            Period                  = 55,
            ScanDuration            = 56,
            EnergyList              = 57,
            SecureDissimination     = 58,
            ThreadDomainName        = 59,
            DomainPrefix            = 60,
            AeSteeringData          = 61,
            NkpSteeringData         = 62,
            CommissionerToken       = 63,
            CommissionerSignature   = 64,
            AeUdpPort               = 65,
            NkpUdpPort              = 66,
            TriHostName             = 67,
            RegistrarIPv6Address    = 68,
            RegistrarHostName       = 69,
            CommissionerPenSignature= 70,
            CommissionerPenToken    = 71,
            DiscoverRequest         = 128,
            DiscoverResponse        = 129
        };

        private static int[] kMeshCopTlvTlvLen = {  3,  2,  8,-16,-16, 16,  4,  8,-16,  2,
                                                  -64,  2,  4,  0,  8,  2,  1,  0,  2,  8,
                                                    2, 16,999,999,999,999,999,999,999,999,
                                                  999,999,-64,-32,-32,-16,-64,  6,999,999,
                                                  999,999,999,999,999,999,999,999,  0, 16,
                                                  999,  8,  4,  0,  1,  2,  2,  0,  0,-16,
                                                  999, -16,-16,  0,  0, 2,  2,-254, 0,-254,
                                                    0,  0,999,999,999,999,999,999,999,999,
                                                  999,999,999,999,999,999,999,999,999,999,
                                                  999,999,999,999,999,999,999,999,999,999,
                                                  999,999,999,999,999,999,999,999,999,999,
                                                  999,999,999,999,999,999,999,999,999,999,
                                                  999,999,999,999,999,999,999,999,  2, 2  };

        TcatTlvType type;
        byte[] data;

        public byte[] Data { get => data; set => data = value; }
        public TcatTlvType Type { get => type; set => type = value; }

        public TcatTlv(TcatCommand aTcatCommand)
        {
            type = TcatTlvType.Command;
            data = new byte[] { (byte)aTcatCommand };
        }

        public TcatTlv(TcatStatus aStatus)
        {
            type = TcatTlvType.Status;
            data = new byte[] { (byte)aStatus };
        }

        public TcatTlv(TcatTlvType aTcatTlvType, MeshCopTlvType aMeshCopTlvType, byte[] aMeshCopData)
        {
            if ((int)aMeshCopTlvType > 129) throw new InvalidEnumArgumentException("MeshCoP TLV type");

            int size = kMeshCopTlvTlvLen[(int)aMeshCopTlvType];

            if (size == 999) throw new InvalidEnumArgumentException("MeshCoP TLV type not allowed");
            if (size == 0) size = aMeshCopData.Length;
  
            if (size < 0)
            {
                if(aMeshCopData.Length > -size) throw new InvalidDataException("MeshCoP TLV too long");
                size = aMeshCopData.Length;
            }

            if (size > 254) throw new NotImplementedException("MeshCoP TLV >254 bytes");

            if (aMeshCopData.Length != size) throw new InvalidDataException("MeshCoP TLV size not matching");

            type = aTcatTlvType;
            data = new byte[size + 2];
            data[0] = (byte)aMeshCopTlvType;
            data[1] = (byte)size;
            data.CopyTo(aMeshCopData, 2);
        }

        public TcatTlv(TcatTlvType aTcatTlvType, MeshCopTlvType aMeshCopTlvType)
        {
            if ((int)aMeshCopTlvType > 129) throw new InvalidEnumArgumentException("MeshCoP TLV type");

            int size = kMeshCopTlvTlvLen[(int)aMeshCopTlvType];

            if(size == 999 || size <= 0) throw new InvalidDataException("MeshCoP TLV type unknown");

            type = aTcatTlvType;
            data = new byte[size + 2];
            data[0] = (byte)aMeshCopTlvType;
            data[1] = (byte)size;
        }

        public TcatTlv(ushort aChannel, ushort aPanId, byte[] aNetworkkey)
        {
            type = TcatTlvType.ActiveDataset;
            data = Array.Empty<byte>();

            if (aChannel < 11 || aChannel > 26) throw new InvalidDataException("invalid network channel");
            if (aPanId == 0xFFFF) throw new InvalidDataException("pan id cannot be 0xFFFF");
            if (aNetworkkey.Length != 16) throw new InvalidDataException("networkkey must be 16 bytes");

            TcatTlv channelTlv = new(type, MeshCopTlvType.Channel);
            TcatTlv panIdTlv = new(type, MeshCopTlvType.PanId);
            TcatTlv networkkeyTlv = new(type, MeshCopTlvType.NetworkKey);

            channelTlv.data[2] = 0;   // channel page
            channelTlv.data[3] = (byte)(aChannel >> 8);
            channelTlv.data[4] = (byte)aChannel;
            panIdTlv.data[2] = (byte)(aPanId >> 8);
            panIdTlv.data[3] = (byte)aPanId;
            networkkeyTlv.data.CopyTo(aNetworkkey, 2);

            Merge(channelTlv);
            Merge(panIdTlv);
            Merge(networkkeyTlv);
        }

        public void Merge(TcatTlv aTcatTlv)
        {
            if (aTcatTlv.type != type) throw new InvalidDataException("Cannot merge data from two different TLV types");

            byte[] bytes = new byte[data.Length + aTcatTlv.data.Length];
            bytes.CopyTo(data, 0);
            bytes.CopyTo(aTcatTlv.data, data.Length);
            data = bytes;
        }

        public byte[] GetBytes()
        {
            int headerLen = data.Length > 254 ? 4 : 2;
            byte[] bytes = new byte[data.Length + headerLen];

            bytes[0] = (byte)type;

            if (data.Length == 4)
            {
                bytes[1] = 255;
                bytes[2] = (byte)(data.Length >> 8);
                bytes[3] = (byte)data.Length;
            }
            else bytes[1] = (byte)data.Length;
            
            bytes.CopyTo(data, headerLen);

            return bytes;
        }
    }
}
