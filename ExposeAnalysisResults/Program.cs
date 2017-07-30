using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExposeAnalysisResultsTool
{

    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //string[] SourceFiles = new string[] { @"C:\Users\hoge\Desktop\Files\ExposeAnalysisResults.cs", @"C:\Users\hoge\Desktop\Files\Form1.cs", @"C:\Users\hoge\Desktop\Files\Program.cs", "C:\\Users\\hoge\\Desktop\\Files\\SourceFile.cs" };

            //try
            //{
            //    //source.SetModefiedLines(new string[] { "▼", "てくまく"}, new string[] { "▲", "てくまく" });
            //    //source.SetModefiedLines(new string[] { "▼", "田中"}, new string[] { "▲", "田中" });
            //    //CExposeWarning exposeWarn = new CExposeWarning(SourceFiles, new string[] { "▼", "てくまく"}, new string[] { "▲", "てくまく" });
            //    CExposeWarning exposeWarn = new CExposeWarning(SourceFiles, new string[] { "▼", "田中"}, new string[] { "▲", "田中" });
            //    exposeWarn.Exec(@"C:\Users\hoge\Desktop\Files\test.csv", @"C:\Users\hoge\Desktop\新しいフォルダー");
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show("エラー:\n" + ex.Message, "エラー");
            //}

            //AnalysiskResults.CopyAnalysisResults(@"C:\Users\hoge\Desktop\test.xlsx", @"C:\Users\hoge\Desktop\test2.xlsx");

            //AnalysisResults WarningResult = new AnalysisResults(@"C:\Users\hoge\Desktop\test2.xlsx");

            //MessageBox.Show(WarningResult.FetchWarning(10).ToString());

            //WarningResult.DeleteWarning(1);

            //SourceFile NewFile = new SourceFile(@"C:\Users\hoge\Desktop\新しいフォルダー (2)\VsVim導入方法 - コピー.txt");

            //NewFile.SetModefiedLines("▼-+", "▲-+", true);

            //NewFile.AddLine(0, "*0");
            //NewFile.AddLine(1, "*1");
            //NewFile.AddLine(2, "*2");
            //NewFile.AddLine(2, "*3");
            //NewFile.AddLine(13, "*4");
            //NewFile.AddLine(13, "*5");
            //NewFile.AddLine(13, "*6");

            //NewFile.ExportFile(@"C:\Users\hoge\Desktop\新しいフォルダー (2)\test.txt");
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
