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

namespace Catch_HK86_byCat
{
    static class Program
    {
        static string rootDir = ConfigurationManager.AppSettings["rootDir"].ToString();//@"C:\alljobs";
        static string mappingfile = ConfigurationManager.AppSettings["mappingfile"].ToString();
        static string logfile = ConfigurationManager.AppSettings["logfile"].ToString();
       
        static void Main(string[] args)
        {
            string url = "http://www.hk861.com/data.php?channel_id={0}&page=1&provinces=0&city=0&district=0";
            string detailurl = "http://www.hk861.com/web.php?id={0}";
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
                    for (int pageindex = page; pageindex < 50; pageindex++)
                    {
                        pathurl = SetPageParameter(url_result, pageindex);

                        string json = "";
                        using (WebClient wc = new WebClient())
                        {
                            json = wc.DownloadString(url_result);
                        }
                        if (string.IsNullOrEmpty(json))
                            continue;

                        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        for (int i = 0; i < data.Count; i++)
                        {
                            string urlpath = string.Format(detailurl, data[i].web_id);
                            Console.WriteLine("urlpath: " + urlpath);
                            detailId = data[i].web_id;
                            DataTable dt = SQLHelper.ExecuteDt("SELECT [temppostid] FROM [grabpost] with(nolock) where sourceRefId=" + detailId);
                            if (dt.Rows.Count > 0)
                                continue;
                           
                            
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

                            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h2[@class='projectTitle']");//A
                            HtmlNode desNode = doc.DocumentNode.SelectSingleNode("//p[@class='introduceBrief']");//B
                            HtmlNode regionNode = doc.DocumentNode.SelectSingleNode("//ul[@class='region']");//E
                            HtmlNode keywrodNode = doc.DocumentNode.SelectSingleNode("//span[@class='tip']");//暫未添加關鍵字 //C
                            HtmlNode CatNode = doc.DocumentNode.SelectSingleNode("//ul[@class='classify']");//D
                            HtmlNode lat_lng=doc.DocumentNode.SelectSingleNode("//div[@class='contactMap']");//K
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
                                keyword = keywrodNode.InnerHtml.Replace("\n", "").Trim();
                                if (keyword.IndexOf("暫未添加關鍵字")!=-1)
                                {
                                    keyword = "";
                                }
                            }
                            if (CatNode != null)
                            {
                                string[] separators = { ",", ".", "|", ">", "．" };
                                Catstring =string.Join(",",CatNode.InnerText.Trim().Replace("\n","").Replace("\t","").Replace(" ","").Split(separators, StringSplitOptions.RemoveEmptyEntries));
                                keyword = keyword + Catstring; //C+D
                            }
                            if (memberavatorNode != null)
                            {
                                memberavator = memberavatorNode.Attributes["src"].Value;
                            }

                            if (lat_lng != null)
                            {
                                string[] lat_lngs = lat_lng.InnerText.Split('\n');
                                foreach (string str in lat_lngs)
                                {
                                    string row = str.Trim().Replace(";","");
                                    if (row.IndexOf("var lat") != -1)
                                    {
                                        string[] separators = { ",", "=" };
                                        string[] lats=row.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                        if (lats.Length >= 4)
                                        {
                                            lat = lats[1];
                                            lng = lats[3];
                                            float.TryParse(lat,out flat);
                                            float.TryParse(lng, out flng);
                                            break;
                                        }

                                        /*
                                       string matching="^\\d*\\.?\\d*$";
                                       MatchCollection matchs = Regex.Matches(row, matching);

                                       if (matchs.Count>2)
                                       {
                                           lat = matchs[0].Value;
                                           lng = matchs[1].Value;
                                       }
                                         * */

                                    }

                                }
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
                                            membermobile = cell.ParentNode.ChildNodes[3].InnerHtml.Replace("\n", "").Trim(); //F
                                        }
                                        if (cell.InnerText.StartsWith("聯繫電話"))
                                        {
                                            mebercontactno = cell.ParentNode.ChildNodes[3].InnerHtml.Replace("\n", "").Trim();//G
                                        }
                                        if (cell.InnerText.StartsWith("聯繫地址"))
                                        {
                                            memberaddress = cell.ParentNode.ChildNodes[3].InnerHtml.Replace("\n", "").Trim(); ;//H
                                            postaddress = memberaddress;
                                        }
                                        if (cell.InnerText.StartsWith("Email"))
                                        {
                                            memberemail = cell.ParentNode.ChildNodes[3].InnerText.Replace("\n", "").Trim();//I
                                            if (memberemail.IndexOf("暫無") != -1)
                                            {
                                                memberemail = "";
                                            }
                                            if (!string.IsNullOrEmpty(memberemail))
                                            {
                                                memberemail = memberemail.Trim().Replace(" ", "");
                                                if (!CheckEmail(memberemail))
                                                {
                                                    
                                                    IList<string> list=new List<string>();
                                                    list.Add(" email invaild:" + urlpath+"");
                                                    File.AppendAllLines(rootDir + logfile, list);
                                                    continue;
                                                }
                                            }
                                        }
                                        if (cell.InnerText.StartsWith("網址"))
                                        {
                                            memberwebsite = cell.ParentNode.ChildNodes[3].InnerText.Replace("\n", "").Trim();//J
                                            if (memberwebsite.IndexOf("暫無") != -1)
                                            {
                                                memberwebsite = "";
                                            }
                                        }


                                        indexcell++;
                                    }
                                    rowIndex++;
                                }
                            }

                            if (membermobile.IndexOf("暫無") != -1)
                            {
                                postcontactno1 = "";
                                membermobile = "";
                            }
                            else
                            {
                                postcontactno1 = membermobile;
                            }
                           

                            if (mebercontactno.IndexOf("暫無") != -1)
                            {
                                postcontactno2 = "";
                                mebercontactno = "";
                            }
                            else
                            {
                                postcontactno2 = mebercontactno;
                            }


                            if (string.IsNullOrEmpty(postcontactno1) && !string.IsNullOrEmpty(postcontactno2))
                            {
                                postcontactno1 = postcontactno2;
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
                                        new SqlParameter("@sourceSite", SqlDbType.NVarChar,100){Value="hk861_2016_dec"},
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
                File.AppendAllText(rootDir + logfile, ex.ToString() + sqlresult);
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
        public static bool CheckEmail(string EmailAddress)
        {


            string strPattern = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";


            if ( System.Text.RegularExpressions.Regex.IsMatch(EmailAddress, strPattern) )
            { return true; }
            return false;
        }
    }



}
