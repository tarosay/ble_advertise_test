using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace BLE_Advertise_Test
{
    public class iBeacon
    {
        private const int MinimumLengthInBytes = 25;//最小の長さ
        private const int AdjustedLengthInBytes = -2;//CompanyID分の2桁ずれている為読み取り位置補正

        //プロパティ
        public string Name { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public BluetoothLEAdvertisementType AdvertisementType { get; set; }

        public int ManufacturerId { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public string UUID { get; set; }
        public string mData { get; set; }
        public short Rssi { get; set; }
        public short MeasuredPower { get; set; }
        public double ManufacturerReserved { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Illuminance { get; set; }
        public double Pressure { get; set; }

        //精度（accuracy）
        public double Accuracy
        {
            get { return calcAccuracy(MeasuredPower, Rssi); }
        }

        //近接度（Proximity）：近接（immidiate）、1m以内（near）、1m以遠（far）、不明（Unknown）
        public string Proximity
        {
            get
            {

                string _Proximity = "Unknown";

                //Rssi未取得ならUnknown
                if (Rssi == 0) { return _Proximity; }

                //rssi値からProximityを判別
                if (Rssi > -40)
                {
                    _Proximity = "immidiate";//近接
                }
                else if (Rssi > -59)
                {
                    _Proximity = "near";//1m以内
                }
                else
                {
                    _Proximity = "far";//1m以遠
                }
                return _Proximity;
            }
        }

        //コンストラクタ
        public iBeacon()
        {
            ManufacturerId = -1;
            Major = -1;
            Minor = -1;
            Rssi = 0;
            UUID = "";
            MeasuredPower = -1;
            ManufacturerReserved = -1.0;

            Temperature = 0.0;
            Humidity = 0.0;
            Illuminance = 0.0;
            Pressure = 0.0;
        }

    //コンストラクタ２
    public iBeacon(BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {

            //出力されているbyteデータから各値を抽出する
            var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            Timestamp = eventArgs.Timestamp;
            AdvertisementType = eventArgs.AdvertisementType;

            //var md = eventArgs.Advertisement.ManufacturerData.FirstOrDefault();
            //var data1 = new byte[md.Data.Length];
            //var manufacturerData1 = eventArgs.Advertisement.ManufacturerData[1];


            Debug.WriteLine("manufacturerSections.Count= " + manufacturerSections.Count.ToString());

            if (manufacturerSections.Count > 0)
            {
                var manufacturerData = manufacturerSections[0];
                var data = new byte[manufacturerData.Data.Length];

                iBeacon bcon = new iBeacon();

                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(data);
                }

                Name = eventArgs.Advertisement.LocalName;
                Debug.WriteLine(Name);


                //長さをチェック
                if (data == null || data.Length < MinimumLengthInBytes + AdjustedLengthInBytes)
                {
                    if (Name == "EP")
                    {
                        mData = BitConverter.ToString(data);

                        Temperature = ((double)data[1] + (double)data[2] * 256.0) / 100.0;
                        Humidity = ((double)data[3] + (double)data[4] * 256.0) / 100.0;
                        Illuminance = ((double)data[5] + (double)data[6] * 256.0);
                        Pressure = ((double)data[9] + (double)data[10] * 256.0) / 10.0;
                    }
                    return;
                }

                //イベントから取得
                Rssi = eventArgs.RawSignalStrengthInDBm;
                //Name = eventArgs.Advertisement.LocalName;
                ManufacturerId = manufacturerData.CompanyId;

                //バイトデータから抽出
                //公式での出力値（Windowsでは2byteずれているので補正が必要）
                // Byte(s)  WinByte(s) Name
                // --------------------------
                // 0-1      none       Manufacturer ID (16-bit unsigned integer, big endian)
                // 2-3      0-1        Beacon code (two 8-bit unsigned integers, but can be considered as one 16-bit unsigned integer in little endian)
                // 4-19     2-17       ID1 (UUID)
                // 20-21    18-19      ID2 (16-bit unsigned integer, big endian)
                // 22-23    20-21      ID3 (16-bit unsigned integer, big endian)
                // 24       22         Measured Power (signed 8-bit integer)
                // 25       23         Reserved for use by the manufacturer to implement special features (optional)

                //BigEndianの値を取得
                UUID = BitConverter.ToString(data, 4 + AdjustedLengthInBytes, 16); // Bytes 2-17
                MeasuredPower = Convert.ToSByte(BitConverter.ToString(data, 24 + AdjustedLengthInBytes, 1), 16); // Byte 22

                //もし追加データがあればここで取得
                if (data.Length >= MinimumLengthInBytes + AdjustedLengthInBytes + 1)
                {
                    ManufacturerReserved = data[25 + AdjustedLengthInBytes]; // Byte 23
                }

                //.NET FramewarkのEndianはCPUに依存するらしい
                if (BitConverter.IsLittleEndian)
                {
                    //LittleEndianの値を取得
                    byte[] revData;

                    revData = new byte[] { data[20 + AdjustedLengthInBytes], data[21 + AdjustedLengthInBytes] };// Bytes 18-19
                    Array.Reverse(revData);
                    Major = BitConverter.ToUInt16(revData, 0);

                    revData = new byte[] { data[22 + AdjustedLengthInBytes], data[23 + AdjustedLengthInBytes] };// Bytes 20-21
                    Array.Reverse(revData);
                    Minor = BitConverter.ToUInt16(revData, 0);
                }
                else
                {
                    //BigEndianの値を取得
                    Major = BitConverter.ToUInt16(data, 20 + AdjustedLengthInBytes); // Bytes 18-19
                    Minor = BitConverter.ToUInt16(data, 22 + AdjustedLengthInBytes); // Bytes 20-21
                }
            }
            else
            {
                new iBeacon();
            }
        }

        //精度を計算する
        protected static double calcAccuracy(short measuredPower, short rssi)
        {
            if (rssi == 0)
            {
                return -1.0; //nodata return -1.
            }

            double ratio = rssi * 1.0 / measuredPower;
            if (ratio < 1.0)
            {
                return Math.Pow(ratio, 10);
            }
            else
            {
                double accuracy = (0.89976) * Math.Pow(ratio, 7.7095) + 0.111;
                return accuracy;
            }
        }

    }
}
