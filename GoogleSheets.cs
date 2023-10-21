using Google.Apis.Auth.OAuth2;
using System.Collections;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Google.Apis.Requests.BatchRequest;

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

        public string KelimeAra(string AranacakWord)
        {
            List<string> sonuc = new List<string>();

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
                                sonuc.Add(columnKelimeVeri[i][j].ToString());
                            }
                        }
                    }
                }
            }
            var yinelemeSil = new System.Collections.Generic.HashSet<string>(sonuc); //yinelemelri kaldırmak için.

            return string.Join(", ", yinelemeSil);
        }

        public void VeriEkle(string word, string kelime)
        {
            // A ve B sütunlarının sonuna veri eklemek için hedef hücreyi belirleyin
            var requestRange = $"Sayfa1!A:B";

            // Var olan veriyi alın
            SpreadsheetsResource.ValuesResource.GetRequest getRequest = service.Spreadsheets.Values.Get(spreadsheetId, requestRange);
            ValueRange response = getRequest.Execute();
            IList<IList<object>> values = response.Values;

            // Yeni veriyi oluşturun
            var newRow = new List<object> { word, kelime };

            // Var olan veriye yeni veriyi ekleyin
            if (values == null)
            {
                values = new List<IList<object>>();
            }
            values.Add(newRow);

            // Değişiklikleri kaydedin
            ValueRange body = new ValueRange { Values = values };
            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = service.Spreadsheets.Values.Update(body, spreadsheetId, requestRange);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            UpdateValuesResponse responseUpdate = updateRequest.Execute();

            // İşlem sonucunu kontrol edin
            if (responseUpdate.UpdatedCells > 0)
            {
                Console.WriteLine("Veri başarıyla eklendi.");//Değişebilir
            }
            else
            {
                Console.WriteLine("Veri ekleme başarısız.");
            }
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
  
    }
}
