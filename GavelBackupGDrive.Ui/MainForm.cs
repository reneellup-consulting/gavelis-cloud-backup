using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GavelBackupGDrive.Ui {
    public partial class MainForm : DevExpress.XtraEditors.XtraForm {
        private string _backupFolder = ConfigurationManager.AppSettings["_backupFolder"];
        private string _credentialsFilePath = ConfigurationManager.AppSettings["_credentialsFilePath"];
        private string DirectoryId = ConfigurationManager.AppSettings["DirectoryId"];
        private string _applicationName = ConfigurationManager.AppSettings["_applicationName"];

        private ServiceController _serviceController;

        private bool _isLoggedIn = false;

        public MainForm() {
            InitializeComponent();

            string sourceFilePath = Path.Combine(Application.StartupPath, "GoogleDriveCredentials.json");
            string destFilePath = @"C:\GoogleDriveCredentials.json";

            if (!File.Exists(destFilePath)) {
                File.Copy(sourceFilePath, destFilePath);
            }

            if (!Directory.Exists(_backupFolder)) {
                Directory.CreateDirectory(_backupFolder);
            }

            Icon myIcon = Properties.Resources.gavelisback;

            notifyIcon1.Icon = myIcon;

            // Add context menu items
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Start", null, StartService);
            contextMenu.Items.Add("Stop", null, StopService);
            contextMenu.Items.Add("Exit", null, ExitService);
            notifyIcon1.ContextMenuStrip = contextMenu;

            lblTimeRemaining.ForeColor = System.Drawing.Color.Green;

            _serviceController = new ServiceController("GavelBackupUploader");

            var status = ServiceControllerStatus.Stopped;
            var isRunning = status == ServiceControllerStatus.Running;

            lblTimeRemaining.Text = "";

            var populateThread = new Thread(() => {
                while (true) {
                    _backupFolder = ConfigurationManager.AppSettings["_backupFolder"];
                    var backupFileList = Directory.GetFiles(_backupFolder, "*.bak").ToList();

                    // Retrieve a list of files in the Google Drive folder
                    var credentials = GoogleCredential.FromFile(_credentialsFilePath).CreateScoped(new[] { DriveService.Scope.Drive });
                    var service = new DriveService(new BaseClientService.Initializer() {
                        HttpClientInitializer = credentials,
                        ApplicationName = _applicationName
                    });

                    var fileListRequest = service.Files.List();
                    fileListRequest.Q = $"'{DirectoryId}' in parents and trashed = false"; // Filter by the Google Drive folder ID
                    fileListRequest.Fields = "nextPageToken, files(id, name)";

                    var fileList = fileListRequest.Execute().Files;
                    var existingFileNames = fileList.Select(file => file.Name).ToList();

                    // Filter out the files that already exist in the Google Drive folder
                    backupFileList = backupFileList.Where(filePath => !existingFileNames.Contains(Path.GetFileName(filePath))).ToList();

                    Invoke(new Action(() => { backupListBoxControl.DataSource = backupFileList; }));

                    Thread.Sleep(60000);
                }
            });

            populateThread.Start();

            var updateThread = new Thread(() => {
                using (var client = new NamedPipeClientStream(".", "MyServicePipe", PipeDirection.In)) {
                    client.Connect();
                    var reader = new StreamReader(client);

                    while (true) {
                        var message = reader.ReadLine();
                        if (message == null) {
                            break;
                        }

                        Invoke(new Action(() => { lblTimeRemaining.Text = isRunning ? message : ""; }));
                    }

                    reader.Close();
                    client.Close();
                }
            });

            updateThread.Start();
        }

        private void StartService(object sender, EventArgs e) {
            var startTime = DateTime.Now;
            ServiceController service = new ServiceController("GavelBackupUploader");
            if (service.Status == ServiceControllerStatus.Stopped) {
                try {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    // Update NotifyIcon text to indicate that service is running
                    notifyIcon1.Text = "GAVEL I.S Offsite Backup (Running)";
                } catch (Exception ex) {
                    MessageBox.Show("Error starting service: " + ex.Message);
                }
            } else {
                MessageBox.Show("Service is already running.");
            }
        }

        private void StopService(object sender, EventArgs e) {
            using (ServiceController sc = new ServiceController("GavelBackupUploader")) {
                if (sc.Status != ServiceControllerStatus.Stopped) {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
        }

        private void ExitService(object sender, EventArgs e) {
            // Exit the service
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        private void startButton_Click(object sender, EventArgs e) {
            try {
                _serviceController.Start();
                _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                UpdateStatus();
            } catch (Exception ex) {
                MessageBox.Show($"An error occurred while starting the service: {ex.Message}");
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            notifyIcon1.Visible = true;

            UpdateStatus();

            GetUserCredential();
        }

        private void UpdateStatus() {
            var status = _serviceController.Status;
            var isRunning = status == ServiceControllerStatus.Running;
            serviceStatusLabel.Text = isRunning ? "Running" : "Stopped";
            serviceStatusLabel.ForeColor = isRunning ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            startButton.Enabled = !isRunning && _isLoggedIn;
            stopButton.Enabled = isRunning;
            loginButton.Text = _isLoggedIn ? "Logout" : "Login";
            loginButton.Enabled = !isRunning;
        }

        private void stopButton_Click(object sender, EventArgs e) {
            try {
                _serviceController.Stop();
                _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                UpdateStatus();
            } catch (Exception ex) {
                MessageBox.Show($"An error occurred while stopping the service: {ex.Message}");
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.Visible = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                this.Visible = false;
            }
        }

        private void GetUserCredential() {
            var tcs = new TaskCompletionSource<bool>();
            var thread = new Thread(() => {
                try {
                    string[] scopes = { DriveService.Scope.Drive };
                    string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/drive-dotnet-GavelBackupGDriveUploader.json");

                    using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read)) {
                        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;

                        Console.WriteLine("Credential file saved to: " + credPath);
                        if (credential != null) {
                            //loginButton.Text = "Logout";
                            Invoke(new Action(() => {
                                loginButton.Text = "Logout";
                                _isLoggedIn = true;
                            }));

                        } else {
                            Invoke(new Action(() => {
                                loginButton.Text = "Login";
                                _isLoggedIn = false;
                            }));
                        }
                        Invoke(new Action(() => { UpdateStatus(); }));
                    }
                    tcs.SetResult(true);
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    Invoke(new Action(() => {
                        loginButton.Text = "Login";
                        _isLoggedIn = false;
                    }));
                    tcs.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void loginButton_Click(object sender, EventArgs e) {
            string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".credentials/drive-dotnet-GavelBackupGDriveUploader.json");

            if (loginButton.Text == "Login") {
                var tcs = new TaskCompletionSource<bool>();
                var thread = new Thread(() => {
                    try {
                        string[] scopes = { DriveService.Scope.Drive };

                        using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read)) {
                            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                GoogleClientSecrets.FromStream(stream).Secrets,
                                scopes,
                                "user",
                                CancellationToken.None,
                                new FileDataStore(credPath, true)).Result;

                            Console.WriteLine("Credential file saved to: " + credPath);
                            if (credential != null) {
                                Invoke(new Action(() => {
                                    loginButton.Text = "Logout";
                                    _isLoggedIn = true;
                                }));

                            } else {
                                Invoke(new Action(() => {
                                    loginButton.Text = "Login";
                                    _isLoggedIn = false;
                                }));
                            }
                            Invoke(new Action(() => { UpdateStatus(); }));
                        }
                        tcs.SetResult(true);
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                        loginButton.Text = "Login";
                        _isLoggedIn = false;
                        // Handle any exceptions that may occur during the login process
                        tcs.SetException(ex);
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            } else {
                try {
                    var status = _serviceController.Status;
                    var isRunning = status == ServiceControllerStatus.Running;
                    if (isRunning) {
                        _serviceController.Stop();
                        _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        UpdateStatus();
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"An error occurred while stopping the service: {ex.Message}");
                } finally {
                    // Revoke the access token and delete the token file
                    if (Directory.Exists(credPath)) {
                        Directory.Delete(credPath, true);
                    }

                    // Disable the Logout button and enable the Login button
                    loginButton.Text = "Login";

                    _isLoggedIn = false;
                    UpdateStatus();
                }
            }
        }

    }
}
