using Newtonsoft.Json;
using RepzScreenshot.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RepzScreenshot.DataAccess
{
    class ImgurDataAccess
    {
        
        public static async Task<String> UploadImage(Player player, BitmapImage img)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(ms);
                byte[] bitmapdata = ms.ToArray();

                string base64 = Convert.ToBase64String(bitmapdata);

                NameValueCollection data = new NameValueCollection();
                data["image"] = base64;
                data["type"] = "base64";
                data["title"] = player.Name;
                data["description"] = String.Format("Screenshot of {0}", player.Name);
                //data["album"] = "MG9S5ujZM46pWpe";

                using (var client = new WebClient())
                {
                    client.Headers.Add("Authorization: Client-ID b3f4d2fd8fcbe3c");

                    byte[] responsePayload = await client.UploadValuesTaskAsync(new Uri("https://api.imgur.com/3/image/"), "POST", data);
                    string response = Encoding.ASCII.GetString(responsePayload);
                    Console.Write(response);
                    dynamic json = JsonConvert.DeserializeObject(response);

                    string imageUrl = "https://imgur.com/" + json.data.id.ToString();
                    Console.WriteLine(json);

                    return imageUrl;
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

    }
}
