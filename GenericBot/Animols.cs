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
            webclient = new HttpClient(new HttpClientHandler(){AllowAutoRedirect = false});
            RenewDogs();
            RenewCats();
        }

        public string GetCat()
        {
            string cat = cats.ElementAt(0);
            cats.RemoveAt(0);
            return cat;
        }

        public async void RenewCats()
        {
            while (cats.Count < 5)
            {
                var url = webclient.GetAsync("https://thecatapi.com/api/images/get").Result.Headers.GetValues("original_image").First();
                cats.Add(url);
            }
        }

        public string GetDog()
        {
            string dog = dogs.ElementAt(0);
            dogs.RemoveAt(0);
            return dog;
        }

        public async void RenewDogs()
        {
            while (dogs.Count < 5)
            {
                var url = JsonConvert.DeserializeObject<Dictionary<string, string>>(webclient
                    .GetStringAsync(new Uri("https://random.dog/woof.json")).Result)["url"];
                while (url.EndsWith("mp4"))
                {
                    url = JsonConvert.DeserializeObject<Dictionary<string, string>>(webclient
                        .GetStringAsync(new Uri("https://random.dog/woof.json")).Result)["url"];
                }
                dogs.Add(url);
            }
        }
    }
}
