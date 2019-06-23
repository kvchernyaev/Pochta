#region usings
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
#endregion

namespace Pochta.Server
{
    class Program
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetLogger(nameof(Program));


        static void Main(string[] args)
        {
            Logger.Warn("started");

            string baseAddress = ConfigurationManager.AppSettings["baseAddress"];
            if (string.IsNullOrWhiteSpace(baseAddress))
                throw new Exception("Parameter [baseAddress] can not be omitted");

            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Logger.Info("listening: " + baseAddress);

                Console.WriteLine("press 'q' or 'exit' to exit, 'print n' to print last n records");
                while (true)
                {
                    string line = Console.ReadLine().Trim();
                    if (ProcessCommand(line)) break;
                }
            }
            Logger.Warn("exiting");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns>Whether to close application</returns>
        static bool ProcessCommand(string line)
        {
            ICommand c = null;
            if (line == "q" || line == "exit") return true;
            else if (line.StartsWith(PrintCommand.Name)) c = new PrintCommand(line);

            if (c != null)
                c.Execute();
            return false;
        }
    }
}   
