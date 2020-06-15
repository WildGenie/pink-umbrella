using System;

namespace PinkUmbrella.Models
{
    public class LoggedExceptionModel
    {
        public int Id { get; set; }
        public string RequestCode { get; set; }
        public int? UserId { get; set; }
        public DateTime Logged { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string Serialized { get; set; }
        public string Stringified { get; set; }
    }
}