using GenericBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GenericBot.CommandModules
{
    class ImageModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();
            Command cat = new Command("cat");
            cat.Description = "Link a cat pic";
            cat.SendTyping = false;
            cat.ToExecute += async (context) =>
            {
                var catStruct = new { file = string.Empty };
                using (var webclient = new WebClient())
                {
                    var catResult = JsonConvert.DeserializeAnonymousType(webclient.DownloadString(new Uri("http://aws.random.cat/meow")), catStruct);
                    await context.Message.ReplyAsync(catResult.file);
                }

            };
            commands.Add(cat);

            Command dog = new Command("dog");
            dog.Description = "Link a dog pic";
            dog.SendTyping = false;
            dog.ToExecute += async (context) =>
            {
                string url = string.Empty;
                using (var webclient = new WebClient())
                {
                    url = webclient.DownloadString(new Uri("https://random.dog/woof")); 
                    while (url.EndsWith("mp4")) 
                    {
                        url = webclient.DownloadString(new Uri("https://random.dog/woof")); 
                    }
                }
                await context.Message.ReplyAsync("http://random.dog/" + url);
            };
            commands.Add(dog);

            Command jeff = new Command("jeff");
            jeff.ToExecute += async (context) =>
            {
                string filename = "";
                if (context.Parameters.IsEmpty())
                {
                    var user = context.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else if (Uri.IsWellFormedUriString(context.Parameters[0], UriKind.RelativeOrAbsolute) &&
                                         (context.Parameters[0].EndsWith(".png") || context.Parameters[0].EndsWith(".jpg") ||
                                          context.Parameters[0].EndsWith("jpeg") || context.Parameters[0].EndsWith(".gif")))
                {
                    filename = $"files/img/{context.Message.Id}.{context.ParameterString.Split('.').Last()}";
                    using (WebClient webclient = new WebClient())
                    {
                        await webclient.DownloadFileTaskAsync(new Uri(context.ParameterString), filename);
                    }
                }
                else if (context.Message.GetMentionedUsers().Any())
                {
                    var user = context.Message.GetMentionedUsers().First();
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }

                {
                    int targetWidth = 1278;
                    int targetHeight = 717; //height and width of the finished image
                    Image baseImage = Image.FromFile("files/img/jeff.png");
                    Image avatar = Image.FromFile(filename);

                    //be sure to use a pixelformat that supports transparency
                    using (var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            //this ensures that the backgroundcolor is transparent
                            canvas.Clear(Color.White);

                            //this paints the frontimage with a offset at the given coordinates
                            canvas.DrawImage(avatar, 523, 92, 269, 269);

                            //this selects the entire backimage and and paints
                            //it on the new image in the same size, so its not distorted.
                            canvas.DrawImage(baseImage, 0, 0, targetWidth, targetHeight);
                            canvas.Save();
                        }

                        bitmap.Save($"files/img/jeff_{context.Message.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    await Task.Delay(100);
                    await context.Channel.SendFileAsync($"files/img/jeff_{context.Message.Id}.png");
                    baseImage.Dispose();
                    avatar.Dispose();
                    File.Delete(filename);
                    File.Delete($"files/img/jeff_{context.Message.Id}.png");
                }
            };
            commands.Add(jeff);

            Command warm = new Command("warm");
            warm.ToExecute += async (context) =>
            {
                string filename = "";
                if (context.Parameters.IsEmpty())
                {
                    var user = context.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl(size: 512)),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else if (Uri.IsWellFormedUriString(context.Parameters[0], UriKind.RelativeOrAbsolute) &&
                                         (context.Parameters[0].EndsWith(".png") || context.Parameters[0].EndsWith(".jpg") ||
                                          context.Parameters[0].EndsWith("jpeg") || context.Parameters[0].EndsWith(".gif")))
                {
                    filename = $"files/img/{context.Message.Id}.{context.ParameterString.Split('.').Last()}";
                    using (WebClient webclient = new WebClient())
                    {
                        await webclient.DownloadFileTaskAsync(new Uri(context.ParameterString), filename);
                    }
                }
                else if (context.Message.GetMentionedUsers().Any())
                {
                    var user = context.Message.GetMentionedUsers().First();
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }

                {
                    int targetWidth = 596;
                    int targetHeight = 684; //height and width of the finished image
                    Image baseImage = Image.FromFile("files/img/warm.png");
                    Image avatar = Image.FromFile(filename);

                    //be sure to use a pixelformat that supports transparency
                    using (var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            //this ensures that the backgroundcolor is transparent
                            canvas.Clear(Color.White);

                            //this paints the frontimage with a offset at the given coordinates
                            canvas.DrawImage(avatar, 20, 466, 218, 218);

                            //this selects the entire backimage and and paints
                            //it on the new image in the same size, so its not distorted.
                            canvas.DrawImage(baseImage, 0, 0, targetWidth, targetHeight);
                            canvas.Save();
                        }

                        bitmap.Save($"files/img/warm{context.Message.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    await Task.Delay(100);
                    await context.Channel.SendFileAsync($"files/img/warm{context.Message.Id}.png");
                    baseImage.Dispose();
                    avatar.Dispose();
                    File.Delete(filename);
                    File.Delete($"files/img/warm{context.Message.Id}.png");
                }
            };
            commands.Add(warm);

            Command star = new Command("star");
            star.ToExecute += async (context) =>
            {
                string filename = "";
                if (context.Parameters.IsEmpty())
                {
                    var user = context.Message.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else if (Uri.IsWellFormedUriString(context.Parameters[0], UriKind.RelativeOrAbsolute) &&
                                         (context.Parameters[0].EndsWith(".png") || context.Parameters[0].EndsWith(".jpg") ||
                                          context.Parameters[0].EndsWith("jpeg") || context.Parameters[0].EndsWith(".gif")))
                {
                    filename = $"files/img/{context.Message.Id}.{context.Parameters[0].Split('.').Last()}";
                    using (WebClient webclient = new WebClient())
                    {
                        await webclient.DownloadFileTaskAsync(new Uri(context.Parameters[0]), filename);
                    }
                }
                else if (context.Message.GetMentionedUsers().Any())
                {
                    var user = context.Message.GetMentionedUsers().First();
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }

                {
                    int targetWidth = 1242;
                    int targetHeight = 764; //height and width of the finished image
                    Image baseImage = Image.FromFile("files/img/staroranangel.png");
                    Image avatar = Image.FromFile(filename);

                    //be sure to use a pixelformat that supports transparency
                    using (var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            //this ensures that the backgroundcolor is transparent
                            canvas.Clear(Color.Transparent);

                            //this paints the frontimage with a offset at the given coordinates
                            canvas.DrawImage(avatar, 283, 228, 118 * avatar.Width / avatar.Height, 118);
                            canvas.DrawImage(avatar, 746, 250, 364 * avatar.Width / avatar.Height, 346);

                            //this selects the entire backimage and and paints
                            //it on the new image in the same size, so its not distorted.
                            canvas.DrawImage(baseImage, 0, 0, targetWidth, targetHeight);
                            canvas.Save();
                        }

                        bitmap.Save($"files/img/star_{context.Message.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    await Task.Delay(100);
                    await context.Channel.SendFileAsync($"files/img/star_{context.Message.Id}.png");
                    while (true)
                    {
                        try
                        {
                            baseImage.Dispose();
                            avatar.Dispose();
                            File.Delete(filename);
                            File.Delete($"files/img/star_{context.Message.Id}.png");
                            break;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            };
            commands.Add(star);

            return commands;
        }
    }
}
