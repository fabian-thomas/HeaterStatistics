using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace HeaterListener
{
    class Program
    {
        private static UdpClient UdpClient = new UdpClient();
        private static Config Config;
        private const string CONFIG_FILE_NAME = "config.json";

        static void Main(string[] args)
        {
            if (File.Exists("config.json"))
            {
                try
                {
                    Config = JsonSerializer.Deserialize<Config>(File.ReadAllText(CONFIG_FILE_NAME));
                }
                catch (Exception e)
                {
                    Console.WriteLine(CONFIG_FILE_NAME + " konnte nicht eingelesen werden\n" + e.ToString());
                }
            }
            else
            {
                var input = ConsoleHelper.AskUserForIPAndPort();
                Config = new Config { IpAddress = input.Item1.ToString(), Port = input.Item2 };

                File.WriteAllText(CONFIG_FILE_NAME, JsonSerializer.Serialize(Config, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
                Console.WriteLine(CONFIG_FILE_NAME + " angelegt. Bitte anpassen und die Anwendung neu starten.");
                ConsoleHelper.ExitDialog();
            }

            // check for faulted ipaddress
            var success = IPAddress.TryParse(Config.IpAddress, out IPAddress ip);

            if (!success)
            {
                Console.WriteLine($"Format der IP-Adresse in {CONFIG_FILE_NAME} falsch. Bitte anpassen und neustarten");
                ConsoleHelper.ExitDialog();
            }
            var sender = new IPEndPoint(ip, Config.Port);

            // try to register listener
            try
            {
                UdpClient.Client.Bind(sender);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                Console.WriteLine("Fehler beim Registrieren des Listeners. Bitte IP und Port in der Config überprüfen.");
                ConsoleHelper.ExitDialog();
            }

            Task.Run(() =>
            {
                while (true)
                {
                    // var receiveBuffer = UdpClient.Receive(ref from);
                    // Console.WriteLine("received: " + Encoding.UTF8.GetString(receiveBuffer));
                    try
                    {
                        var bytes = UdpClient.Receive(ref sender);
                        string message = Encoding.UTF8.GetString(bytes);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Folgender Fehler ist beim Lesen ankommender Pakete von {Config.IpAddress} aufgetreten:");
                        Console.WriteLine(e);
                        Console.WriteLine();
                    }
                }
            });

            SendTestBroadcasts(Config.Port);

            // keep app running
            while (true)
            {
                Console.ReadKey();
            }
        }

        private static void ProcessData(string data)
        {
            Console.WriteLine("Received: {0}", data);
        }

        private static void SendTestBroadcasts(int port)
        {
            // var data = Encoding.UTF8.GetBytes("Test Network Broadcast");
            var data = Encoding.UTF8.GetBytes("pm 72.1 190.9 6.0 72.6 70.5 37.5 6.6 53.6 120.0 20.0 20.0 64.0 53.0 0.0 23.5 20.0 15 14 14 14 71.5 100 6 6.0 21.0 190.0 0.0 79.0 -20.0 -20.0 20.0 20.0 -20.0 0.0 0.0 20.0 20.0 -20.0 -20.0 20.0 20.0 -20.0 0.0 0.0 20.0 20.0 70 38.7 59.4 73 0.0 0.0 0.0 0.0 0.0 0.0 0.0 121.0 60.0 5.9 0.0 0.0 0.0 0.00 0.00 0.00 0.00 -20.0 0.0 20.0 20.0 -20.0 1 3 1 1 1 1 1 20.0 20.0 20.0 20.0 20.0 20.0 20.0 -20.0 1 15.9 0.0 0 0 2 -20 60 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 4 1 0 0 0 0 0 0 0 0 0 0 0 0 1 53 53 57332.2 24183.1 4678.9 2162.4 69.6 8277.8 0.0 0.0 0.0 0.0 0 1 0 0 0 0 0 0 0 0 0 -20.0 0.0 0.0 0.0 0102 0003 0000 0000 0000 0000 0000 0000");

            Task.Run(() =>
            {
                while (true)
                {
                    new UdpClient().Send(data, data.Length, "255.255.255.255", port);
                    Thread.Sleep(300);
                }
            });
        }
    }
}

