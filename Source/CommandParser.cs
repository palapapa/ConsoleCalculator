using System.Collections.Generic;

namespace ConsoleCalculator
{
    public static class CommandParser
    {
        public static void Parse(string input, IList<Command> commands)
        {
            int firstSpaceIndex = input.IndexOf(' ');
            string commandName = input.GetRange(0, firstSpaceIndex);
            string argument = input.GetRange(firstSpaceIndex + 1, input.Length - (firstSpaceIndex + 1));
            foreach (Command command in commands)
            {
                if (command.Name == commandName)
                {
                    command.Process(argument);
                }
            }
        }
    }
}