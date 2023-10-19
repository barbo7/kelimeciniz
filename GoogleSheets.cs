using Google.Apis.Auth.OAuth2;

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
        private ValueRange responseSutunA;

        public GoogleSheets()
        {
            string spreadsheetId = "1kafz2KAuvxqSdGbfNOou1S5keIf5wQDIDRLsdm9t6l8"; // excel tablosunun id'si
            string sutun1 = "Sayfa1!A:A"; //hangi satırı yazmak istediğim.
            string appName = "Desktop client 1";

            using (var stream = new FileStream("C:\\Users\\as\\source\\repos\\kelimeciniz\\client_secret_950495088287-0n98o9v2m357pvrif86ffbhspb73dg0d.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None
                ).Result;
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = appName,//Uygulamanın ismi
            });

           
            SpreadsheetsResource.ValuesResource.GetRequest request =
               service.Spreadsheets.Values.Get(spreadsheetId, sutun1);

             responseSutunA = request.Execute();

            
        }
        public string[] ExcelVeriSutunA()
        {
            List<string> ASutunu = new List<string>();
            IList<IList<object>> values = responseSutunA.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    foreach (string cell in row)
                    {
                        ASutunu.Add(cell);
                    }
                }
            }
            return ASutunu.ToArray();
        }
    }
}
