using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsInfoTool
{
    public class RuntimeInstaller : UserControl
    {
        private Label lblSystemVersion;
        private FlowLayoutPanel panel;
        private Button btnInstall;
        private Button btnExport;
        private Button btnImport;
        private ProgressBar progressBar;

        private string basePath = @"C:\Users\Administrator\Desktop\2025\code\20250808\";

        private Dictionary<string, string> runtimeFiles = new()
        {
            { ".NET 9 SDK", "dotnet-sdk-9.0.304-win-x64.exe" },
            { ".NET 8 SDK", "dotnet-sdk-8.0.413-win-x64.exe" },
            { ".NET 7 SDK", "dotnet-sdk-7.0.410-win-x64.exe" },
            { ".NET 6 SDK", "dotnet-sdk-6.0.428-win-x64.exe" },
            { ".NET 5 SDK", "dotnet-sdk-5.0.408-win-x64.exe" },
            { ".NET Core 3.1 SDK", "dotnet-sdk-3.1.426-win-x64.exe" },
            { ".NET Core 3.0 SDK", "dotnet-sdk-3.0.103-win-x64.exe" },
            { ".NET Core 2.2 SDK", "dotnet-sdk-2.2.207-win-x64.exe" },
            { ".NET Framework 4.8.1 DevPack", "ndp481-devpack-cht.exe" }
        };

        private Dictionary<string, CheckBox> checkboxes = new();

        public RuntimeInstaller()
        {
            InitializeUI();
            LoadRuntimeStatus();
        }

        private void InitializeUI()
        {
            // 顯示系統版本
            lblSystemVersion = new Label
            {
                Text = "📌 當前系統版本：" + GetOSVersionText(),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Padding = new Padding(10),
                Dock = DockStyle.Top
            };
            this.Controls.Add(lblSystemVersion);

            panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Height = 300
            };

            foreach (var item in runtimeFiles.Keys)
            {
                CheckBox cb = new() { Text = item, AutoSize = true };
                panel.Controls.Add(cb);
                checkboxes[item] = cb;
            }

            btnInstall = new Button { Text = "安裝選取項目", Width = 200 };
            btnInstall.Click += BtnInstall_Click;

            btnExport = new Button { Text = "匯出選擇", Width = 100 };
            btnExport.Click += BtnExport_Click;

            btnImport = new Button { Text = "匯入選擇", Width = 100 };
            btnImport.Click += BtnImport_Click;

            progressBar = new ProgressBar { Dock = DockStyle.Bottom, Height = 20 };

            FlowLayoutPanel bottomPanel = new()
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight
            };
            bottomPanel.Controls.AddRange(new Control[] { btnInstall, btnExport, btnImport });

            this.Controls.AddRange(new Control[] { panel, bottomPanel, progressBar });
        }

        private void LoadRuntimeStatus()
        {
            foreach (var item in runtimeFiles.Keys)
            {
                bool installed = IsRuntimeInstalled(item);
                var cb = checkboxes[item];

                cb.Text = $"{item} {(installed ? "✅" : "❌")}";

                if (installed)
                {
                    cb.Enabled = false;
                    cb.Checked = false;
                    cb.ForeColor = Color.Gray;
                }
                else
                {
                    cb.Enabled = true;
                    cb.Checked = false;
                    cb.ForeColor = Color.Red;
                }
            }
        }

        private bool IsRuntimeInstalled(string name)
        {
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey);
            if (rk != null)
            {
                foreach (string subKeyName in rk.GetSubKeyNames())
                {
                    using RegistryKey subKey = rk.OpenSubKey(subKeyName);
                    string displayName = subKey?.GetValue("DisplayName") as string ?? "";
                    if (displayName.Contains(name, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        private async void BtnInstall_Click(object sender, EventArgs e)
        {
            var selected = new List<string>();
            foreach (var item in checkboxes)
            {
                if (item.Value.Checked && item.Value.Enabled)
                    selected.Add(item.Key);
            }

            if (selected.Count == 0)
            {
                MessageBox.Show("請先勾選要安裝的項目");
                return;
            }

            progressBar.Maximum = selected.Count;
            progressBar.Value = 0;

            foreach (var key in selected)
            {
                string exePath = Path.Combine(basePath, runtimeFiles[key]);
                if (!File.Exists(exePath))
                {
                    MessageBox.Show($"找不到安裝檔：{exePath}");
                    continue;
                }

                ProcessStartInfo psi = new()
                {
                    FileName = exePath,
                    Arguments = "/quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    Process proc = Process.Start(psi);
                    await proc.WaitForExitAsync();
                    progressBar.Value += 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"安裝失敗：{key}\n{ex.Message}");
                }
            }

            MessageBox.Show("安裝作業完成！");
            LoadRuntimeStatus();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            var selected = new List<string>();
            foreach (var kv in checkboxes)
            {
                if (kv.Value.Checked && kv.Value.Enabled)
                    selected.Add(kv.Key);
            }

            string json = JsonSerializer.Serialize(selected);
            File.WriteAllText("selected_runtime.json", json);
            MessageBox.Show("已匯出選擇至 selected_runtime.json");
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (!File.Exists("selected_runtime.json"))
            {
                MessageBox.Show("找不到 selected_runtime.json");
                return;
            }

            string json = File.ReadAllText("selected_runtime.json");
            var selected = JsonSerializer.Deserialize<List<string>>(json);

            foreach (var kv in checkboxes)
            {
                if (kv.Value.Enabled)
                    kv.Value.Checked = selected.Contains(kv.Key);
            }

            MessageBox.Show("已匯入選擇");
        }

        private string GetOSVersionText()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = key?.GetValue("ProductName")?.ToString() ?? "Windows";
                string buildNumber = key?.GetValue("CurrentBuildNumber")?.ToString() ?? "?";
                string displayVersion = key?.GetValue("DisplayVersion")?.ToString();
                string releaseId = key?.GetValue("ReleaseId")?.ToString();

                if (!string.IsNullOrWhiteSpace(displayVersion))
                    return $"{productName} {displayVersion} (Build {buildNumber})";
                else if (!string.IsNullOrWhiteSpace(releaseId))
                    return $"{productName} {releaseId} (Build {buildNumber})";
                else
                    return $"{productName} (Build {buildNumber})";
            }
            catch
            {
                return "無法取得系統版本資訊";
            }
        }
    }
}