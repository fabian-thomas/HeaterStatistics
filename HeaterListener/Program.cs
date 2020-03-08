using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using NetworkPacketConfigToJson.Models;
using System.Collections.Generic;
using System.Linq;
using System.Buffers.Binary;
using System.Collections;
using System.Text.Encodings.Web;

namespace HeaterListener
{
    class Program
    {
        private static UdpClient UdpClient = new UdpClient();
        private static Config Config;
        private static HeaterNetworkPacketConfig NetworkPacketConfig;
        private const string CONFIG_FILE_NAME = "config.json";
        private const string PACKET_CONFIG_FILE_NAME = "network_packet_config.json";
        private const string DATA_FILE_NAME = "data.json";

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

            // check package configuration file
            if (!File.Exists(PACKET_CONFIG_FILE_NAME))
            {
                Console.WriteLine("Bitte " + PACKET_CONFIG_FILE_NAME + "ins Verzeichnis der Anwendung legen und Anwendung neu starten.");
                ConsoleHelper.ExitDialog();
            }

            // read package configuration file
            try
            {
                NetworkPacketConfig = JsonSerializer.Deserialize<HeaterNetworkPacketConfig>(File.ReadAllText(PACKET_CONFIG_FILE_NAME));
            }
            catch (JsonException e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                Console.WriteLine(PACKET_CONFIG_FILE_NAME + " lässt sich nicht einlesen. Bitte prüfen Sie die Datei und starten die Anwendung neu.");
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


            Console.WriteLine("Paket-Empfang gestartet...");
            Task.Run(() =>
            {
                string previousInput = "";
                while (true)
                {
                    // var receiveBuffer = UdpClient.Receive(ref from);
                    // Console.WriteLine("received: " + Encoding.UTF8.GetString(receiveBuffer));
                    try
                    {
                        var bytes = UdpClient.Receive(ref sender);
                        string message = Encoding.UTF8.GetString(bytes);
                        Console.WriteLine("Received: " + message);

                        if (message != previousInput)
                        {
                            previousInput = message;
                            ProcessData(message);
                            Console.WriteLine("Processed");
                        }

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

            SendTestPackets(Config.Port);

            // keep app running
            // while (true)
            // {
            Console.ReadKey();
            // }
        }

        private static void ProcessData(string input)
        {
            if (input.StartsWith("pm "))
            {
                var data = new List<CapturedDataModel>();

                input = input.Substring("pm ".Length, input.Length - "pm ".Length);
                var split = input.Split(" ");

                // read analog values
                int i = 0;
                while (i < NetworkPacketConfig.AnalogNetworkPacketConfig.Count)
                {
                    var c = new CapturedDataModel() { Id = i };
                    var filtered = NetworkPacketConfig.AnalogNetworkPacketConfig.Where(p => p.Id == i);
                    if (filtered.Count() == 0)
                    {
                        Console.WriteLine($"Paket-Konfiguration ungültig. Id {i} konnte nicht in den Konfiguration gefunden werden: " + input);
                        ConsoleHelper.ExitDialog();
                    }

                    var config = filtered.First();
                    c.Name = config.Name;
                    c.Unit = config.Unit;
                    double v;
                    if (double.TryParse(split[i].Replace(".", ","), out v))
                        c.Value = v;
                    else
                    {
                        Console.WriteLine("Paket ungültig. Double erwartet jedoch anderen Wert empfangen: " + input);
                        ConsoleHelper.ExitDialog();
                    }
                    data.Add(c);

                    i++;
                }

                // read digital values
                while (i < NetworkPacketConfig.DigitalNetworkPacketConfig.Count + NetworkPacketConfig.AnalogNetworkPacketConfig.Count)
                {
                    var currentId = i - NetworkPacketConfig.AnalogNetworkPacketConfig.Count;
                    var filtered = NetworkPacketConfig.DigitalNetworkPacketConfig.Where(p => p.Id == currentId);
                    if (filtered.Count() == 0)
                    {
                        Console.WriteLine($"Paket-Konfiguration ungültig. Id {currentId} konnte nicht in den Konfiguration gefunden werden: " + input);
                        ConsoleHelper.ExitDialog();
                    }
                    var config = filtered.First();

                    // go through bits
                    short o;
                    if (short.TryParse(split[i], out o))
                    {
                        int[] bits = Convert.ToString(o, 2).PadLeft(config.Bits.Last().Bit + 1, '0')
                                     .Select(c => int.Parse(c.ToString()))
                                     .ToArray();

                        for (int b = 0; b < config.Bits.Count; b++)
                            data.Add(new CapturedDataModel() { Id = i + config.Id, Value = bits[config.Bits[b].Bit], Name = config.Bits[b].Name, Unit = "bool" });
                    }
                    else
                    {
                        Console.WriteLine("Paket ungültig. Short erwartet jedoch anderen Wert empfangen: " + input);
                        ConsoleHelper.ExitDialog();
                    }

                    i++;
                }

                File.WriteAllText(DATA_FILE_NAME, JsonSerializer.Serialize(data, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));
            }
            else Console.WriteLine("Ungültiges Paket empfangen: " + input);


        }

        private static void SendTestPackets(int port)
        {
            var localIp = GetLocalIPAddress();
            Task.Run(() =>
            {
                while (true)
                {
                    var data = Encoding.UTF8.GetBytes($"pm {DateTime.Now.Second} 190.9 6.0 72.6 70.5 37.5 6.6 53.6 120.0 20.0 20.0 64.0 53.0 0.0 23.5 20.0 15 14 14 14 71.5 100 6 6.0 21.0 190.0 0.0 79.0 -20.0 -20.0 20.0 20.0 -20.0 0.0 0.0 20.0 20.0 -20.0 -20.0 20.0 20.0 -20.0 0.0 0.0 20.0 20.0 70 38.7 59.4 73 0.0 0.0 0.0 0.0 0.0 0.0 0.0 121.0 60.0 5.9 0.0 0.0 0.0 0.00 0.00 0.00 0.00 -20.0 0.0 20.0 20.0 -20.0 1 3 1 1 1 1 1 20.0 20.0 20.0 20.0 20.0 20.0 20.0 -20.0 1 15.9 0.0 0 0 2 -20 60 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 4 1 0 0 0 0 0 0 0 0 0 0 0 0 1 53 53 57332.2 24183.1 4678.9 2162.4 69.6 8277.8 0.0 0.0 0.0 0.0 0 1 0 0 0 0 0 0 0 0 0 -20.0 0.0 0.0 0.0 9999 0003 0203 9300 0245 0102 0050 0203");

                    new UdpClient().Send(data, data.Length, localIp, port);
                    Thread.Sleep(1000);
                }
            });
        }

        private static string GetLocalIPAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }
    }
}

