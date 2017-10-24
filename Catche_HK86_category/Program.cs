using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Catche_HK86_category
{
    class Program
    {
        static string rootDir = ConfigurationManager.AppSettings["rootDir"].ToString();
        static string logfile = ConfigurationManager.AppSettings["logfile"].ToString();
        static void Main(string[] args)
        {
           
            try
            {
                string parentUrl = "http://www.hk861.com/index.php";
                HtmlWeb website = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = website.Load(parentUrl);
                StringBuilder sb = new StringBuilder();
                sb.Append("CATID,CATNAME,Link").AppendLine();
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//div[@class='lftBox']//li//a"))
                {
                    String cid = link.Attributes["tourl"].Value.Replace("channel.php?cid=","");
                    String catName = link.InnerText;
                    string suburl = "http://www.hk861.com/"+link.Attributes["tourl"].Value;
                    HtmlAgilityPack.HtmlDocument subdoc = website.Load(suburl);
                    //sb.Append(cid + "," + catName + "," + suburl).AppendLine();
                    sb.AppendLine();
                    HtmlNodeCollection cols = subdoc.DocumentNode.SelectNodes("//div[@class='cellList js_cellList']//ul//li//a");
                    if (cols == null)
                        continue;
                    foreach (HtmlNode sublink in cols)
                    {
                        String sub_cid = sublink.Attributes["tourl"].Value.Replace("channel.php?cid=", "");
                        String sub_catName = sublink.InnerText;
                        string subsuburl = "http://www.hk861.com/" + sublink.Attributes["tourl"].Value;
                        //sb.Append("->," + sub_cid + "," + sub_catName + "," + subsuburl).AppendLine();
                        sb.Append( sub_cid + "," + sub_catName + "," + subsuburl).AppendLine();
                       
                        HtmlAgilityPack.HtmlDocument subsubdoc = website.Load(subsuburl);
                        //sb.Append(cid + "," + catName + "," + subsuburl).AppendLine();

                        HtmlNodeCollection subsubcols = subsubdoc.DocumentNode.SelectNodes("//div[@class='cellList js_cellList']//ul//li//a");
                        if (subsubcols != null)
                        {
                            foreach (HtmlNode subsublink in subsubcols)
                            {
                                String sub_sub_cid = subsublink.Attributes["tourl"].Value.Replace("channel.php?cid=", "");
                                String sub_sub_catName = subsublink.InnerText;
                                string subsubsuburl = "http://www.hk861.com/" + subsublink.Attributes["tourl"].Value;
                                //sb.Append("->,->," + sub_sub_cid + "," + sub_sub_catName + "," + subsubsuburl).AppendLine();

                                
                            }
                        }
                    }
                    
                }
                String files = string.Format(rootDir+"CatData_{0}.csv", DateTime.Now.Date.ToString("dd-MM-yyyy"));
                System.IO.File.WriteAllText(files, sb.ToString());

            }
            catch (Exception ex)
            {
                File.WriteAllText(rootDir + logfile, ex.ToString());
            }          


        }
    }
}
