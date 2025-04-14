using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Threading.Tasks;

public class HashCalculatorForm : Form
{
    private string filePath;
    private TextBox txtMD5, txtSHA1;
    private ProgressBar progressBar;
    private Label lblStatus;

    public HashCalculatorForm(string filePath)
    {
        this.filePath = filePath;
        InitializeForm();
        
        // 立即显示界面，然后异步计算哈希
        _ = CalculateHashesAsync();
    }

    private void InitializeForm()
    {
        this.Text = "文件哈希计算 - " + Path.GetFileName(filePath);
        this.Size = new System.Drawing.Size(500, 250);
        
        // MD5
        var lblMD5 = new Label { Text = "MD5:", Top = 20, Left = 20, Width = 50 };
        txtMD5 = new TextBox { Top = 20, Left = 80, Width = 300, ReadOnly = true };
        var btnCopyMD5 = new Button { Text = "复制", Top = 20, Left = 390, Width = 80 };
        btnCopyMD5.Click += (s, e) => CopyToClipboard(txtMD5.Text);
        
        // SHA1
        var lblSHA1 = new Label { Text = "SHA1:", Top = 60, Left = 20, Width = 50 };
        txtSHA1 = new TextBox { Top = 60, Left = 80, Width = 300, ReadOnly = true };
        var btnCopySHA1 = new Button { Text = "复制", Top = 60, Left = 390, Width = 80 };
        btnCopySHA1.Click += (s, e) => CopyToClipboard(txtSHA1.Text);

        // 进度条
        progressBar = new ProgressBar { Top = 100, Left = 20, Width = 450, Height = 20, Visible = false };
        lblStatus = new Label { Text = "计算中...", Top = 130, Left = 20, Width = 450, Visible = false };
        
        // 添加到窗体
        this.Controls.AddRange(new Control[] { lblMD5, txtMD5, btnCopyMD5, lblSHA1, txtSHA1, btnCopySHA1, progressBar, lblStatus });
    }

    private async Task CalculateHashesAsync()
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            bool isLargeFile = fileInfo.Length > 10 * 1024 * 1024; // 大于10MB视为大文件

            if (isLargeFile)
            {
                progressBar.Visible = true;
                lblStatus.Visible = true;
                progressBar.Style = ProgressBarStyle.Continuous;
            }

            // 计算MD5
            txtMD5.Text = await ComputeHashAsync(MD5.Create(), filePath, isLargeFile);

            // 计算SHA1
            txtSHA1.Text = await ComputeHashAsync(SHA1.Create(), filePath, isLargeFile);

            if (isLargeFile)
            {
                lblStatus.Text = "计算完成！";
                await Task.Delay(1000); // 让用户看到完成状态
                progressBar.Visible = false;
                lblStatus.Visible = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"错误: {ex.Message}", "计算失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
    }

    private async Task<string> ComputeHashAsync(HashAlgorithm hashAlgorithm, string filePath, bool showProgress)
    {
        return await Task.Run(() =>
        {
            using (hashAlgorithm)
            using (var stream = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                long totalBytesRead = 0;
                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
                    totalBytesRead += bytesRead;

                    if (showProgress)
                    {
                        int progress = (int)((double)totalBytesRead / fileSize * 100);
                        this.Invoke(new Action(() => progressBar.Value = progress));
                    }
                }

                hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
                return BitConverter.ToString(hashAlgorithm.Hash).Replace("-", "").ToLower();
            }
        });
    }

    private void CopyToClipboard(string text)
    {
        Clipboard.SetText(text);
        MessageBox.Show("已复制到剪贴板", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            MessageBox.Show("请右键点击文件选择'计算哈希值'使用本功能", "提示");
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new HashCalculatorForm(args[0]));
    }
}