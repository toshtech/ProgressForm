using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestProgressBar2
{
    public partial class Form1 : Form
    {
        // max件数
        int maxLoops;
        // プログレス画面
        ProgressForm pform;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // プログレス画面を表示する
            pform = new ProgressForm();
            pform.Show();

            //処理が行われているときは、何もしない
            if (backgroundWorker1.IsBusy)
            {
                pform.Dispose();

                return;
            }

            //Button1を無効にする
            button1.Enabled = false;

            // プログレス画面のプログレスバーのコントロール取得
            ProgressBar pbar = (ProgressBar)pform.Controls["progressBar1"];

            //コントロールを初期化する
            pbar.Minimum = 0;
            pbar.Value = 0;

            //max件数
            maxLoops = 0;

            //BackgroundWorkerのProgressChangedイベントが発生するようにする
            backgroundWorker1.WorkerReportsProgress = true;
            //DoWorkで取得できるパラメータ(10)を指定して、処理を開始する
            //パラメータが必要なければ省略できる
            backgroundWorker1.RunWorkerAsync();

            //キャンセルできるようにする
            backgroundWorker1.WorkerSupportsCancellation = true;

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if(maxLoops != 0) return;
            BackgroundWorker bgWorker = (BackgroundWorker)sender;

            // 最大件数の取得
            maxLoops = 10;

            //時間のかかる処理を開始する
            for (int i = 1; i <= maxLoops; i++)
            {
                //キャンセルされたか調べる
                if (bgWorker.CancellationPending)
                {
                    //キャンセルされたとき
                    e.Cancel = true;
                    pform.Dispose();
                    return;
                }

                //1秒間待機する（時間のかかる処理があるものとする）
                System.Threading.Thread.Sleep(1000);

                //ProgressChangedイベントハンドラを呼び出し、
                //コントロールの表示を変更する
                bgWorker.ReportProgress(i);
            }

            //ProgressChangedで取得できる結果を設定する
            //結果が必要なければ省略できる
            e.Result = maxLoops;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar pbar = (ProgressBar)pform.Controls["progressBar1"];
            Label label = (Label)pform.Controls["Label1"];

            //ProgressBar1の値を変更する
            pbar.Value = e.ProgressPercentage;
            pbar.Maximum = maxLoops;
            //Label1のテキストを変更する
            label.Text = String.Format("{0}件中{1}件処理", maxLoops, e.ProgressPercentage.ToString());

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //イベントハンドラをイベントに関連付ける
            //フォームデザイナを使って関連付けを行った場合は、不要
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Label label = (Label)pform.Controls["Label1"];
            if (e.Error != null)
            {
                //エラーが発生したとき
                label.Text = "エラー:" + e.Error.Message;
            }
            else if (e.Cancelled)
            {
                //キャンセルされたとき
                label.Text = "キャンセルされました。";
                pform.Dispose();
            }
            else
            {
                //正常に終了したとき
                //結果を取得する
                int result = (int)e.Result;
                label.Text = result.ToString() + "回で完了しました。";
            }

            //Button1を有効に戻す
            button1.Enabled = true;
        }
    }
}
