/*
 * Author: Jeremy Broadbent (versx)
 * Date: 3/13/2017
*/

namespace GrandStream
{
	using System;
    using System.Collections.Generic;
    using System.IO;

    /** References
	 * http://www.grandstream.com/sites/default/files/Resources/CTI_Guide.pdf
	 * http://stackoverflow.com/questions/16759349/the-server-committed-a-protocol-violation-section-responseheader-detail-cr-must
	 */
    class Program
	{
        static readonly string[] _prefixes = { "--", "-" };

        static void Main(string[] args)
		{
            var hosts = string.Empty;
            var verbose = false;

            // If no parameters passed, show usage.
            if (args.Length == 0)
            {
                // Show usage
                return;
            }

            // Parse command line parameters
            var parameters = CommandLine.ParseArgs(_prefixes, args);
            foreach (var item in parameters)
            {
                switch (item.Key)
                {
                    case "h":
                    case "hosts":
                        hosts = Convert.ToString(item.Value);
                        break;
                    case "v":
                    case "verbose":
                        verbose = Convert.ToBoolean(item.Value);
                        break;
                }
            }

            if (!File.Exists(hosts))
            {
                throw new FileNotFoundException("The 'hosts.txt' file path specified could not be found, exiting...", nameof(hosts));
            }

            var list = ParseHostsFile(hosts);
            if (list.Count == 0)
            {
                Console.Error.WriteLine("Failed to parse `hosts.txt` file.");
                return;
            }

            var reboot = new GSReboot(list, verbose);
            reboot.Run();

            Console.Read();
		}

        static List<DeviceConfig> ParseHostsFile(string hostsFilePath)
        {
            var list = new List<DeviceConfig>();
            var lines = File.ReadAllLines(hostsFilePath);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var split = line.Split(':');
                if (split.Length >= 2)
                {
                    var ipAddress = split[0];
                    var password = split[1];
                    var note = string.Empty;
                    if (split.Length > 2)
                    {
                        note = split[2];
                    }
                    list.Add(new DeviceConfig(ipAddress, password, note));
                }
            }
            return list;
        }
    }
}