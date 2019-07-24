using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Net.Mime;


namespace ImageDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("输入网址");
            string url = Console.ReadLine();
            Console.WriteLine("\n请输入保存路径\n输入目标文件夹的完整路径(不存在则会自动创建，注意要完整路径)");
            Console.WriteLine("输入D会直接保存到桌面");
            Console.WriteLine("输入除D以外的字符会在Debug目录下创建一个新文件夹并存放图片");
            string destFolder = Console.ReadLine();

            ImageDownload.Download(url,destFolder);

            Console.ReadKey();
        }
    }
}
