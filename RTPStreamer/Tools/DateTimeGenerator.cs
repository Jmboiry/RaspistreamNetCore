/********
This work is an RTSP/RTP server in C# Net.Core by Jean-Marie Boiry <jean-marie.boiry@live.fr> 
**/
/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 3 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
**********/

using System;
using System.Net;
using System.Net.Sockets;

namespace RTPStreamer.Tools
{
	public class DateTimeGenerator
    {
        private static DateTimeGenerator myInstance;

        //-------------------------------------------------------------------------------------------------
        //--- Singleton, make sure that only 1 Instance of the class exists
        //-------------------------------------------------------------------------------------------------
        public static DateTimeGenerator Instance
        {
            get
            {
                if (myInstance == null)
                {
                    myInstance = new DateTimeGenerator();
                }

                return myInstance;
            }
        }

        //-------------------------------------------------------------------------------------------------
        //--- Private constructor to initialise the internal variables
        //-------------------------------------------------------------------------------------------------
        private DateTimeGenerator()
        {
        }       
        
        //-------------------------------------------------------------------------------------------------
        //--- Get the UTC time from a public NTP server
        //-------------------------------------------------------------------------------------------------
        public DateTime GetNTPTime()
        {
            const string myNTPServer = "pool.ntp.org";
            DateTime TheNetworkTime;

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var myNTPDataArray = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            myNTPDataArray[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            try
            {
                var myAddresses = Dns.GetHostEntry(myNTPServer).AddressList;

                //The UDP port number assigned to NTP is 123
                var myIPEndPoint = new IPEndPoint(myAddresses[0], 123);
				//NTP uses UDP
				using (var mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
				{
					//await Task.Run(() =>
					//    {
					mySocket.Connect(myIPEndPoint);

					//Stops code hang if NTP is blocked
					mySocket.ReceiveTimeout = 3000;

					mySocket.Send(myNTPDataArray);
					mySocket.Receive(myNTPDataArray);
					//}
					mySocket.Close();
				}
                 
                TheNetworkTime = ParseNetworkTime(myNTPDataArray);
            }
            catch (Exception ex)
            {
                TheNetworkTime = DateTime.UtcNow;
            }

            return TheNetworkTime;
        }

#region Helper Internal methods
        //-------------------------------------------------------------------------------------------------
        //--- Helper Internal methods
        //-------------------------------------------------------------------------------------------------
        private DateTime ParseNetworkTime(byte[] TheByteArray)
        {
            DateTime TheNetworkTime;
            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte TheServerReplyTime = 40;

            //Get the seconds part
            ulong TheIntPart = BitConverter.ToUInt32(TheByteArray, TheServerReplyTime);

            //Get the seconds fraction
            ulong TheFractPart = BitConverter.ToUInt32(TheByteArray, TheServerReplyTime + 4);

            //Convert From big-endian to little-endian
            TheIntPart = SwapEndianness(TheIntPart);
            TheFractPart = SwapEndianness(TheFractPart);

            var TheMilliseconds = (TheIntPart * 1000) + ((TheFractPart * 1000) / 0x100000000L);

            //**UTC** time
            TheNetworkTime = (new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)TheMilliseconds);

            //Adapt for empty ByteArray
            if (DateTime.Equals(TheNetworkTime, new DateTime(1900, 1, 1)))
                TheNetworkTime = DateTime.UtcNow;

            return TheNetworkTime;
        }


        // stackoverflow.com/a/3294698/162671
        private uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }

#endregion

    }
}
