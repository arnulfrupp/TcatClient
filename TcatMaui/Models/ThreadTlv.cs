using System.ComponentModel;


namespace TcatMaui.Models
{
    internal class TcatTlv
    {
        /**
         * TCAT TLV Types.
         * 
         */

        public enum TcatTlvType
        {
            // Command Class General
            Undefined                       = 0,    // undefiened TCAT TLV
            ResponseWithStatus              = 1,    // TCAT response with status value TLV
            ResponseWithPayload             = 2,    // TCAT response with payload TLV
            ResponseEvent                   = 3,    // TCAT response event TLV (reserved)
            GetNetworkName                  = 8,    // TCAT network name query TLV
            Disconnect                      = 9,    // TCAT disconnect request TLV
            Ping                            = 10,   // TCAT ping request TLV
            GetDeviceId                     = 11,   // TCAT device ID query TLV
            GetExtendedPanID                = 12,   // TCAT extended PAN ID query TLV
            PresentPskdHash                 = 16,   // TCAT commissioner rights elevation request TLV using PSKd hash
            PresentPskcHash                 = 17,   // TCAT commissioner rights elevation request TLV using PSKc hash
            PresentInstallCodeHash          = 18,   // TCAT commissioner rights elevation request TLV using install code
            RequestRandomNumChallenge       = 19,   // TCAT random number challenge query TLV
            RequestPskdHash                 = 20,   // TCAT PSKd hash request TLV

            // Command Class Commissioning
            SetActiveOperationalDataset     = 32,   // TCAT active operational dataset TLV
            SetActiveOperationalDataset1    = 33,   // TCAT active operational dataset alterative #1 TLV
            GetProvissioningTlvs            = 36,   // TCAT provisioning TLVs query TLV
            GetCommissionerCertificate      = 37,   // TCAT commissioner certificate query TLV
            GetDiagnosticTlvs               = 38,   // TCAT diagnostics TLVs query TLV
            StartThreadInterface            = 39,   // TCAT start thread interface request TLV
            StopThreadInterface             = 40,   // TCAT stop thread interface request TLV

            // Command Class Extraction
            GetActiveOperationalDataset     = 64,   // TCAT active oerational dataset query TLV
            GetActiveOperationalDataset1    = 65,   // TCAT active oerational dataset alterative #1 query TLV

            // Command Class Decommissioning
            Decommission                    = 96,   // TCAT decommission request TLV

            // Command Class Application
            SelectApplicationLayerUdp       = 128,  // TCAT select UDP protocol application layer request TLV
            SelectApplicationLayerTcp       = 129,  // TCAT select TCP protocol application layer request TLV
            SendApplicationData             = 130,  // TCAT send application data TLV
            SendVendorSpecificData          = 159,  // TCAT send vendor specific command or data TLV

            // Command Class CCM
            SetLDevIdOperationalCert        = 160,  // TCAT LDevID operational certificate TLV
            SetLDevIdPrivateKey             = 161,  // TCAT LDevID operational certificate pricate key TLV
            SetDomainCaCert                 = 162,  // TCAT domain CA certificate TLV
        };


        /**
         * TCAT Response Types.
         *
         */

        public enum TcatStatus
        {
            TcatSuccess         = 0,            // Command or request was successfully processed
            TcatUnsupported     = 1,            // Requested command or received TLV is not supported
            TcatParseError      = 2,            // Request / command could not be parsed correctly
            TcatValueError      = 3,            // The value of the transmitted TLV has an error
            TcatGeneralError    = 4,            // An error not matching any other category occurred
            TcatBusy            = 5,            // Command cannot be executed because the resource is busy
            TcatUndefined       = 6,            // The requested value, data or service is not defined (currently) or not present
            TcatHashError       = 7,            // The hash value presented by the commissioner was incorrect
            TcatUnauthorized    = 8,            // Sender does not have sufficient authorization for the given command
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

        // MeshCoP TLV lenght n = kMeshCopTlvTlvLen[type]
        // n = 999  : TLV type invalid
        // n > 0    : TLV data length = n
        // n < 0    : TLV data length <= -n
        // n = 0    : any TLV data lenght 
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


        /**
         * Create empty TCAT TLV
         *
         */

        public TcatTlv()
        {
            type = TcatTlvType.Undefined;
            data = Array.Empty<byte>();
        }


        /**
         * Create TCAT TLV without data
         *
         */

        public TcatTlv(TcatTlvType aTcatTlvType)
        {
            type = aTcatTlvType;
            data = Array.Empty<byte>();
        }

        /**
         * Create TCAT TLV with data
         *
         */

        public TcatTlv(TcatTlvType aTcatTlvType, byte[] aData)
        {
            type = aTcatTlvType;
            data = aData;
        }


        /**
         * Create TCAT response TLV with status
         *
         */

        public TcatTlv(TcatStatus aResponse)
        {
            type = TcatTlvType.ResponseWithStatus;
            data = new byte[] { (byte)aResponse };
        }


        /**
         * Create TCAT TLV with a MeshCoP having arbitrary data payload
         *
         */

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
            aMeshCopData.CopyTo(data, 2);
        }


        /**
         * Create TCAT TLV with a MeshCoP TLV having a fixed data lenght
         *
         */

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


        /**
         * Create TCAT TLV with MeshCoP TLVs setting the channel, PANID and Network Key.
         *
         */

        public TcatTlv(ushort aChannel, ushort aPanId, byte[] aNetworkkey)
        {
            type = TcatTlvType.SetActiveOperationalDataset;
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
            aNetworkkey.CopyTo(networkkeyTlv.data, 2);

            Merge(channelTlv);
            Merge(panIdTlv);
            Merge(networkkeyTlv);
        }


        /**
         * Merge two TCAT TLVs of the same type into one with combined payload
         * This methord is typically used to join two TCAT TLVs containing MeshCoP TLVs as payload
         *
         */

        public void Merge(TcatTlv aTcatTlv)
        {
            if (aTcatTlv.type != type) throw new InvalidDataException("Cannot merge data from two different TLV types");

            byte[] bytes = new byte[data.Length + aTcatTlv.data.Length];
            data.CopyTo(bytes, 0);
            aTcatTlv.data.CopyTo(bytes, data.Length);
            data = bytes;
        }


        /**
         * Find a specific MeshCoP TLV inside a TCAT TLV payload and extract the value
         *
         */

        public byte?[] FindTlv(MeshCopTlvType aMeshCopTlvType)
        {
            int index = 0;

            if(type != TcatTlvType.SetActiveOperationalDataset) throw new InvalidDataException("Encoded TCAT type does not contain MeshCoP TLVs");

            while(index + 1 < data.Length)
            {
                if (data[index] == (byte)aMeshCopTlvType)
                {
                    int len = data[index + 1];
                    byte?[] result = new byte?[len];

                    index += 2;

                    if (len == 255) throw new InvalidDataException("Malformed MeshCoP TLV (loo long)");
                    if (data.Length  < index + len) throw new InvalidDataException("Malformed MeshCoP TLV");

                    for(int i = 0; i < len; i++) 
                    {
                        result[i] = data[index + i];
                    }

                    return result;
                }
            }

            return null;
        }


        /**
         * Find a specific MeshCoP TLV inside a TCAT TLV payload and extract the value as unsigned short
         *
         */

        public ushort? FindTlvAsUShort(MeshCopTlvType aMeshCopTlvType)
        {
            byte?[] result = FindTlv(aMeshCopTlvType);

            if (result == null) return null;
            if (result.Length != 2) throw new InvalidDataException("MeshCoP TLV does not contain unsigned short value");

            return (ushort?)(result[0] >> 8 + result[1]);
        }


        /**
         * Get the TLV byte array
         *
         */

        public byte[] GetBytes()
        {
            int headerLen = data.Length > 254 ? 4 : 2;
            byte[] bytes = new byte[data.Length + headerLen];

            bytes[0] = (byte)type;

            if (headerLen == 4)
            {
                bytes[1] = 255;
                bytes[2] = (byte)(data.Length >> 8);
                bytes[3] = (byte)data.Length;
            }
            else bytes[1] = (byte)data.Length;
            
            data.CopyTo(bytes, headerLen);

            return bytes;
        }
    }
}
