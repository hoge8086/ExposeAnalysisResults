using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ExposeAnalysisResultsTool
{
	public partial class Form1 : Form
	{

		public Form1()
		{
			InitializeComponent();

            //フォームのサイズを固定
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;

            //列の追加
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn());

            //行の幅をユーザが変更不可能にする。
            dataGridView1.AllowUserToResizeRows = false;

            //見出し列・行を表示しない
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersVisible = false;

            //行単位で選択する（セル単位だと削除できないので）
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            //列幅をグリッドの幅に追従する
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //追記行を表示しない
            dataGridView1.AllowUserToAddRows = false;

            //ボタンのクリックイベントハンドラ
            this.button_SeleceFolder.Click += new System.EventHandler(this.button_SelectFolder_Click);
            this.button_Paste.Click += new System.EventHandler(this.button_Paste_Click);
            this.button_Delete.Click += new System.EventHandler(this.button_Delete_Click);
            this.button_Exec.Click += new System.EventHandler(this.button_Copy_Click);

            //チェックボックスのクリックイベントハンドラ
            //this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);

            //テキストへドラッグ&ドロップのイベントハンドラを追加
            this.textBox1.AllowDrop = true;
            this.textBox2.AllowDrop = true;
            this.textBox1.DragDrop += new System.Windows.Forms.DragEventHandler(CommonText_FileDragDrop);
            this.textBox1.DragEnter += new System.Windows.Forms.DragEventHandler(CommonObj_FileDragEnter);
            this.textBox2.DragDrop += new System.Windows.Forms.DragEventHandler(CommonText_FileDragDrop);
            this.textBox2.DragEnter += new System.Windows.Forms.DragEventHandler(CommonObj_FileDragEnter);

            //データグリッドへドラッグ&ドロップのイベントハンドラを追加
            dataGridView1.AllowDrop = true;
            this.dataGridView1.DragDrop += new System.Windows.Forms.DragEventHandler(dataGridView1_DragDrop);
            this.dataGridView1.DragEnter += new System.Windows.Forms.DragEventHandler(CommonObj_FileDragEnter);
            //データグリッドへキーイベントハンドラを追加
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);

            //起点フォルダのチェックボックスを初期化(OFF)
            //checkBox1.Checked = false;
            //checkBox1_CheckedChanged(null, null);
        }

        //ドラッグ&ドロップのイベントハンドラ
        private void CommonObj_FileDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        //ドラッグ&ドロップのイベントハンドラ
        private void CommonText_FileDragDrop(object sender, DragEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            textBox.Text = fileNames[0];
        }

        //ドラッグ&ドロップのイベントハンドラ
        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            //System.Windows.Forms.DragEventArgs de = (System.Windows.Forms.DragEventArgs)e;
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach(string fileName in fileNames)
                dataGridView1.Rows.Insert(0, fileName);

        }

        //実行
        private void button_Copy_Click(object sender, System.EventArgs e)
        {
            //出力先フォルダパスのチェック
            string outDir = textBox1.Text;
            if(!System.IO.Path.IsPathRooted(outDir))
            {
                MessageBox.Show("エラー：\n出力先フォルダが不正です。絶対パスを指定してください。", "エラー");
                return;
            }

            //警告一覧ファイルパスのチェック
            string warningFilePath = textBox2.Text;
            if (!File.Exists(warningFilePath))
            {
                MessageBox.Show("エラー：\n警告一覧ファイル(" + warningFilePath + ")が存在しません。", "エラー");
                return;
            }
            string warningFileExtension = System.IO.Path.GetExtension(warningFilePath);
            if (warningFileExtension != ".csv")
            {
                MessageBox.Show("エラー：\n警告一覧ファイル(" + warningFilePath + ")の拡張子が不適切です。", "エラー");
                return;
            }

            //キーワードチェック
            string sourceKeyword = textBox3.Text;
            if(sourceKeyword == "")
            {
                MessageBox.Show("エラー：\nキーワードを入力してください。", "エラー");
                return;

            }

            //ソースファイルパスのチェック
            List<string> sourceFiles = new List<string>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string source = "";

                //データグリッドビューからソースファイルのパスを読み込む
                source = (string)row.Cells[0].Value;

                if (!File.Exists(source))
                {
                    MessageBox.Show("エラー：\nソースファイル(" + source + ")が存在しません。", "エラー");
                    return;
                    
                }
                sourceFiles.Add(source);
            }


            try
            {
                CExposeWarning exposeWarn = new CExposeWarning( sourceFiles.ToArray(),
                                                                new string[] { "▼", textBox3.Text},
                                                                new string[] { "▲", textBox3.Text});
                //警告抽出&コード埋込実行
                exposeWarn.Exec(warningFilePath, outDir);
            }
            catch(Exception ex)
            {
                MessageBox.Show("エラー:\n" + ex.Message, "エラー");
            }
        }


        //Cntl+Vによる貼り付け
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.V) {
                addClipBoardFilesToFileDataTable();
            }
        }

        //貼り付けボタン押下
        private void button_Paste_Click(object sender, EventArgs e)
        {
                addClipBoardFilesToFileDataTable();
        }

        //クリップボードからデータグリッドへの貼り付け
        private void addClipBoardFilesToFileDataTable()
        {
            //クリップボードの内容を取得
            string pasteText = Clipboard.GetText();
            if (string.IsNullOrEmpty(pasteText) || pasteText == "")
                return;

            //改行で分ける
            pasteText = pasteText.Replace("\r\n", "\n");
            pasteText = pasteText.Replace('\r', '\n');
            pasteText = pasteText.TrimEnd(new char[] { '\n' });
            string[] fileNames = pasteText.Split('\n');

            //データグリッドに追加
            foreach (string fileName in fileNames)
                dataGridView1.Rows.Insert(0, fileName);
        }

        //出力先フォルダの参照ダイアログ呼び出し
        private void button_SelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //fbd.SelectedPath = @"";
            fbd.ShowNewFolderButton = true;

            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                //選択されたフォルダを表示する
               textBox1.Text =  fbd.SelectedPath;
            }
        }


        //データグリッドビューから削除
        private void button_Delete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dataGridView1.SelectedRows)
            {
                dataGridView1.Rows.Remove(r);
            }
        }

        //警告一覧ファイルの参照ダイアログ呼び出し
        private void button_SelectWarningFile_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //fbd.SelectedPath = @"";
            fbd.ShowNewFolderButton = true;

            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = @"C:\";
            ofd.Filter = "(*.csv)|*.csv";
            ofd.FilterIndex = 1;
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                Console.WriteLine(ofd.FileName);
            }
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                //選択されたフォルダを表示する
               textBox2.Text =  fbd.SelectedPath;
            }

        }
    }
}
