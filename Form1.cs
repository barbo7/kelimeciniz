using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Drive.v3;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Speech.Synthesis;

namespace kelimeciniz
{
    public partial class Form1 : Form
    {
        GoogleSheets gs;
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            Tuple<List<string>, List<string>> veriTuple = gs.Sayfa1Veri();

            string[] sutunA = veriTuple.Item1.ToArray();
            string[] sutunB = veriTuple.Item2.ToArray();

            listBox1.Items.AddRange(sutunA);
            listBox2.Items.AddRange(sutunB);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gs= new GoogleSheets();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();

            var kelimeArama = gs.KelimeAra(textBox1.Text);//english

            string[] words = kelimeArama.Item1.ToArray();
            string[] kelimeler = kelimeArama.Item2.ToArray();

            foreach(string word in words)
            {
                synthesizer.Speak(word);
                Thread.Sleep(1500);
            }

            listBox1.Items.AddRange(words);
            listBox2.Items.AddRange(kelimeler);
        }

        private void button3_Click(object sender, EventArgs e)
        {
             gs.VeriEkle(textBox1.Text, textBox2.Text);
            MessageBox.Show("Ekleme başarılı.");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox2.Text = gs.KelimeAra(textBox1.Text, true);
            Thread.Sleep(1800);
            synthesizer.Speak(textBox2.Text);


        }

        private void button6_Click(object sender, EventArgs e)
        {
            Tuple<string,string> veri = gs.RastgeleKelimeGetirVTOrMyList(true);
            listBox1.Items.Add(veri.Item1);
            listBox2.Items.Add(veri.Item2);

            Thread.Sleep(1800);
            synthesizer.Speak(veri.Item2);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();


            Tuple<string, string, string> vericik = gs.RastgeleCumle();
            listBox1.Items.Add(vericik.Item1);
            listBox2.Items.Add(vericik.Item2);
            listBox3.Items.Add(vericik.Item3);

            Thread.Sleep(1800);
            synthesizer.Speak(vericik.Item1);
            Thread.Sleep(1000);
            synthesizer.Speak(vericik.Item2);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            KelimeTahmin kt = new KelimeTahmin();
            kt.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            HarfAlayim ha = new HarfAlayim();
            ha.Show();
        }
    }
    
}
