using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace BLE_Advertise_Test
{
    class MoniMesg
    {
        /// <summary>
        /// 状態モニタに表示されるメッセージキュー
        /// </summary>
        public static Queue queue = new Queue();                        //モニタメッセージを保存するキュー
        public static AutoResetEvent qlock = new AutoResetEvent(true);  //キュー読み書き排他処理用

        //public static bool AlertFlag = false;                           //アラートメッセージの有る無しフラグ

        //public static void SetMessage(string mesg)
        //{
        //    MoniMesg.qlock.WaitOne();
        //    {
        //        MoniMesg.queue.Enqueue(mesg);
        //    }
        //    MoniMesg.qlock.Set();
        //}

        public static void SetBluetoothLEAdvertisementReceivedEventArgs(BluetoothLEAdvertisementReceivedEventArgs args)
        {
            MoniMesg.qlock.WaitOne();
            {
                MoniMesg.queue.Enqueue(args);
            }
            MoniMesg.qlock.Set();
        }

        public MoniMesg()
        {
            //qlock.WaitOne();
            //{
            //    queue.Enqueue("テスト");
            //}
            //qlock.Set();

            //if (queue.Count > 0)
            //{
            //    qlock.WaitOne();
            //    {
            //        string mess = (string)queue.Dequeue();
            //    }
            //    qlock.Set();
            //}
        }
    }
}
