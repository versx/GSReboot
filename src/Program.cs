/*
 * Author: Jeremy Broadbent (versx)
 * Date: 3/13/2017
*/

namespace GrandStream
{
	using System;
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
            var pass = string.Empty;
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
                    case "p":
                    case "password":
                        pass = Convert.ToString(item.Value);
                        break;
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

            if (string.IsNullOrEmpty(pass))
            {
                throw new NullReferenceException("The 'pass' parameter cannot be null, exiting...");
            }
            if (!File.Exists(hosts))
            {
                throw new FileNotFoundException("The 'hosts.txt' file path specified could not be found, exiting...", nameof(hosts));
            }

            var list = File.ReadAllText(hosts).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var reboot = new GSReboot
            {
                Password = pass,
                LogErrors = verbose
            };
            reboot.Hosts.AddRange(list);
            reboot.Run();

            Console.Read();
		}
    }
}