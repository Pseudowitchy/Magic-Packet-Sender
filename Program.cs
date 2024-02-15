using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

class Program
{
    static void Main(string[] args)
    {
        if (!File.Exists("aliases.txt")) { File.Create("aliases.txt"); }

        if (args.Length > 0)
        {

            List<Alias> aliases = AssembleAliases();
            if (args[0].ToLower() == "send" || args[0].ToLower() == "wake"
                || args[0].ToLower() == "s" || args[0].ToLower() == "w")
            {
                Console.WriteLine("send recieved");
                if (args.Length > 1)
                {
                    if (PhysicalAddress.TryParse(args[1], out PhysicalAddress? mac))
                    {
                        SendPacket(mac);
                        Console.WriteLine("Packet Sent!");
                    }
                    else { Console.WriteLine("Invalid Mac Address."); }
                }
                else { Console.WriteLine("No MAC address provided, please try again"); }
            }
            else if (args[0].ToLower() == "add" || args[0].ToLower() == "a")
            {
                bool nameExists = false;
                foreach (Alias alias in aliases) { if (alias.Name.ToLower() == args[1].ToLower()) { nameExists = true; break; } }
                
                if (nameExists == true) { Console.WriteLine("That alias already exists, please choose a new name."); }
                else if (args.Length > 2)
                {
                    if (PhysicalAddress.TryParse(args[2], out PhysicalAddress? _))
                    {
                        using (var textWriter = File.AppendText("aliases.txt"))
                            textWriter.WriteLine(args[1] + "," + args[2].ToUpper());
                            Console.WriteLine($"Successfully saved alias: {args[1]} at {args[2].ToUpper()}");
                    }
                    else { Console.WriteLine("Address was in an invalid format, please try again."); }
                }
                else { Console.WriteLine("Please provide the alias name and address you wish to save"); }
            }
            else if (args[0].ToLower() == "remove" || args[0].ToLower() == "r")
            {
                if (aliases.Count == 0)
                    Console.WriteLine("No aliases saved currently. Use the add argument to create a new alias.");
                else
                {
                    if (args.Length > 1)
                    {
                        bool removed = false;
                        foreach (Alias alias in aliases)
                        {
                            if (alias.Name == args[1] || alias.Mac.ToString() == args[1])
                            {
                                aliases.Remove(alias);
                                removed = true;
                                Console.WriteLine($"Removed {args[1]} from list of saved aliases");
                                break;
                            }
                        }
                        if (removed == false) { Console.WriteLine($"{args[1]} was not a saved alias."); }
                        else if (removed == true)
                        {
                            string[] content = new string[aliases.Count];
                            int i = 0;
                            foreach (Alias alias in aliases)
                            {
                                content[i] = alias.Name + ',' + alias.Mac.ToString();
                                i++;
                            }
                            File.WriteAllLines("aliases.txt", content);
                        }
                    }
                    else { Console.WriteLine("Please provide the alias name or address you wish to remove"); }
                }
            }
            else if (args[0].ToLower() == "list" || args[0].ToLower() == "l")
            {
                Console.WriteLine("Saved Aliases:");
                foreach (Alias alias in aliases) { Console.WriteLine("  - " + alias.Name); }
            }
            else if (args[0].ToLower() == "help") { Help(); }
            else
            {
                Console.WriteLine("Invalid Arguments. Available commands:");
                Help();
            }
        }
        else { Console.WriteLine("No arguments given. Please provide an argument:"); Help(); }
        _ = Console.ReadKey();
    }

    static void SendPacket(PhysicalAddress mac)
    {
        var header = Enumerable.Repeat(byte.MaxValue, 6);
        var data = Enumerable.Repeat(mac.GetAddressBytes(), 16).SelectMany(m => m);

        var magicPacket = header.Concat(data).ToArray();

        using UdpClient client = new();

        client.Send(magicPacket, magicPacket.Length, new IPEndPoint(IPAddress.Broadcast, 9));
    }

    static List<Alias> AssembleAliases()
    {
        int count = File.ReadAllLines("aliases.txt").Length;
        List<Alias> aliases = new();
        for (int i = 0; i < count; i++)
        {
            string line = File.ReadLines("aliases.txt").Skip(i).Take(1).First();
            string name = line.Remove(line.IndexOf(','));
            string mac = line.Remove(0, line.IndexOf(",") + 1);

            aliases.Add(new Alias(name, mac));
        }
        return aliases;
    }

    static void Help()
    {
        Console.WriteLine("\r\n   " +
            "Send/Wake -- Sends a magic packet to wake the device you specify: " +
                "'send|s|wake|w (MAC Address or saved alias)'\r\n   " +
            "Add -- Adds a device's mac address to a list with a provided alias: " +
                "'add|r AA-AA-AA-AA-AA-AA-AA-AA officepc\r\n   " +
            "Remove -- Removes an alias from the storage: 'remove|r officepc'\r\n   " +
            "List -- Displays list of saved aliases: 'list|l'\r\n   " +
            "Help -- Display this info");
    }

    public class Alias
    {
        public string Name { get; set; }
        public PhysicalAddress Mac { get; set; }
        public Alias(string name, string mac)
        {
            Name = name;
            Mac = PhysicalAddress.Parse(mac);
        }
    }
}