using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RainBoots
{
    public class Boots : Dictionary<string, Boot>
    {
        private bool shouldContinue = false;

        public Boots()
        {
        }

        public Boots(IDictionary<string, Boot> dictionary) : base(dictionary)
        {
        }

        public Boots(IEnumerable<KeyValuePair<string, Boot>> collection) : base(collection)
        {
        }

        public async Task<string> InterpretBoot(string name, string arguments)
        {
            if (TryGetValue(name, out var boot))
            {
                var output = await boot.Interpreter(boot.InputRegex.Match(arguments));
                var mtSeparator = output.IndexOf(' ');
                if (mtSeparator > 0)
                {
                    var content = output.Substring(mtSeparator + 1);
                    switch (output.Substring(0, mtSeparator))
                    {
                        case "application/json":
                        return content;
                        case "text/plain":
                        return content;
                        default:
                        return output;
                    }
                }
                else
                {
                    return "Error: missing media type";
                }
            }
            else if (name == "quit" || name == "exit" || name == "q")
            {
                shouldContinue = false;
                return "Exiting...";
            }
            else if (name == "help" || name == "h")
            {
                var sb = new StringBuilder();
                sb.Append("text/plain Main commands:");
                foreach (var c in this)
                {
                    sb.Append("\n");
                    sb.Append(c.Key);
                    sb.Append(":\t");
                    sb.Append(c.Value.Description);
                }
                return sb.ToString();
            }

            return "Error: invalid boot";
        }

        public void Add(string name, string description, Regex regex, Func<Match, Task<string>> handler)
        {
            this.Add(name, new Boot(name, description, regex, handler));
        }

        public async Task Loop(Func<string> input, Action<string> output)
        {
            shouldContinue = true;
            while (shouldContinue)
            {
                try 
                {
                    var inStr = input();
                    var endOfBootName = inStr.IndexOf(' ');
                    if (endOfBootName >= 0 && endOfBootName < inStr.Length)
                    {
                        var bootName = inStr.Substring(0, endOfBootName);
                        var bootArgs = inStr.Substring(endOfBootName + 1);
                        output(await InterpretBoot(bootName, bootArgs));
                    }
                    else
                    {
                        output(await InterpretBoot(inStr, string.Empty));
                    }
                    output("\n");
                }
                catch (Exception e)
                {
                    await Console.Error.WriteLineAsync(e.ToString());
                }
            }
        }
    }
}