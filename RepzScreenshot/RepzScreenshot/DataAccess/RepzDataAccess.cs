﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepzScreenshot.Error;
using RepzScreenshot.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace RepzScreenshot.DataAccess
{
    class RepzDataAccess : DataAccessBase
    {
        private const string API_BASE = "http://server.repziw4.de/api/";
        private const double API_DELAY = 1.5;

        private static Dictionary<string, Player> PlayerCache;

        private WebClient client = new WebClient();
        private static DateTime LastRequest {get;set;}

        
       
        public RepzDataAccess()
        {
            //client.Proxy = null;
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            
            client.Headers.Add("User-Agent", "Repz Screenshot Tool/"+version+"(by tccr(352737))");
            
        }

        static RepzDataAccess()
        {
            LastRequest = new DateTime();
            PlayerCache = new Dictionary<string, Player>();
        }
        protected async Task<dynamic> ApiCallAsync(string url)
        {
            
            if(!url.EndsWith("/"))
            {
                url += "/";
            }

            TimeSpan diff = DateTime.Now - LastRequest;
            
            while ((diff=DateTime.Now - LastRequest) < TimeSpan.FromSeconds(API_DELAY))
            {
                //diff = DateTime.Now - LastRequest;

                TimeSpan delay = (TimeSpan.FromSeconds(API_DELAY) - diff);
                if (delay < TimeSpan.FromMilliseconds(1))
                {
                    delay = TimeSpan.FromMilliseconds(100);
                }
                await Task.Delay(delay);
                
            }
            LastRequest = DateTime.Now;
            
            string res = "";
            try
            {
                Console.WriteLine("Request " + url);
                res = await client.DownloadStringTaskAsync(new Uri(url));
            }
            catch(WebException ex)
            {
                
                Console.WriteLine("WebException "+url+" " + ex.Message);
                switch(ex.Status)
                {
                    case WebExceptionStatus.ConnectFailure:
                    case WebExceptionStatus.Timeout:
                    case WebExceptionStatus.NameResolutionFailure:
                        throw new ApiException("Server unreachable");

                    case WebExceptionStatus.ProtocolError:
                        throw new InvalidResponseException();
                }
                
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
            finally
            {
                LastRequest = DateTime.Now;
            }
            
            try
            {
                dynamic json = JsonConvert.DeserializeObject(res);
            
                return json;
            }
            catch(Exception)
            {
                throw new InvalidResponseException();
            }
            
        }

        
        public async Task GetIdAsync(Player player)
        {
            try
            {
                Player p = await GetPlayer(player.Name);
                player.Id = p.Id;
                player.Name = p.Name;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task GetPresenceDataAsync(Player p)
        {
            dynamic json = await ApiCallAsync(API_BASE + "presenceData/" + p.Id);

            if(json.status == 200)
            {
                dynamic result = JObject.Parse((string)json.result);
                string hostname = result.hostname;
                p.Hostname = hostname;
            }
            else if(json.status == 204 && json.result == "Offline")
            {
                throw new UserOfflineException(p);
            }
            
        }

        public async Task<List<Player>> FindPlayers(string name)
        {
            List<Player> players = new List<Player>();
            try
            {
                dynamic json = await ApiCallAsync(API_BASE + "username/" + name);
                if (json.result != null)
                {
                    foreach (dynamic player in json.result)
                    {
                        int id = player.user_id;
                        string username = player.username;
                        Player p = new Player(id, username);

                        //add player to cache
                        if (!PlayerCache.ContainsKey(username))
                        {
                            PlayerCache.Add(username, p);
                        }
                        players.Add(p);
                    }
                }

                return players;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Player> GetPlayer(string name)
        {

            //check cache
            if(PlayerCache.ContainsKey(name))
            {
                return PlayerCache[name];
            }
            try
            {
                List<Player> players = await FindPlayers(name);
            
                if(players.Count == 0)
                {
                    throw new UserNotFoundException();
                }
                else if (players.Count == 1)
                {
                    return players[0];
                }

                //check for exact match
                foreach(Player p in players)
                {
                    if (p.Name == name)
                        return p;

                }

                //find player beginning with name
                foreach (Player p in players)
                {
                    if (p.Name.StartsWith(name))
                        return p;
                }

                //return first in the list
                return players[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<BitmapImage> GetScreenshotAsync(Player player)
        {
            bool done = false;
            for(int tries=0;tries<20 && !done; ++tries)
            {
                try
                {
                    dynamic json = await ApiCallAsync(API_BASE + "screenshot/" + player.Id);

                    if (json.status == 200 && json.result != "Waiting for answer.")
                    {
                        string img = json.result;
                        Byte[] bitmapData = new Byte[img.Length];
                        bitmapData = Convert.FromBase64String(img);
                        System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                        BitmapImage bitImage = new BitmapImage();
                        bitImage.BeginInit();
                        bitImage.StreamSource = streamBitmap;
                        bitImage.EndInit();

                        return bitImage;
                        
                    }
                    else if(json.result == "Offline")
                    {
                        throw new UserOfflineException(player);
                    }
                    else if ((json.status == 200 && json.result == "Waiting for answer.") || (json.status == 204 && json.result == "Request sent."))
                    {
                        
                        await Task.Delay(2000);
                    }
                    else
                    {
                        break;
                    }
                }
                catch(Exception ex)
                {
                    Console.Write(ex.StackTrace);
                    throw ex;
                }
            }

            
            //unable to get screenshot
            throw new ScreenshotException("Unable to get Screenshot");
            

        }

        public async Task<List<Server>> GetServersAsync()
        {
            try
            {
                dynamic json = await ApiCallAsync(API_BASE + "findSessions/61586/1/");
                List<Server> servers = new List<Server>();
                foreach (dynamic server in json)
                {
                    string hostname = server.Info.hostname;
                    IPAddress address = IPAddress.Parse(server.session.address.ToString());

                    int port = server.session.port;
                    int clients = server.Info.clients;
                    int maxclients = server.Info.sv_maxclients;
                    string map = server.Info.mapname;
                    string gametype = server.Info.gametype;
                    servers.Add(new Server(hostname, address, port, clients, maxclients, map, gametype));
                }

                return servers;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


    }
}
