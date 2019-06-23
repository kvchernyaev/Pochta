#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Pochta.Server
{
    class PrintCommand : ICommand
    {
        public const string Name = "print";


        readonly int? _top;


        public PrintCommand(string fullLine)
        {
            string args = fullLine.Substring(Name.Length).Trim();
            if (int.TryParse(args, out int i))
                _top = i;
        }


        public void Execute()
        {
            Db db = new Db();
            List<IncomingRequest> i = db.GetIncomingRequests(_top);

            foreach (IncomingRequest r in i)
                Console.WriteLine($"{r.IncomedDate}: [{r.Str ?? "(null)"}] (id {r.Id}) {r.ClientAddress}");
        }
    }
}
