using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RainBoots
{
    public class Boot
    {
        public Boot(string name, string description, Regex regex, Func<Match, Task<string>> interpreter)
        {
            Name = name;
            Description = description;
            InputRegex = regex;
            Interpreter = interpreter;
        }

        public string Name { get; }
        public string Description { get; }
        public Regex InputRegex { get; }
        public Func<Match, Task<string>> Interpreter { get; }
    }
}