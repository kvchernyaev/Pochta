#region usings
using Pochta.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Client
{
    class Program
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetLogger(nameof(Program));
        static readonly Db db = new Db();
        static readonly DispatchersHandler _dispatchersHandler = new DispatchersHandler();


        static void Main(string[] args)
        {
            Logger.Warn("started");
            Console.WriteLine("press 'q' or 'exit' to exit, any other line - to send it to server");

            try
            {
                _dispatchersHandler.Start(new Dispatcher());

                while (true)
                {
                    string line = Console.ReadLine().Trim();
                    if (ProcessCommand(line)) break;
                }
            }
            finally
            {
                _dispatchersHandler.Stop();
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
            if (line == "q" || line == "exit") return true;

            db.InsertQueueItem(line);
            Logger.Info("new queueitem inserted: {0}", line);
            return false;
        }
    }
}
