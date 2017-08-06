using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ExposeAnalysisResultsTool
{
    class CLineFormat
    {
        public string GetBeginLine()
        {
            return "****************************************************************************************";
        }

        public string GetEndLine()
        {
            return "****************************************************************************************";
        }
    }

    class CSourceFile
    {
        private List<bool>   m_isModefiedLine = new List<bool>();  //修正された行かどうか
        private List<string> m_lines = new List<string>();         //ソースコード(一行ごとのリスト)

        //行数をキーとした追加行(リスト)のハッシュテーブル
        private Dictionary<int, List<Object>> m_addLines = new Dictionary<int, List<Object>>();
        CLineFormat m_format;

        //元からある行とAddLine関数で後から追加した行とは別々で管理するため、行を追加しても行数はずれない
        //元の行に追加行がぶら下がってるイメージ

        //ファイルを読み込むコンストラクタ
        public CSourceFile(string a_filePath)
        {
            if (m_format == null) m_format = new CLineFormat();

            StreamReader readFile = null;
            try
            {
                //ファイルを読み込む
                readFile = new StreamReader(a_filePath, System.Text.Encoding.GetEncoding("shift_jis"));

                string line = null; 
                //一行ごとにリストに格納
                while ((line = readFile.ReadLine()) != null)
                {
                    m_lines.Add(line);
                    m_isModefiedLine.Add(true);
                }
            }
            catch(Exception ex)
            {
                throw ex;// MyAppException("ソースファイル(" + a_filePath +")の読み込みに失敗しました。");
            }
            finally
            {
                if(readFile != null)
                    readFile.Dispose();
            }
        }

        //ファイルを出力する
        public void Export(string a_filePath)
        {
            StreamWriter writeFile = null;
            try
            {
                writeFile = new System.IO.StreamWriter(a_filePath, false, System.Text.Encoding.GetEncoding("shift_jis"));

                //行数でループ
                for (int i = 0; i < m_lines.Count; i++)
                {
                    //この行に追加行があるか?
                    if(m_addLines.ContainsKey(i))
                    {
                        List<Object> addLines = m_addLines[i];

                        //追加行の見出し行を出力
                        writeFile.WriteLine(m_format.GetBeginLine());

                        //追加行を出力
                        for (int j = 0; j < addLines.Count; j++)
                        {
                            writeFile.WriteLine(addLines[j].ToString());
                        }

                        //追加行の終了行を出力
                        writeFile.WriteLine(m_format.GetEndLine());
                    }

                    //元の行を出力
                    writeFile.WriteLine(m_lines[i]);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if(writeFile != null)
                    writeFile.Dispose();
            }

        }

        //開始文字列と終了文字列に囲まれた行を修正行として設定する関数
        public void SetModefiedLines(string[] a_beginStr, string[] a_endStr)
        {
            bool isModefied = false;

            //行数でループ
            for(int i=0; i<m_isModefiedLine.Count; i++)
            {
                //行が開始行にヒット
                if (IsContainWordsInLine(a_beginStr, m_lines[i]))
                    isModefied = true;

                //行が終了行にヒット
                if (IsContainWordsInLine(a_endStr, m_lines[i]))
                    isModefied = false;
                
                //その行が修正行かどうかを設定する
                m_isModefiedLine[i] = isModefied;

            }

        }

        //テキスト内を各文字列でAND検索してヒットするかどうか
        private bool IsContainWordsInLine(string[] words, string text)
        {
            bool isContain = true;

            //各文字列を検索する
            foreach (string word in words)
                if (text.IndexOf(word) == -1)
                    isContain = false;

            return isContain;
        }

        //任意の行数に行を追加する
        public void AddLine(int a_line, string a_lineStr)
        {
            int i = a_line - 1;

            //範囲外は無視する
            if (i < 0 || m_isModefiedLine.Count <= i)
                return;

            //以前に、この行に追加していなければ、追加行リストを生成
            if (!m_addLines.ContainsKey(i))
                m_addLines.Add(i, new List<object>());

            //追加行を追加
            m_addLines[i].Add(a_lineStr);
        }

        //修正行かどうかを返す関数
        public bool IsModefiedLine(int a_line)
        {
            int i = a_line - 1;
            //範囲外チェック
            if (i < 0 || m_isModefiedLine.Count <= i)
                return false;

            return m_isModefiedLine[i];
        }
      
    }
}
