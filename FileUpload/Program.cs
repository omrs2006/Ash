using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.Exceptions;
using Box.V2.Models;
using Nito.AsyncEx;
using System.IO;

namespace FileUpload
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Form1());
        //}
        const string CLIENT_ID = "YOUR_CLIENT_ID";
        const string CLIENT_SECRET = "YOUR_CLIENT_SECRET";
        const string TOKEN_FILENAME = "Data.dat";
        const char TOKEN_SPLIT_CHAR = '|';
        const string BOX_FOLDER_PATH = "/Personal Backups/Test"; //for this example code, make sure this folder structure exists in Box

        //set these to point to whatever file you want to upload; make sure it exists!
        const string PATH_TO_FILE = "C:\\";
        const string FILENAME = "example.pdf";

        static string access_token;
        static string refresh_token;

        static BoxClient client;

        static void Main(string[] args)
        {
            string tokenText;
            if (args.Length == 1)
            {
                //If tokens are passed in as args assume they are valid; this is the way to bootstrap the refresh cycle.
                //You only need to do this the first time you run the program and then the tokens will automatically refresh across runs.
                //Use the tool at this URL to generate your first set of tokens: https://box-token-generator.herokuapp.com/
                //There should be one command line arg that looks like: ACCESS_TOKEN|REFRESH_TOKEN
                tokenText = args[0];
            }
            else
            {
                //After you have bootstrapped the tokens you should not send anything in as command line args.
                //The program will read the token file, refresh the tokens, and then create a new session to Box.
                tokenText = File.ReadAllText(TOKEN_FILENAME);
            }

            var tokens = tokenText.Split(TOKEN_SPLIT_CHAR);
            access_token = tokens[0];
            refresh_token = tokens[1];

            //http://blog.stephencleary.com/2012/02/async-console-programs.html
            try
            {
                var config = new BoxConfig(CLIENT_ID, CLIENT_SECRET, new Uri("http://localhost"));
                var session = new OAuthSession(access_token, refresh_token, 3600, "bearer");
                client = new BoxClient(config, session);

                AsyncContext.Run(() => MainAsync());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            Console.WriteLine();
            Console.Write("Press return to exit...");
            Console.ReadLine();
        }

        static async Task MainAsync()
        {
            //first we need to refresh the tokens and persist them in a file
            var newSession = await client.Auth.RefreshAccessTokenAsync(access_token);

            //store the tokens on disk
            File.WriteAllText(TOKEN_FILENAME, newSession.AccessToken + TOKEN_SPLIT_CHAR + newSession.RefreshToken);

            //look up the folder id based on the path
            var boxFolderId = await FindBoxFolderId(BOX_FOLDER_PATH);

            using (FileStream fs = File.Open(PATH_TO_FILE + FILENAME, FileMode.Open, FileAccess.Read))
            {
                Console.WriteLine("Uploading file...");

                // Create request object with name and parent folder the file should be uploaded to
                BoxFileRequest request = new BoxFileRequest()
                {
                    Name = DateTime.Now.ToFileTime() + "-" + FILENAME, //for this demo, don't collide with any other files
                    Parent = new BoxRequestEntity() { Id = boxFolderId }
                };
                BoxFile f = await client.FilesManager.UploadAsync(request, fs);
            }
        }

        static async Task<String> FindBoxFolderId(string path)
        {
            var folderNames = path.Split('/');
            folderNames = folderNames.Where((f) => !String.IsNullOrEmpty(f)).ToArray(); //get rid of leading empty entry in case of leading slash

            var currFolderId = "0"; //the root folder is always "0"
            foreach (var folderName in folderNames)
            {
                var folderInfo = await client.FoldersManager.GetInformationAsync(currFolderId);
                var foundFolder = folderInfo.ItemCollection.Entries.OfType<BoxFolder>().First((f) => f.Name == folderName);
                currFolderId = foundFolder.Id;
            }

            return currFolderId;
        }
    }
}
