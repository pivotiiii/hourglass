using System;
using System.Linq;

using Hourglass.Timing;

namespace Hourglass
{
    internal class ArgsValidatorEntry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CommandLineArguments arguments = CommandLineArguments.Parse(args);
            string toWrite = "{\"result\":" + (arguments.HasParseError ? "false" : "true") + ",";
            toWrite = toWrite + "\"timeStrings\":[";

            if (!arguments.HasParseError)
            {
                int index = arguments.TimerStart.OfType<TimerStart>().Count();
                foreach (var timerStart in arguments.TimerStart.OfType<TimerStart>())
                {
                    toWrite = toWrite + "\"" + timerStart.ToString() + "\"" + (index > 1 ? "," : "");
                    index = index - 1;
                }
            }
            toWrite = toWrite + "]}";

            Console.WriteLine(toWrite);
            return;
        }
    }
}
