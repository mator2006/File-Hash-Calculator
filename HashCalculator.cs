using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

public class HashCalculatorForm : Form
{
    private string filePath;
    private TextBox txtMD5, txtSHA1;

    public HashCalculatorForm(string filePath)
    {
        this.filePath = filePath;
        InitializeForm();
        CalculateHashes();
    }

    private void InitializeForm()
    {
        this.Text = "文件哈希计算 - " + Path.GetFileName(filePath);
        this.Size = new System.Drawing.Size(500, 200);
        
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
        
        // 添加到窗体
        this.Controls.AddRange(new Control[] { lblMD5, txtMD5, btnCopyMD5, lblSHA1, txtSHA1, btnCopySHA1 });
    }

    private void CalculateHashes()
    {
        try
        {
            // 计算MD5
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                txtMD5.Text = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
            }

            // 计算SHA1
            using (var sha1 = SHA1.Create())
            using (var stream = File.OpenRead(filePath))
            {
                txtSHA1.Text = BitConverter.ToString(sha1.ComputeHash(stream)).Replace("-", "").ToLower();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"错误: {ex.Message}", "计算失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
        }
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