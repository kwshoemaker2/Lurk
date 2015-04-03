using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace LurkClient
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Page
    {
        /// <summary>
        /// Current game instance
        /// </summary>
        private LurkGame game;

        /// <summary>
        /// Queue of messages to display to the user
        /// </summary>
        private Queue<string> outputQueue = new Queue<string>();

        /// <summary>
        /// Timer for updating the window
        /// </summary>
        private DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// Create the game window, set up the LurkGame instance and the timer, and start the game
        /// </summary>
        public GameWindow()
        {
            InitializeComponent();
            inputBox.Focus();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;
            timer.Start();

            game = new LurkGame(this, outputQueue);
            Thread gameThread = new Thread(new ThreadStart(game.RunGame));
            gameThread.Start();
        }

        /// <summary>
        /// Add the input from the user to the LurkGame instance's input Queue
        /// </summary>
        private void KeyPressedHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                string input = inputBox.Text;
                inputBox.Text = "";
                if (input.Length > 0)
                {
                    lock (this)
                    {
                        game.AddInput(input);
                    }
                }
            }
        }

        /// <summary>
        /// Add data to the text being displayed on the window
        /// </summary>
        /// <param name="data">
        /// Message to add to the output window
        /// </param>
        public void PushData(string data)
        {
            gameTextBox.Text += "**********\n" + data + "\n";
            gameTextBox.ScrollToEnd();
        }

        /// <summary>
        /// Clean up and close application if the game is finished,
        /// otherwise see if any output is in the outputQueue and display it
        /// on the window
        /// </summary>
        void timer_Tick(object sender, EventArgs e)
        {
            lock (this)
            {
                if (game.GameFinished)
                {
                    PushData("Moriturus te saluto");
                    Globals.MainWin.Close();
                    CleanUp();
                }

                else if (outputQueue.Count != 0)
                {
                    string output = outputQueue.Dequeue();
                    PushData(output);
                }
            }
        }

        /// <summary>
        /// Clean up anything that might still be opened or running
        /// </summary>
        public void CleanUp()
        {
            if (!game.GameFinished)
            {
                game.End();
            }
            if (Globals.ClientSocket.Connected)
            {
                Globals.ClientSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                Globals.ClientSocket.Close();
            }
            timer.Stop();
        }
    }
}
