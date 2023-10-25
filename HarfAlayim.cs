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
        GoogleSheets gs = new GoogleSheets();
        List<System.Windows.Forms.Button> buttonList = new List<System.Windows.Forms.Button>();
        List<System.Windows.Forms.Button> buttonTahminList = new List<System.Windows.Forms.Button>();

        string kelime = default; // Harf yerleştirilecek kelimeyi burada tanımlayın
        int buttonLeft = 10; // İlk butonun sola olan uzaklığı
        int yenibuttonLeft = 10;
        int left = 10;
        int tahminButtonSayisi = 0;
        public HarfAlayim()
        {
            InitializeComponent();
            OlusturVeDuzenleButonlar();

        }
        private void ButtonSira(Button button, int siraNo, bool tahminMi)
        {
            button.Height = 30;
            button.Width = 30;
            int buttonTop = tahminMi ? 180 : 50; //Eğer tahminMi değeri true ise top değeri 180 olacak tahmin değilse 50.
            left = tahminMi ? yenibuttonLeft : buttonLeft;//hangi yere buttonu göndereceğimi belirliyorum.
            if (siraNo < 8)
            {
                //diğer top da 180den başlıyo.
                button.Top = buttonTop; // Butonun yatay konumunu ayarlayın
                button.Left = left; // Butonun dikey konumunu ayarlayın
                left += button.Width + 10; // Bir sonraki butonun sola olan uzaklığını ayarlayın
            }
            else if (siraNo == 8)
            {
                button.Top = buttonTop + 30;
                left = 10;
                button.Left = left; // Butonun dikey konumunu ayarlayın
            }
            else if (siraNo > 8 && siraNo <= 16)
            {
                left += button.Width + 10; // Bir sonraki butonun sola olan uzaklığını ayarlayın
                button.Top = buttonTop+30;
                button.Left = left; // Butonun dikey konumunu ayarlayın
            }
            if (tahminMi)
                yenibuttonLeft = left;
            else buttonLeft = left;
        }

        private void OlusturVeDuzenleButonlar()
        {
        again:
            Tuple<string, string> veri = gs.RastgeleKelimeGetirVTOrMyList(true);
            kelime = veri.Item2;
            if (veri.Item2.Length >= 16)
            {
                buttonLeft = 10;
                goto again;
            }
            label1.Text = veri.Item1;


            for (int i = 0; i < kelime.Length; i++)
            {
                Button button = new Button();
                button.Text = kelime[i].ToString();

                ButtonSira(button, i, false);//Bu fonksiyonu kelimeleri düzenli bir sırada eklemek için oluşturdum.

                buttonList.Add(button);
                this.Controls.Add(button); // Butonu forma ekleyin
            }
            buttonLeft = 10;
            foreach (Button buton in buttonList)
            {
                buton.Click += Button_Click;
            }
        }
        private void Button_Click(object sender, EventArgs e)
        {
            char harf = default;
            Button butonIslem = (Button)sender;
            harf = Convert.ToChar(butonIslem.Text);
            this.Controls.Remove(butonIslem); // Button'u formdan kaldır
            buttonList.Remove(butonIslem); // Button'u liste içinden kaldır

            Button newButton = new Button();
            newButton.Text = harf.ToString();

            //if (buttonTahminList.IndexOf(newButton) >= 8) ikinciMiSirada = true;
            //else ikinciMiSirada = false;

            ButtonSira(newButton, tahminButtonSayisi, true);//Bu fonksiyonu kelimeleri düzenli bir sırada eklemek için oluşturdum.

            buttonTahminList.Add(newButton);
            if (kelime.Length > tahminButtonSayisi)
                tahminButtonSayisi++;
            else tahminButtonSayisi = 0;
            this.Controls.Add(newButton);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tahminButtonSayisi = 0;
            buttonLeft = 10;
            yenibuttonLeft = 10;
            foreach (Button buton in buttonList)
            {
                this.Controls.Remove(buton);
            }
            foreach (Button buton in buttonTahminList)
            {
                this.Controls.Remove(buton);
            }

            OlusturVeDuzenleButonlar();
        }
    }
}
