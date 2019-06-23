#region usings
using System;
#endregion


namespace Pochta.Server
{
    public class IncomingRequest
    {
        public int Id { get; set; }
        public string ClientAddress { get; set; }
        public string Str { get; set; }
        public DateTime IncomedDate { get; set; }
    }
}   
