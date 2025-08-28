using System.Diagnostics;
using System.IO;

public class BatchFileExecutor
{
    public static void RunGhostscriptBatchFile(string batchFilePath, string directory, string arguments = "")
    {
        if (!File.Exists(batchFilePath))
            throw new FileNotFoundException($"Batch file not found: {batchFilePath}");

        try
        {
            // Create a new ProcessStartInfo object
            ProcessStartInfo startInfo = new()
            {
                Arguments = $"/C \"{batchFilePath}\" {arguments}", // /C executes the command and then terminates
                CreateNoWindow = true, // Do not create a new window for the process
                FileName = "cmd.exe", // Specify the command shell
                RedirectStandardError = true,
                RedirectStandardOutput = true, // Redirect output to capture it
                UseShellExecute = false, // Do not use the shell to execute
                WorkingDirectory = directory
            };

            // Start the process
            using Process? process = Process.Start(startInfo);

            if (process is not null)
            {
                // Optionally, read the output
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                int exitCode = process.ExitCode;

                process.WaitForExit(); // Wait for the process to complete

                /*
                if (process.ExitCode != 0)
                {
                    // Handle errors if the batch file failed
                    Console.WriteLine($"Batch file exited with code: {process.ExitCode}");
                    Console.WriteLine($"Output: {output}");
                }
                else
                    Console.WriteLine($"Batch file executed successfully. Output: {output}");
                */
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing batch file: {ex.Message}");
        }
    }
}