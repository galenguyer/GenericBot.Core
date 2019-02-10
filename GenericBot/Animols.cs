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

        // For deserialization
        private class Cat { public string file { get; set; } public Cat() { } }

        public Animols()
        {
            webclient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });
            System.Threading.Thread t = new System.Threading.Thread(() =>
            {
                while (cats.Count < 5)
                {
                    try
                    {
                        var url = JsonConvert.DeserializeObject<Cat>(webclient.GetStringAsync("https://aws.random.cat/meow").Result).file;
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
            var url = JsonConvert.DeserializeObject<Cat>(webclient.GetStringAsync("https://aws.random.cat/meow").Result).file;
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
