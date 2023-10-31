using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kelimeciniz
{
    public partial class HarfAlayim : Form
    {
        GoogleSheets gs = new GoogleSheets(); //Api ile veri çektiğim class
        List<Button> buttonList = new List<Button>(); //Harfleri oluşturup not ettiğim harfler
        List<Button> buttonTahminList = new List<Button>(); //Tahminleri ekleyip tuttuğum harfler
        List<EklenenButtonBilgi> butonBilgi = new List<EklenenButtonBilgi>(); //Harflerin bilgilerini tuttuğum yer
        List<EklenenButtonBilgi> butonTahminBilgi = new List<EklenenButtonBilgi>(); //Buttonların gitmesi gereken yerleri not aldığım liste

        Dictionary<string, string> birOncekiKelime = new Dictionary<string, string>();//Bildiğimiz kelimeleri not almak için kullandığım dict.
        List<string> bakilanButtonNameler = new List<string>();
        int[] indexNereyeGitti; //Hangi button hangi indexe gitti bunu takip etmek için.

        bool[] bakilanIndexler; //Hangi harfler tahmin edildi takip etmek için.

        Random rn = new Random();
     
        string kelime = default; // Harf yerleştirilecek kelimeyi burada tanımlayın
        string birOncekiKelimeKey = "";

        int buttonLeft = 10; // İlk butonun sola olan uzaklığı
        int yenibuttonLeft = 10;
        int left = 10;
        int tahminButtonSayisi = 0;
        int dogruTahmin = 0;
        int buttonTiklamaSayisi = 1;

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
        private void ButtonSira(Button button, int siraNo, bool tahminMi)//Hangi button hangi satır-sütun a gidilmesi gerek onu bulmamı ve eklememi sağlayan method.
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

        private void OlusturVeDuzenleButonlar()//Keklimeynin harflerini bölmek ve bilgileir eklemek için kullandığım method.
        {
        again:
            Tuple<string, string> veri = gs.RastgeleKelimeGetirVTOrMyList(true);
            kelime = veri.Item2;
            if (veri.Item2.Length >= 16)
            {
                buttonLeft = 10;
                goto again;
            }

            indexNereyeGitti = new int[kelime.Length];
            for (int i = 0; i < kelime.Length; i++)
                indexNereyeGitti[i] = i;

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

                string amk = "Text" + indexNereyeGitti[i];

                butonTahminBilgi.Add(new EklenenButtonBilgi
                {
                    Konum = tahminButtonu.Location,
                    ButtonId = "Text" + indexNereyeGitti[i]

                    //buttonId değerini de alırsam güzel olabilir.
                });

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
                label2.Text = birOncekiKelime[birOncekiKelimeKey] + "\n" + birOncekiKelimeKey;
        }
        private void TahminButtonGeriCek_Click(object sender, EventArgs e)//Tahmin edilen buttonu eski yerine almak için.
        {
            //int index = rastgele ? hangiSira : tahminButtonSayisi; //soldaki değeri bir method oluşturup rastgele bir indexin bilgilerini gönderecek şekilde deiştireceğim.
            Button IslemButtonGeri = (Button)sender;
            char geriAlinanHarf = Convert.ToChar(IslemButtonGeri.Text);
            this.Controls.Remove(IslemButtonGeri);

            string TiklananButton = IslemButtonGeri.Name;

            buttonTahminList.Remove(IslemButtonGeri);
            if (yenibuttonLeft > 10)
                yenibuttonLeft -= 40;
            else
                yenibuttonLeft = 10;
            bakilanIndexler[tahminButtonSayisi] = false;

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
            for (int i = 0; i < butonBilgi.Count; i++)
            {
                if (TiklananButton == butonBilgi[i].ButtonId)
                {
                    butonGeri.Location = butonBilgi[i].Konum;
                    bakilanIndexler[i] = false;
                }
            }
            butonGeri.Name = TiklananButton;

            butonGeri.Click += Button_Click;
            buttonList.Add(butonGeri);
            this.Controls.Add(butonGeri);
        }

        private async void EskiBilinenKelimeNeydi() //Kelimeyi doğru tahmin edince 1.5 saniye sonrasında yeni kelime geliyor.
        {
            birOncekiKelimeKey = kelime;
            await Task.Delay(1500);
            YeniKelimeGetir();
        }

        private void DogruTahminMi(Button buton) //Harfin doğru sırada olup olmadığını öğrenmek için.
        {
            //int index = rastgele ? hangiSira : dogruTahmin;
            if (buton.Text == kelime[dogruTahmin].ToString())
            {
                if (buton.Location == butonTahminBilgi[dogruTahmin].Konum)
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

        private void Button_Click(object sender, EventArgs e)
        {
            if(!bakilanIndexler[tahminButtonSayisi])
            {
                char harf = default;
                Button butonIslem = (Button)sender;
                harf = Convert.ToChar(butonIslem.Text);
                string butonIsim = butonIslem.Name;

                this.Controls.Remove(butonIslem); // Button'u formdan kaldır
                buttonList.Remove(butonIslem); // Button'u liste içinden kaldır

                if (bakilanButtonNameler.Contains(butonIsim))
                    return;

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
            
                bakilanButtonNameler.Add(butonIsim);
                bakilanIndexler[tahminButtonSayisi] = true;

                ButtonSira(newButton, tahminButtonSayisi, true);//Bu fonksiyonu kelimeleri düzenli bir sırada eklemek için oluşturdum.

                DogruTahminMi(newButton);//Koyulan harf doğru mu onu test ediyorum.

                buttonTahminList.Add(newButton);
                newButton.Click += TahminButtonGeriCek_Click;

                if (kelime.Length >= tahminButtonSayisi)
                    tahminButtonSayisi++;
                else tahminButtonSayisi = 0;
                this.Controls.Add(newButton);
            }

        }

        public string KelimeKaristir(string word)// Kelimenin harflerini bölüp karıştırdığım bölüm.
        {
            char[] harfler = word.ToCharArray();
            for (int i = word.Length - 1; i > 0; i--)
            {
                int hangiHarf = rn.Next(i + 1);//rastgele index üretilir.
                char harf = harfler[i];//harfe indeks numarasına göre harf gönderilir.
                indexNereyeGitti[i] = indexNereyeGitti[hangiHarf];
                indexNereyeGitti[hangiHarf] = i;

                harfler[i] = harfler[hangiHarf];//kelimenin sıradaki indeksine rastgele bir indeksteki veri gönderilir.
                harfler[hangiHarf] = harf;//rastgele indeksteki  veriye de sıradaki verinin indeksindeki veri girilir.
            }

            return new string(harfler);
        }
        void YeniKelimeGetir() // Değişkenleri sıfırlayıp yeni kelimenin getirildiği method.
        {
            butonBilgi.Clear();
            butonTahminBilgi.Clear();
            bakilanButtonNameler.Clear();
            dogruTahmin = 0;
            tahminButtonSayisi = 0;
            buttonLeft = 10;
            yenibuttonLeft = 10;
            buttonTiklamaSayisi = 0;
            
            foreach (Button buton in buttonList)
            {
                this.Controls.Remove(buton);
            }
            foreach (Button buton in buttonTahminList)
            {
                this.Controls.Remove(buton);
            }

            OlusturVeDuzenleButonlar();
            button2.Enabled = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            YeniKelimeGetir();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            // Butonu devre dışı bırak
            button2.Enabled = false;

            int suankiDogruTahmin = dogruTahmin;
            for (int i = 0; i < buttonList.Count; i++)
            {
                DogruTahminMi(buttonList[i]);
                if (dogruTahmin != suankiDogruTahmin)
                {
                    dogruTahmin--;
                    buttonList[i].PerformClick();
                    break;
                }
            }

            // İşlem tamamlandığında butonu etkinleştir
            button2.Enabled = true;
            if (buttonTiklamaSayisi++ >= 3)// Bu değeri her kelimede sıfırlamak yerine toplam 4 hakla sınırlayıp hak verebilrim bir de tahmin buttonları maks 6 kadar yanlış yerleştirmee izin vereblr. 
                button2.Enabled = false;
        }

    }
    public class EklenenButtonBilgi
    {
        public string ButtonId { get; set; }
        public Point Konum { get; set; }
    }
}
