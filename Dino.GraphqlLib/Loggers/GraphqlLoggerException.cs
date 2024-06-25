using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.GraphqlLib.Loggers
{
    public class GraphqlLoggerException : Exception
    {
        public GraphqlLoggerException()
        {
        }
        public GraphqlLoggerException(string keyName, params string[] messages) : base(string.Join("\n", messages))
        {
            KeyName = keyName;
            Messages = messages;
        }
        public IEnumerable<string> Messages { get; set; }
        public string KeyName { get; set; }
    }
    public class GraphqlLoggerExceptions : Exception
    {
        public GraphqlLoggerExceptions(params (string, string)[] messages)
        {
            Messages = messages;
        }
        public IEnumerable<(string, string)> Messages { get; set; }
    }
}
