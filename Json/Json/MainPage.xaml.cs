using System;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Json
{
    public partial class MainPage : ContentPage
    {
        private string url;
        static string requestUrl;
        private Entry titleName;
        
        /*
        public MainPage()
        {
            InitializeComponent();

            var p = new person();
            p.Name = "Kamada Yuto";
            p.country = "Japan";
            p.Age = 20;

            var json = JsonConvert.SerializeObject(p);  //---------------------------Json形式に変更
            var layout = new StackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand,VerticalOptions = LayoutOptions.CenterAndExpand };

            var label = new Label
            {
                Text = $"{json}" //{"Name":"Kamada Yuto","Age":20}
            };
            layout.Children.Add(label);

            var deserialized = JsonConvert.DeserializeObject<person>(json); //---------------------------Json形式から元に戻す
            var label2 = new Label
            {
                Text = $"Name: {deserialized.Name}" //Kamada Yuto
            };
            var label3 = new Label
            {
                Text = $"Age: {deserialized.Age}" //20
            };
            layout.Children.Add(label2);
            layout.Children.Add(label3);

            Content = layout;
        }
        */

        public MainPage()
        {
            InitializeComponent();

            url = "https://app.rakuten.co.jp/services/api/BooksBook/Search/20170404?format=json&applicationId=1051637750796067320&formatVersion=2"; //formatVersion=2にした

            var layout = new StackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand };

            /*
            isbn = new Entry    //EntryでISBNコードを入力
            {
                //Placeholder = "ISBNコードを入力",
                //PlaceholderColor = Color.Gray,
                Text = "9784838729036", //面倒だからTextでISBN設定
                WidthRequest = 170
            };
            layout.Children.Add(isbn);
            //実行url https://app.rakuten.co.jp/services/api/BooksBook/Search/20170404?format=json&applicationId=1051637750796067320&formatVersion=2&isbn=9784838729036
            */

            titleName = new Entry
            {
                Placeholder = "タイトルを入力",
                PlaceholderColor = Color.Gray,
                WidthRequest = 170
            };
            layout.Children.Add(titleName);

         

            var Serch = new Button
            {
                WidthRequest = 60,
                Text = "Serch!",
                TextColor = Color.Red,
            };
            layout.Children.Add(Serch);
            Serch.Clicked += Serch_Click;

            Content = layout;
        }

        //--------------------------------Serchボタンイベントハンドラ-----------------------------------
        private async void Serch_Click(object sender, EventArgs e)
        {
            try
            {
                var layout2 = new StackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand };
                var scroll = new ScrollView { Orientation = ScrollOrientation.Vertical };
                layout2.Children.Add(scroll);
                var layout = new StackLayout { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand };
                scroll.Content = layout;

                string Title = titleName.Text;
                string encodedtitle = Uri.EscapeUriString(Title); //Systemアセンブリ中に存在 UTF-8のみ
                requestUrl = url + "&title=" + encodedtitle; 

              

                //------------------------------ボタン再配置--------------------------
                titleName = new Entry  
                {
                    Placeholder = "タイトルを入力",
                    PlaceholderColor = Color.Gray,
                    WidthRequest = 170
                };
                layout.Children.Add(titleName);

                var Serch = new Button
                {
                    WidthRequest = 60,
                    Text = "Serch!",
                    TextColor = Color.Red,
                };
                layout.Children.Add(Serch);
                Serch.Clicked += Serch_Click;
            　　
                //-------------------------------ボタン再配置--------------------------

                //HTTPアクセスメソッドを呼び出す
                string APIdata = await GetApiAsync(); //jsonをstringで受け取る
                
                //HTTPアクセス失敗処理(404エラーとか名前解決失敗とかタイムアウトとか)
                if (APIdata is null)
                {
                    await DisplayAlert("接続エラー","接続に失敗しました","OK");
                }

                /*
                //レスポンス(JSON)をstringに変換-------------->しなくていい
                Stream s = GetMemoryStream(APIdata); //GetMemoryStreamメソッド呼び出し
                StreamReader sr = new StreamReader(s);
                string json = sr.ReadToEnd();
                */
                /*
                //デシリアライズ------------------>しなくていい
                var rakutenBooks = JsonConvert.DeserializeObject<RakutenBooks>(json.ToString());
                */

                //パースする *重要*   パースとは、文法に従って分析する、品詞を記述する、構文解析する、などの意味を持つ英単語。
                var json = JObject.Parse(APIdata); //stringのAPIdataをJObjectにパース
                var Items = JArray.Parse(json["Items"].ToString()); //Itemsは配列なのでJArrayにパース

                //結果を出力
                foreach (JObject jobj in Items)
                {
                    //↓のように取り出す
                    JValue titleValue = (JValue)jobj["title"];
                    string title = (string)titleValue.Value;

                    JValue titleKanaValue = (JValue)jobj["titleKana"];
                    string titleKana = (string)titleKanaValue.Value;

                    JValue itemCaptionValue = (JValue)jobj["itemCaption"];
                    string itemCaption = (string)itemCaptionValue.Value;

                    JValue gazoValue = (JValue)jobj["largeImageUrl"];
                    string gazo = (string)gazoValue.Value;

                    //書き出し
                    layout.Children.Add(new Label { Text = $"title: { title }" });
                    layout.Children.Add(new Label { Text = $"titleKana: { titleKana }" });
                    layout.Children.Add(new Label { Text = $"itemCaption: { itemCaption }" });
                    layout.Children.Add(new Image { Source = gazo });
                    String A = gazo;

                };

                layout.Children.Add(new Label { Text = "読み取り終了", TextColor = Color.Black });

                layout.Children.Add(new Label { Text = "" });//改行
                
                layout.Children.Add(new Label { Text = "JSON形式で書き出す", TextColor = Color.Red });
                layout.Children.Add(new Label { Text = json.ToString() });

                Content = layout2;
            }
            catch (Exception x) { await DisplayAlert("警告", x.ToString(), "OK"); }
        }


        //HTTPアクセスメソッド
        public static async Task<string> GetApiAsync()
        {
            string APIurl = requestUrl;

            using (HttpClient client = new HttpClient())
                try
                {
                    string urlContents = await client.GetStringAsync(APIurl);
                    await Task.Delay(1000); //1秒待つ(楽天API規約に違反するため)
                    return urlContents;
                }
                catch (Exception e)
                {
                    string a = e.ToString();
                    return null;
                }
        }
        //UTF-8エンコードメソッド ------------------>しなくていい
        public MemoryStream GetMemoryStream(string text)
        {
            string a = text;
            return new MemoryStream(Encoding.UTF8.GetBytes(a));
        }
    }
}
