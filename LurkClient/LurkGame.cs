using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LurkClient
{
    class LurkGame
    {
        /// <summary>
        /// Queue for the input received from the user
        /// </summary>
        private Queue<string> inputQueue = new Queue<string>();

        /// <summary>
        /// Queue for messages received by the server
        /// </summary>
        private Queue<string> messageQueue = new Queue<string>();

        /// <summary>
        /// Queue for output used in the GameWindow
        /// </summary>
        private Queue<string> outputQueue;

        /// <summary>
        /// Description of the game
        /// </summary>
        private string description = "";

        /// <summary>
        /// bool indicating whether or not the game has been finished
        /// </summary>
        private bool gameFinished = false;

        /// <summary>
        /// bool indicating if the game has started yet
        /// </summary>
        private bool started = false;

        public bool GameFinished
        {
            get
            {
                return gameFinished;
            }
        }

        /// <summary>
        /// GameWindow that is currently displayed to the user
        /// </summary>
        private GameWindow gameWin;

        /// <summary>
        /// Constructor for the LurkGame instance
        /// </summary>
        /// <param name="gw">
        /// Value for instance variable gameWin
        /// </param>
        /// <param name="oq">
        /// Value for instance variable outputQueue
        /// </param>
        public LurkGame(GameWindow gw, Queue<string> oq)
        {
            gameWin = gw;
            outputQueue = oq;
        }

        /// <summary>
        /// Put user input into a Queue for processing
        /// </summary>
        /// <param name="input">
        /// The user input
        /// </param>
        public void AddInput(string input)
        {
            inputQueue.Enqueue(input);
        }

        /// <summary>
        /// Get the game description and list of extensions from the server
        /// and add the extensions as commands to the InputConverter
        /// </summary>
        public void SetupGame()
        {
            Globals.SendData("QUERY");
            string resp = Globals.ReceiveData(1024 * 1024);
            List<List<string>> blocks = new List<List<string>>();
            Globals.ParseData(resp, blocks);
            string qString = blocks[0][1];
            string extensionName = "";
            string extensionProt = "";
            string extensionDescription = "";

            string[] qBlocks = qString.Split('\n', ':');
            for(int i = 0; i < qBlocks.Length; ++i)
            {
                string chunk = qBlocks[i];
                if (chunk == "GameDescription")
                {
                    description = qBlocks[i + 1];

                }
                else if (chunk == "Extension")
                {
                    extensionProt = qBlocks[i + 1].Substring(1);
                }
                else if (chunk == "NiceName")
                {
                    extensionName = qBlocks[i + 1].Substring(1);
                }
                else if (chunk == "Type")
                {
                    if (qBlocks[i + 1] == " ACTON")
                    {
                        extensionProt = "ACTON " + extensionProt;
                    }
                }
                else if (chunk == "Description")
                {
                    extensionDescription = qBlocks[i + 1].Substring(1);
                    InputConverter.AddCommand(extensionName, extensionProt, 1, extensionDescription);
                }
            }
        }

        /// <summary>
        /// Main game loop for the LurkGame
        /// </summary>
        public void RunGame()
        {
            SetupGame();
            lock (this)
            {
                outputQueue.Enqueue(description);
               // outputQueue.Enqueue("Please allocate yourself attack, defense, and regen points "
               //                     + "and a description, then enter start when ready\n");
                outputQueue.Enqueue("Enter cmds for a list of commands");
            }

            // Set up the thread for receiving messages from the server
            Thread recvThread = new Thread(new ThreadStart(receiveMessages));
            recvThread.Start();
            string[] startCmds = { "login", "setattack", "setdefense", "setregen", "start" };
            while (!gameFinished)
            {
                // see if user has given any input
                if (inputQueue.Count != 0)
                {
                    lock (this)
                    {
                        string command = inputQueue.Dequeue();
                        if (command == "cmds")
                        {
                            outputQueue.Enqueue(InputConverter.GetString());
                            continue;
                        }

                        // Don't allow users to use commands for starting the game after they've started (buggy)
                        else if (startCmds.Contains(command.Split(' ')[0]) && started)
                        {
                            outputQueue.Enqueue("Invalid command: You have already started!");
                            continue;
                        }

                        string prot = InputConverter.ToProtocol(command);
                        if (prot != "")
                        {
                            if (command == "logout")
                            {
                                gameFinished = true;
                            }

                            // So user can't run starting commands again
                            else if (command == "start" && !started)
                            {
                                started = true;
                            }

                            Globals.SendData(prot);
                        }
                        else
                        {
                            outputQueue.Enqueue("Invalid command: " + command);
                        }
                    }
                }

                // output any messages that have come in
                while (messageQueue.Count > 0)
                {
                    lock (this)
                    {
                        outputQueue.Enqueue(messageQueue.Dequeue());
                    }
                }
            }
            recvThread.Abort(); // should be join?
        }

        /// <summary>
        /// Receive messages from the server and add them to messageQueue
        /// </summary>
        private void receiveMessages()
        {
            string resp;
            List<List<string>> blocks = new List<List<string>>();
            while (!gameFinished)
            {
                resp = Globals.ReceiveData(1024 * 1024);
                blocks.Clear();
                lock (this)
                {
                    try
                    {
                        Globals.ParseData(resp, blocks);
                    }
                    catch (Exception) // errors in server messages will cause parser to not work. This is a temporary fix
                    {
                    }

                    foreach (var block in blocks)
                    {
                        messageQueue.Enqueue(block[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Sets gameFinished to true, can be extended to clean up other things later if needed
        /// </summary>
        public void End()
        {
            gameFinished = true;
        }
    }
}
