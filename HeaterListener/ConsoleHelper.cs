using System;
using System.Net;

namespace HeaterListener
{
    public static class ConsoleHelper
    {
        public static (IPAddress, int) AskUserForIPAndPort()
        {
            // read ip
            Console.WriteLine("IPAdresse: ");

            bool success = false;
            IPAddress ip = null;
            while (!success)
            {
                var input = Console.ReadLine();
                success = IPAddress.TryParse(input, out ip);

                if (!success)
                {
                    Console.WriteLine("Ungültige Eingabe");
                    Console.WriteLine("IPAdresse: ");
                }
            }

            // read port
            Console.WriteLine("Port: ");

            success = false;
            int port = 0;
            while (!success)
            {
                var input = Console.ReadLine();
                success = int.TryParse(input, out port);

                if (!success)
                {
                    Console.WriteLine("Ungültige Eingabe");
                    Console.WriteLine("Port: ");
                }
            }

            return (ip, port);
        }

        public static void ExitDialog()
        {
            Console.WriteLine();
            Console.WriteLine("Beliebige Taste zum Beenden drücken");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}