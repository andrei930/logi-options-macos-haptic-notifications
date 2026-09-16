namespace Loupedeck.NotificationHapticSafePlugin
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public sealed class NotificationHapticAction : PluginDynamicCommand
    {
        private const String EventName = "notificationReceived";
        private const Int32 DebounceMsec = 1500;

        private Process _logStreamProcess;
        private Thread _monitorThread;
        private volatile Boolean _isRunning;
        private Int64 _lastTriggerTimeTicks;

        public NotificationHapticAction()
            : base(
                displayName: "Notification Haptic",
                description: "Triggers haptic feedback when a macOS notification is presented",
                groupName: "Haptics")
        {
        }

        protected override Boolean OnLoad()
        {
            this.Plugin.PluginEvents.AddEvent(
                EventName,
                "Notification Received",
                "Plays haptic feedback when a macOS notification is presented");

            this.StartNotificationMonitor();
            PluginLog.Info("NotificationHapticSafe loaded; monitoring Notification Center presentation events.");
            return true;
        }

        protected override Boolean OnUnload()
        {
            this.StopNotificationMonitor();
            PluginLog.Info("NotificationHapticSafe unloaded.");
            return true;
        }

        private void StartNotificationMonitor()
        {
            this._isRunning = true;
            this._monitorThread = new Thread(this.MonitorNotifications)
            {
                IsBackground = true,
                Name = "NotificationHapticSafeMonitor"
            };
            this._monitorThread.Start();
        }

        private void MonitorNotifications()
        {
            while (this._isRunning)
            {
                try
                {
                    this.StartLogStreamProcess();
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Notification monitor failed: {ex.GetType().Name}: {ex.Message}");
                }

                if (this._isRunning)
                {
                    Thread.Sleep(2000);
                }
            }
        }

        private void StartLogStreamProcess()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/log",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add("stream");
            startInfo.ArgumentList.Add("--style");
            startInfo.ArgumentList.Add("syslog");
            startInfo.ArgumentList.Add("--level");
            startInfo.ArgumentList.Add("info");
            startInfo.ArgumentList.Add("--predicate");
            startInfo.ArgumentList.Add("process == \"NotificationCenter\"");

            this._logStreamProcess = new Process { StartInfo = startInfo };
            this._logStreamProcess.OutputDataReceived += this.OnLogDataReceived;
            this._logStreamProcess.ErrorDataReceived += this.OnLogErrorReceived;

            this._logStreamProcess.Start();
            this._logStreamProcess.BeginOutputReadLine();
            this._logStreamProcess.BeginErrorReadLine();
            this._logStreamProcess.WaitForExit();
        }

        private void OnLogDataReceived(Object sender, DataReceivedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }

            var lower = e.Data.Trim().ToLowerInvariant();
            if (!lower.Contains("[com.apple.unc:application]"))
            {
                return;
            }

            if (!lower.Contains("queuing action present"))
            {
                return;
            }

            this.TriggerHapticWithDebounce();
        }

        private void OnLogErrorReceived(Object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                PluginLog.Warning("macOS log stream reported an error.");
            }
        }

        private void TriggerHapticWithDebounce()
        {
            var nowTicks = DateTime.UtcNow.Ticks;
            var lastTicks = Interlocked.Read(ref this._lastTriggerTimeTicks);
            var elapsedMs = (nowTicks - lastTicks) / TimeSpan.TicksPerMillisecond;

            if (elapsedMs < DebounceMsec)
            {
                return;
            }

            Interlocked.Exchange(ref this._lastTriggerTimeTicks, nowTicks);
            this.Plugin.PluginEvents.RaiseEvent(EventName);
            PluginLog.Info("macOS notification presentation detected; haptic event raised.");
        }

        private void StopNotificationMonitor()
        {
            this._isRunning = false;

            try
            {
                if (this._logStreamProcess != null && !this._logStreamProcess.HasExited)
                {
                    this._logStreamProcess.Kill();
                    this._logStreamProcess.WaitForExit(2000);
                }
                this._logStreamProcess?.Dispose();
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to stop notification monitor cleanly: {ex.GetType().Name}: {ex.Message}");
            }

            this._monitorThread?.Join(TimeSpan.FromSeconds(3));
        }

        protected override void RunCommand(String actionParameter)
        {
            this.Plugin.PluginEvents.RaiseEvent(EventName);
            PluginLog.Info("Manual notification haptic test triggered.");
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            "Notification\nHaptic";
    }
}
