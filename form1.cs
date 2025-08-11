using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WindowsInfoTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string versionInfo = GetWindowsVersion();
            txtInfo.Text = versionInfo;

            // 初始化樣式選單
            cmbThemes.Items.AddRange(new string[] { "XP", "Vista", "Windows7", "Windows8", "Windows10", "Windows11" });

            // 根據版本自動設定樣式
            string osTheme = DetectOSTheme();
            int index = cmbThemes.Items.IndexOf(osTheme);
            if (index >= 0)
            {
                cmbThemes.SelectedIndex = index;
                ApplyTheme(osTheme);
            }
            else
            {
                cmbThemes.SelectedIndex = 1; // 預設 Vista
                ApplyTheme("Vista");
            }

            if (!versionInfo.Contains("Windows 11"))
            {
                chkClassicContextMenu.Enabled = false;
                chkClassicContextMenu.Text += "（僅限 Windows 11）";
            }
        }

        private string DetectOSTheme()
        {
            Version ver = Environment.OSVersion.Version;

            if (ver.Major == 5)
                return "XP";
            else if (ver.Major == 6 && ver.Minor == 0)
                return "Vista";
            else if (ver.Major == 6 && ver.Minor == 1)
                return "Windows7";
            else if (ver.Major == 6 && (ver.Minor == 2 || ver.Minor == 3))
                return "Windows8";
            else if (ver.Major == 10 && ver.Build < 22000)
                return "Windows10";
            else if (ver.Major == 10 && ver.Build >= 22000)
                return "Windows11";
            else
                return "Vista";
        }

        private string GetWindowsVersion()
        {
            var os = Environment.OSVersion;
            string regVersion = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion",
                "DisplayVersion", "")?.ToString() ?? "";
            return $"OS Version: {os.Version}\\r\\n" +
                   $"Platform: {os.Platform}\\r\\n" +
                   $"Display Version: {regVersion}";
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtInfo.Text);
            MessageBox.Show("已複製版本資訊！");
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FileName = "WindowsVersionInfo.txt"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, txtInfo.Text);
                MessageBox.Show("匯出完成！");
            }
        }

        private void chkClassicContextMenu_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string keyPath = @"Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (chkClassicContextMenu.Checked)
                        key.SetValue("", "", RegistryValueKind.String);
                    else
                        key.DeleteValue("", false);
                }

                MessageBox.Show("設定已變更，需重新啟動資源總管才能生效。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"發生錯誤：{ex.Message}");
            }
        }

        private void btnRestartExplorer_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("explorer"))
                {
                    proc.Kill();
                }

                System.Threading.Thread.Sleep(1000);
                Process.Start("explorer.exe");
                MessageBox.Show("已重啟資源總管！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("重啟 Explorer 失敗：" + ex.Message);
            }
        }

        private void btnRestartAsAdmin_Click(object sender, EventArgs e)
        {
            if (!IsRunningAsAdministrator())
            {
                try
                {
                    ProcessStartInfo procInfo = new ProcessStartInfo
                    {
                        FileName = Application.ExecutablePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(procInfo);
                    Application.Exit();
                }
                catch
                {
                    MessageBox.Show("使用者取消了系統管理員權限的請求。");
                }
            }
            else
            {
                MessageBox.Show("目前已是系統管理員權限執行。");
            }
        }

        private bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private void ApplyTheme(string theme)
        {
            Color backColor, buttonBackColor, groupBackColor, foreColor;
            Font defaultFont = new Font("Segoe UI", 9, FontStyle.Regular);
            Font groupFont = new Font("Segoe UI", 9, FontStyle.Bold);

            switch (theme)
            {
                case "XP":
                    backColor = Color.FromArgb(212, 208, 200);
                    buttonBackColor = Color.LightGray;
                    groupBackColor = Color.FromArgb(240, 240, 240);
                    foreColor = Color.Black;
                    break;
                case "Vista":
                    backColor = Color.FromArgb(214, 223, 247);
                    buttonBackColor = Color.FromArgb(193, 210, 240);
                    groupBackColor = Color.FromArgb(234, 240, 255);
                    foreColor = Color.Black;
                    break;
                case "Windows7":
                    backColor = Color.WhiteSmoke;
                    buttonBackColor = Color.Gainsboro;
                    groupBackColor = Color.White;
                    foreColor = Color.Black;
                    break;
                case "Windows8":
                    backColor = Color.White;
                    buttonBackColor = Color.SkyBlue;
                    groupBackColor = Color.White;
                    foreColor = Color.Black;
                    break;
                case "Windows10":
                    backColor = Color.White;
                    buttonBackColor = Color.LightGray;
                    groupBackColor = Color.White;
                    foreColor = Color.Black;
                    break;
                case "Windows11":
                    backColor = Color.White;
                    buttonBackColor = Color.LightSteelBlue;
                    groupBackColor = Color.White;
                    foreColor = Color.Black;
                    break;
                default:
                    return;
            }

            this.BackColor = backColor;
            this.ForeColor = foreColor;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Standard;
                    btn.BackColor = buttonBackColor;
                    btn.ForeColor = foreColor;
                    btn.Font = defaultFont;
                }
                else if (ctrl is TextBox txt)
                {
                    txt.BackColor = Color.White;
                    txt.ForeColor = foreColor;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.Font = defaultFont;
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = foreColor;
                    lbl.Font = defaultFont;
                }
                else if (ctrl is CheckBox cbx)
                {
                    cbx.ForeColor = foreColor;
                    cbx.Font = defaultFont;
                }
                else if (ctrl is GroupBox grp)
                {
                    grp.ForeColor = foreColor;
                    grp.Font = groupFont;
                    grp.BackColor = groupBackColor;
                }
            }
        }
    }
}
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;

private readonly string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
private readonly string backupPath = Path.Combine(Application.StartupPath, "Backup", "taskmgr_backup.exe");
private readonly string versionsFolder = Path.Combine(Application.StartupPath, "TaskMgrVersions");

private void InitializeTaskMgrUI()
{
    cmbTaskMgrVer.Items.AddRange(new string[] { "xp", "vistaor7" });
    cmbTaskMgrVer.SelectedIndex = 0;
    UpdateTaskMgrVersionLabel();
}

private void UpdateTaskMgrVersionLabel()
{
    string taskmgrPath = Path.Combine(systemPath, "taskmgr.exe");
    if (File.Exists(taskmgrPath))
    {
        var info = FileVersionInfo.GetVersionInfo(taskmgrPath);
        lblTMVersion.Text = $"目前版本: {info.FileVersion}";
    }
    else
    {
        lblTMVersion.Text = "找不到 taskmgr.exe";
    }
}

private void btnSwitchTM_Click(object sender, EventArgs e)
{
    if (!IsAdministrator()) return;

    string selected = cmbTaskMgrVer.SelectedItem.ToString();
    string sourcePath = Path.Combine(versionsFolder, selected, "taskmgr.exe");
    string targetPath = Path.Combine(systemPath, "taskmgr.exe");

    if (!File.Exists(sourcePath))
    {
        MessageBox.Show("找不到指定版本的 taskmgr.exe。");
        return;
    }

    if (IsTaskMgrRunning())
    {
        MessageBox.Show("請先關閉 Task Manager 再切換版本！");
        return;
    }

    try
    {
        TakeOwnership(targetPath);

        if (!File.Exists(backupPath))
        {
            File.Copy(targetPath, backupPath, true);
        }

        File.Copy(sourcePath, targetPath, true);
        MessageBox.Show("Task Manager 版本已切換！");
        UpdateTaskMgrVersionLabel();
    }
    catch (Exception ex)
    {
        MessageBox.Show("切換失敗：" + ex.Message);
    }
}

private void btnRestoreTM_Click(object sender, EventArgs e)
{
    if (!IsAdministrator()) return;

    string targetPath = Path.Combine(systemPath, "taskmgr.exe");

    if (!File.Exists(backupPath))
    {
        MessageBox.Show("找不到備份檔案！");
        return;
    }

    if (IsTaskMgrRunning())
    {
        MessageBox.Show("請先關閉 Task Manager 再還原！");
        return;
    }

    try
    {
        TakeOwnership(targetPath);
        File.Copy(backupPath, targetPath, true);
        MessageBox.Show("已還原原始 Task Manager！");
        UpdateTaskMgrVersionLabel();
    }
    catch (Exception ex)
    {
        MessageBox.Show("還原失敗：" + ex.Message);
    }
}

private bool IsTaskMgrRunning()
{
    return Process.GetProcessesByName("taskmgr").Length > 0;
}

private bool IsAdministrator()
{
    if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
    {
        MessageBox.Show("請以系統管理員身份重新啟動此程式！");
        return false;
    }
    return true;
}

private void TakeOwnership(string filePath)
{
    ProcessStartInfo psi1 = new ProcessStartInfo("takeown", $"/F \"{filePath}\"")
    {
        CreateNoWindow = true,
        UseShellExecute = false
    };
    Process.Start(psi1).WaitForExit();

    ProcessStartInfo psi2 = new ProcessStartInfo("icacls", $"\"{filePath}\" /grant administrators:F")
    {
        CreateNoWindow = true,
        UseShellExecute = false
    };
    Process.Start(psi2).WaitForExit();
}
