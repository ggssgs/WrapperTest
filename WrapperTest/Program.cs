using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHapticsWrapper;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WrapperTest
{
    class Program
    {
        static bool isFollowing = false;
        static OpenHaptics openHaptics = new OpenHaptics(true, false);
        static MqttClient client = new MqttClient("13.228.3.82");
        //static MqttClient client = new MqttClient("155.69.21.187");
        //static MqttClient client = new MqttClient("116.197.193.105");
        static string outTopic = "Haptic/Demo";
        static string inTopic = "Haptic/Force";
        static int counter = 0;
        static byte[] outData = new byte[8 * sizeof(double) + sizeof(bool)];

        static void ShowPosition()
        {
            //Console.WriteLine("From callback: " + openHaptics.outRotX.ToString("+000.00;-000.00") + " " + openHaptics.outRotY.ToString("+000.00;-000.00") + " " + openHaptics.outRotZ.ToString("+000.00;-000.00"));
            //Console.WriteLine(openHaptics.outX.ToString() + " " + openHaptics.outY.ToString() + " " + openHaptics.outZ.ToString() + " " + openHaptics.outRotHandleX.ToString() + " " + openHaptics.outRotHandleY.ToString() + " " + openHaptics.outRotX.ToString() + " " + openHaptics.outRotY.ToString() + " " + openHaptics.outRotZ.ToString() + " " + openHaptics.bButtonState.ToString());
        }

        static void SendData()
        {
            //client.Publish(topic, Encoding.ASCII.GetBytes(openHaptics.jointAngle1.ToString("+000.00;-000.00") + " " + openHaptics.jointAngle2.ToString("+000.00;-000.00") + " " + openHaptics.jointAngle3.ToString("+000.00;-000.00") + " " + openHaptics.outRotX.ToString() + " " + openHaptics.outRotY.ToString() + " " + openHaptics.outRotZ.ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            //client.Publish(outTopic, Encoding.ASCII.GetBytes(openHaptics.outX.ToString() + " " + openHaptics.outY.ToString() + " " + openHaptics.outZ.ToString() + " " + openHaptics.outRotHandleX.ToString() + " " + openHaptics.outRotHandleY.ToString() + " " + openHaptics.outRotX.ToString() + " " + openHaptics.outRotY.ToString() + " " + openHaptics.outRotZ.ToString() + " " + openHaptics.bButtonState.ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            
            BitConverter.GetBytes(openHaptics.outX).CopyTo(outData, 0);
            BitConverter.GetBytes(openHaptics.outY).CopyTo(outData, 1 * sizeof(double));
            BitConverter.GetBytes(openHaptics.outZ).CopyTo(outData, 2 * sizeof(double));
            BitConverter.GetBytes(openHaptics.outRotHandleX).CopyTo(outData, 3 * sizeof(double));
            BitConverter.GetBytes(openHaptics.outRotHandleY).CopyTo(outData, 4 * sizeof(double));
            BitConverter.GetBytes(openHaptics.outRotX).CopyTo(outData, 5 * sizeof(double));
            BitConverter.GetBytes(openHaptics.outRotY).CopyTo(outData, 6 * sizeof(double));
            BitConverter.GetBytes(openHaptics.outRotZ).CopyTo(outData, 7 * sizeof(double));
            BitConverter.GetBytes(openHaptics.bButtonState).CopyTo(outData, 8 * sizeof(double));
            
            client.Publish(outTopic, outData, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            //Console.WriteLine(counter++);
        }

        static void Test()
        {
            client.Publish(outTopic, Encoding.ASCII.GetBytes("1111111111111111111111111111111111111111"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        static void Main(string[] args)
        {
            if (isFollowing)
            {
                client.Connect("HapticFollower");
                client.Subscribe(new string[] { outTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceivedFollow;
                openHaptics.InitDevice();
                openHaptics.userFunctionHandler = SendData;
            }
            else
            {
                client.Connect("HapticSender");
                client.Subscribe(new string[] { inTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                openHaptics.InitDevice();
                openHaptics.userFunctionHandler = SendData;
                //openHaptics.userFunctionHandler = Test;
            }

            openHaptics.Run();
            Console.Read();
        }

        private static void Client_MqttMsgPublishReceivedFollow(object sender, MqttMsgPublishEventArgs e)
        {
            openHaptics.inX = BitConverter.ToDouble(e.Message, 0);
            openHaptics.inY = BitConverter.ToDouble(e.Message, 1 * sizeof(double))/* + 200*/;
            openHaptics.inZ = BitConverter.ToDouble(e.Message, 2 * sizeof(double));
            openHaptics.inRotX = BitConverter.ToDouble(e.Message, 5 * sizeof(double));
            openHaptics.inRotY = BitConverter.ToDouble(e.Message, 6 * sizeof(double)) + 0.2;
            openHaptics.inRotZ = BitConverter.ToDouble(e.Message, 7 * sizeof(double)); ;

            //Console.WriteLine(message);
            Console.WriteLine(openHaptics.inX.ToString() + " " + openHaptics.inY.ToString() + " " + openHaptics.inZ.ToString() + " " + openHaptics.inRotX.ToString() + " " + openHaptics.inRotY.ToString() + " " + openHaptics.inRotZ.ToString());
        }
        
        private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            /*
            openHaptics.inForceX = BitConverter.ToDouble(e.Message, 0);
            openHaptics.inForceY = BitConverter.ToDouble(e.Message, 1 * sizeof(double));
            openHaptics.inForceZ = BitConverter.ToDouble(e.Message, 2 * sizeof(double));
            openHaptics.isForceInput = BitConverter.ToBoolean(e.Message, 3 * sizeof(double));
            */
            openHaptics.inX = BitConverter.ToDouble(e.Message, 0);
            openHaptics.inY = BitConverter.ToDouble(e.Message, 1 * sizeof(double));
            openHaptics.inZ = BitConverter.ToDouble(e.Message, 2 * sizeof(double));

            Console.WriteLine(openHaptics.inX.ToString() + " " + openHaptics.inY.ToString() + " " + openHaptics.inZ.ToString() + " " + openHaptics.isForceInput.ToString());
            //Console.WriteLine(Encoding.ASCII.GetString(e.Message));
        }
    }
}
