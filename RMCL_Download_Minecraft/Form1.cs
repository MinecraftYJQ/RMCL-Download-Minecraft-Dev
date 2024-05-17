using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMCL_Download_Minecraft
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public class Root
        {
            public Latest latest { get; set; }
            public List<Version> versions { get; set; }

        }
        public class Artifact
        {
            public string Path { get; set; }
            public string Url { get; set; }
        }

        public class RootObject
        {
            public Library[] Libraries { get; set; }
        }

        public class Library
        {
            public Download Downloads { get; set; }
        }

        public class Download
        {
            public Artifact Artifact { get; set; }
        }
        public class Latest
        {
            public string release { get; set; }
            public string snapshot { get; set; }
        }

        public class Version
        {
            public string id { get; set; }
            public string type { get; set; }
            public string url { get; set; }
            public string time { get; set; }
            public string releaseTime { get; set; }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            refresh();
        }
        private void download(string name)
        {
            Command($"md .minecraft");
            Command($"md .minecraft\\libraries");
            Command($"md .minecraft\\versions");
            Command($"md .minecraft\\versions\\{name}");
            Command($"md .minecraft\\assets");
            Command($"md .minecraft\\assets\\objects");
            Command($"md .minecraft\\assets\\indexes");
            for (int i = 0; i <= releaseIds.LongCount(); i++)
            {
                if (releaseIds[i] == name)
                {
                    string json = GetResponse(releaseUrls[i]);
                    Thread.Sleep(1);
                    //获取资源索引
                    var rootObject = JObject.Parse(json);
                    var assetIndexObject = rootObject["assetIndex"];
                    string url=null;
                    if (assetIndexObject != null && assetIndexObject.Type == JTokenType.Object)
                    {
                        var assetIndexDict = assetIndexObject.ToObject<Dictionary<string, object>>();
                        url = assetIndexDict.ContainsKey("url") ? (string)assetIndexDict["url"] : null;
                    }

                    Task.Run(() =>
                    {
                        dow_res(url);
                    });

                    //下载游戏jar
                    dynamic data = JsonConvert.DeserializeObject(json);
                    string clientUrl = data.downloads.client.url;

                    Command($".minecraft\\versions\\{name}");
                    Thread.Sleep(1);
                    File.WriteAllText($".minecraft\\versions\\{name}\\{name}.json", json);
                    File.WriteAllText($".minecraft\\versions\\{name}\\{name}.jar", GetResponse(clientUrl));

                    RootObject root = JObject.Parse(json).ToObject<RootObject>();

                    foreach (Library library in root.Libraries)
                    {
                        string path = library.Downloads.Artifact.Path;
                        string urls = library.Downloads.Artifact.Url;

                        /*Console.WriteLine("Path: " + path);
                        Console.WriteLine("URL: " + urls);*/
                        string directoryPath = Path.GetDirectoryName(".minecraft\\libraries\\"+path);

                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                            //Console.WriteLine("文件夹已创建：" + directoryPath);
                        }
                        try
                        {
                            File.ReadAllText(".minecraft\\libraries\\" + path);
                        }
                        catch (Exception ex)
                        {
                            File.WriteAllText(".minecraft\\libraries\\" + path, GetResponse(urls));
                        }
                        Console.WriteLine($"[OK]:{path}");
                    }
                    break;
                }
            }
        }
        int xc = 999999999, jxc = 0;
        private void dow_res(string url)
        {
            //Console.WriteLine(url);
            string get_json = GetResponse(url);
            Console.WriteLine(url.Substring(84, url.Length - 84));
            File.WriteAllText(".minecraft\\assets\\indexes\\"+url.Substring(84, url.Length - 84), get_json);
            //Console.WriteLine($"{get_json}");
            int s = 0, js = 0;
            for (int j = 1; j <= get_json.Length - 50; j++)
            {
                if (get_json.Substring(j, 4) == "hash")
                {
                    s++;
                }
            }
            for (int j = 1; j <= get_json.Length - 50; j++)
            {
                if (get_json.Substring(j, 4) == "hash")
                {
                    //Command($"md .minecraft\\objects\\{get_json.Substring(j + 8, 40).Substring(0, 2)}");
                    if (!Directory.Exists($".minecraft\\assets\\objects\\{get_json.Substring(j + 8, 40).Substring(0, 2)}"))
                    {
                        // 如果不存在，创建文件夹
                        Directory.CreateDirectory($".minecraft\\assets\\objects\\{get_json.Substring(j + 8, 40).Substring(0, 2)}");
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            //Console.WriteLine("https://resources.download.minecraft.net/" + get_json.Substring(j + 8, 40).Substring(0, 2) + "/" + get_json.Substring(j + 8, 40));
                            //Console.WriteLine($".minecraft\\objects\\{get_json.Substring(j + 8, 40).Substring(0, 2)}\\{get_json.Substring(j + 8, 40)}", GetResponse("https://resources.download.minecraft.net/" + get_json.Substring(j + 8, 40).Substring(0, 2) + "/" + get_json.Substring(j + 8, 40)));
                            try
                            {
                                File.ReadAllText($".minecraft\\assets\\objects\\{get_json.Substring(j + 8, 40).Substring(0, 2)}\\{get_json.Substring(j + 8, 40)}");
                            }
                            catch
                            {
                                try
                                {
                                    File.WriteAllText($".minecraft\\assets\\objects\\{get_json.Substring(j + 8, 40).Substring(0, 2)}\\{get_json.Substring(j + 8, 40)}", GetResponse("https://resources.download.minecraft.net/" + get_json.Substring(j + 8, 40).Substring(0, 2) + "/" + get_json.Substring(j + 8, 40)));
                                    jxc++;
                                    //Thread.Sleep(1);
                                    if (jxc == xc)
                                    {
                                        while (true)
                                        {
                                            if (jxc < xc)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    Console.WriteLine($"[{js}/{s}]:下载成功！");
                                }
                                catch {
                                    jxc++;
                                    //Thread.Sleep(1);
                                    if (jxc == xc)
                                    {
                                        while (true)
                                        {
                                            if (jxc < xc)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    Console.WriteLine($"[{js}/{s}]下载失败！请前往https://resources.download.minecraft.net/{get_json.Substring(j + 8, 40).Substring(0, 2) + "/" + get_json.Substring(j + 8, 40)} 下载到.minecraft\\assets\\objects\\{get_json.Substring(j + 8, 40).Substring(0, 2)}\\");
                                }
                            }
                            js++;
                            jxc--;
                            
                        }
                        catch (Exception e) { }
                    });
                    
                    //Console.WriteLine("https://resources.download.minecraft.net/"+get_json.Substring(j+8, 40).Substring(0,2)+"/"+ get_json.Substring(j + 8, 40));
                }
            }
        }
        private void Command(string str)
        {
            //Console.WriteLine("Command启动");
            //hxexe();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
        }
        private void button_MouseClick(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            string path = button.Text;
            download(path);
        }
        int urs = 1;
        string[] url = new string[32767];
        public string GetResponse(string Url)
        {
            string ResponseData = string.Empty;
            try
            {
                //Message_fs message = new Message_fs("RMCL启动器", "正在加载下载版本...");
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(Url);
                httpWebRequest.Timeout = 5000;
                httpWebRequest.Method = "GET";

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                ResponseData = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
            }
            catch (Exception)
            {
                ResponseData = null;
            }
            return ResponseData;
        }
        // 提取类型为"release"的url和id
        List<string> releaseUrls = new List<string>();
        List<string> releaseIds = new List<string>();
        private void refresh()
        {
            try
            {

                string jg = GetResponse("https://launchermeta.mojang.com/mc/game/version_manifest.json");

                var data = JsonConvert.DeserializeObject<Root>(jg);

                

                foreach (var version in data.versions)
                {
                    if (version.type == "release")
                    {
                        releaseUrls.Add(version.url);
                        releaseIds.Add(version.id);
                    }
                }

                // 打印结果
                //Console.WriteLine("类型为\"release\"的url：");
                foreach (var url in releaseUrls)
                {
                    //Console.WriteLine(url);
                }

                //Console.WriteLine("\n类型为\"release\"的id：");
                foreach (var id in releaseIds)
                {
                    //Console.WriteLine(id);
                }

                //string jg = File.ReadAllText("RMCL\\Mojang.txt").ToString();
                //Console.WriteLine(jg);
                //Console.WriteLine(jg);
                //Console.WriteLine(jg.Length);
                int y1 = 10;
                if (jg != null)
                {

                    Controls.Clear();
                    for (int i = 50; i < jg.Length - 3; i++)
                    {
                        if (jg[i] == 'r' && jg[i + 1] == 'e' && jg[i + 2] == 'l' && jg[i + 7] != 'T')
                        {
                            int temp = 0, temp1 = 0;
                            for (int j = i - 40; j <= i - 1; j++)
                            {
                                temp++;
                                if (jg[j] == '\"')
                                {
                                    temp1++;
                                    if (temp1 == 4)
                                    {
                                        Button button = new Button();
                                        Controls.Add(button);
                                        button.Location = new System.Drawing.Point(10, y1);
                                        y1 += 56;
                                        button.MouseClick += new MouseEventHandler(button_MouseClick);
                                        button.Width = Width;
                                        //button.Width = panel1.Width - 20;
                                        button.Height = 46;
                                        button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top)));
                                        //button.Text += "                                                                                                                                    \n此版本暂无介绍";
                                        //button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
                                        button.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));

                                        for (int x = j + 1; x <= j + temp; x++)
                                        {
                                            if (jg[x] == '\"')
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                //Console.Write(jg[x]);
                                                url[urs] += jg[x];
                                                button.Name += jg[x];
                                                button.Text += jg[x];

                                            }
                                        }
                                        for (int x = j - 19; x <= j; x++)
                                        {
                                            if (jg[x] == '\"')
                                            {
                                                for (int y = j + 35; y <= j; y++)
                                                {
                                                    //Console.Write(jg[y]);
                                                }
                                                //Console.WriteLine();
                                            }
                                        }
                                        urs++;


                                        //Console.WriteLine();
                                        //Thread.Sleep(10);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Controls.Clear();
                }


            }
            catch (Exception ex)
            {
                Controls.Clear();
            }
        }
    }
}
