using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace GenericBot
{
    public class Animols
    {
        private HttpClient webclient;
        private List<string> dogs = new List<string>();
        private List<string> cats = new List<string>();

        public Animols()
        {
            webclient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                while (cats.Count < 5)
                {
                    try
                    {
                        var url = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(webclient.GetStringAsync("https://aws.random.cat/meow").Result).Value;
                        cats.Add(url);
                    }

                    catch { cats.Add("Something broke here"); }
                }
                while (dogs.Count < 5)
                {
                    var url = webclient.GetStringAsync(new Uri("https://random.dog/woof")).Result;
                    while (url.EndsWith("mp4"))
                    {
                        url = webclient.GetStringAsync(new Uri("https://random.dog/woof")).Result;
                    }
                    dogs.Add("http://random.dog/" + url);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public string GetCat()
        {
            lock ("cats")
            {
                string cat = cats.ElementAt(0);
                cats.RemoveAt(0);
                return cat;
            }
        }

        public void RenewCats()
        {
            var url = webclient.GetAsync("https://thecatapi.com/api/images/get?api_key=MzE0MDUx").Result.Headers.GetValues("original_image").First();
            lock ("cats")
                cats.Add(url);
        }

        public string GetDog()
        {
            lock ("dogs")
            {
                string dog = dogs.ElementAt(0);
                dogs.RemoveAt(0);
                return dog;
            }
        }

        public void RenewDogs()
        {
            var url = webclient.GetStringAsync(new Uri("https://random.dog/woof")).Result;
            while (url.EndsWith("mp4"))
            {
                url = webclient.GetStringAsync(new Uri("https://random.dog/woof")).Result;
            }
            lock ("dogs")
                dogs.Add("http://random.dog/" + url);
        }
    }
}
