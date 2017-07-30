using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic.FileIO;

namespace ExposeAnalysisResultsTool
{
    // 警告クラス
    class CWarning : Object, System.IComparable
    {
        //ソースファイルのパス
        public string m_filePath;
        //行数
        public int m_line;
        //通し番号
        public string m_num;
        //警告番号
        public int m_warning;
        //警告レベル
        public int m_level;
        //警告の内容
        public string m_message;   

        //出力フォーマット
        override public string ToString()
        {
            return string.Format(" ■ [Warning] No.{0}、 警告:{1}、レベル:{2}、{3}", m_num, m_warning, m_level, m_message);
        }

        public int CompareTo(object obj)
        {
            //nullより大きい
            if (obj == null)
            {
                return 1;
            }

            //違う型とは比較できない
            if (this.GetType() != obj.GetType())
            {
                throw new ArgumentException("別の型とは比較できません。", "obj");
            }

            CWarning warn = (CWarning)obj;

            //ファイル名、行数、警告番号の順に比較する
            int ret = m_filePath.CompareTo(warn.m_filePath);
            if(ret == 0)
            {
                ret = m_line.CompareTo(warn.m_line);
                if(ret == 0)
                {
                    ret = m_warning.CompareTo(warn.m_warning);
                }
            }
            return ret;
        }

    }

    interface IWarnListCommon
    {
        //a_index番目の警告を取得する
        CWarning GetWarning(int a_index);
        //a_index番目の警告をエクスポート対象にする
        void SetToExportWarning(int a_index);
        //警告のリストを出力する
        int Export(string a_filePath, ref string a_ErrorMsg);
        //警告のリストを読み込む
        int Import(string a_filePath, ref string a_ErrorMsg);
        //警告の総数を取得する
        int Count();

        //a_index番目の警告をエクスポート時に除外する
        //void ExcludeWarningWhenExport(int a_index);
    }

    class CWarnListCsv : IWarnListCommon
    {
        List<CWarning> m_warnList     = new List<CWarning>();
        //List<int>      m_deleteRows  = new List<int>();
        List<int>      m_exportRows  = new List<int>();
        private const int ROW_OFFSET = 1;
        private const int COLUMN_OFFSET = 0;
        private System.Text.Encoding fileEncoding = System.Text.Encoding.GetEncoding("shift_jis");

        // CSVファイルから警告リストをインポートする
        // 戻り値: 0以上 解析成功(フィールドが解析できなかった行数を返す)、-1 解析に失敗
        public int Import(string a_filePath, ref string a_ErrorMsg)
        {
            //CSVパーサー
            TextFieldParser csvParser;

            //解析に失敗した行数
            int countParsseError = 0;

            //警告リスト
            m_warnList     = new List<CWarning>();
            //m_deleteRows  = new List<int>();
            m_exportRows  = new List<int>();

            using (csvParser = new TextFieldParser(a_filePath, fileEncoding))
            {
                try
                {

                    csvParser.TextFieldType = FieldType.Delimited;
                    csvParser.SetDelimiters(",");               //カンマ区切り
                    csvParser.HasFieldsEnclosedInQuotes = true; //フィールドが引用符で囲まれることがある
                    csvParser.TrimWhiteSpace = false;

                    int row = 0;
                    while (!csvParser.EndOfData)
                    {
                        //フィールドを読込む
                        //※フィールドが全てからの場合はnullが返る
                        //※空行は予読み飛ばされる
                        string[] csvfield = csvParser.ReadFields();

                        if (csvfield != null && row >= ROW_OFFSET)  //タイトル行(ROW_OFFSET行分)は飛ばす
                        {
                            //フィールドの内容から警告オブジェクトを生成する
                            CWarning warn = CreateWarning(csvfield);

                            if (warn != null)
                               //解析に成功
                                m_warnList.Add(CreateWarning(csvfield));
                            else
                                //解析に失敗
                                countParsseError++;
                        }
                        row++;
                    }
                }
                catch (Exception ex)
                {
                    //解析に失敗
                    a_ErrorMsg = ex.Message;
                    return -1;
                }
            }

            //ソートする
            m_warnList.Sort();

            return countParsseError;
        }

        //a_index番目の警告を取得する
        public CWarning GetWarning(int a_index)
        {
            if(a_index >= 0 && a_index < m_warnList.Count)
                return m_warnList[a_index];

            return null;
        }

        public int Count()
        {
             return m_warnList.Count;
        }

        //CSVのフィールドから警告オブジェクトを作成する
        private CWarning CreateWarning(string[] csvfields)
        {
            CWarning warn = new CWarning();

            //フィールドの数が足りない
            if (csvfields.Count() < 5)
                return null;

            //フィールド（文字列）からそれぞれの型に型変換
            warn.m_filePath = csvfields[COLUMN_OFFSET + 0];
            if(!Int32.TryParse(csvfields[COLUMN_OFFSET + 1], out warn.m_line))
                return null;
            if(!Int32.TryParse(csvfields[COLUMN_OFFSET + 2], out warn.m_level))
                return null;
            if(!Int32.TryParse(csvfields[COLUMN_OFFSET + 3], out warn.m_warning))
                return null;
            warn.m_message = csvfields[COLUMN_OFFSET + 4];

            return warn;
        }

        //a_index番目の警告をエクスポート時に除外する
        //public void ExcludeWarningWhenExport(int a_index)
        //{
        //    //バッファにためて、エクスポート時に出力しないようにする
        //    if (!m_deleteRows.Contains(a_index))
        //        m_deleteRows.Add(a_index);
        //}

        //a_index番目の警告をエクスポート対象にする
        public void SetToExportWarning(int a_index)
        {
            //バッファにためて、エクスポート時に出力しないようにする
            if (!m_exportRows.Contains(a_index))
                m_exportRows.Add(a_index);
        }

        //ファイルに警告を出力する
        //戻り値：0以上で出力した警告数を返す、-1は失敗
        public int Export(string a_filePath, ref string a_ErrorMsg)
        {
            System.IO.StreamWriter fileWriter = null;
            int count = 0;
            try
            {
                //出力ファイルを作成する
                fileWriter = new System.IO.StreamWriter(a_filePath, false, fileEncoding);

                //警告を順に書き込む
                for(int i = 0; i < m_warnList.Count; i++)
                {
                    //警告がエクスポート対象かどうか
                    if (m_exportRows.Contains(i))
                    {
                        CWarning warn = m_warnList[i];
                        //警告をCSVのフォーマットにする
                        string csvLine = string.Format("{0},{1},{2},{3},{4},{5}",
                                                            MakeCsvFieldText(warn.m_num),
                                                            MakeCsvFieldText(warn.m_filePath),
                                                            warn.m_line,
                                                            warn.m_level,
                                                            warn.m_warning,
                                                            MakeCsvFieldText(warn.m_message));
                        //警告を書き込む
                        fileWriter.WriteLine(csvLine);
                        count++;
                    }

                }

            }

            catch(Exception ex)
            {
                a_ErrorMsg = ex.Message;
                return -1;
            }
            finally
            {
                if(fileWriter !=null)
                    fileWriter.Dispose();
            }

            //出力した警告数を返す
            return count;

        }

        //CSVのフィールド用に文字列をエスケープする
        private string MakeCsvFieldText(string a_text)
        {
            if(a_text == null)
                return "";

            string csvText = a_text;

            if (a_text.Contains(",") || a_text.Contains("\""))
                csvText= "\"" + csvText.Replace("\"", "\"\"") + "\"";

            return csvText;
        }
    }

}
