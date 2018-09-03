using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth.Advertisement;

namespace BLE_Advertise_Test
{
    public partial class Form1 : Form
    {
        BluetoothLEAdvertisementWatcher advWatcher = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.advWatcher = new BluetoothLEAdvertisementWatcher();
            this.advWatcher.SignalStrengthFilter.SamplingInterval = TimeSpan.FromMilliseconds(2000);

            //// rssi >= -60のとき受信開始するっぽい
            //this.advWatcher.SignalStrengthFilter.InRangeThresholdInDBm = -60;
            //// rssi <= -65が2秒続いたら受信終わるっぽい
            //this.advWatcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -65;
            //this.advWatcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);

            this.advWatcher.Received += this.Watcher_Received;
            this.advWatcher.Start();

            Debug.WriteLine("advWatcher.Start");

        }

        private void Form1_Shown(object sender, EventArgs e)
        {

        }

        //        private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            //await this.Dispatcher.InvokeAsync(() => {
            //    // ここで処理
            //});

            Debug.WriteLine(DateTime.Now.ToString() + " Watcher_Received");

            MoniMesg.SetBluetoothLEAdvertisementReceivedEventArgs(args);

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.advWatcher.Received -= this.Watcher_Received;
            this.advWatcher.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            display();

            timer1.Enabled = true;
        }

        /// <summary>
        /// メッセージの表示を行います
        /// </summary>
        private void display()
        {
            //メッセージの書き込み
            if (MoniMesg.queue.Count > 0)
            {
                MoniMesg.qlock.WaitOne();
                {
                    BluetoothLEAdvertisementReceivedEventArgs args = (BluetoothLEAdvertisementReceivedEventArgs)MoniMesg.queue.Dequeue();

                    iBeacon bcon = new iBeacon(args);

                    //if (bcon.UUID != null && bcon.UUID=="UUID")
                    if (bcon.Name != null && bcon.Name == "EP")
                    {
                        // iBeacon
                        DateTimeOffset timestamp = args.Timestamp;
                        this.textBox1.AppendText(timestamp.ToString("HH\\:mm\\:ss\\.fff") + "\r\n");
                        this.textBox1.AppendText("Name= " + bcon.Name + "\r\n");
                        this.textBox1.AppendText("00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18 19\r\n");
                        this.textBox1.AppendText(bcon.mData + "\r\n");

                        this.textBox1.AppendText("温度= " + bcon.Temperature.ToString() + "\r\n");
                        this.textBox1.AppendText("湿度= " + bcon.Humidity.ToString() + "\r\n");
                        this.textBox1.AppendText("照度= " + bcon.Illuminance.ToString() + "\r\n");
                        this.textBox1.AppendText("気圧= " + bcon.Pressure.ToString() + "\r\n");

                        //DateTimeOffset timestamp = args.Timestamp;
                        string retBeaconData;
                        retBeaconData = "{";
                        retBeaconData += string.Format("uuid:'{0}'\r\n", bcon.UUID);//"00000000-0000-0000-0000-000000000000"
                        retBeaconData += string.Format("major:{0}\r\n", bcon.Major.ToString("D"));
                        retBeaconData += string.Format("minor:{0}\r\n", bcon.Minor.ToString("D"));
                        retBeaconData += string.Format("measuredPower:{0}\r\n", bcon.MeasuredPower.ToString("D"));
                        retBeaconData += string.Format("rssi:{0}\r\n", bcon.Rssi.ToString("D"));
                        retBeaconData += string.Format("accuracy:{0}\r\n", bcon.Accuracy.ToString("F6"));
                        retBeaconData += string.Format("proximity:'{0}'\r\n", bcon.Proximity);
                        retBeaconData += "}";

                        //this.textBox1.AppendText(timestamp.ToString("HH\\:mm\\:ss\\.fff") + ":\r\n" + retBeaconData + "\r\n");

                        if (textBox1.TextLength >= 32000)
                        {
                            //string str = textBox1.Text;
                            //string word = "-----\r\n";
                            //int len = str.LastIndexOf(word);
                            //str = str.Substring(len + word.Length);
                            //textBox1.Text = str;
                            textBox1.Text = "";
                        }
                    }
                }
                MoniMesg.qlock.Set();
            }
            else
            {
                //if (!MoniMesg.AlertFlag)
                //{
                //    lblMessage.Text = "";
                //}
            }
            Application.DoEvents();
        }
    }
}
