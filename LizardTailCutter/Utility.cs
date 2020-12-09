using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LizardTailCutter
{
    class Utility
    {
        /// <summary>
        /// ログの出力
        /// ex:LizardTailCutter.Utility.log(System.DateTime.Now + " " + "error1 : " + ex.Message);
        /// </summary>
        static public void log(string msg)
        {
            SafeCreateDirectory(@"C:\tmp\");    // ディレクトリがなければ作成
            FileStream fs = new FileStream(@"C:\tmp\ltc.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(msg);
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// 指定したパスにディレクトリが存在しない場合
        /// すべてのディレクトリとサブディレクトリを作成します
        /// </summary>
        public static DirectoryInfo SafeCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return null;
            }
            return Directory.CreateDirectory(path);
        }
    }


}
