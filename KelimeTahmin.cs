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
using System.Speech.Synthesis;

namespace kelimeciniz
{
    public partial class KelimeTahmin : Form
    {
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        private Random random = new Random();
        private System.Windows.Forms.RadioButton[] radioButtons; // System.Windows.Forms.RadioButton türünü kullanıyoruz

        GoogleSheets gs = new GoogleSheets();
        int randomIndex;
        int yanlis = 0;
        int dogru = 0;
        int soruS = 0;
        string yaziyanlis = "Yanlış Sayısı = ";
        string yazidogru = "Doğru Sayısı = ";
        string soru = "Soru "; 

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
       
        private async void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.RadioButton rb = (System.Windows.Forms.RadioButton)sender;

            if (rb.Checked)//Eğer herhangi bir radiobutton checked yapılırsa çalışır.
            {
                radioButtons[randomIndex].BackColor = System.Drawing.Color.Green;//doğru olan cevabı yeşil işaretliyorum.

                if(rb != radioButtons[randomIndex])
                {
                    rb.BackColor = System.Drawing.Color.Red;//eğer doğru cevap değil ise kırmızı renkte olsun işaretlediğim.
                    yanlis++;
                    label4.Text = yaziyanlis + yanlis;
                }
                else
                {
                    dogru++;
                    label3.Text = yazidogru + dogru;
                }
                label6.Text = "Doğruluk oranı = %" + (dogru * 100 / (dogru+yanlis)).ToString();
                synthesizer.Speak(label1.Text + " " + radioButtons[randomIndex].Text);
               

                for (int i = 0; i < radioButtons.Length; i++)
                    radioButtons[i].Enabled = false;//kullanıcı başka bir seçeneğe tıklamasın diye buttonların tıklanabilirlik özelliğini kapatıyorum.

                await Task.Delay(2000); // 3 saniye bekleniyor kullanıcı doğru cevabı görsün diye

                rb.Checked = false;//seçili olan radiobutton kaldırıyorum.
                // Bekleme süresi sonrasında yapılacak iş
                sirala();//yeni şıkları getiriyorum.

                for (int i = 0; i < radioButtons.Length; i++)
                {
                    radioButtons[i].BackColor = System.Drawing.Color.Transparent;//diğer buttonların renklerini düzenliyorum ve aşağıdaki kodda da buttonları aktif ediyorum.
                    radioButtons[i].Enabled = true;
                }
                await Task.Delay(1500);
                synthesizer.Speak(label1.Text);
            }


        }
        private void sirala()
        {
            Tuple<string, string> dogruCevap = gs.RastgeleKelimeGetirVTOrMyList(true);

            soruS++;
            label1.Text = dogruCevap.Item2;
            label5.Text = soru + soruS;

            // Rastgele bir RadioButton seçin
            randomIndex = random.Next(0, radioButtons.Length); // 0 ile (RadioButton dizisinin uzunluğu - 1) arasında rastgele bir indeks seçin

            for (int i = 0; i < radioButtons.Length; i++)
            {
                if (i != randomIndex)
                {
                    Tuple<string, string> yanlisCevap = gs.RastgeleKelimeGetirVTOrMyList(true);
                    radioButtons[i].Text = yanlisCevap.Item1;//eğer seçilen rastgele button değil ise diğer buttonlara değer giriyorum rastgele.
                }
                else
                    radioButtons[i].Text = dogruCevap.Item1;//belirlediğim cevabı atıyorum.

            }
        }

        private void KelimeTahmin_Load(object sender, EventArgs e)
        {
            sirala();
            label3.Text = yazidogru + dogru;
            label4.Text = yaziyanlis + yanlis;
            Thread.Sleep(3200);
            synthesizer.Speak(label1.Text);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Task.Delay(1000); // 1 saniye bekleme
            synthesizer.Speak(label1.Text);
        }
    }
}
