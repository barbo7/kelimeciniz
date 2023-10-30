using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kelimeciniz
{
    public partial class HarfAlayim : Form
    {
        GoogleSheets gs = new GoogleSheets();
        List<Button> buttonList = new List<Button>();
        List<Button> buttonTahminList = new List<Button>();
        List<EklenenButtonBilgi> butonBilgi = new List<EklenenButtonBilgi>();
        List<EklenenButtonBilgi> butonTahminBilgi = new List<EklenenButtonBilgi>();
        List<RastgeleHarfBilgi> rastgeleHarf = new List<RastgeleHarfBilgi>();

        Dictionary<string, string> birOncekiKelime = new Dictionary<string, string>();
        bool[] bakilanIndexler;


        Random rn = new Random();

        string kelime = default; // Harf yerleştirilecek kelimeyi burada tanımlayın
        string birOncekiKelimeKey = "";
        int buttonLeft = 10; // İlk butonun sola olan uzaklığı
        int yenibuttonLeft = 10;
        int left = 10;
        int tahminButtonSayisi = 0;
        int dogruTahmin = 0;

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

            birOncekiKelime.Add(kelime, veri.Item1);//Türkçe-İngilizce
            label1.Text = veri.Item1;

            bakilanIndexler = new bool[kelime.Length];

            for (int i = 0; i < karisikkelime.Length; i++)
            {
                Button button = new Button();
                button.Text = karisikkelime[i].ToString();
                button.Name = "Text" + (i).ToString();
                ButtonSira(button, i, false);//Bu fonksiyonu kelimeleri düzenli bir sırada eklemek için oluşturdum.

                buttonList.Add(button);
                this.Controls.Add(button); // Butonu forma ekleyin

                Button tahminButtonu = new Button();//Buttonların olması gereken yerlerini belirlemek için.
                tahminButtonu.Text = kelime[i].ToString();

                ButtonSira(tahminButtonu, i, true);
                butonTahminBilgi.Add(new EklenenButtonBilgi
                {
                    Konum = tahminButtonu.Location
                    //buttonId değerini de alırsam güzel olabilir.
                }) ;

                butonBilgi.Add(new EklenenButtonBilgi
                {
                    ButtonId = button.Name,
                    Konum = button.Location
                });
            }
            yenibuttonLeft = 10;

            foreach (Button buton in buttonList)
            {
                buton.Click += Button_Click;
            }

            if (birOncekiKelimeKey != "") 
                label2.Text = birOncekiKelime[birOncekiKelimeKey] +"\n"+ birOncekiKelimeKey;
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

        private async void EskiBilinenKelimeNeydi()
        {
            birOncekiKelimeKey = kelime;
            await Task.Delay(1500);
            YeniKelimeGetir();
        }

        private void DogruTahminMi(Button buton)
        {
            if (buton.Text== kelime[dogruTahmin].ToString())
            {
                if(buton.Location == butonTahminBilgi[dogruTahmin].Konum)
                {
                    buton.Enabled = false;

                }
                dogruTahmin++;
                if (dogruTahmin >= kelime.Length)
                {
                    EskiBilinenKelimeNeydi();
                }
            }
        }
        bool rastgele = false;
        private void Button_Click(object sender, EventArgs e)
        {
            int index = rastgele ? 31 : tahminButtonSayisi; //soldaki değeri bir method oluşturup rastgele bir indexin bilgilerini gönderecek şekilde deiştireceğim.
            if (!bakilanIndexler[index])//eğer indeksimize değer atanmadıysa çalışacak fonksiyon.
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


            bakilanIndexler[index] = true;

            ButtonSira(newButton, index, true);

            DogruTahminMi(newButton);

            buttonTahminList.Add(newButton);
            newButton.Click += TahminButtonGeriCek_Click;

            if (index == tahminButtonSayisi && kelime.Length > tahminButtonSayisi)
                tahminButtonSayisi++;

            else if (kelime.Length <= tahminButtonSayisi) tahminButtonSayisi = 0;
            this.Controls.Add(newButton);
            }
        }

        private List<RastgeleHarfBilgi> RastgeleDogruHarfGetir()
        {
            tekrar:
            int rastgeleIndex = rn.Next(0, kelime.Length-dogruTahmin);
            if (!bakilanIndexler[rastgeleIndex])
                ;
            else goto tekrar;

            Point a =butonTahminBilgi[1].Konum;//Burada buttonList'de bulunan buttonun bilgilerini alıp buttonClicke gönderip kodu çalışır hale getirmem gerek.
            return rastgeleHarf;
        }

        public string KelimeKaristir(string word)
        {
            char[] harfler = word.ToCharArray();

            for (int i=word.Length-1;i>0;i--)
            {
                int hangiHarf = rn.Next(i + 1);//rastgele index üretilir.
                char harf = harfler[i];//harfe indeks numarasına göre harf gönderilir.

                harfler[i] = harfler[hangiHarf];//kelimenin sıradaki indeksine rastgele bir indeksteki veri gönderilir.
                harfler[hangiHarf] = harf;//rastgele indeksteki  veriye de sıradaki verinin indeksindeki veri girilir.
            }
            return new string(harfler);
        }

        void YeniKelimeGetir()
        {
            butonBilgi.Clear();
            butonTahminBilgi.Clear();
            dogruTahmin = 0;
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

        //private Button HangiButtonGelmeli()
        //{
        //    Button bt = new Button();

        //    int suankiDogruTahmin = dogruTahmin;

        //    for (int i = 0; i < buttonList.Count; i++)
        //    {
        //        DogruTahminMi(buttonList[i]);
        //        if (dogruTahmin != suankiDogruTahmin)
        //        {
        //            buttonDondur++;
        //            dogruTahmin--;
        //            bt = buttonList[i];
        //            buttonList.RemoveAt(i);
        //            break;
        //        }
        //    }
        //    return bt;
        //}
        private void button2_Click(object sender, EventArgs e)
        {
            //HangiButtonGelmeli().PerformClick();

            //if (buttonDondur >= 3)// en fazla 3 tane harf getirelim.
            //    button2.Enabled = false;

        }

    }
    public class EklenenButtonBilgi
    {
        public string ButtonId { get; set; }
        public Point Konum { get; set; }
    }
    public class RastgeleHarfBilgi
    {
        public int ButtonId { get; set; }
        public Button buton { get; set; }
    }
}
