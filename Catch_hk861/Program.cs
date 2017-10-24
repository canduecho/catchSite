
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Catch_hk861
{
    static class Program
    {
        static string rootDir = ConfigurationManager.AppSettings["rootDir"].ToString();//@"C:\alljobs";
        static string urlfile = ConfigurationManager.AppSettings["urlfile"].ToString();
        static string logfile=ConfigurationManager.AppSettings["logfile"].ToString();
        
        static void Main(string[] args)
        {
            string url= "http://www.hk861.com/web.php?id={0}";
            try
            {
                string urlfilepath = rootDir + urlfile;
                IEnumerable<string> ls = File.ReadLines(urlfilepath);
                foreach (string path in ls.ToList())
                {
                    //HtmlWeb website = new HtmlWeb();
                    //HtmlAgilityPack.HtmlDocument doc = website.Load(path);
                    //dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(doc.);
                    int page = getPageParameter(path);
                    string pathurl = path;
                    for (int pageindex = page; pageindex < 150; pageindex++)
                    {
                        pathurl = SetPageParameter(path, pageindex);

                        string json = "";
                        using (WebClient wc = new WebClient())
                        {
                            json = wc.DownloadString(path);
                        }
                        if (string.IsNullOrEmpty(json))
                            continue;

                        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        for (int i = 0; i < data.Count; i++)
                        {
                            string urlpath = string.Format(url, data[i].web_id);
                            HtmlWeb website = new HtmlWeb();
                            HtmlAgilityPack.HtmlDocument doc = website.Load(urlpath);
                            long categoryid = data[i].cat_id;



                            string membermobile = "";
                            string mebercontactno = "";
                            string memberaddress = "";
                            string memberemail = "";
                            string memberwebsite = "";
                            string memberavator = "";
                            string membername = "";
                            string district = "";
                            string keyword = "";
                            string postcontent = "";
                            string summary = "";
                            string title = "";

                            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h2[@class='projectTitle']");
                            HtmlNode desNode = doc.DocumentNode.SelectSingleNode("//p[@class='introduceBrief']");
                            HtmlNode regionNode = doc.DocumentNode.SelectSingleNode("//ul[@class='region']");//substring(">")
                            HtmlNode keywrodNode = doc.DocumentNode.SelectSingleNode("//span[@class='tip']");//暫未添加關鍵字
                            HtmlNode memberavatorNode = doc.DocumentNode.SelectSingleNode("//div[@class='companyLogo']/img");
                            if (titleNode == null)
                            {
                                continue;
                            }
                            if (titleNode != null)
                            {
                                title = titleNode.InnerHtml.Replace("\n", "").Trim();
                                membername = title;
                            }
                            if (desNode != null)
                            {
                                postcontent = desNode.InnerHtml;
                                summary = desNode.InnerHtml;
                            }
                            if (regionNode != null)
                            {
                                district = regionNode.InnerHtml;
                                if (regionNode.InnerHtml.IndexOf(">") != 1)
                                {
                                    district = regionNode.InnerHtml.Remove(0, regionNode.InnerHtml.LastIndexOf(">") + 1).Replace("\n", "").Trim();
                                }

                            }
                            if (keywrodNode != null)
                            {
                                keyword = keywrodNode.InnerHtml.Replace("\n", "").Trim(); ;
                            }
                            if (memberavatorNode != null)
                            {
                                memberavator = memberavatorNode.Attributes["src"].Value;
                            }


                            //string des = doc.DocumentNode.SelectSingleNode("//span[@class='tip']").InnerHtml;

                            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table[@class='contactTab']"))
                            {
                                Console.WriteLine("Found: " + table.Id);
                                int rowIndex = 0;
                                foreach (HtmlNode row in table.SelectNodes("tr"))
                                {


                                    int indexcell = 0;
                                    foreach (HtmlNode cell in row.SelectNodes("th|td"))
                                    {
                                        if (cell.InnerText.StartsWith("手機號碼"))
                                        {
                                            membermobile = cell.ParentNode.ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                                        }
                                        if (cell.InnerText.StartsWith("聯繫電話"))
                                        {
                                            mebercontactno = cell.ParentNode.ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                                        }
                                        if (cell.InnerText.StartsWith("聯繫地址"))
                                        {
                                            memberaddress = cell.ParentNode.ChildNodes[3].InnerHtml.Replace("\n", "").Trim(); ;
                                        }
                                        if (cell.InnerText.StartsWith("Email"))
                                        {
                                            memberemail = cell.ParentNode.ChildNodes[3].InnerText.Replace("\n", "").Trim();
                                        }
                                        if (cell.InnerText.StartsWith("網址"))
                                        {
                                            memberwebsite = cell.ParentNode.ChildNodes[3].InnerText.Replace("\n", "").Trim();
                                        }


                                        indexcell++;
                                    }
                                    rowIndex++;
                                }
                            }


                            SQLHelper sqlHelper = new SQLHelper();
                            SqlParameter[] parameters = {
                                  new SqlParameter("@categoryid", SqlDbType.BigInt){Value=categoryid},                              
                                  new SqlParameter("@title", SqlDbType.NVarChar,512){Value=title},
                                  new SqlParameter("@summary", SqlDbType.NVarChar,512){Value=summary },
                                  new SqlParameter("@postcontent", SqlDbType.NVarChar,-1){Value=postcontent },

                                  new SqlParameter("@keyword", SqlDbType.NVarChar,512){Value=keyword },

                                  new SqlParameter("@district", SqlDbType.NVarChar,64){Value=district },

                                  new SqlParameter("@membername", SqlDbType.NVarChar,256){Value=membername },

                                  new SqlParameter("@memberemail", SqlDbType.VarChar,100){Value=memberemail  },


                                  new SqlParameter("@memberavator", SqlDbType.VarChar,512){Value=memberavator},

                                  new SqlParameter("@membermobile", SqlDbType.VarChar,30){Value=membermobile},

                                  new SqlParameter("@mebercontactno", SqlDbType.VarChar,30){Value=mebercontactno },

                                  new SqlParameter("@memberaddress", SqlDbType.NVarChar,512){Value=memberaddress},

                                  new SqlParameter("@memberwebsite", SqlDbType.NVarChar,512){Value=memberwebsite},


                            };
                            sqlHelper.RunProc("[dbo].insertgrabpost", parameters);



                        }
                    }

                }

            }
            catch (Exception ex)
            {
                File.WriteAllText(rootDir + logfile, ex.ToString());
            }
            
            
        }

        public static string SetPageParameter(this string url, int pageNumber)
        {
            var queryStartIndex = url.IndexOf("?") + 1;
            if (queryStartIndex == 0)
            {
                return string.Format("{0}?page={1}", url, pageNumber);
            }
            var oldQueryString = url.Substring(queryStartIndex);
            var queryParameters = HttpUtility.ParseQueryString(oldQueryString);
            queryParameters["page"] = pageNumber.ToString();
            return url.Substring(0, queryStartIndex) + queryParameters.ToString();
        }
        public static int getPageParameter(this string url)
        {
            var queryStartIndex = url.IndexOf("?") + 1;
           
            var oldQueryString = url.Substring(queryStartIndex);
            var queryParameters = HttpUtility.ParseQueryString(oldQueryString);
            return int.Parse(queryParameters["page"]);
        }
     }


    
}
