# GAVEL BACKUP TO GDRIVE

This repository contains a C# solution for automatically uploading local database backups to Google Drive. It consists of two main components: a background Windows Service that handles the upload process, and a Windows Forms User Interface (UI) that allows users to manage the service, monitor progress, and handle Google Drive authentication.

## Components

### 1. Windows Service (`GavelBackupGDriveUploader`)
The core upload logic is handled by a Windows Service (`Service1` extending `ServiceBase`). 
* **Scheduled Uploads:** Instead of triggering database backups directly, the service monitors a specified local backup folder (configured via `_backupFolder`) for SQL Server backup files (`*.bak`). A timer is scheduled to run every 5 minutes to check for new files.
* **Google Drive API:** It uses the Google Drive v3 API to authenticate and upload files. Before uploading, it queries the specified Google Drive folder to ensure the file doesn't already exist off-site, preventing duplicates.
* **Inter-Process Communication:** The service uses a `NamedPipeServerStream` to broadcast its current status and upload progress (in percentage and MBs) to the UI client.
* **Logging:** The `NLog` library is used to robustly log information, errors, and system events.

### 2. User Interface (`GavelBackupGDrive.Ui`)
A desktop interface (built with DevExpress XtraForms) provides an accessible control panel for the background service.
* **Service Management:** Users can Start, Stop, and monitor the status of the `GavelBackupUploader` Windows service directly from the UI.
* **Authentication:** Handles Google Drive OAuth 2.0 user consent. It provides a Login/Logout button that generates and manages the required `client_secret.json` credentials securely.
* **Real-Time Monitoring:** Uses a `NamedPipeClientStream` to listen to the Windows Service and displays real-time upload progress.
* **Pending Backups Display:** The UI independently queries the Google Drive folder and compares it with the local backup folder, displaying a list of `.bak` files that are pending upload. 
* **System Tray Integration:** The application minimizes to the system tray using a NotifyIcon, allowing it to run unobtrusively in the background while still providing quick context-menu access to Start, Stop, or Exit the service. 

## Configuration
The application relies on an `App.config` file to set vital parameters:
* `_backupFolder`: The local directory where your `.bak` files are stored (e.g., `C:\GDriveBackups`).
* `DirectoryId`: The ID of the target folder in your Google Drive.
* `_applicationName`: The registered name of the Google API application.
