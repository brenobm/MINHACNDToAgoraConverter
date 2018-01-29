using System;
using System.IO;
using System.Net.Http;

namespace MINHACNDToAgoraConverter
{
    class Program
    {
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">
        /// Needs to receive:
        ///  - Remote file url in MINHA CND format
        ///  - Local file path to generate output in Agora format
        /// </param>
        static void Main(string[] args)
        {
            // Validate the params
            if (args.Length < 2)
            {
                Console.WriteLine("Wrong parameters. Usage: MINHACNDToAgoraConverter http://remotefile ./localfile");
                return;
            }

            CreateAgoraFile(args[0], GetAbsolutePath(args[1]));

            Console.WriteLine("Process Completed!");
            Console.ReadKey();
        }

        /// <summary>
        /// Get the absolute path to output file and create the directories if not exists.
        /// </summary>
        /// <param name="relativePath">
        /// Relative path
        /// </param>
        /// <returns>
        /// Absolut path
        /// </returns>
        private static string GetAbsolutePath(string relativePath)
        {
            var absolutePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), relativePath);

            var directory = Path.GetDirectoryName(absolutePath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return absolutePath;
        }


        /// <summary>
        /// Process  the remote url file (MINHA CND) and generate a output file in Agora format
        /// </summary>
        /// <param name="remoteURL">
        /// URL file in MINHA CND format
        /// </param>
        /// <param name="localFile">
        /// Output file to generated Agora file
        /// </param>
        private static async void CreateAgoraFile(string remoteURL, string localFile)
        {
            try
            {
                // Use the HttpClient to retrieve the content of the file
                // and use a async stream to process the lines while the content is
                // receveid.
                HttpClient httpClient = new HttpClient();

                Stream stream = await httpClient.GetStreamAsync(remoteURL);

                StreamReader sr = new StreamReader(stream);

                string line = null;

                // Create a local file and write the headers and content
                using (var file = File.AppendText(localFile))
                {
                    //Headers
                    file.WriteLine("#Version: 1.0");
                    file.WriteLine($"#Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                    file.WriteLine("#Fields: provider http-method status-code uri-path time-taken response-size cache-status");

                    //For each line in stream reader
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Convert to Agora format and write to a file
                        file.WriteLine(FormatLine(line));
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An error occurs: {ex.Message}");

                // If throw any exception, the output file will be deleted
                if (File.Exists(localFile))
                {
                    File.Delete(localFile);
                }                
            }
        }

        /// <summary>
        /// Convert MINHA CND line format to Agora line format
        /// </summary>
        /// <param name="input">
        /// Input in MINHA CND format
        /// </param>
        /// <returns>
        /// Output in Agora format
        /// </returns>
        private static string FormatLine(string input)
        {
            try
            {
                // Remove " from the line
                // Break the line with separtors | and space
                var pieces = input.Replace("\"", "").Split('|', ' ');

                // Create a string output in Agora format
                return $"\"MINHA CDN\" {pieces[3]} {pieces[1]} {pieces[4]} {pieces[6].Split('.')[0]} {pieces[0]} {pieces[2]}";
            }
            catch
            {
                // If throw any exception it's because the input is not in MINHA CND format.

                throw new Exception("The Source file ins't a valid \"MINHA CND\" file");
            }
        }
    }
}
