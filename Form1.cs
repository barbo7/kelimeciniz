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
            Tuple<List<string>, List<string>> veriTuple = gs.Sayfa1Veri();

            string[] sutunA = veriTuple.Item1.ToArray();
            string[] sutunB = veriTuple.Item2.ToArray();

            listBox1.Items.AddRange(sutunA);
            listBox1.Items.AddRange(sutunB);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gs= new GoogleSheets();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            //string[] arama = gs.GetBValueByKeyword(textBox1.Text).Split(',');
            //foreach (var i in arama)
                //listBox1.Items.Add(i.Trim());
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //gs.EkleVeri(textBox1.Text, textBox2.Text);
            MessageBox.Show("Ekleme başarılı.");
        }
    }
    
}
