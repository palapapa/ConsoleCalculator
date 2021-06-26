using System;
using System.Collections.Generic;

namespace ConsoleCalculator
{
    public class Command
    {
        public string Name { get; set; }
        public object Info { get; set; }
        public CommandDelegate Process { get; set; }

        public Command()
        {
        }

        public Command(string name, object info, CommandDelegate process)
        {
            Name = name;
            Info = info;
            Process = process;
        }

        public static IList<Command> GetDefaultCommands()
        {
            Command calc = new Command
            {
                Name = "calc",
                Info = MathOperator.GetDefaultOperators()
            };
            calc.Process = (string argument) =>
            {
                if (calc.Info is IList<MathOperator> operators)
                {
                    Console.WriteLine(MathExpressionParser.Parse(argument, operators));
                }
                else
                {
                    throw new ArgumentException($"Is not {nameof(IList<MathOperator>)}", nameof(Info));
                }
            };
            return new List<Command>
            {
                calc
            };
        }
    }
}