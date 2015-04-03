using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LurkClient
{
    /// <summary>
    /// Converts user input to the appropriate client message to send to the server
    /// </summary>
    class InputConverter
    {
        /// <summary>
        /// Relates commands to their appropriate client messages
        /// </summary>
        static private Dictionary<string, string> commands =
            new Dictionary<string, string>{
                {"login", "CNNCT"}, {"logout", "LEAVE"}, {"setattack", "ATTCK"}, 
                {"setdefense", "DEFNS"}, {"setdescription", "DESCR"}, {"setregen", "REGEN"}, 
                {"start", "START"}, {"goto", "ACTON CHROM"}, {"message", "ACTON MESSG"}, 
                {"fight", "ACTON FIGHT"}, {"joinbattle", "JOINB"}};

        /// <summary>
        /// Relates each command to the number of parameters it has
        /// </summary>
        static private Dictionary<string, int> commandParams =
            new Dictionary<string, int>{
                {"login", 1}, {"logout", 0}, {"setattack", 1}, 
                {"setdefense", 1}, {"setdescription", 1}, {"setregen", 1}, 
                {"start", 1}, {"goto", 1}, {"message", 2}, 
                {"fight", 0}, {"joinbattle", 1}};

        /// <summary>
        /// Relates each command to its description
        /// </summary>
        static private Dictionary<string, string> commandDescriptions =
            new Dictionary<string, string>
            {
                {"login", "Login as character"}, {"logout", "logout of the game"}, 
                {"setattack", "Set the attack stat"}, {"setdefense", "setdefenseSet the defense stat"}, 
                {"setdescription", "Set the character's description"}, 
                {"setregen", "Set the regen stat"}, {"start", "Start playing the game"}, 
                {"goto", "Go to a connected room"}, {"message", "Message character"}, 
                {"fight", "Fight every willing participant in the room"}, {"joinbattle", "Set whether or not to join a battle (0 or 1)"}
            };

        /// <summary>
        /// Converts the user-entered command input to a client message
        /// </summary>
        /// <param name="input">
        /// The input from the user
        /// </param>
        /// <returns>
        /// Returns the client message on successful completion or an empty string otherwise
        /// </returns>
        static public string ToProtocol(string input)
        {
            string prot = "";
            var parts = input.Split(' ');
            if (parts.Length >= 1)
            {
                string head = parts[0];
                if (commands.ContainsKey(head) && parts.Length >= commandParams[head])
                {
                    prot = commands[head];
                    for (int i = 1; i < parts.Length; ++i)
                    {
                        prot += ' ' + parts[i];
                    }
                    if (prot[prot.Length - 1] == ' ')
                    {
                        prot.Remove(prot.Length - 1);
                    }
                }
            }

            return prot;
        }

        /// <summary>
        /// Adds a command for recognition and conversion by the InputConverter
        /// </summary>
        /// <param name="commandname">
        /// Name of the command
        /// </param>
        /// <param name="protname">
        /// The header for the protocol associated with this command
        /// </param>
        /// <param name="parameterCount">
        /// Number of parameters for the command
        /// </param>
        /// <param name="description">
        /// Description of the command
        /// </param>
        static public void AddCommand(string commandname, string protname, int parameterCount, string description)
        {
            commands.Add(commandname, protname);
            commandParams.Add(commandname, parameterCount);
            commandDescriptions.Add(commandname, description);
        }

        /// <summary>
        /// Generates string of all commands and their descriptions for
        /// display to the user
        /// </summary>
        /// <returns>
        /// Description of all available commands
        /// </returns>
        static public string GetString()
        {
            string s = "";
            foreach (var key in commands.Keys)
            {
                s += key + ":\n\t" + commandDescriptions[key] + "\n";
            }

            return s;
        }
    }
}
