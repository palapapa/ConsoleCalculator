using System;
using System.Collections.Generic;

namespace ConsoleCalculator
{
    class Program
    {
        static void Main()
        {
            IList<Command> commands = Command.GetDefaultCommands();
            while (true)
            {
                string input = Console.ReadLine();
                try
                {
                    CommandParser.Parse(input, commands);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
