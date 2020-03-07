using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using NetworkPacketConfigToJson.Models;

namespace NetworkPacketConfigToJson
{
    class Program
    {
        private const string OUTPUT_FILE_NAME = "network_packet_config.json";

        static void Main(string[] args)
        {
            Console.WriteLine("Legen sie die Paket-Konfigurationsdatei (.txt) in das gleiche Verzeichnis wie die EXE und geben sie den Dateinamen an:");
            var path = Console.ReadLine();

            if (File.Exists(path))
            {
                var text = File.ReadAllText(path);
                var toRead = text.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ');

                #region Analog

                // read Analog Config
                var analog = new List<AnalogNetworkPacketModel>();
                var startedReading = false;
                AnalogNetworkPacketModel currentAnalog = null;
                try
                {
                    while (true)
                    {
                        if (toRead == "")
                        {
                            throw new ReadParameterException("no ending </ANALOG> tag found");
                        }
                        else
                        {
                            toRead = toRead.TrimStart();
                            if (startedReading)
                            {
                                if (toRead.StartsWith("id="))
                                {
                                    int id;
                                    var tuple = ReadApastrophContent(toRead.Substring(("id=").Length, toRead.Length - ("id=").Length));
                                    if (int.TryParse(tuple.Item1, out id))
                                    {
                                        currentAnalog = new AnalogNetworkPacketModel() { Id = id };
                                        analog.Add(currentAnalog);
                                    }
                                    else throw new ReadParameterException($"Id has to be integer: {toRead}");
                                    toRead = tuple.Item2;
                                }
                                else if (toRead.StartsWith("name="))
                                {
                                    var tuple = ReadApastrophContent(toRead.Substring(("name=").Length, toRead.Length - ("name=").Length));
                                    currentAnalog.Name = tuple.Item1;
                                    toRead = tuple.Item2;
                                }
                                else if (toRead.StartsWith("unit="))
                                {
                                    var tuple = ReadApastrophContent(toRead.Substring(("unit=").Length, toRead.Length - ("unit=").Length));
                                    currentAnalog.Unit = tuple.Item1;
                                    toRead = tuple.Item2;
                                }
                                else if (toRead.StartsWith("</ANALOG>"))
                                {
                                    toRead = toRead.Substring("</ANALOG>".Length, toRead.Length - "</ANALOG>".Length);
                                    break;
                                }
                                else throw new ReadParameterException($"unexcepted character: {toRead}");
                            }
                            else if (toRead.StartsWith("<ANALOG>"))
                            {
                                startedReading = true;
                                toRead = toRead.Substring("<ANALOG>".Length, toRead.Length - "<ANALOG>".Length);
                            }
                            else throw new ReadParameterException($"unexcepted character: {toRead}");
                        }
                    }
                }
                catch (ReadParameterException e)
                {
                    Console.WriteLine("Fehler bei der Konvertierung:\n" + e);
                }

                // sort and remove duplicates
                analog.Sort(new IdComparer());
                var lastId = -1;
                var i = 0;
                while (i < analog.Count)
                {
                    if (analog[i].Id != lastId)
                    {
                        lastId = analog[i].Id;
                        i++;
                    }
                    else
                        analog.RemoveAt(i);
                }

                #endregion

                #region Digital

                // read Analog Config
                var digital = new List<DigitalNetworkPacketModel>();
                startedReading = false;
                DigitalNetworkPacketModel currentDigital = null;
                BitModel currentBit = null;
                try
                {
                    if (toRead != "")
                    {
                        while (true)
                        {
                            if (toRead == "")
                            {
                                throw new ReadParameterException("no ending </DIGITAL> tag found");
                            }
                            else
                            {
                                toRead = toRead.TrimStart();
                                if (startedReading)
                                {
                                    if (toRead.StartsWith("id="))
                                    {
                                        int id;
                                        var tuple = ReadApastrophContent(toRead.Substring(("id=").Length, toRead.Length - ("id=").Length));
                                        if (int.TryParse(tuple.Item1, out id))
                                        {
                                            if (currentDigital != null && currentDigital.Id == id)
                                            {
                                                currentBit = new BitModel();
                                                currentDigital.Bits.Add(currentBit);
                                            }
                                            else
                                            {
                                                currentDigital = new DigitalNetworkPacketModel() { Id = id };
                                                digital.Add(currentDigital);
                                                currentBit = new BitModel();
                                                currentDigital.Bits.Add(currentBit);
                                            }
                                        }
                                        else throw new ReadParameterException($"Id has to be integer: {toRead}");
                                        toRead = tuple.Item2;
                                    }
                                    else if (toRead.StartsWith("name="))
                                    {
                                        var tuple = ReadApastrophContent(toRead.Substring(("name=").Length, toRead.Length - ("name=").Length));
                                        currentBit.Name = tuple.Item1;
                                        toRead = tuple.Item2;
                                    }
                                    else if (toRead.StartsWith("bit="))
                                    {
                                        int bit;
                                        var tuple = ReadApastrophContent(toRead.Substring(("bit=").Length, toRead.Length - ("bit=").Length));
                                        if (int.TryParse(tuple.Item1, out bit))
                                        {
                                            currentBit.Bit = bit;
                                        }
                                        else throw new ReadParameterException($"Bit has to be integer: {toRead}");
                                        toRead = tuple.Item2;
                                    }
                                    else if (toRead.StartsWith("</DIGITAL>"))
                                    {
                                        toRead = toRead.Substring("</DIGITAL>".Length, toRead.Length - "</DIGITAL>".Length);
                                        break;
                                    }
                                    else throw new ReadParameterException($"unexcepted character: {toRead}");
                                }
                                else if (toRead.StartsWith("<DIGITAL>"))
                                {
                                    startedReading = true;
                                    toRead = toRead.Substring("<DIGITAL>".Length, toRead.Length - "<DIGITAL>".Length);
                                }
                                else throw new ReadParameterException($"unexcepted character: {toRead}");
                            }
                        }
                    }
                }
                catch (ReadParameterException e)
                {
                    Console.WriteLine("Fehler bei der Konvertierung:\n" + e);
                }

                // sort and remove duplicates
                digital.Sort(new IdComparer());
                lastId = -1;
                i = 0;
                while (i < digital.Count)
                {
                    if (digital[i].Id != lastId)
                    {
                        digital[i].Bits.Sort(new BitComparer());
                        var lastBit = -1;
                        var f = 0;
                        while (f < digital[i].Bits.Count)
                        {
                            if (digital[i].Bits[f].Bit != lastBit)
                            {
                                lastBit = digital[i].Bits[f].Bit;
                                f++;
                            }
                            else
                                digital[i].Bits.RemoveAt(f);
                        }
                        lastId = digital[i].Id;
                        i++;
                    }
                    else
                        digital.RemoveAt(i);
                }

                #endregion

                // write back to file
                var json = JsonSerializer.Serialize(new HeaterNetworkPacketConfig() { AnalogNetworkPacketConfig = analog, DigitalNetworkPacketConfig = digital }, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                File.WriteAllText(OUTPUT_FILE_NAME, json);

                Console.WriteLine($"Erfolgreich. Konfiguration in {OUTPUT_FILE_NAME} gespeichert");
            }
            else
                Console.WriteLine("Datei nicht vorhanden");

            Console.WriteLine("Beliebige Taste drücken um Anwendung zu beenden");
            Console.ReadKey();
        }

        private static Tuple<string, string> ReadApastrophContent(string s)
        {
            var charArray = s.ToCharArray();
            if (charArray.Length == 0 || charArray[0] != '\'')
                throw new ReadParameterException($"No leading apostrophe found: ${s}");
            else
            {
                var val = "";
                for (int i = 1; i < charArray.Length; i++)
                {
                    if (charArray[i] == '\'')
                    {
                        return new Tuple<string, string>(val, new string(SubArray(charArray, i + 1, charArray.Length - i - 1)));
                    }
                    else val += charArray[i];
                }
                throw new ReadParameterException($"No ending apostrophe found: ${s}");
            }
        }

        private static char[] SubArray(char[] data, int index, int length)
        {
            var result = new char[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

    class IdComparer : Comparer<NetworkPacketModel>
    {
        public override int Compare(NetworkPacketModel x, NetworkPacketModel y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }

    class BitComparer : Comparer<BitModel>
    {
        public override int Compare(BitModel x, BitModel y)
        {
            return x.Bit.CompareTo(y.Bit);
        }
    }

    class ReadParameterException : Exception
    {
        public ReadParameterException(string message)
            : base(message)
        {
        }
    }
}

