using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkClient
{
    /// <summary>
    /// A class for parsing messages from the Lurk server
    /// </summary>
    /// <remarks>
    /// Will probably replace this class with regular expressions to simplify parsing
    /// </remarks>
    class DataParser
    {
        /// <summary>
        /// The message headers that will come from the server
        /// </summary>
        static string[] HEADERS = { "MESSG", "ACEPT", "REJEC", "RESLT", "INFOM", "NOTIF" };

        /// <summary>
        /// The data currently being parsed
        /// </summary>
        private string data = "";
        
        /// <summary>
        /// The parsed blocks of data. Each block has a protocol header
        /// and then data associated with that header
        /// </summary>
        private List<List<string>> blocks = new List<List<string>>();

        /// <summary>
        /// The current block of data being parsed
        /// </summary>
        private List<string> curBlock = new List<string>();

        /// <summary>
        /// The current index of the parser
        /// </summary>
        private int curInd = 0;

        /// <summary>
        /// The current header for the block we are parsing
        /// </summary>
        private string curHeader = "";

        /// <summary>
        /// Number of characters that still need to be read (not provided in original data)
        /// </summary>
        int toRead = 0;

        public DataParser() { }

        /// <summary>
        /// Sets up the parser for parsing dat and parses it. Puts the blocks of data into
        /// blocks_ if no more characters need to be read
        /// </summary>
        /// <param name="dat">
        /// The data that needs to be parsed
        /// </param>
        /// <param name="blocks_">
        /// The place to store the blocks of data after parsing is complete
        /// </param>
        /// <returns>
        /// Amount of characters that need to be parsed that was missing from dat
        /// </returns>
        public int ParseData(string dat, List<List<string>> blocks_)
        {
            reset();
            data = dat;
            startParse();
            if(toRead <= 0)
                blocks_.AddRange(blocks);

            return toRead;
        }

        /// <summary>
        /// Continue the parsing using dat and put the parsed data into blocks_
        /// if no more characters need to be read
        /// </summary>
        /// <param name="dat">
        /// The data that needs to be parsed
        /// </param>
        /// <param name="blocks_">
        /// The place to store the blocks of data after parsing is complete</param>
        /// <returns>
        /// The number of characters that need to be read
        /// </returns>
        public int ContinueParse(string dat, List<List<string>> blocks_)
        {
            data += dat;
            toRead -= dat.Length;
            curBlock[1] += dat;
            if (toRead <= 0)
            {
                blocks.Add(curBlock);
                blocks_.AddRange(blocks);
            }
            return toRead;
        }

        /// <summary>
        /// Helper method for the ParseData method
        /// Parses the provided data into blocks
        /// </summary>
        private void startParse()
        {
            while (curInd < data.Length)
            {
                // Headers are always five characters long
                curHeader = data.Substring(curInd, 5);
                curInd += 5;

                if (curHeader == "INFOM")
                {
                    curBlock.Add(curHeader);
                    parseINFOM();
                }

                // MESSGs are parsed in a special way to avoid a bug that allows users to send server messages to a player
                else if (curHeader == "MESSG")
                {
                    curBlock.Add(curHeader);
                    curBlock.Add(data.Substring(curInd));
                    blocks.Add(curBlock);
                    break;
                }

                else if (HEADERS.Contains<string>(curHeader))
                {
                    curBlock.Add(curHeader);
                    parseOthers();
                }

                else
                {
                    System.Console.WriteLine("WEIRD INVALID HEADER STARTING AT " + curInd.ToString());
                }
            }
        }

        /// <summary>
        /// Parses out the number for the size of the INFOM message
        /// and parses the rest of that message into a block
        /// </summary>
        private void parseINFOM()
        {
            string intBuff = "";
            curInd += 1; // skip space
            bool done = false;
            char curChar;
            // parse out the message size
            while (curInd < data.Length && !done)
            {
                curChar = data[curInd];
                int n = 0;
                if (int.TryParse(curChar.ToString(), out n))
                {
                    intBuff += curChar;
                    curInd += 1;
                }
                else
                    done = true;
            }
            int needToRead = int.Parse(intBuff);
            int leftover = needToRead - (data.Length - curInd);

            // we've already parsed all the data we need for this INFOM
            if (leftover <= 0)
            {
                curBlock.Add(data.Substring(curInd, needToRead));
                blocks.Add(curBlock);
                curBlock = new List<string>();
                curInd += needToRead;
            }

            // more data needs to be read
            else
            {
                curBlock.Add(data.Substring(curInd, needToRead - leftover));
                toRead = leftover;
                curInd += (needToRead - leftover);
            }
        }

        /// <summary>
        /// Parses server messages for any other header besides INFOM or MESSG
        /// </summary>
        private void parseOthers()
        {
            string chunkBuff = "";
            bool done = false;
            curBlock.Add("");
            char curChar;
            curInd += 1;
            while (curInd < data.Length && !done)
            {
                curChar = data[curInd];
                if (curChar == ' ' && chunkBuff.Length > 0)
                {
                    if (chunkBuff.Length >= 5 && HEADERS.Contains<string>(chunkBuff.Substring(0, 5))) // first five characters of chunk is a header
                    {
                        done = true;
                        curInd -= chunkBuff.Length;
                        chunkBuff = chunkBuff.Substring(5);
                    }
                    else if (chunkBuff.Length >= 5 && HEADERS.Contains<string>(chunkBuff.Substring(chunkBuff.Length - 5))) // last five characters of a chunk is a header
                    {
                        done = true;
                        curInd -= 5;
                        chunkBuff = chunkBuff.Substring(0, chunkBuff.Length - 5);
                    }

                    else
                    {
                        curBlock[1] += chunkBuff + " ";
                        chunkBuff = "";
                        curInd += 1;
                    }
                }

                else
                {
                    chunkBuff += curChar;
                    curInd += 1;
                }
            }

            curBlock[1] += chunkBuff;
            blocks.Add(curBlock);
            curBlock = new List<string>();
        }

        /// <summary>
        /// Reset all of the variables back to their initial state to prepare
        /// for a new set of data to parse
        /// </summary>
        private void reset()
        {
            data = "";
            blocks = new List<List<string>>();
            curBlock = new List<string>();
            curInd = 0;
            curHeader = "";
        }
    }
}






