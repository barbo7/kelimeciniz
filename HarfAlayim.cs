using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace kelimeciniz
{
    public partial class HarfAlayim : Form
    {
        GoogleSheets gs = new GoogleSheets();
        List<System.Windows.Forms.Button> buttonList = new List<System.Windows.Forms.Button>();
        List<System.Windows.Forms.Button> buttonTahminList = new List<System.Windows.Forms.Button>();
        List<EklenenButtonBilgi> butonBilgi = new List<EklenenButtonBilgi>();
        Random rn = new Random();
        //string sonTıklananButton = "";
        //Point buttonLocation;
        //Tuple<List<Button>, Point[]>;
        

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
        /// <summary>
        /// Button'u alıyorum ve sırayla eklemek için kullanıyorun. eğer tahmin mi özelliği aktif olursa tahmin edilen harfler yerine aktarılıyor false ise tam tersi.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="siraNo"></param>
        /// <param name="tahminMi"></param>
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
                button.Top = buttonTop + 30;
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
            string karisikkelime = KelimeKaristir(kelime);
            label1.Text = veri.Item1;


            for (int i = 0; i < karisikkelime.Length; i++)
            {
                Button button = new Button();
                button.Text = karisikkelime[i].ToString();
                button.Name = "Text" + (karisikkelime.Length - i).ToString();
                ButtonSira(button, i, false);//Bu fonksiyonu kelimeleri düzenli bir sırada eklemek için oluşturdum.

                buttonList.Add(button);
                this.Controls.Add(button); // Butonu forma ekleyin

                butonBilgi.Add(new EklenenButtonBilgi
                {
                    ButtonId = button.Name,
                    Konum = button.Location
                });
            }

            foreach (Button buton in buttonList)
            {
                buton.Click += Button_Click;
            }
        }
        private void TahminButtonGeriCek_Click(object sender, EventArgs e)
        {
            Button IslemButtonGeri = (Button)sender;
            char geriAlinanHarf = Convert.ToChar(IslemButtonGeri.Text);
            this.Controls.Remove(IslemButtonGeri);

            string TiklananButton = IslemButtonGeri.Name;

            buttonTahminList.Remove(IslemButtonGeri);
            if (yenibuttonLeft > 10)
                yenibuttonLeft -= 40;
            else yenibuttonLeft = 10;
            tahminButtonSayisi--;

            if (buttonTahminList.Count < 1)
            {
                yenibuttonLeft = 10;
                tahminButtonSayisi = 0;
            }

            Button butonGeri = new Button();
            butonGeri.Width = 30;
            butonGeri.Height = 30;
            butonGeri.Text = geriAlinanHarf.ToString();
            for(int i=0;i<butonBilgi.Count;i++)
            {
                if (TiklananButton == butonBilgi[i].ButtonId)
                    butonGeri.Location = butonBilgi[i].Konum;
            }
            butonGeri.Name = TiklananButton;

            butonGeri.Click += Button_Click;
            buttonList.Add(butonGeri);
            this.Controls.Add(butonGeri);
        }

      
        private void Button_Click(object sender, EventArgs e)
        {
            char harf = default;
            Button butonIslem = (Button)sender;
            harf = Convert.ToChar(butonIslem.Text);
            string butonIsim = butonIslem.Name;

            this.Controls.Remove(butonIslem); // Button'u formdan kaldır
            buttonList.Remove(butonIslem); // Button'u liste içinden kaldır
            if (buttonLeft > 10)
                buttonLeft -= 40;
            else buttonLeft = 10;

            if (buttonList.Count < 1)
            {
                buttonLeft = 10;
                tahminButtonSayisi = 0;
            }

            Button newButton = new Button();
            newButton.Text = harf.ToString();
            newButton.Name = butonIsim;


            ButtonSira(newButton, tahminButtonSayisi, true);//Bu fonksiyonu kelimeleri düzenli bir sırada eklemek için oluşturdum.
            buttonTahminList.Add(newButton);
            newButton.Click += TahminButtonGeriCek_Click;

            if (kelime.Length > tahminButtonSayisi)
                tahminButtonSayisi++;
            else tahminButtonSayisi = 0;
            this.Controls.Add(newButton);
        }
        public string KelimeKaristir(string word)
        {
            char[] harfler = word.ToCharArray();
            
            for(int i=word.Length-1;i>0;i--)
            {
                int hangiHarf = rn.Next(i + 1);
                char harf = harfler[i];
                harfler[i] = harfler[hangiHarf];
                harfler[hangiHarf] = harf;
            }
            

            return new string(harfler);
        }
        void YeniKelimeGetir()
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
        private void button1_Click(object sender, EventArgs e)
        {
            YeniKelimeGetir();
        }
    }
    public class EklenenButtonBilgi
    {
        public string ButtonId { get; set; }
        public Point Konum { get; set; }
    }
}
