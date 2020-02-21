namespace GrandStream
{
    using System;
    using System.Collections.Generic;
	using System.Diagnostics;
    using System.Net;

    public class GSReboot
    {
        #region Constants

        // Base endpoint to reboot.
        private const string RebootRequestUri = "http://{0}/cgi-bin/api-sys_operation?passcode={1}&request=REBOOT";

        #endregion

        #region Variables

        private int _failed;
        private int _success;
        private bool _cancel;
        private bool _complete;
        private List<string> _phoneErrorList;
		private Stopwatch _stopwatch;

        #endregion

        #region Properties

        /// <summary>
        /// List of hostnames or IP addresses.
        /// </summary>
        public List<string> Hosts { get; }

        /// <summary>
        /// Admin password to access phones.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Log errors.
        /// </summary>
        public bool LogErrors { get; set; }

        #endregion

        #region Constructor(s)

        public GSReboot()
        {
            Hosts = new List<string>();
            _phoneErrorList = new List<string>();
			_stopwatch = new Stopwatch();
        }

        public GSReboot(List<string> hosts, string password)
            : this()
        {
            Hosts = hosts;
            Password = password;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Run the reboot operation.
        /// </summary>
        public void Run()
        {
            _cancel = false;
            _complete = false;

            Log(ConsoleColor.Green, true, $"Starting to reboot {Hosts.Count.ToString("N0")} VoIP phones in total, please wait as this may take a few minutes depending on the amount of hosts...");
            Log(ConsoleColor.White, true, string.Empty);

            _stopwatch.Start();
            for (var i = 0; i < Hosts.Count; i++)
            {
                if (_cancel) break;

                var host = Hosts[i];
                Log(ConsoleColor.White, false, "Rebooting VoIP phone {0}...", host);

                var result = DownloadString(string.Format(RebootRequestUri, host, Password));
                /**Available responses:
                 * var success = "{\"response\":\"success\", \"body\": \"savereboot\"}"
                 * var failed = "{\"response\":\"error\", \"body\": \"unknown\"}"
                 */
                if (string.IsNullOrEmpty(result) || result.Contains("error"))
                {
                    _failed++;
                    _phoneErrorList.Add(host);
                    Log(ConsoleColor.Red, true, " [ERR]");
                }
                else
                {
                    _success++;
                    Log(ConsoleColor.Green, true, " [OK]");
                }
            }
			_stopwatch.Stop();
            _complete = true;

            PrintCompleteMessage();
        }

        /// <summary>
        /// Cancel the reboot operation.
        /// </summary>
        public void Cancel()
        {
            _cancel = true;

            if (!_complete)
            {
                _complete = true;
                Log(ConsoleColor.Yellow, true, "A cancel operation has been initiated...");
            }
        }

        #endregion

        #region Private Methods

        private string DownloadString(string uri)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Proxy = null;
                    var data = wc.DownloadString(uri);
                    return data;
                }
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ServerProtocolViolation)
                {
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                if (LogErrors)
                {
                    Log(ConsoleColor.Red, true, $"An error occurred: {ex}");
                }
            }

            return string.Empty;
        }

        private void PrintCompleteMessage()
        {
            Log(ConsoleColor.White, true, string.Empty);
            Log(ConsoleColor.White, true, string.Empty);

            if (_failed > 0)
            {
                Log(ConsoleColor.Red, true, "{0}/{1} VOIP phones failed to reboot.", _failed, Hosts.Count);
                Log(ConsoleColor.Red, true, "List of IP addresses that failed to reboot:");
                Log(ConsoleColor.Gray, true, "".PadRight(15, '*'));
                foreach (string failedHost in _phoneErrorList)
                {
                    Log(ConsoleColor.White, true, failedHost);
                }
            }
            else
            {
                Log(ConsoleColor.Green, true, "{0}/{1} VOIP phones successfully rebooted.", _success, Hosts.Count);
            }
			
			Log(ConsoleColor.White, true, string.Empty);
			Log(ConsoleColor.White, true, "Total time taken: {0}", _stopwatch.Elapsed);
        }

        private void Log(ConsoleColor color, bool newline, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            var msg = args.Length > 0 ? string.Format(format, args) : format;
            if (newline)
            {
                Console.WriteLine(msg);
            } else
            {
                Console.Write(msg);
            }
            Console.ResetColor();
        }

        #endregion
    }
}