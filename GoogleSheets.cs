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



        public GoogleSheets()
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

            sutunAVeri = responseSutunA.Values;
            sutunBVeri = responseSutunB.Values;
            columnWordData = responseSearchWord.Values;
            columnKelimeVeri = responseAramaKelime.Values;
        }

        public Tuple<List<string>, List<string>> KelimeAra(string AranacakWord)
        {
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

            if (!kelimeBulundu)
            {
                sonucA.Add(KelimeDuzelt(AranacakWord));
                sonucB.Add(EnglishToTurkish(AranacakWord).ToString()); // EnglishToTurkish metodunu await ile bekletin
            }

            var uniqueSonucA = new HashSet<string>(sonucA);
            var uniqueSonucB = new HashSet<string>(sonucB);

            return new Tuple<List<string>, List<string>>(uniqueSonucA.ToList(), uniqueSonucB.ToList());
        }



        private string KelimeDuzelt(string kelime)
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
                if (word.ToLower() == words[i].ToLower())
                {
                    kelime = KelimeDuzelt(kelimeler[i]);
                }
                else
                {
                    kelime = EnglishToTurkish(word).ToString(); // EnglishToTurkish metodunu asenkron olarak çağırın
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


        private string EnglishToTurkish(string text)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://api.mymemory.translated.net/get?q={text}&langpair=en|tr";
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
