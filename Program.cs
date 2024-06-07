using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;

namespace Edgeless;

class Program
{
    static void Main(string[] args)
    {
        // Check if running as administrator
        if (!IsAdministrator())
        {
            Console.WriteLine("Please run this application as an administrator.");
            Add2Log("Main", "Please run this application as an administrator.");
            Console.Read();
            return;
        }

        // Steps to remove Microsoft Edge
        TerminateEdgeProcesses();
        RemoveEdgeRegistryEntries();
        RemoveEdgeShortcuts();
        RemoveEdgeDirectories();

        //Probably should check if there were errors before saying this with any authority.
        Console.WriteLine("Microsoft Edge has been removed. Please restart your computer.");
        Console.Read();
    }

    // Check if the current user is an administrator
    static bool IsAdministrator()
    {
        using WindowsIdentity? identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal? principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    // Terminate any running Edge processes
    static void TerminateEdgeProcesses()
    {
        string[] edgeProcesses = ["msedge", "MicrosoftEdge", "MicrosoftEdgeUpdate"];

        foreach (var processName in edgeProcesses)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"Terminated process: {processName}");
                    Add2Log("TerminateEdgeProcess", $"Terminated process: {processName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to terminate process {processName}: {ex.Message}");
                    Add2Log("TerminateEdgeProcess", $"Exception: {ex.Message}");
                }
            }
        }
    }

    // Remove Edge installation directories and files
    static void RemoveEdgeDirectories()
    {
        string[] edgePaths =
        {
            @"C:\Windows\System32\MicrosoftEdgeCP.exe",
            @"C:\Windows\System32\MicrosoftEdgeSH.exe",
            @"C:\Windows\System32\MicrosoftEdgeBCHost.exe",
            @"C:\Windows\System32\MicrosoftEdgeDevTools.exe",
            @"C:\Program Files (x86)\Microsoft\EdgeUpdate\MicrosoftEdgeUpdate.exe",

            @"C:\Program Files (x86)\Microsoft\Edge",
            @"C:\Program Files\Microsoft\Edge",
            @"C:\Program Files (x86)\Microsoft\EdgeUpdate",
            @"C:\Program Files (x86)\Microsoft\EdgeCore",
            @"C:\Program Files\Microsoft\EdgeUpdate",
            @"C:\ProgramData\Microsoft\Edge",
            @"C:\ProgramData\Microsoft\EdgeUpdate",
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Microsoft\Edge"),
            Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Roaming\Microsoft\Edge")
        };

        foreach (var path in edgePaths)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    Console.WriteLine($"Deleted directory: {path}");
                    Add2Log("RemoveEdgeDirectories", $"Deleted directory: {path}");
                }
                else if (File.Exists(path))
                {
                    TakeOwnership(path);
                    File.Delete(path);
                    Console.WriteLine($"Deleted file: {path}");
                    Add2Log("RemoveEdgeDirectories", $"Deleted file: {path}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access to the path '{path}' is denied. Run the application as an administrator.");
                Add2Log("RemoveEdgeDirectories", $"Access to the path '{path}' is denied. Run the application as an administrator.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {path}: {ex.Message}");
                Add2Log("RemoveEdgeDirectories", $"Error deleting {path}: {ex.Message}");
            }
        }
    }

    // Remove Edge registry entries
    static void RemoveEdgeRegistryEntries()
    {
        string[] edgeRegistryKeys =
        {
            @"SOFTWARE\Microsoft\Edge",
            @"SOFTWARE\WOW6432Node\Microsoft\Edge",
        };

        foreach (var key in edgeRegistryKeys)
        {
            try
            {
                using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(key, true);
                if (registryKey != null)
                {
                    Registry.LocalMachine.DeleteSubKeyTree(key);
                    Console.WriteLine($"Deleted registry key: {key}");
                    Add2Log("RemoveEdgeRegistryEntries", $"Deleted registry key: {key}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting registry key {key}: {ex.Message}");
                Add2Log("RemoveEdgeRegistryEntries", $"Error deleting registry key {key}: {ex.Message}");
            }
        }
    }

    // Take ownership and grant full control of a file
    // It's wild how easy it is to take over files this way. 
    static bool TakeOwnership(string filePath)
    {
        try
        {
            // Take ownership of the file
            if (!RunProcessAsAdmin("takeown", $"/f \"{filePath}\""))
            {
                return false;
            }

            // Grant full control to administrators
            if (!RunProcessAsAdmin("icacls", $"\"{filePath}\" /grant administrators:F"))
            {
                return false;
            }

            Console.WriteLine("Take Ownership and Permission Granted: " + filePath);
            Add2Log("TakeOwnership", $"Take Ownership and Permissions Granted for: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error taking ownership: {ex.Message}");
            Add2Log("TakeOwnership", $"Error taking ownership: {ex.Message}");
            return false;
        }
    }

    // Run a process with administrative privileges
    static bool RunProcessAsAdmin(string fileName, string arguments)
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/c {fileName} {arguments}",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Verb = "runas"
            };

            using Process process = new()
            {
                StartInfo = startInfo
            };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"{fileName} OK: {arguments}");
                Add2Log("RunProcessAsAdmin", $"{fileName} OK: {arguments}");
                return true;
            }
            else
            {
                Console.WriteLine(process.StandardError.ReadToEnd());
                Add2Log("RunProcessAsAdmin", process.StandardError.ReadToEnd());
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running {fileName}: {ex.Message}");
            Add2Log("RunProcessAsAdmin", $"Error running {fileName}: {ex.Message}");
            return false;
        }
    }

    // Remove Edge shortcuts from common and user-specific locations
    // For the life of me I can't find a way to remove shortcuts from the start menu.
    static void RemoveEdgeShortcuts()
    {
        // Common desktop and start menu paths for all users
        string[] commonPaths =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
        ];

        // User-specific desktop and start menu paths
        string[] userPaths =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + @"\programs", //This shows up if you manually install Edge
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms)
        };

        // Edge shortcut filenames
        string[] edgeShortcuts =
        {
            "Microsoft Edge.lnk",
            "Microsoft Edge - Shortcut.lnk",
            "Edge.lnk"
        };

        // Remove from common paths
        foreach (var path in commonPaths)
        {
            RemoveShortcutsFromDirectory(path, edgeShortcuts);
        }

        // Remove from all user-specific paths
        var userDirectories = Directory.GetDirectories(@"C:\Users");
        foreach (var userDir in userDirectories)
        {
            foreach (var path in userPaths)
            {
                var userPath = Path.Combine(userDir, path.TrimStart('\\'));
                RemoveShortcutsFromDirectory(userPath, edgeShortcuts);
            }
        }
    }

    // Remove specified shortcuts from a given directory
    static void RemoveShortcutsFromDirectory(string directory, string[] shortcuts)
    {
        foreach (var shortcut in shortcuts)
        {
            var shortcutPath = Path.Combine(directory, shortcut);
            try
            {
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                    Console.WriteLine($"Deleted shortcut: {shortcutPath}");
                    Add2Log("RemoveShortcutsFromDirectory", $"Deleted shortcut: {shortcutPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {shortcutPath}: {ex.Message}");
                Add2Log("RemoveShortcutsFromDirectory", $"Error deleting {shortcutPath}: {ex.Message}");
            }
        }
    }

    static void Add2Log(string Method, string Message)
    {
        try
        {
            string logFilePath = "log.txt";
            using StreamWriter writer = new StreamWriter(logFilePath, true);
            writer.WriteLine($"{DateTime.Now}: In {Method} : {Message}");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}
