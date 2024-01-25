using System.Diagnostics;

namespace DataFeedsWorker.Services;


public class TmuxConfig
{
    public string Command { get; set; }
}


    public class TmuxService
    {
        private readonly TmuxConfig _tmuxConfig;
        private readonly ILogger<TmuxService> _logger;

        public TmuxService(TmuxConfig config, ILogger<TmuxService> logger)
        {
            _tmuxConfig = config;
            _logger = logger;
        }

        public void SendCtrlCSignalToPane(string pane)
        {
            string sendCtrlCCommand = $"tmux send-keys -t {pane} C-c";
            string result = ExecuteShellCommand(sendCtrlCCommand);

            if (!string.IsNullOrWhiteSpace(result))
            {
                _logger.LogError(result);
            }
            else
            {
                _logger.LogInformation($"Sent CTRL+C to the pane {pane}");
            }
        }

        public void KillTmuxSession(string sessionName)
        {
            string killSessionCommand = $"tmux kill-session -t {sessionName}";
            string result = ExecuteShellCommand(killSessionCommand);

            if (!string.IsNullOrWhiteSpace(result))
            {
                _logger.LogError(result);
            }
            else
            {
                _logger.LogInformation($"Killed tmux session {sessionName}");
            }
        }

        public void StartTmuxinatorSession()
        {
            string tmuxinatorCommand = _tmuxConfig.Command;
            string result = ExecuteShellCommand(tmuxinatorCommand);

            if (!string.IsNullOrWhiteSpace(result))
            {
                _logger.LogError(result);
            }
            else
            {
                _logger.LogInformation("Tmuxinator session started successfully.");
            }
        }

        private string ExecuteShellCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                using StreamReader outputReader = process.StandardOutput;
                using StreamReader errorReader = process.StandardError;

                string result = outputReader.ReadToEnd();
                string error = errorReader.ReadToEnd();
                process.WaitForExit();  // Wait for the process to complete

                if (process.ExitCode != 0)
                {
                    return error.Trim();
                }

                return result.Trim();
            }
        }
    }