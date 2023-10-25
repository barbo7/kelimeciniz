using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kelimeciniz
{
    public partial class HarfAlayim : Form
    {
        private string kelime = "MERHABA"; // Harf yerleştirilecek kelimeyi burada tanımlayın
        private int buttonLeft = 10; // İlk butonun sola olan uzaklığı
        public HarfAlayim()
        {
            InitializeComponent();
            OlusturVeDuzenleButonlar();
        }

        private void OlusturVeDuzenleButonlar()
        {
            for (int i = 0; i < kelime.Length; i++)
            {
                Button button = new Button();
                button.Text = kelime[i].ToString();
                button.Top = 50; // Butonun yatay konumunu ayarlayın
                button.Left = buttonLeft; // Butonun dikey konumunu ayarlayın
                button.Width = 30; // Butonun genişliğini ayarlayın
                button.Height = 30; // Butonun yüksekliğini ayarlayın
                this.Controls.Add(button); // Butonu forma ekleyin

                buttonLeft += button.Width + 10; // Bir sonraki butonun sola olan uzaklığını ayarlayın
            }
        }

    }
}
