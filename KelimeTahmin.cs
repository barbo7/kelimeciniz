using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace kelimeciniz
{
    public partial class KelimeTahmin : Form
    {
        private Random random = new Random();
        private System.Windows.Forms.RadioButton[] radioButtons; // System.Windows.Forms.RadioButton türünü kullanıyoruz

        GoogleSheets gs = new GoogleSheets();
        int randomIndex;

        public KelimeTahmin()
        {
            InitializeComponent();
            // RadioButton'ların CheckedChanged olayına olay işleyici ekleyin
            radioButton1.CheckedChanged += RadioButton_CheckedChanged;
            radioButton2.CheckedChanged += RadioButton_CheckedChanged;
            radioButton3.CheckedChanged += RadioButton_CheckedChanged;
            radioButton4.CheckedChanged += RadioButton_CheckedChanged;
            radioButton5.CheckedChanged += RadioButton_CheckedChanged;

            radioButtons = new System.Windows.Forms.RadioButton[] { radioButton1, radioButton2, radioButton3, radioButton4, radioButton5 };
        }
       
        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton rb = (System.Windows.Forms.RadioButton)sender;
            if (rb.Checked)
            {
                radioButtons[randomIndex].BackColor = System.Drawing.Color.Green;

                if(rb == radioButtons[randomIndex])
                    rb.BackColor = System.Drawing.Color.Green;
                else rb.BackColor = System.Drawing.Color.Red;


                for (int i = 0; i < radioButtons.Length; i++)
                    radioButtons[i].Enabled = false;
                rb.Checked = false;
            }
                
                Thread thread = new Thread(() =>
                {
                    // Bekleme süresi: 3 saniye (3000 milisaniye)
                    Thread.Sleep(3000);

                    // Bekleme süresi sonrasında yapılacak iş
                    this.Invoke(new MethodInvoker(() =>
                    {
                        radioButtons[randomIndex].Checked = false;
                        // Burada bekleme sonrası işlemi gerçekleştirin
                        sirala();

                        for (int i = 0; i < radioButtons.Length; i++)
                        {
                            radioButtons[i].BackColor = System.Drawing.Color.Transparent;
                            radioButtons[i].Enabled = true;
                        }
                    }));
                });

                thread.Start();
            }
        private void sirala()
        {
            Tuple<string, string> dogruCevap = gs.RastgeleKelimeGetirVTOrMyList(true);
            label1.Text = dogruCevap.Item2;

            // Rastgele bir RadioButton seçin
            randomIndex = random.Next(0, radioButtons.Length); // 0 ile (RadioButton dizisinin uzunluğu - 1) arasında rastgele bir indeks seçin

            for (int i = 0; i < radioButtons.Length; i++)
            {
                if (i != randomIndex)
                {
                    Tuple<string, string> yanlisCevap = gs.RastgeleKelimeGetirVTOrMyList(true);
                    radioButtons[i].Text = yanlisCevap.Item1;
                }
                else
                    radioButtons[i].Text = dogruCevap.Item1;

            }
        }

        private void KelimeTahmin_Load(object sender, EventArgs e)
        {
            sirala();

        }
    }
}
