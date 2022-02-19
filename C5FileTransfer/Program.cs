using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using CommandLine;
using System.Linq;

namespace C5FileTransfer
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);

            return 0;
        }

        static void RunOptions(Options opts)
        {
            int loginTime = Properties.Settings.Default.LoginTime;
            string chromeBinaryLocation = Properties.Settings.Default.ChromeBinaryLocation;            
            var ft = new C5FileTransfer(logger, opts, chromeBinaryLocation, loginTime);
            ft.DownloadFiles();
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {            
            logger.Error("Parameter parse error");
        }
    }
}