# Edgeless

## Overview

Edgeless is a utility designed to completely remove Microsoft Edge from a Windows system. It terminates running Edge processes, deletes registry entries, removes directories and files, and deletes shortcuts associated with Microsoft Edge.

## Features

- **Checks for Administrator Privileges**: Ensures the application is running with the necessary rights.
- **Terminates Edge Processes**: Stops any running Microsoft Edge processes.
- **Removes Edge Registry Entries**: Deletes specific registry entries related to Microsoft Edge.
- **Deletes Edge Directories and Files**: Removes files and directories associated with Microsoft Edge from various system locations.
- **Removes Edge Shortcuts**: Deletes shortcuts to Microsoft Edge from both common and user-specific directories.

## Prerequisites

- .NET Framework 4.8 or later
- Administrator privileges

## Usage

1. **Clone the repository**:
   ```sh
   git clone https://github.com/TheyCallMeTojo/edgeless.git
   cd edgeless
   ```

2. **Build the solution**:
   Open the solution in Visual Studio and build it, or use the .NET CLI:
   ```sh
   dotnet restore
   dotnet build
   ```

3. **Run the application as Administrator**:
   Ensure you run the application with administrator privileges.

## Compatibility

This utility has been tested on the following operating systems:
- **Windows 11 Pro**
- **Windows 11 Workstations**

### Possible Compatibility Issues

- **Windows Versions**: The utility is designed for Windows 11. It may not function correctly on earlier versions of Windows (e.g., Windows 10, Windows 8, etc.). Testing on these versions is recommended before deployment.
- **System Integrity**: Removing system applications like Microsoft Edge can potentially affect system stability and integrity. It is advisable to create a system restore point or backup before using this utility.
- **User Profiles**: The utility attempts to remove Edge-related directories and shortcuts from all user profiles on the system. Ensure all user data is backed up as a precaution.
- **Permissions**: The utility requires administrator privileges to function correctly. Running it without the necessary permissions will result in failure to complete the removal process.

## Contributing

Contributions are welcome! Please fork the repository and submit pull requests.

## License

This project is licensed under the MIT License.

## Disclaimer

Use this utility at your own risk. Ensure you have backups of your important data before running the application. The author is not responsible for any damage caused by using this utility.
