using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Cloud.Translation.V2;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;

namespace kelimeciniz
{
    internal class GoogleSheets
    {
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private static UserCredential credential;
        private SheetsService service;
        string appName = "Desktop client 1";

        ValueRange responseSutunA;
        ValueRange responseSutunB;
        ValueRange responseSearchWord;
        ValueRange responseAramaKelime;


        IList<IList<object>> sutunAVeri;
        IList<IList<object>> sutunBVeri;
        IList<IList<object>> columnWordData;
        IList<IList<object>> columnKelimeVeri;

        string spreadsheetId = "1kafz2KAuvxqSdGbfNOou1S5keIf5wQDIDRLsdm9t6l8"; // excel tablosunun id'si
        string sutun1 = "Sayfa1!A:A"; //hangi satırı yazmak istediğim.
        string sutun2 = "Sayfa1!B:B"; //hangi satırı yazmak istediğim.
        string VeriKumesiIng = "Veriler!A:A";
        string VeriKumesiTr = "Veriler!B:B";

        SpreadsheetsResource.ValuesResource.AppendRequest verireq;
        ValueRange body;
        string range = "Sayfa1!A:B"; //verieklemeSatırı

        static string myMemoryApiKey = "1fb0d0fab1c449d5df11";



        public GoogleSheets()//Class çağırıldığında çalışmasını istediğim constructor(Bazı verileri çekip ön belleğe almak için.)
        {
            using (var stream = new FileStream("C:\\Users\\as\\source\\repos\\kelimeciniz\\client_secret_950495088287-0n98o9v2m357pvrif86ffbhspb73dg0d.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None
                ).Result;
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = appName,//Uygulamanın ismi
            });

           //Api'ye istek atıyorum
            SpreadsheetsResource.ValuesResource.GetRequest request1 =
               service.Spreadsheets.Values.Get(spreadsheetId, sutun1);

            SpreadsheetsResource.ValuesResource.GetRequest request2 =
               service.Spreadsheets.Values.Get(spreadsheetId, sutun2);

            SpreadsheetsResource.ValuesResource.GetRequest reqSearch =
              service.Spreadsheets.Values.Get(spreadsheetId, VeriKumesiIng);
            SpreadsheetsResource.ValuesResource.GetRequest reqArama =
              service.Spreadsheets.Values.Get(spreadsheetId, VeriKumesiTr);

            responseSutunA = request1.Execute();
            responseSutunB = request2.Execute();
            responseSearchWord = reqSearch.Execute();
            responseAramaKelime = reqArama.Execute();

            //değerleri listelere çekiyorum.
            sutunAVeri = responseSutunA.Values;
            sutunBVeri = responseSutunB.Values;
            columnWordData = responseSearchWord.Values;
            columnKelimeVeri = responseAramaKelime.Values;
        }

        public Tuple<List<string>, List<string>> KelimeAra(string AranacakWord)
        {
            //Kendi Database'imde bulunan değerleri çekip atıyorum listelere
            List<string> sonucA = new List<string>();
            List<string> sonucB = new List<string>();

            bool kelimeBulundu = false; // Kelimenin bulunup bulunmadığını kontrol etmek için bir bayrak

            if (columnWordData != null && columnWordData.Count > 0)
            {
                for (int i = 0; i < columnWordData.Count; i++)
                {
                    for (int j = 0; j < columnWordData[i].Count; j++)
                    {
                        string[] words = columnWordData[i][j].ToString().Split(new[] { ' ', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string word in words)
                        {
                            if (word.Trim().Equals(AranacakWord, StringComparison.OrdinalIgnoreCase))
                            {
                                sonucA.Add(columnWordData[i][j].ToString());
                                sonucB.Add(columnKelimeVeri[i][j].ToString());
                                kelimeBulundu = true; // Kelimeyi bulduk, bayrağı true yap
                            }
                        }
                    }
                }
            }

            if (!kelimeBulundu)//Eğer database'imde aradığım kelime yok ise api ile çekiyorum veriyi.
            {
                sonucA.Add(KelimeDuzelt(AranacakWord));
                sonucB.Add(Ceviri(AranacakWord,false).ToString()); 
            }

            var uniqueSonucA = new HashSet<string>(sonucA);
            var uniqueSonucB = new HashSet<string>(sonucB);

            return new Tuple<List<string>, List<string>>(uniqueSonucA.ToList(), uniqueSonucB.ToList());
        }
        /// <summary>
        /// Bu overloading methodu direkt olarak ingilizceden türkçeye arama yapmamız için oluşturduğum bir kod.
        /// tr adlı değişken true olursa devreye girer.
        /// </summary>
        /// <param name="kelime"></param>
        /// <param name="tr"></param>
        /// <returns></returns>
        public string KelimeAra(string kelime,bool tr)
        {
            string result = Ceviri(kelime,true).ToString();

            return result; 
        }
        /// <summary>
        /// Rastgele kelime ve anlamını çekmek için oluşturduğum method. Sözlükten veri çekilmesini istiyorsanız değişken true olmalı. Veri ekleyerek oluşturduğumuz listeden veri çekmek için false olması gerekiyor.
        /// </summary>
        /// <param name="VeritabaniMi"></param>
        /// <returns></returns>
        public Tuple<string,string> RastgeleKelimeGetirVTOrMyList(bool VeritabaniMi)
        {
            //kaç satırlık veri var bunların değerini çekiyorum.
            int sonsatirMyList = sutunAVeri.Count();
            int sonsatirVT = columnKelimeVeri.Count();

            Random rn = new Random();
            int hangiSatirMyList = rn.Next(0, sonsatirMyList);//veri sayısına göre rastgele bir satırdan veri çekmeyi istiyorum.
            int hangiSatirVT = rn.Next(0, sonsatirVT);

            string kelime = VeritabaniMi ? columnKelimeVeri[hangiSatirVT][0].ToString(): sutunAVeri[hangiSatirMyList][0].ToString();//Eğer veritabaniMi sorgusu true gelirse kendi sözlüğümden veri çekip değişkene atayacağım değilse sayfa1'de bulunan kendi eklediğim kelimelerden veri çekip değişkene atayacağım.
            string word = VeritabaniMi ? columnWordData[hangiSatirVT][0].ToString() : sutunBVeri[hangiSatirMyList][0].ToString();//aynı mantıkla kelimenin anlamını çekiyorum.

            return Tuple.Create(KelimeDuzelt(kelime), KelimeDuzelt(word));//verileri Tuple nesnesine çevirip gönderiyorum.
        }


        private string KelimeDuzelt(string kelime)//Sayfama veri eklerken bu formatta eklensin istiyorum.
        {
            string sonuc = char.ToUpper(kelime[0]) + kelime.Substring(1).ToLower();
            return sonuc;
        }

        public void VeriEkle(string word, string kelime)
        {
            // KelimeAra metodunu asenkron olarak çağırın
            var kelimeAraResult =  KelimeAra(word);

            string[] words = kelimeAraResult.Item1.ToArray();
            string[] kelimeler = kelimeAraResult.Item2.ToArray();

            for (int i = 0; i < words.Length; i++)
            {
                if (word.ToLower() == words[i].ToLower())//Eğer database'imde aradığım kelime varsa bu çalışacak yoksa aşağıdaki
                {
                    kelime = KelimeDuzelt(kelimeler[i]);
                }
                else
                {
                    kelime = Ceviri(word, false).ToString(); // EnglishToTurkish metodunu kullanarak arama yapıyorum.
                }
            }
            // Veriyi eklemek için gereken parametreleri oluştur
            body = new ValueRange
            {
                Values = new List<IList<object>> { new List<object> { KelimeDuzelt(word), KelimeDuzelt(kelime) } }
            };

            verireq = service.Spreadsheets.Values.Append(body, spreadsheetId, range);//Veri ekleme

            // Veriyi eklemek istediğiniz hücreyi belirleyin

            // Veriyi ekle
            verireq.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            verireq.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            AppendValuesResponse response = verireq.Execute();

            // İşlem sonucunu kontrol edin
            //if (responseUpdate.UpdatedCells > 0)
            //{
            //    Console.WriteLine("Veri başarıyla eklendi.");//Değişebilir
            //}
            //else
            //{
            //Console.WriteLine("Veri ekleme başarısız.");
            //}
        }

        public Tuple<List<string>,List<string>> Sayfa1Veri()
        {
            List<string> ASutunu = new List<string>();
            List<string> BSutunu = new List<string>();

            if (sutunAVeri != null && sutunAVeri.Count > 0)
            {
                foreach (var row in sutunAVeri)
                {
                    foreach (string cell in row)
                    {
                        ASutunu.Add(cell);
                    }
                }
            }
            if (sutunBVeri != null && sutunBVeri.Count > 0)
            {
                foreach (var row in sutunBVeri)
                {
                    foreach (string cell in row)
                    {
                        BSutunu.Add(cell);
                    }
                }
            }
            Tuple<List<string>, List<string>> veri = new Tuple<List<string>, List<string>>(ASutunu, BSutunu);

            return veri;
        }


        private string Ceviri(string text,bool tr)
        {
            using (HttpClient client = new HttpClient())
            {
                string ceviridil = tr ? "tr|en" : "en|tr";
                string apiUrl = $"https://api.mymemory.translated.net/get?q={text}&langpair={ceviridil}";

                HttpResponseMessage response = client.GetAsync(apiUrl).Result; // Bekleyerek sonucu al

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result; // Bekleyerek içeriği al
                    dynamic json = JObject.Parse(responseBody);
                    string translatedText = json.responseData.translatedText;

                    return translatedText;
                }
                else
                {
                    return "Çevirisiz.";
                }
            }
        }


    }
}
