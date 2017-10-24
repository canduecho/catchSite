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

namespace Catch_591ComHk
{
    static class Program
    {
        static string rootDir = ConfigurationManager.AppSettings["rootDir"].ToString();//@"C:\alljobs";
        static string mappingfile = ConfigurationManager.AppSettings["mappingfile"].ToString();
        static string logfile = ConfigurationManager.AppSettings["logfile"].ToString();
        static string sourceSite = ConfigurationManager.AppSettings["sourceSite"].ToString();
        static string fieldmapping = ConfigurationManager.AppSettings["fieldmapping"];
        static string PropPostType = ConfigurationManager.AppSettings["PropPostType"];
        static void Main(string[] args)
        {
            string url = "https://{0}/?m=home&c=search&a=rslist&v=new&type=1&purpose=1&searchtype=1&p=1";
            string detailurl = "http://asiaweddingnetwork.com/en/vendor-detail/about-contact?vendor_id={0}";
            string sqlresult = "";
            string detailId = "";


            try
            {
                Dictionary<string, Dictionary<string, string>> fieldMapping = new Dictionary<string, Dictionary<string, string>>();
                Dictionary<string, Dictionary<string, string>> otherfieldMapping = new Dictionary<string, Dictionary<string, string>>();
                string fieldmappingPath = rootDir + fieldmapping;
                IEnumerable<string> fieldLs = File.ReadLines(fieldmappingPath);

                foreach (string field in fieldLs.ToList())
                {
                    string[] fields=field.Split(',');
                    if (!fieldMapping.ContainsKey(fields[0]))
                    {
                        Dictionary<string, string> fieldDic = new Dictionary<string, string>();
                        fieldDic.Add(fields[1], fields[2]);
                        fieldMapping.Add(fields[0], fieldDic);

                         Dictionary<string, string> otherfieldDic = new Dictionary<string, string>();
                         otherfieldDic.Add(fields[1] + "_" + fields[3], fields[2]);
                         otherfieldMapping.Add(fields[0], otherfieldDic);
                    }
                    else
                    {
                        fieldMapping[fields[0]].Add(fields[1], fields[2]);

                        otherfieldMapping[fields[0]].Add(fields[1] + "_" + fields[3], fields[2]);

                    }
                }


                string urlfilepath = rootDir + mappingfile;
                IEnumerable<string> ls = File.ReadLines(urlfilepath);



                foreach (string cats in ls.ToList())
                {

                    //HtmlWeb website = new HtmlWeb();
                    //HtmlAgilityPack.HtmlDocument doc = website.Load(path);
                    //dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(doc.);
                    if (string.IsNullOrEmpty(cats))
                        continue;
                    string[] catgroup = cats.Split(',');
                    string cat_88 = catgroup[catgroup.Length - 1];
                    string cat_name = catgroup[0];

                    string url_result = string.Format(url, cat_name);
                    int page = getPageParameter(url_result);
                    string pathurl = url;
                    string jsondata = "";
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("Referer", "https//" + cat_name);
                        wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                         jsondata = wc.DownloadString(url_result);
                    }
                    if (string.IsNullOrEmpty(jsondata))
                        continue;

                    dynamic initData = Newtonsoft.Json.JsonConvert.DeserializeObject(jsondata);

                    double total = double.Parse(initData.count.ToString().Replace(",", ""));
                    int pageCount = int.Parse(Math.Ceiling(total / 20.0).ToString()); ;
                    //page = 2;
                   
                    //int pageNumber=page_indicator
                    for (int pageindex = page; pageindex < pageCount; pageindex++)
                    {
                        pathurl = SetPageParameter(url_result, pageindex);
                        string json = "";
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("Referer", "https//" + cat_name);
                            wc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                            json = wc.DownloadString(pathurl);
                        }
                        if (string.IsNullOrEmpty(json))
                            continue;

                        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                        for (int i = 0; i < data.items.Count; i++)
                        {

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
                            string lat = "";
                            string lng = "";
                            float flat = 0;
                            float flng = 0;
                            string Catstring = "";
                            string postcontactno1 = "";
                            string postcontactno2 = "";
                            string postaddress = "";
                            string contactemail = "";
                            string sourceRef = "";

                            string price = "0";
                            string postdataXml = "";

                            string propperroom = "";
                            string saleablearea = "";
                            string grossarea = "";
                            string propertype = "";
                            string propertitlechi = "";

                            string decoration = "";
                            string properfloor = "";
                            string properdir = "";
                            string properview = "";

                            List<string> homeappliances = new List<string>();

                            string visitable = "vis02";



                            detailId = data.items[i].detailUrl;
                            detailId = detailId.Remove(0, detailId.LastIndexOf("-") + 1).Replace(".html", "");
                            string urlpath = data.items[i].detailUrl;//string.Format(detailurl, detailId);
                            Console.WriteLine("urlpath: " + urlpath);

                            if (data.items[i].tagArr.Count >= 2)
                            {
                                visitable = "vis01";
                            }


                            HtmlWeb website = new HtmlWeb();
                            HtmlAgilityPack.HtmlDocument doc = website.Load(urlpath);
                            long categoryid = 0;//long.Parse(cat);





                            //string keyword="";
                            HtmlNode priceNode = doc.DocumentNode.SelectSingleNode("//span[@id='price']");//A
                            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h2[@class='title fl']");//M
                            HtmlNodeCollection mainNodes = doc.DocumentNode.SelectNodes("//ol[@class='con']/li");//B,C,D,E,G

                            HtmlNode nameNode = doc.DocumentNode.SelectSingleNode("//p[@class='name']");//H
                            HtmlNode telNode = doc.DocumentNode.SelectSingleNode("//span[@class='tel']");//I
                            HtmlNodeCollection sencondTelNodes = doc.DocumentNode.SelectNodes("//ul[@class='clearfix']/li");// J ,K
                            HtmlNode sourceRefIDNode = doc.DocumentNode.SelectSingleNode("//div[@class='bread-nav-item']");// L
                            HtmlNode contentNode = doc.DocumentNode.SelectSingleNode("//div[@class='house-txt']");
                            HtmlNodeCollection roomOthersNode = doc.DocumentNode.SelectNodes("//div[@class='add-data']/ul/li");
                            HtmlNodeCollection roomDetailsNode = doc.DocumentNode.SelectNodes("//div[@class='with-list']/dl/dd/span[@class='status_1']");
                            //HtmlNode lat_lng = doc.DocumentNode.SelectSingleNode("//div[@id='wedding_content']").PreviousSibling.PreviousSibling;//H
                            //HtmlNode memberavatorNode = doc.DocumentNode.SelectSingleNode("//div[@class='companyLogo']/img");



                            if (titleNode == null)
                            {
                                continue;
                            }
                            if (titleNode != null)
                            {
                                title = titleNode.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                                //membername = title;
                            }

                            if (sourceRefIDNode != null)
                            {
                                sourceRef = sourceRefIDNode.LastChild.InnerText.Replace("\t", "").Replace("\n", "").Replace("（", "_").Replace("）", "").Trim();
                                string[] sourceRefs = sourceRef.Split('_');
                                if (sourceRefs.Length > 1)
                                {
                                    sourceRef = sourceRefs[1];
                                }
                                detailId = sourceRef;
                                DataTable dt = SQLHelper.ExecuteDt("SELECT [temppostid] FROM [grabpost] with(nolock) where sourceRefId='" + sourceRef + "' and sourceSite='" + sourceSite + "'");
                                if (dt.Rows.Count > 0)
                                {
                                    //System.Threading.Thread.Sleep(3000);
                                    continue;
                                }

                            }

                            if (telNode != null)
                            {
                                postcontactno1 = telNode.InnerText.Replace("\n", "").Trim();
                                if (postcontactno1.IndexOf("@") != -1)
                                {
                                    postcontactno1 = "";
                                }
                            }

                            if (sencondTelNodes != null)
                            {
                                for (int si = 0; si < sencondTelNodes.Count; si++)
                                {
                                    if (si == 0)
                                    {
                                        postcontactno2 = sencondTelNodes[si].LastChild.InnerText;
                                        if (postcontactno2.IndexOf("--") != -1)
                                        {
                                            postcontactno2 = "";
                                        }
                                    }
                                    if (si == 1)
                                    {
                                        memberemail = sencondTelNodes[si].LastChild.InnerText;
                                        if (memberemail.IndexOf("--") != -1)
                                        {
                                            memberemail = "";
                                        }
                                    }

                                }
                            }

                            if (string.IsNullOrEmpty(postcontactno1))
                            {
                                postcontactno1 = postcontactno2;
                                postcontactno2 = "";
                            }

                            if (string.IsNullOrEmpty(memberemail))
                                continue;

                            if (priceNode != null)
                            {
                                price = priceNode.InnerText.Trim();
                            }
                           
                            if (nameNode != null)
                            {
                                membername = nameNode.InnerText.Trim().Replace("(業主)", "");
                            }

                            if (mainNodes != null)
                            {
                                for (int xmlIndex = 0; xmlIndex < mainNodes.Count; xmlIndex++)
                                {
                                    string label = mainNodes[xmlIndex].FirstChild.InnerText;
                                    string value = mainNodes[xmlIndex].LastChild.InnerText;
                                    if (label.IndexOf("間隔")!=-1)
                                    {
                                        foreach (string key in fieldMapping["propperroom"].Keys)
                                        {
                                            string[] splitArray = { "房", "Room" };
                                            string[] romInfo = value.Split(splitArray, StringSplitOptions.RemoveEmptyEntries);
                                            if (romInfo.Length >= 1 && (key.IndexOf(romInfo[0].Trim()) != -1 || fieldMapping["propperroom"][key].IndexOf(romInfo[0].Trim()) != -1))
                                            {
                                                propperroom = fieldMapping["propperroom"][key];
                                            }
                                        }
                                    }

                                    if (label.IndexOf("面積") != -1)
                                    {
                                        //saleablearea = value;
                                        string[] areas = mainNodes[xmlIndex].LastChild.InnerHtml.Replace("<br>", "|").Split('|');
                                        saleablearea = areas[0];
                                        if (areas.Length > 1)
                                        {
                                            grossarea = areas[1];
                                        }
                                    }
                                    if (label.IndexOf("用途") != -1)
                                    {
                                        foreach (string key in fieldMapping["propertype"].Keys)
                                        {
                                            if (value.IndexOf(key) != -1 || key.IndexOf(value) != -1)
                                            {
                                                propertype = fieldMapping["propertype"][key];
                                            }
                                        }
                                    }

                                    if (label.IndexOf("物業") != -1)
                                    {
                                        propertitlechi = value.Trim();
                                    }

                                    if (label.IndexOf("地址") != -1)
                                    {
                                        postaddress = value.Remove(0, value.IndexOf(" ") + 1);
                                    }


                                }

                            }



                            if (contentNode != null)
                            {
                                postcontent = contentNode.InnerText.Trim();
                                summary = postcontent;
                            }

                            if (roomOthersNode != null)
                            {
                                foreach (HtmlNode otherNode in roomOthersNode)
                                {
                                    if (otherNode.InnerText.IndexOf("裝潢") != -1)
                                    {
                                        foreach (string key in fieldMapping["decoration"].Keys)
                                        {
                                            if (otherNode.LastChild == null)
                                                continue;
                                            if (otherNode.LastChild.InnerText.IndexOf(key) != -1 || key.IndexOf(otherNode.LastChild.InnerText) != -1)
                                            {
                                                decoration = fieldMapping["decoration"][key];
                                                break;
                                            }
                                        }
                                    }
                                    //=======================
                                    if (otherNode.InnerText.IndexOf("樓層") != -1)
                                    {
                                        foreach (string key in fieldMapping["properfloor"].Keys)
                                        {
                                            if (otherNode.LastChild == null)
                                                continue;
                                            if (otherNode.LastChild.InnerText.Split('F').Length == 2)
                                            {
                                                if (otherNode.LastChild.InnerText.IndexOf(key) != -1 || key.IndexOf(otherNode.LastChild.InnerText) != -1)
                                                {
                                                    properfloor = fieldMapping["properfloor"][key];
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                properfloor = "aaa04";
                                                string textStr = otherNode.LastChild.InnerText.Replace("F", "");
                                                if (!string.IsNullOrEmpty(textStr))
                                                {
                                                    string[] floorLevels = textStr.Split('/');
                                                    if (floorLevels.Length > 1)
                                                    {
                                                        double level = 0.0;
                                                        double totalLevel = 1.0;
                                                        double.TryParse(floorLevels[0], out level);
                                                        double.TryParse(floorLevels[1], out totalLevel);
                                                        double avarageLevel = level / totalLevel;
                                                        if (avarageLevel > 5.0)
                                                        {
                                                            properfloor = "aaa03";
                                                        }
                                                        if (avarageLevel > 8.0)
                                                        {
                                                            properfloor = "aaa02";
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }

                                    //=============
                                    if (otherNode.InnerText.IndexOf("坐向") != -1)
                                    {
                                        foreach (string key in fieldMapping["properdir"].Keys)
                                        {
                                            if (otherNode.LastChild == null)
                                                continue;
                                            if (otherNode.LastChild.InnerText.IndexOf(key) != -1 || key.IndexOf(otherNode.LastChild.InnerText) != -1)
                                            {
                                                properdir = fieldMapping["properdir"][key];
                                                break;
                                            }
                                        }
                                    }

                                    //====================
                                    //properview
                                    if (otherNode.InnerText.IndexOf("景觀") != -1)
                                    {
                                        foreach (string key in fieldMapping["properview"].Keys)
                                        {
                                            if (otherNode.LastChild == null)
                                                continue;
                                            if (otherNode.LastChild.InnerText.IndexOf(key) != -1 || key.IndexOf(otherNode.LastChild.InnerText) != -1)
                                            {
                                                properview = fieldMapping["properview"][key];
                                                break;
                                            }
                                        }
                                    }

                                }
                            }


                            if (roomDetailsNode != null)
                            {
                                foreach (HtmlNode detailNode in roomDetailsNode)
                                {
                                    foreach (string key in otherfieldMapping["homeappliances"].Keys)
                                    {

                                        if (detailNode.FirstChild == null)
                                            continue;
                                        string cssClass = detailNode.FirstChild.Attributes["class"].Value.Replace("with_ico ", "");
                                        if (cssClass.IndexOf(key) != -1 || key.IndexOf(cssClass) != -1)
                                        {
                                            homeappliances.Add(otherfieldMapping["homeappliances"][key]);
                                            break;
                                        }
                                    }
                                }
                            }

                            StringBuilder sbPostdataXml = new StringBuilder();
                            sbPostdataXml.Append("<fields>");
                            if (string.IsNullOrEmpty(postdataXml))
                            {
                                if (price != "0" && !string.IsNullOrEmpty(price))
                                {
                                    price = price.Replace(",", "");
                                    sbPostdataXml.Append(genxmldata("averageprice", "freetext", price));
                                }
                                if (!string.IsNullOrEmpty(propperroom))
                                {
                                    sbPostdataXml.Append(genxmldata("propperroom", "schema", propperroom));
                                }
                                if (!string.IsNullOrEmpty(saleablearea))
                                {
                                    sbPostdataXml.Append(genxmldata("saleablearea", "freetext", saleablearea));
                                }
                                if (!string.IsNullOrEmpty(grossarea))
                                {
                                    sbPostdataXml.Append(genxmldata("grossarea", "freetext", grossarea));
                                }
                                if (!string.IsNullOrEmpty(propertype))
                                {
                                    sbPostdataXml.Append(genxmldata("propertype", "schema", propertype));
                                }
                                if (!string.IsNullOrEmpty(propertitlechi))
                                {
                                    sbPostdataXml.Append(genxmldata("propertitlechi", "freetext", propertitlechi));
                                }

                                if (!string.IsNullOrEmpty(decoration))
                                {
                                    sbPostdataXml.Append(genxmldata("decoration", "schema", decoration));
                                }

                                if (!string.IsNullOrEmpty(properfloor))
                                {
                                    sbPostdataXml.Append(genxmldata("properfloor", "schema", properfloor));
                                }

                                if (!string.IsNullOrEmpty(properdir))
                                {
                                    sbPostdataXml.Append(genxmldata("properdir", "schema", properdir));
                                }

                                if (!string.IsNullOrEmpty(properview))
                                {
                                    sbPostdataXml.Append(genxmldata("properview", "schema", properview));
                                }

                                if (homeappliances.Count > 0)
                                {
                                    sbPostdataXml.Append(genxmldata("homeappliances", "schema", homeappliances));
                                }
                                /*
      
                               
                                string saleablearea = "";
                                string grossarea = "";
                                string propertype = "";
                                string propertitlechi = "";

                                string decoration = "";
                                string properfloor = "";
                                string properdir = "";
                                string properview = "";

                                List<string> homeappliances = new List<string>();

                                string visitable = "vis02";
                                 * */

                            }
                            sbPostdataXml.Append("</fields>");
                            postdataXml = sbPostdataXml.ToString();

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
                             * @postdata xml=null,         --postdata
  @averageprice money=0,   --价钱
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
                                    new SqlParameter("@sourceRefID", SqlDbType.NVarChar,100){Value=detailId},
                                    new SqlParameter("@postdata", SqlDbType.Xml){Value=postdataXml},
                                     new SqlParameter("@averageprice", SqlDbType.Money){Value=price},
                                     new SqlParameter("@PropPostType", SqlDbType.SmallInt){Value=PropPostType}
                                     
                                                        };
                            StringBuilder paramstr = new StringBuilder();
                            paramstr.Append(" exec [dbo].insertgrabpost{");
                            //paramstr=parameters.t
                            foreach (SqlParameter p in parameters)
                            {
                                paramstr.Append(p.ParameterName + "=N'" + p.Value + "',").AppendLine(); //paramstr + p.ParameterName + "=" + p.Value+"||||";
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
                File.AppendAllText(rootDir + logfile, ex.InnerException + "||" + ex.StackTrace + sqlresult + "||detailId:" + detailId);
            }



        }

        public static string genxmldata(string id, string srctype,string data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<fielddata id=\"{0}\" srctype=\"{1}\">");
            sb.Append("<data><![CDATA[{2}]]></data>");
            sb.Append("</fielddata>");

            return string.Format(sb.ToString(), id, srctype, data);
        }
        public static string genxmldata(string id, string srctype, List<string> data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<fielddata id=\"{0}\" srctype=\"{1}\">");
            foreach (string value in data)
            {
                sb.Append("<data>" + value + "</data>");
            }
           
            sb.Append("</fielddata>");

            return string.Format(sb.ToString(), id, srctype);
        }

        public static string SetPageParameter(this string url, int pageNumber)
        {
            var queryStartIndex = url.IndexOf("?") + 1;
            if (queryStartIndex == 0)
            {
                return string.Format("{0}?p={1}", url, pageNumber);
            }
            var oldQueryString = url.Substring(queryStartIndex);
            var queryParameters = HttpUtility.ParseQueryString(oldQueryString);
            queryParameters["p"] = pageNumber.ToString();
            return url.Substring(0, queryStartIndex) + queryParameters.ToString();
        }
        public static int getPageParameter(this string url)
        {
            var queryStartIndex = url.IndexOf("?") + 1;

            var oldQueryString = url.Substring(queryStartIndex);
            var queryParameters = HttpUtility.ParseQueryString(oldQueryString);
            return int.Parse(queryParameters["p"]);
        }
        public static bool CheckEmail(string EmailAddress)
        {


            string strPattern = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";


            if (System.Text.RegularExpressions.Regex.IsMatch(EmailAddress, strPattern))
            { return true; }
            return false;
        }
    }

}
