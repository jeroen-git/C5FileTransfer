using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace C5FileTransfer
{
    public class Options
    {
        public Options()
        {
            //Default Inteqnion user/pw for the web server
            Username = "Administrator";
            Password = "p...";
            Number = int.MaxValue; //All
            Dir = $"C:\\Users\\{Environment.UserName}\\Downloads\\";
        }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('i', "ip", Required = true, HelpText = "IP Address of the HMI")]
        public string IPAddress { get; set; }

        [Option('u', "user", Required = false, HelpText = "Web username of the HMI")]
        public string Username { get; set; }

        [Option('p', "password", Required = false, HelpText = "Password of web username of the HMI")]
        public string Password { get; set; }

        [Option('f', "files", Required = true, HelpText = "List of files to get from the HMI. e.g. /StorageCardSD/UsedRawMaterials.csv")]
        public IEnumerable<string> Files { get; set; }

        [Option('d', "dir", Required = false, HelpText = "Directory were to store the downloaded files")]
        public string Dir { get; set; }

        [Option('n', "number", Required = false, HelpText = "Number of files to download starting with the newest")]
        public int Number { get; set; }

    }
}

