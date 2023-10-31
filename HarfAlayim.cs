﻿using System;
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
        int[] indexNereyeGitti; //Hangi button hangi indexe gitti bunu takip etmek için.

        bool[] bakilanIndexler; //Hangi harfler tahmin edildi takip etmek için.


        Random rn = new Random();

        string kelime = default; // Harf yerleştirilecek kelimeyi burada tanımlıyorum.
        string birOncekiKelimeKey = "";
        int buttonLeft = 10; // İlk butonun sola olan uzaklığı
        int yenibuttonLeft = 10; //tahminButtonlarının yeri.
        int left = 10;
        int tahminButtonSayisi = 0;
        int dogruTahmin = 0;
        bool rastgele = false;
        int hangiSira = -1;


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
        private void ButtonSira(Button button,int sira)
        {
            button.Height = 30;
            button.Width = 30;
            int buttonTop = 180;
            int left1 = 10;
            sira= sira % 8; //Sırada sadece 8 harf olabileceği için.
            for (int i = 0; i < sira; i++)
            {
                left1 += button.Width + 10;
            }

            if (sira < 8)
            {
                button.Top = buttonTop; // Butonun yatay konumunu ayarlayın
                button.Left = left1;
            }
            else if (sira == 8)
            {
                button.Top = buttonTop + 30;
                button.Left = 10;
            }
            else if (sira > 8 && sira <= 16)
            {
                button.Top = buttonTop + 30;
                button.Left = left1;
            }

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
        private void TahminButtonGeriCek_Click(object sender, EventArgs e)//Tahmin edilen buttonu eski yerine almak için.
        {
            int index = rastgele ? hangiSira : tahminButtonSayisi; //soldaki değeri bir method oluşturup rastgele bir indexin bilgilerini gönderecek şekilde deiştireceğim.
            Button IslemButtonGeri = (Button)sender;
            char geriAlinanHarf = Convert.ToChar(IslemButtonGeri.Text);
            this.Controls.Remove(IslemButtonGeri);

            string TiklananButton = IslemButtonGeri.Name;

            buttonTahminList.Remove(IslemButtonGeri);
            if (yenibuttonLeft > 10)
                yenibuttonLeft -= 40;
            else 
                yenibuttonLeft = 10;

            if(!rastgele)
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
                {
                    butonGeri.Location = butonBilgi[i].Konum;
                    bakilanIndexler[i] = false;
                }
            }
            butonGeri.Name = TiklananButton;

            butonGeri.Click += Button_Click;
            buttonList.Add(butonGeri);
            this.Controls.Add(butonGeri);
            rastgele = false;
        }

        private async void EskiBilinenKelimeNeydi() //Kelimeyi doğru tahmin edince 1.5 saniye sonrasında yeni kelime geliyor.
        {
            birOncekiKelimeKey = kelime;
            await Task.Delay(1500);
            YeniKelimeGetir();
        }

        private void DogruTahminMi(Button buton) //Harfin doğru sırada olup olmadığını öğrenmek için.
        {
            int index = rastgele ? hangiSira : dogruTahmin;
            if (buton.Text== kelime[index].ToString())
            {
                if(buton.Location == butonTahminBilgi[index].Konum)
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
        private void Button_Click(object sender, EventArgs e) //Yukarıaki Buttonlara tıkladığım zaman çalışıp tahmin yerine gönderen method.
        {
            int index = rastgele ? hangiSira : tahminButtonSayisi; //soldaki değeri bir method oluşturup rastgele bir indexin bilgilerini gönderecek şekilde deiştireceğim.
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

                if(rastgele)
                {
                    hangiSira = tahminButtonSayisi;
                    ButtonSira(newButton, index);
                }
                else
                    ButtonSira(newButton, index, true);

                DogruTahminMi(newButton);

                buttonTahminList.Add(newButton);
                newButton.Click += TahminButtonGeriCek_Click;

                if (index == tahminButtonSayisi && kelime.Length > tahminButtonSayisi)
                    tahminButtonSayisi++;

                else if (kelime.Length <= tahminButtonSayisi) 
                    tahminButtonSayisi = 0;

                rastgele = false;
                this.Controls.Add(newButton);
            }
        }
        
        private void RastgeleDogruHarfGetir() //rastgele doğru bir harfi koymaıy planlıyorum.
        {
        tekrar:
            int rastgeleIndex = rn.Next(0, kelime.Length);
            if (bakilanIndexler[rastgeleIndex])
                goto tekrar;
            else
            {
                Button btn = new Button();
                string id = butonTahminBilgi[rastgeleIndex].ButtonId.ToString();

                for(int i=0;i<buttonList.Count();i++)
                {
                    if (id == buttonList[i].Name)
                    {
                        btn = buttonList[i];
                        break;
                    }
                }
                hangiSira = rastgeleIndex;
                rastgele = true;
                btn.PerformClick();
            }
        }

        public string KelimeKaristir(string word)// Kelimenin harflerini bölüp karıştırdığım bölüm.
        {
            char[] harfler = word.ToCharArray();
            for (int i=word.Length-1;i>0;i--)
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
            RastgeleDogruHarfGetir();
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
}
