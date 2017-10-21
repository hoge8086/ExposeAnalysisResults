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
        CSrouceFileMgr m_sourceFiles;

        public CExposeWarning(string[] a_sourcePaths, string[] a_beginStr, string[] a_endStr)
        {
            m_sourceFiles = new CSrouceFileMgr(a_sourcePaths, a_beginStr, a_endStr);
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
            if (numParseError == -1)
            {
                //解析に失敗した
                throw new Exception("警告一覧ファイルの読み込みに失敗しました。\n例外:" + errMsg);
            }

            if (numParseError > 0)
            {
                //解析できなかった警告を報告
                MessageBox.Show("警告のフォーマットが" + numParseError + "件、正しくありません。スキップします。", "警告");
            }

            //修正したソースコードを対象とする警告を出力する
            int numWarning = OutputExtractedWarning(outWarnListPath, warnList, ref errMsg);
            if (numWarning < 0)
            {
                //出力に失敗
                throw new Exception("警告一覧ファイルの出力が失敗しました。\n例外:" + errMsg);
            }

            m_sourceFiles.ExportFiles(a_outputDirectory + "Source\\");

            //出力に成功
            MessageBox.Show("対象の警告を" + numWarning + "件、抽出しました。\n(全体:" + warnList.Count() + "件)", "完了");

        }

        //修正したソースコードを対象とする警告を抽出し出力する
        private int OutputExtractedWarning(string outWarnListPath, IWarnListCommon warnList, ref string errMsg)
        {
            //出力する警告の通し番号
            int count = 1;
            //出力する警告リスト
            IWarnListCommon outputWarnList = new CWarnListCsv();

            //警告を一つづつ処理
            for (int i = 0; i < warnList.Count(); i++)
            {
                CWarning warn = warnList.GetWarning(i);

                //警告のソースファイル(パス)が解析対象に含まれるか?
                CSourceFile source = m_sourceFiles.GetSourceFile(warn.m_filePath);

                if(source != null)
                {

                    //警告の行数がソースファイルの修正した部分かどうか?
                    if (source.IsModefiedLine(warn.m_line))
                    {
                        //警告の通し番号を設定
                        warn.m_num = Convert.ToString(count);

                        //警告を出力対象にする
                        outputWarnList.AddWarning(warn);

                        //ソースファイルに警告を埋め込む
                        source.AddLine(warn.m_line, warn.ToString());
                        count++;
                    }
                }
            }

            //対象の警告を抽出した、警告リストを出力する
            return outputWarnList.Export(outWarnListPath, ref errMsg);
        }
    }
}
