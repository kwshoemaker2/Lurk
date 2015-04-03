using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace LurkClient
{
    /// <summary>
    /// Class for globals variables and functions used throughout the program
    /// </summary>
    static class Globals
    {
        /// <summary>
        /// Socket used for communication with the Lurk server
        /// </summary>
        static private Socket cs = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// DataParser object for parsing data from the server
        /// </summary>
        static private DataParser dp = new DataParser();

        static public Socket ClientSocket
        {
            get
            {
                return cs;
            }
        }

        /// <summary>
        /// The main window for the application
        /// </summary>
        static public MainWindow MainWin;

        /// <summary>
        /// Sends data to the Lurk server
        /// </summary>
        /// <param name="data">
        /// Message to send to the Lurk serverr
        /// </param>
        /// <returns>
        /// return the number of bytes sent
        /// </returns>
        static public int SendData(string data)
        {
            Byte[] toSend = Encoding.UTF8.GetBytes(data);
            int sent = ClientSocket.Send(toSend);
            return sent;
        }

        /// <summary>
        /// Receive amount bytes of data from the Lurk server
        /// </summary>
        /// <param name="amount">
        /// Number of bytes to receive
        /// </param>
        /// <returns>
        /// The data received from the server
        /// </returns>
        static public string ReceiveData(int amount)
        {
            Byte[] received = new Byte[amount];
            int recvd = ClientSocket.Receive(received);
            return Encoding.UTF8.GetString(received).Substring(0, recvd);
        }

        /// <summary>
        /// Parses data received from the server
        /// </summary>
        /// <seealso cref="DataParser.ParseData"/>
        static public void ParseData(string data, List<List<string>> blocks)
        {
            int toRead = dp.ParseData(data, blocks);
            if (toRead > 0)
            {
                string resp = Globals.ReceiveData(toRead);
                toRead = dp.ContinueParse(resp, blocks);
            }
        }
    }
}