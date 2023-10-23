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

namespace kelimeciniz
{
    public partial class Form1 : Form
    {
        GoogleSheets gs;
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

            string[] words = gs.KelimeAra(textBox1.Text).Item1.ToArray();
            string[] kelimeler = gs.KelimeAra(textBox1.Text).Item2.ToArray();

            listBox1.Items.AddRange(words);
            listBox2.Items.AddRange(kelimeler);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            gs.VeriEkle(textBox1.Text, textBox2.Text);
            MessageBox.Show("Ekleme başarılı.");
        }
    }
    
}
