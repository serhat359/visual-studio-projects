namespace CasualConsoleCore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var interpreter = new Interpreter.Interpreter();
            //var sss = interpreter.InterpretCode("var f = x => `asd${x}asd`; [f(1),f(2),f(3)]");

            Interpreter.Interpreter.Test();
            Interpreter.Interpreter.Benchmark();

            StartInterpreterConsole();
        }

        private static void StartInterpreterConsole()
        {
            Console.WriteLine("Welcome to Serhat's Interpreter!");
            var consoleInterpreter = new Interpreter.Interpreter();
            while (true)
            {
                Console.Write("$: ");
                string line = Console.ReadLine();
                try
                {
                    var val = consoleInterpreter.InterpretCode(line);
                    if (val is bool valbool)
                        Console.WriteLine(valbool ? "true" : "false");
                    else
                        Console.WriteLine(val?.ToString() ?? "(null)");
                }
                catch (Exception e)
                {
                    var oldColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.Message);
                    Console.ForegroundColor = oldColor;
                }
            }
        }
    }
}