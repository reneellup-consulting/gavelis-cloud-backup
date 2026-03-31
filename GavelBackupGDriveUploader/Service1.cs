using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace GavelBackupGDriveUploader
{
    public partial class Service1 : ServiceBase, ITimeRemaining
    {
        private static System.Timers.Timer aTimer;
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private static string _backupFolder = ConfigurationManager.AppSettings["_backupFolder"] ?? @"C:\GDriveBackups";
        private static string DirectoryId = ConfigurationManager.AppSettings["DirectoryId"] ?? "1-4mizHVcvyWM8uX6BWEnQXgAkJqBt0IB";
        private static string _applicationName = ConfigurationManager.AppSettings["_applicationName"] ?? "GavelBackupGDriveUploader";

        private NamedPipeServerStream pipeServer;

        public int TimeRemaining { get; private set; }
        public DateTime startTime { get; private set; }

        string newMessage = "";

        public Service1()
        {
            InitializeComponent();

            pipeServer = new NamedPipeServerStream("MyServicePipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            pipeServer.WaitForConnection();

            var writer = new StreamWriter(pipeServer);

            var message = "";

            var updateThread = new Thread(() =>
            {
                while (pipeServer.IsConnected)
                {
                    if (newMessage != message)
                    {
                        message = newMessage;
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                    Thread.Sleep(1000);
                }
                writer.Close();
                pipeServer.Close();
            });
            updateThread.Start();

            try
            {
                if (!Directory.Exists(_backupFolder))
                {
                    Directory.CreateDirectory(_backupFolder);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred while initializing the system: {ex.Message}");
            }
        }

        protected override void OnStart(string[] args)
        {
            ScheduleService();
        }

        private void UpdateMessage(string message)
        {
            newMessage = message;
        }

        private void ScheduleService()
        {
            aTimer = new System.Timers.Timer();

            // Schedule the first backup to run every 5 minutes
            aTimer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;

            aTimer.Elapsed += (sender, e) => BackupToGDrive(sender, e);

            aTimer.Start();
        }

        private void BackupToGDrive(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Info("Performing: BackupToGDrive");
            aTimer.Stop();
            try
            {
                // Create the OAuth 2.0 credentials object.
                var credential = GetUserCredential();

                var backupFileList = Directory.GetFiles(_backupFolder, "*.bak").ToList();

                if (backupFileList.Any())
                {
                    foreach (var filePath in backupFileList)
                    {
                        try
                        {
                            var service = new DriveService(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = _applicationName
                            });

                            var fileName = Path.GetFileName(filePath);
                            var query = $"name='{fileName}' and parents='{DirectoryId}' and trashed = false";
                            var listRequest = service.Files.List();
                            listRequest.Q = query;
                            listRequest.Fields = "files(id, name)";
                            var files = listRequest.Execute().Files;
                            if (files.Count > 0)
                            {
                                UpdateMessage($"{fileName} already exists off-site");
                                continue;
                            }

                            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                            {
                                Name = fileName,
                                Parents = new List<string>() { DirectoryId }
                            };

                            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                            {
                                try
                                {
                                    var createRequest = service.Files.Create(fileMetadata, fileStream, "application/octet-stream");
                                    createRequest.Fields = "id";

                                    createRequest.ProgressChanged += (Google.Apis.Upload.IUploadProgress progress) =>
                                    {
                                        switch (progress.Status)
                                        {
                                            case Google.Apis.Upload.UploadStatus.Completed:
                                                UpdateMessage($"{fileName} upload completed.");
                                                break;
                                            case Google.Apis.Upload.UploadStatus.Failed:
                                                UpdateMessage($"{fileName} upload failed.");
                                                break;
                                            default:
                                                //Console.WriteLine($"Upload backup file {fileName} progress: " + progress.BytesSent + " / " + fileStream.Length);
                                                double percent = (double)progress.BytesSent / fileStream.Length * 100.0;
                                                double fileSizeMB = fileStream.Length / 1000000.0;
                                                UpdateMessage(string.Format("{0} progress: {1:f2}% ({2:f2} MB)", fileName, percent, fileSizeMB));
                                                break;
                                        }
                                    };

                                    var uploadProgress = createRequest.Upload();
                                }
                                catch (Google.GoogleApiException ex)
                                {
                                    Log.Error("Error uploading file: {0}", ex.Message);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"An error occurred while backing up and uploading {filePath} to Google Drive: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while backing up and uploading to Google Drive: " + ex.Message);
            }
            finally
            {
                ScheduleService();
            }
        }

        private static UserCredential GetUserCredential()
        {
            string[] scopes = { DriveService.Scope.Drive };
            string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/drive-dotnet-GavelBackupGDriveUploader.json");

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;

                Log.Info("Credential file saved to: " + credPath);
                return credential;
            }
        }

        protected override void OnStop()
        {
            if (pipeServer != null)
            {
                pipeServer.Close();
            }

            if (aTimer != null)
            {
                //aTimer.Enabled = false;
                aTimer.Stop();
                aTimer.Dispose();
            }
        }
    }
}
