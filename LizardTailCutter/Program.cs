using System;
using System.Drawing;
using System.Windows.Forms;

using System.Text;
using System.IO;

class MainWindow
{
    [STAThread]
    static void Main()
    {
        //二重起動をチェックする
        if (System.Diagnostics.Process.GetProcessesByName(
            System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
        {
            //すでに起動していると判断して終了
            //MessageBox.Show("既に起動しています");
            return;
        }

        LTCMain rm = new LTCMain();
        Application.Run();
    }
}

class LTCMain : Form
{
    HotKey hotKey;
    ClipBoardWatcher mCbw;
    ToolStripMenuItem cMenuItem1;
    String mStrCVal = "";

    public LTCMain()
    {
        this.ShowInTaskbar = false;
        this.setComponents();
    }

    private void Mode_Click(object sender, EventArgs e)
    {
        if (cMenuItem1.Checked)
        {
            cMenuItem1.Checked = false;
        }
        else
        {
            cMenuItem1.Checked = true;
        }
    }

    private void Close_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void setComponents()
    {
        // タスクトレイICONの表示
        System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        NotifyIcon icon = new NotifyIcon();
        icon.Icon = new Icon(myAssembly.GetManifestResourceStream("LizardTailCutter.tsk.ico"));
        icon.Visible = true;
        icon.Text = "トカゲの尻尾切り";

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.ShowItemToolTips = true;

        cMenuItem1 = new ToolStripMenuItem();
        cMenuItem1.Text = "&自動";
        cMenuItem1.ToolTipText = "自動OFF時はCtrl+Shift+Bで実行";

        cMenuItem1.Checked = true;
        cMenuItem1.Click += new EventHandler(Mode_Click);
        menu.Items.Add(cMenuItem1);

        ToolStripMenuItem menuItem2 = new ToolStripMenuItem();
        menuItem2.Text = "&終了";
        menuItem2.Click += new EventHandler(Close_Click);
        menu.Items.Add(menuItem2);
        icon.ContextMenuStrip = menu;

        // ホットキーイベント監視の開始
        startHotKeyWatch();

        // クリップボード監視の開始
        startClipWatch();
    }

    private void startHotKeyWatch()
    {
#if DEBUG
        LizardTailCutter.Utility.log(System.DateTime.Now + " " + "startHotKeyWatch");
#endif
        hotKey = new HotKey(MOD_KEY.CONTROL | MOD_KEY.SHIFT, Keys.B);
        //hotKey = new HotKey(MOD_KEY.ALT | MOD_KEY.CONTROL | MOD_KEY.SHIFT);
        hotKey.HotKeyPush += new EventHandler(hotKey_HotKeyPush);
    }

    private void startClipWatch()
    {
#if DEBUG
        LizardTailCutter.Utility.log(System.DateTime.Now + " " + "startClipWatch");
#endif

        // クリップボード監視の生成
        mCbw = new ClipBoardWatcher();

        try
        {
            mCbw.DrawClipBoard += (sender2, e2) => {
                // 自動ON時のみ動作
                if ((cMenuItem1.Checked) && (Clipboard.ContainsText()))
                {
                    // 既に処理済みだった場合は実行しない(クリップボードに戻す処理をまたフックしてしまうので必要)
                    if (Clipboard.GetText() != mStrCVal)
                    {
#if DEBUG
                        LizardTailCutter.Utility.log(System.DateTime.Now + " " + "Clipboard.GetText : " + Clipboard.GetText());
#endif
                        // クリップボードから取得
                        mStrCVal = Clipboard.GetText().TrimEnd('\r', '\n');
                        if (mStrCVal != "")
                        {
                            // 終端の改行を除去してクリップボードに戻す
                            Clipboard.SetDataObject(mStrCVal);
#if DEBUG
                            LizardTailCutter.Utility.log(System.DateTime.Now + " " + "Clipboard.SetDataObject : " + mStrCVal);
#endif
                        }
                    }
                }
            };
        }
        catch (Exception ex)
        {
#if DEBUG
            LizardTailCutter.Utility.log(System.DateTime.Now + " " + "error1 : " + ex.Message);
#endif
            // 例外発生時はクリップボード監視の生成をやり直す
            if (Clipboard.GetText() != mStrCVal)
            {
                mStrCVal = Clipboard.GetText().TrimEnd('\r', '\n');
                if (mStrCVal != "")
                {
                    Clipboard.SetDataObject(mStrCVal);
                }
            }
            // クリップボード監視を再開させる
            mCbw.Dispose();
            startClipWatch();
        }

    }

    // ホットキー押下検知
    void hotKey_HotKeyPush(object sender, EventArgs e)
    {
        // 自動モードON時は何もしない
        if (cMenuItem1.Checked)
        {
            return;
        }
        try
        {
            IDataObject data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Text))
            {
                // 終端の改行を除去してクリップボードに戻す
                string strTmp = (string)data.GetData(DataFormats.Text);
                if (strTmp != mStrCVal)
                {
                    mStrCVal = strTmp.TrimEnd('\r', '\n');
                    if (mStrCVal != "")
                    {
                        Clipboard.SetDataObject(mStrCVal);
                    }
                }
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            LizardTailCutter.Utility.log(System.DateTime.Now + " " + "error2 : " + ex.Message);
#endif
            // ホットキー監視を再開させる
            hotKey.Dispose();
            startHotKeyWatch();
        }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        hotKey.Dispose();
    }

}