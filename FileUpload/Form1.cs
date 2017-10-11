using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("*****");
            const string CLIENT_ID = "YOUR_CLIENT_ID";
            const string CLIENT_SECRET = "YOUR_CLIENT_SECRET";
            const string DEV_ACCESS_TOKEN = "YOUR_DEV_ACCESS_TOKEN";  //log into developers.box.com and get this for your registered app; it will last for 60 minutes
            const string REFRESH_TOKEN = "THIS_IS_NOT_NECESSARY_FOR_A_DEV_TOKEN_BUT_MUST_BE_HERE";

            //set these to point to whatever file you want to upload; make sure it exists!
            const string PATH_TO_FILE = "C:\\";
            const string FILENAME = "example.pdf";

            const string BOX_FOLDER = "/Personal Backups/Test"; //for this example code, make sure this folder structure exists in Box

             BoxClient client;

             void Main(string[] args)
            {
                //http://blog.stephencleary.com/2012/02/async-console-programs.html
                try
                {
                    var config = new BoxConfig(CLIENT_ID, CLIENT_SECRET, new Uri("http://localhost"));
                    var session = new OAuthSession(DEV_ACCESS_TOKEN, REFRESH_TOKEN, 3600, "bearer");
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

             async Task MainAsync()
            {
                var boxFolderId = await FindBoxFolderId(BOX_FOLDER);

                using (FileStream fs = File.Open(PATH_TO_FILE + FILENAME, FileMode.Open, FileAccess.Read))
                {
                    Console.WriteLine("Uploading file...");

                    // Create request object with name and parent folder the file should be uploaded to
                    BoxFileRequest request = new BoxFileRequest()
                    {
                        Name = FILENAME,
                        Parent = new BoxRequestEntity() { Id = boxFolderId }
                    };
                    BoxFile f = await client.FilesManager.UploadAsync(request, fs);
                }
            }

             async Task<String> FindBoxFolderId(string path)
            {
                var folderNames = path.Split('/');
                folderNames = folderNames.Where((f) => !String.IsNullOrEmpty(f)).ToArray(); //get rid of leading empty entry in case of leading slash

                var currFolderId = "0"; //the root folder is always "0"
                foreach (string folderName in folderNames)
                {
                    var folderInfo = await client.FoldersManager.GetInformationAsync(currFolderId);
                    var foundFolder = folderInfo.ItemCollection.Entries.OfType<BoxFolder>().First((f) => f.Name == folderName);
                    currFolderId = foundFolder.Id;
                }

                return currFolderId;
            }
        }
    }
}




//namespace MoveFileFromDiskToBox
//{
//    //Make sure you install the NuGet package 'Box Windows SDK V2' 

//    class Program
//    {
//        const string CLIENT_ID = "YOUR_CLIENT_ID";
//        const string CLIENT_SECRET = "YOUR_CLIENT_SECRET";
//        const string DEV_ACCESS_TOKEN = "YOUR_DEV_ACCESS_TOKEN";  //log into developers.box.com and get this for your registered app; it will last for 60 minutes
//        const string REFRESH_TOKEN = "THIS_IS_NOT_NECESSARY_FOR_A_DEV_TOKEN_BUT_MUST_BE_HERE";

//        //set these to point to whatever file you want to upload; make sure it exists!
//        const string PATH_TO_FILE = "C:\\";
//        const string FILENAME = "example.pdf";

//        const string BOX_FOLDER = "/Personal Backups/Test"; //for this example code, make sure this folder structure exists in Box

//        static BoxClient client;

//        static void Main(string[] args)
//        {
//            //http://blog.stephencleary.com/2012/02/async-console-programs.html
//            try
//            {
//                var config = new BoxConfig(CLIENT_ID, CLIENT_SECRET, new Uri("http://localhost"));
//                var session = new OAuthSession(DEV_ACCESS_TOKEN, REFRESH_TOKEN, 3600, "bearer");
//                client = new BoxClient(config, session);

//                AsyncContext.Run(() => MainAsync());
//            }
//            catch (Exception ex)
//            {
//                Console.Error.WriteLine(ex);
//            }

//            Console.WriteLine();
//            Console.Write("Press return to exit...");
//            Console.ReadLine();
//        }

//        static async Task MainAsync()
//        {
//            var boxFolderId = await FindBoxFolderId(BOX_FOLDER);

//            using (FileStream fs = File.Open(PATH_TO_FILE + FILENAME, FileMode.Open, FileAccess.Read))
//            {
//                Console.WriteLine("Uploading file...");

//                // Create request object with name and parent folder the file should be uploaded to
//                BoxFileRequest request = new BoxFileRequest()
//                {
//                    Name = FILENAME,
//                    Parent = new BoxRequestEntity() { Id = boxFolderId }
//                };
//                BoxFile f = await client.FilesManager.UploadAsync(request, fs);
//            }
//        }

//        static async Task<String> FindBoxFolderId(string path)
//        {
//            var folderNames = path.Split('/');
//            folderNames = folderNames.Where((f) => !String.IsNullOrEmpty(f)).ToArray(); //get rid of leading empty entry in case of leading slash

//            var currFolderId = "0"; //the root folder is always "0"
//            foreach (string folderName in folderNames)
//            {
//                var folderInfo = await client.FoldersManager.GetInformationAsync(currFolderId);
//                var foundFolder = folderInfo.ItemCollection.Entries.OfType<BoxFolder>().First((f) => f.Name == folderName);
//                currFolderId = foundFolder.Id;
//            }

//            return currFolderId;
//        }
//    }
//}
