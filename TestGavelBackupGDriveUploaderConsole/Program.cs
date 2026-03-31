using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System.Configuration;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using System.Threading;
using System.Net;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;

namespace TestGavelBackupGDriveUploaderConsole
{
    internal class Program
    {
        private static string _backupFolder = ConfigurationManager.AppSettings["_backupFolder"] ?? @"C:\GDriveBackups";
        private static string DirectoryId = ConfigurationManager.AppSettings["DirectoryId"] ?? "1-4mizHVcvyWM8uX6BWEnQXgAkJqBt0IB";
        private static string _applicationName = ConfigurationManager.AppSettings["_applicationName"] ?? "GavelBackupGDriveUploader";

        static void Main(string[] args)
        {
            try
            {
                // Create the OAuth 2.0 credentials object.
                UserCredential credential = GetUserCredential();

                var backupFileList = Directory.GetFiles(_backupFolder, "*.bak").ToList();

                if (backupFileList.Any())
                {
                    foreach (var filePath in backupFileList)
                    {
                        try
                        {
                            var service = new DriveService(new BaseClientService.Initializer()
                            {
                                //HttpClientInitializer = credentials,
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
                                Console.WriteLine($"File {fileName} already exists in the Google Drive folder");
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
                                                Console.WriteLine($"Upload backup file {fileName} completed.");
                                                break;
                                            case Google.Apis.Upload.UploadStatus.Failed:
                                                Console.WriteLine($"Upload backup file {fileName} failed.");
                                                break;
                                            default:
                                                //Console.WriteLine($"Upload backup file {fileName} progress: " + progress.BytesSent + " / " + fileStream.Length);
                                                double percent = (double)progress.BytesSent / fileStream.Length * 100.0;
                                                double fileSizeMB = fileStream.Length / 1000000.0;
                                                Console.WriteLine("Upload backup file {0} progress: {1:f2}% ({2:f2} MB)", fileName, percent, fileSizeMB);
                                                break;
                                        }
                                    };

                                    var uploadProgress = createRequest.Upload();
                                }
                                catch (Google.GoogleApiException ex)
                                {
                                    Console.WriteLine("Error uploading file: {0}", ex.Message);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred while backing up and uploading {filePath} to Google Drive: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while backing up and uploading to Google Drive: " + ex.Message);
            }
        }

        private static UserCredential GetUserCredential()
        {
            string[] scopes = { DriveService.Scope.Drive };
            string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/drive-dotnet-GavelBackupGDriveUploader.json");

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;

                Console.WriteLine("Credential file saved to: " + credPath);
                return credential;
            }
        }
    }
}