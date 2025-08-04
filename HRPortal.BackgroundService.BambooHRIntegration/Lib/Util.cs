using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace HRPortal.BackgroundService.BambooHRIntegration
{
    class Util
    {
        public static string GetLocalIPAddress()
        {
            string localIP = "";
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                string dummyIP = "8.8.8.8";
                socket.Connect(dummyIP, 80); //連到google的DNS(Port隨便打的)，只是一個dummy的動作，目的是要取得目前用來連結的IP
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }
    }
}
