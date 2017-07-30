using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExposeAnalysisResultsTool
{
    class CExposeWarning
    {
        //ソースファイルのリスト
        private Dictionary<string, CSourceFile> m_sourceFiles = new Dictionary<string, CSourceFile>();

        public CExposeWarning(string[] a_sourcePaths, string[] a_beginStr, string[] a_endStr)
        {
            //ソースファイルを解析する
            foreach(string sourcePath in a_sourcePaths)
            {
                if (!System.IO.File.Exists(sourcePath))
                {
                    throw new Exception("致命的エラーです。" + sourcePath + "が存在しません。");
                }

                //ソースファイルを解析する
                CSourceFile source = new CSourceFile(sourcePath);
                //source.SetModefiedLines(new string[] { "▼", "てくまく"}, new string[] { "▲", "てくまく" });
                //source.SetModefiedLines(new string[] { "▼", "田中"}, new string[] { "▲", "田中" });

                //修正行を設定する
                source.SetModefiedLines(a_beginStr, a_endStr);

                //解析したソースファイルを登録する
                m_sourceFiles.Add(sourcePath, source);
            }      

        }

        //(1) 警告リストからソースファイルの修正行に該当する部分を抽出し、出力ディレクトリに出力する。
        //(2) ソースファイルに直接、警告をコメントアウトで追加し、出力ディレクトリに出力する。
        public void Exec(string a_inputFilePath, string a_outputDirectory)
        {
            //出力フォルダのパスにバックスラッシュを付加
            if (!a_outputDirectory.EndsWith(@"\"))
                a_outputDirectory += @"\";

            //出力する警告リストのパス
            string outWarnListPath = a_outputDirectory + System.IO.Path.GetFileName(a_inputFilePath);

            IWarnListCommon warnList = new CWarnListCsv();

            string errMsg = null;

            //警告リストを解析する
            int numParseError = warnList.Import(a_inputFilePath, ref errMsg);
            if(numParseError > 0)
            {
                //解析できなかった警告を報告
                MessageBox.Show("警告のフォーマットが" + numParseError + "件、正しくありません。スキップします。", "警告");
            }
            else if(numParseError == -1)
            {
                //解析に失敗した
                throw new Exception("警告一覧ファイルの読み込みに失敗しました。\n例外:" + errMsg);
            }


            //出力する警告の通し番号
            int count = 1;

            //警告を一つづつ処理
            for(int i = 0; i < warnList.Count(); i++)
            {
                CWarning warn = warnList.GetWarning(i);
                //if (warn == null)
                //    break;

                //警告のソースファイル(パス)が解析対象に含まれるか?
                if (m_sourceFiles.ContainsKey(warn.m_filePath))
                {
                    CSourceFile source = m_sourceFiles[warn.m_filePath];

                    //警告の行数がソースファイルの修正した部分かどうか?
                    if(source.IsModefiedLine(warn.m_line))
                    {
                        //警告の通し番号を設定
                        warn.m_num = Convert.ToString(count);

                        //警告をエクスポート対象にする
                        warnList.SetToExportWarning(i);

                        //ソースファイルに警告を埋め込む
                        source.AddLine(warn.m_line, warn.ToString());
                        count++;
                    }
                }
            }

            //警告を埋め込んだソースファイルを出力する
            foreach (var filePath in m_sourceFiles.Keys)
            {
                m_sourceFiles[filePath].Export(a_outputDirectory + System.IO.Path.GetFileName(filePath));
            }

            //対象の警告を抽出した、警告リストを出力する
            int numWarning = warnList .Export(outWarnListPath, ref errMsg);

            if(numWarning < 0)
            {
                //出力に失敗
                throw new Exception("警告一覧ファイルの出力が失敗しました。\n例外:" + errMsg);
            }
            else
            {
               //出力に成功
               MessageBox.Show("警告一覧ファイルに" + numWarning  + "件、出力しました。\n(全体:" + warnList.Count() + "件)", "完了");
            }

        }

    }
}
