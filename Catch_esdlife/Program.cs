using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Catch_esdlife
{
    static class Program
    {
        static string rootDir = ConfigurationManager.AppSettings["rootDir"].ToString();//@"C:\alljobs";
        static string mappingfile = ConfigurationManager.AppSettings["mappingfile"].ToString();
        static string logfile = ConfigurationManager.AppSettings["logfile"].ToString();
        static string sourceSite = ConfigurationManager.AppSettings["sourceSite"].ToString();
        static void Main(string[] args)
        {
            string url = "http://businessdirectory.esdlife.com/wedding/CH-TC/Wedding2CompInfo.php?pages=1&catID={0}";
            string detailurl = "http://businessdirectory.esdlife.com/wedding/CH-TC/{0}";
            string sqlresult = "";

           

            try
            {
                string urlfilepath = rootDir + mappingfile;
                IEnumerable<string> ls = File.ReadLines(urlfilepath);
                string detailId = "";
             

                foreach (string cats in ls.ToList())
                {

                    //HtmlWeb website = new HtmlWeb();
                    //HtmlAgilityPack.HtmlDocument doc = website.Load(path);
                    //dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(doc.);
                    if (string.IsNullOrEmpty(cats))
                        continue;
                    string[] catgroup = cats.Split('\t');
                    string cat = catgroup[catgroup.Length - 1];
                    string cat_88= catgroup[1];
                    if (string.IsNullOrEmpty(cat_88))
                    {
                        cat_88 = cat;
                    }
                    string url_result = string.Format(url, cat);
                    int page = getPageParameter(url_result);
                    string pathurl = url;
                    for (int pageindex = page; pageindex < 30; pageindex++)
                    {
                        pathurl = SetPageParameter(url_result, pageindex);
                        HtmlWeb websiteList = new HtmlWeb();
                        HtmlAgilityPack.HtmlDocument maindoc = websiteList.Load(pathurl);
                        HtmlNodeCollection nodes= maindoc.DocumentNode.SelectNodes("//div[@class='cn']/h3/a[@class='orangetxt']");

                        for (int i = 0; i < nodes.Count; i++)
                        {
                            string urlpath = string.Format(detailurl, nodes[i].Attributes["href"].Value);
                            Console.WriteLine("urlpath: " + urlpath);
                            detailId = nodes[i].Attributes["href"].Value.Replace("Wedding2ShowCompInfo.php?id=","");
                            DataTable dt = SQLHelper.ExecuteDt("SELECT [temppostid] FROM [grabpost] with(nolock) where sourceRefId=" + detailId + " and sourceSite='" + sourceSite + "'");
                            if (dt.Rows.Count > 0)
                                continue;
                           
                            
                            HtmlWeb website = new HtmlWeb();
                            HtmlAgilityPack.HtmlDocument doc = website.Load(urlpath);
                            long categoryid = long.Parse(cat);



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
                            string lat="";
                            string lng="";
                            float flat = 0;
                            float flng = 0;
                            string Catstring="";
                            string postcontactno1 = "";
                            string postcontactno2 = "";
                            string postaddress = "";
                            string contactemail = "";
                            //string keyword="";

                            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h1[@class='d_title']");//A
                            HtmlNode mainNode = doc.DocumentNode.SelectSingleNode("//div[@class='text']");//B,C,D,E,F,I
                            HtmlNode regionNode = doc.DocumentNode.SelectSingleNode("//div[@id='guide_search']");//E
                            HtmlNode keywrodNode = doc.DocumentNode.SelectSingleNode("//div[@class='ser']/a");//J
                            HtmlNode contentNode = doc.DocumentNode.SelectSingleNode("//div[@class='d_content']");//G  簡介:

                            HtmlNode lat_lng = doc.DocumentNode.SelectSingleNode("//div[@id='wedding_content']").PreviousSibling.PreviousSibling;//H
                            //HtmlNode memberavatorNode = doc.DocumentNode.SelectSingleNode("//div[@class='companyLogo']/img");

                             

                            if (titleNode == null)
                            {
                                continue;
                            }
                            if (titleNode != null)
                            {
                                title = titleNode.InnerHtml.Replace("\n", "").Trim();
                                membername = title;
                            }
                            if (mainNode != null)
                            {
                                foreach (HtmlNode node in mainNode.ChildNodes)
                                {
                                    if(node.InnerText.IndexOf("地址:")!=-1)
                                    {
                                        postaddress = node.InnerText.Replace("\n", "").Replace("\t", "").Replace(" ", "");
                                        memberaddress = postaddress;
                                    }

                                    if(node.InnerText.IndexOf("電話:")!=-1)
                                    {
                                        string[] tels = node.InnerText.Replace("電話:", "").Replace("\n", "").Replace("\t", "").Replace(" ", "").Split('/');
                                        if (tels.Length > 1)
                                        {
                                            postcontactno1 = tels[0];
                                            postcontactno2 = tels[1];
                                        }
                                        else
                                        {
                                            postcontactno1 = tels[0];
                                            postcontactno2 = tels[0];
                                        }
                                    }

                                    if (node.InnerText.IndexOf("電郵:") != -1)
                                    {
                                        string[] emails = node.NextSibling.InnerText.Split('/');

                                        memberemail = emails[0].Trim();
                                        if (emails.Length > 1)
                                        {
                                            contactemail = emails[1].Trim();
                                        }
                                        
                                    }
                                    if (node.InnerText.IndexOf("網址:") != -1)
                                    {
                                        memberwebsite = node.NextSibling.InnerText;
                                    }
                                    if (node.InnerText.IndexOf("備註:") != -1)
                                    {
                                        summary = node.InnerText.Replace("備註:", "").Trim();
                                    }
                                   
                                }
                                //postcontent = desNode.InnerHtml;
                                //summary = desNode.InnerHtml;
                            }
                           
                            
                         
                            if (keywrodNode != null)
                            {
                                keyword = keywrodNode.InnerHtml.Replace("\n", "").Trim();
                                string[] separators = { ",", "/", "|", ">", "．" };
                                keyword = string.Join(",", keyword.Replace("\n", "").Replace("\t", "").Replace(" ", "").Split(separators, StringSplitOptions.RemoveEmptyEntries));
                                //keyword =  Catstring;
                            }                           
                           
                            if (lat_lng != null)
                            {
                                string[] lat_lngs = lat_lng.InnerText.Split('\n');
                                foreach (string str in lat_lngs)
                                {
                                    string row = str.Trim().Replace(";","");
                                    if (row.IndexOf("var MerchantLatitude") != -1)
                                    {
                                        string[] separators = { "=" };
                                        string[] lats = row.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                        if (lats.Length >= 2)
                                        {
                                            lat = lats[1].Replace("'","");
                                            float.TryParse(lat, out flat);
                                            continue;
                                        }

                                       

                                    }
                                    if (row.IndexOf("var MerchantLongitude") != -1)
                                    {
                                        string[] separators = { "=" };
                                        string[] lngs = row.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                        if (lngs.Length >= 2)
                                        {
                                            lng = lngs[1].Replace("'", "");                                            
                                            float.TryParse(lng, out flng);
                                            break;
                                        }
                                       
                                    }

                                }
                            }

                            if (contentNode != null)
                            {
                                postcontent = contentNode.InnerText.Replace("簡介:", "").Trim();
                            }

                            if (regionNode != null)
                            {
                                //regionNode.LastChild.
                            }

                           



                            /*
                                                        exec [dbo].insertgrabpost
                              @categoryid = 1197,    --->cat_88
                              @title =N'物流tile2333', --> title
                              @summary ='summary',--->desNode
                              @postcontent = 'content', --->postcontent
                              @keyword ='keyword', --->keyword
                              @district =N'香港島', --->district
                              @membername =N'abc', --->membername
                              @memberemail ='abc@111.com', ---->memberemail
                              @memberavator ='http://avator', ---->memberavator
                              @membermobile ='+86-187238273', --->membermobile
                              @membercontactno ='932742',   ---->mebercontactno
                              @memberaddress =N'adlfjeosdfdsfsd',   --->memberaddress
                              @memberwebsite ='http://www.88db.com',  ---->memberwebsite
                              @postcontactno1 ='22222',   ---->postcontactno1
                              @postcontactno2 =N'33333你好',--->postcontactno2
                              @postaddress =N'广告地址', --->postaddress
                              @postlatitude = '12323',   //lat
                              @postlongitude ='23232',   //lng
                              @contactemail =N'33333你好',  --->contactemail
                              @memberdesc =N'成员描述111111' --->desNode
                            */
                            SQLHelper sqlHelper = new SQLHelper();
                            SqlParameter[] parameters = {
                                  new SqlParameter("@categoryid", SqlDbType.BigInt){Value=cat_88},                              
                                  new SqlParameter("@title", SqlDbType.NVarChar,512){Value=title},
                                  new SqlParameter("@summary", SqlDbType.NVarChar,512){Value=summary },
                                  new SqlParameter("@postcontent", SqlDbType.NVarChar,-1){Value=postcontent },

                                  new SqlParameter("@keyword", SqlDbType.NVarChar,512){Value=keyword },

                                  new SqlParameter("@district", SqlDbType.NVarChar,64){Value=district },

                                  new SqlParameter("@membername", SqlDbType.NVarChar,256){Value=membername },

                                  new SqlParameter("@memberemail", SqlDbType.VarChar,100){Value=memberemail  },

                                  new SqlParameter("@memberavator", SqlDbType.VarChar,512){Value=memberavator},

                                  new SqlParameter("@membermobile", SqlDbType.VarChar,30){Value=membermobile},

                                  new SqlParameter("@membercontactno", SqlDbType.VarChar,30){Value="" },

                                  new SqlParameter("@memberaddress", SqlDbType.NVarChar,512){Value=memberaddress},

                                  new SqlParameter("@memberwebsite", SqlDbType.NVarChar,512){Value=memberwebsite},
                                  //new
                                  new SqlParameter("@postcontactno1", SqlDbType.NVarChar,30){Value=postcontactno1},
                                  new SqlParameter("@postcontactno2", SqlDbType.NVarChar,30){Value=postcontactno2},
                                  new SqlParameter("@postaddress", SqlDbType.NVarChar,512){Value=postaddress},
                                   new SqlParameter("@postlatitude", SqlDbType.Float){Value=flat},
                                    new SqlParameter("@postlongitude", SqlDbType.Float){Value=flng},
                                     new SqlParameter("@contactemail", SqlDbType.NVarChar,100){Value=contactemail},
                                       new SqlParameter("@memberdesc", SqlDbType.NVarChar,1000){Value=summary},
                                        new SqlParameter("@sourceSite", SqlDbType.NVarChar,100){Value=sourceSite},
                                         new SqlParameter("@sourceRefID", SqlDbType.NVarChar,100){Value=detailId}
                            };
                            StringBuilder paramstr = new StringBuilder();
                            paramstr.Append(" exec [dbo].insertgrabpost{");
                            //paramstr=parameters.t
                            foreach (SqlParameter p in parameters)
                            {
                                paramstr.Append(p.ParameterName + "=N'" + p.Value+"',").AppendLine(); //paramstr + p.ParameterName + "=" + p.Value+"||||";
                            }
                            paramstr.Append("}");
                            sqlresult = paramstr.ToString();
                            sqlHelper.RunProc("[dbo].insertgrabpost", parameters);



                        }
                    }

                }

            }
            catch (Exception ex)
            {
                File.AppendAllText(rootDir + logfile, ex.StackTrace + sqlresult);
            }


        }

        public static string SetPageParameter(this string url, int pageNumber)
        {
            var queryStartIndex = url.IndexOf("?") + 1;
            if (queryStartIndex == 0)
            {
                return string.Format("{0}?pages={1}", url, pageNumber);
            }
            var oldQueryString = url.Substring(queryStartIndex);
            var queryParameters = HttpUtility.ParseQueryString(oldQueryString);
            queryParameters["pages"] = pageNumber.ToString();
            return url.Substring(0, queryStartIndex) + queryParameters.ToString();
        }
        public static int getPageParameter(this string url)
        {
            var queryStartIndex = url.IndexOf("?") + 1;

            var oldQueryString = url.Substring(queryStartIndex);
            var queryParameters = HttpUtility.ParseQueryString(oldQueryString);
            return int.Parse(queryParameters["pages"]);
        }
        public static bool CheckEmail(string EmailAddress)
        {


            string strPattern = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";


            if ( System.Text.RegularExpressions.Regex.IsMatch(EmailAddress, strPattern) )
            { return true; }
            return false;
        }
    }



}
