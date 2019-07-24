using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using HtmlAgilityPack;

namespace ImageDownloader
{
    public static class ImageDownload 
    {
        private static readonly char[] InvalidFileNameChars = new[]
                                                                 {
                                                                      '"',
                                                                      '<',
                                                                      '>',
                                                                      '|',
                                                                      '\0',
                                                                      '\u0001',
                                                                      '\u0002',
                                                                      '\u0003',
                                                                      '\u0004',
                                                                      '\u0005',
                                                                      '\u0006',
                                                                      '\a',
                                                                      '\b',
                                                                      '\t',
                                                                      '\n',
                                                                      '\v',
                                                                      '\f',
                                                                      '\r',
                                                                      '\u000e',
                                                                      '\u000f',
                                                                      '\u0010',
                                                                      '\u0011',
                                                                      '\u0012',
                                                                      '\u0013',
                                                                      '\u0014',
                                                                      '\u0015',
                                                                      '\u0016',
                                                                      '\u0017',
                                                                      '\u0018',
                                                                      '\u0019',
                                                                      '\u001a',
                                                                      '\u001b',
                                                                      '\u001c',
                                                                      '\u001d',
                                                                      '\u001e',
                                                                      '\u001f',
                                                                      ':',
                                                                      '*',
                                                                      '?',
                                                                      '\\',
                                                                      '/'
                                                                  };

        /// <summary>
        /// 检查网页url链接是否有效
        /// </summary>
        private static bool CheckUrl(string url)
        {
            try
            {
                WebRequest.Create(url).GetResponse();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }


        /// <summary>
        /// 检查路径是否存在以及如果路径不存在时创建新文件夹是否成功
        /// </summary>
        private static bool CheckDestFolder(string destFolder)
        {
            if (destFolder != "D")
            {
                if (!Directory.Exists(destFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
            }
            
            return true;
        }


        /// <summary>
        /// 获取所有图片的Url
        /// </summary>
        private static void GetImageUrl(string url, List<string> matchStrings)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            //"//img[@src]"是XPath,代表所有具有src属性的img元素
            var imgs = doc.DocumentNode.SelectNodes("//img");

            foreach (var item in imgs)
            {
                //将src属性值存入matchStrings
                matchStrings.Add(item.Attributes["src"].Value);
            }

            for (int i = 0; i < matchStrings.Count; i++)
            {
                if (matchStrings[i].StartsWith("//"))
                {
                    matchStrings[i] = matchStrings[i].Insert(0, "https:");
                }
            }
        }


        /// <summary>
        /// 获取有效的本地文件名fileName
        /// </summary>
        public static void GetValidFileName(List<string> fileNames, List<string> matchStrings)
        {
            foreach (var matchString in matchStrings)
            {
                fileNames.Add(Path.GetFileName(matchString));
            }

            for (int i = 0;i < fileNames.Count; i++ )
            {
                fileNames[i] = fileNames[i] + "";
                fileNames[i] = InvalidFileNameChars.Aggregate(fileNames[i], (current, c) => current.Replace(c + "", ""));

                if (fileNames[i].Length > 1)
                    if (fileNames[i][0] == '.')
                        fileNames[i] = "dot" + fileNames[i].TrimStart('.');

                if (Path.GetExtension(fileNames[i]) != ".png" && Path.GetExtension(fileNames[i]) != ".jpg" &&
                    Path.GetExtension(fileNames[i]) != ".gif")
                {
                    fileNames.RemoveAt(i);
                    matchStrings.RemoveAt(i);
                }
            }

            
        }


        /// <summary>
        /// 下载图片
        /// </summary>
        private static int DownloadFromNet(List<string> fileNames, List<string> matchStrings, string destFolder)
        {
            int count = 0;

            using (WebClient client = new WebClient())
            {
                for (int i = 0; i < matchStrings.Count; i++)
                {
                    bool flag = true;
                    
                    Console.Write(matchStrings[i]);
                    
                    try
                    {
                        client.DownloadFile(matchStrings[i], fileNames[i]);
                    }
                    catch(Exception e)
                    {
                        Console.Write(e.Message);
                        flag = false;
                    }

                    if (flag)
                    {
                        count++;
                        MoveImage(destFolder);
                        Console.WriteLine(" 文件：" + fileNames[i] + " 下载成功！" + " 计数：" + count);
                    }
                }
            }

            

            return count;
        }


        /// <summary>
        /// 将下载在Debug目录下的图片转移到指定文件夹
        /// </summary>
        private static void MoveImage(string destFolder)
        { 
            //使用相对路径，而不是绝对路径，.\表示当前路径，即Debug文件夹
            string SrcFolder = @".\";
            //如果输入D，则定位到桌面文件夹，用GetFolderPath()方法获取桌面的路径
            if (destFolder == "D")
                destFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            DirectoryInfo directoryInfo = new DirectoryInfo(SrcFolder);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                if (file.Extension == ".png" || file.Extension == ".jpg" || file.Extension == ".gif")
                {
                    try
                    {
                        file.MoveTo(Path.Combine(destFolder, file.Name));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }


        /// <summary>
        /// 实现整个下载
        /// </summary>
        public static void Download(string url,string destFolder)
        {
            if (CheckUrl(url)&&CheckDestFolder(destFolder))
            {
                List<string> matchStrings = new List<string>();
                List<string> fileNames = new List<string>();
                int count;

                Console.WriteLine("\n下载中...\n");

                GetImageUrl(url,matchStrings);

                #region 关闭此注释查看url链接
                //for (int i = 0; i < matchStrings.Count; i++)
                //{
                //    Console.WriteLine(matchStrings[i]);
                //}
                //Console.WriteLine();

                #endregion

                GetValidFileName(fileNames,matchStrings);

                #region 关闭此注释查看文件名
                //for (int i = 0; i < fileNames.Count; i++)
                //{
                //    Console.WriteLine(fileNames[i]);
                //}
                //Console.WriteLine();

                #endregion

                count = DownloadFromNet(fileNames,matchStrings,destFolder);

                Console.WriteLine("\n" + count + "张图片已经成功下载！");
            }

        }
    }
}

