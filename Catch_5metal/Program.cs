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

namespace Catch_5metal
{
    static class Program
    {
      
        static string rootDir = ConfigurationManager.AppSettings["rootDir"].ToString();//@"C:\alljobs";
        static string mappingfile = ConfigurationManager.AppSettings["mappingfile"].ToString();
        static string logfile = ConfigurationManager.AppSettings["logfile"].ToString();
        static string sourceSite = ConfigurationManager.AppSettings["sourceSite"].ToString();
        //static string fieldmapping = ConfigurationManager.AppSettings["fieldmapping"];
        static void Main(string[] args)
        {
            string url = "https://www.5metal.com.hk/ajax/pager/company?tid={0}&page={1}";
            string detailurl = "https://www.5metal.com.hk/{0}";
            string sqlresult = "";
            string detailId = "";


            try
            {
                Dictionary<string, Dictionary<string, string>> fieldMapping = new Dictionary<string, Dictionary<string, string>>();
                Dictionary<string, Dictionary<string, string>> otherfieldMapping = new Dictionary<string, Dictionary<string, string>>();



                string urlfilepath = rootDir + mappingfile;
                IEnumerable<string> ls = File.ReadLines(urlfilepath);



                foreach (string cats in ls.ToList())
                {

                    //HtmlWeb website = new HtmlWeb();
                    //HtmlAgilityPack.HtmlDocument doc = website.Load(path);
                    //dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(doc.);
                    if (string.IsNullOrEmpty(cats))
                        continue;
                    string[] catgroup = cats.Split('\t');
                    string cat_88 = catgroup[1];
                    string cat_name = catgroup[0];
                    string target_siteId = catgroup[2];

                    string url_result = string.Format(url, target_siteId, 1);
                    int page = getPageParameter(url_result);
                    string pathurl = url;

                    HtmlWeb website = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument ldoc = website.Load(url_result);
                    HtmlNode pageNode = ldoc.DocumentNode.SelectSingleNode("//li[@class='pager-current']");
                    if (pageNode == null)
                        continue;

                    string[] pages = pageNode.InnerText.Split(' ');
                    int pageCount = int.Parse(pages[pages.Length - 1]);

                    //page = 2;

                    //int pageNumber=page_indicator
                    for (int pageindex = page; pageindex < pageCount; pageindex++)
                    {
                        url_result = string.Format(url, target_siteId, pageindex);

                        HtmlWeb loadWebsite = new HtmlWeb();
                        HtmlAgilityPack.HtmlDocument doc1 = loadWebsite.Load(url_result);
                        HtmlNode contentListNode = doc1.DocumentNode.SelectSingleNode("//div[@class='view-content']");
                        if (contentListNode == null)
                            continue;


                        HtmlNodeCollection detailListLinks = contentListNode.SelectNodes("//div[@class='logo']/a");

                        for (int i = 0; i < detailListLinks.Count; i++)
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





                            detailId = detailListLinks[i].Attributes["href"].Value.Replace("/node/", "");

                            string urlpath = string.Format(detailurl, detailListLinks[i].Attributes["href"].Value);
                            Console.WriteLine("urlpath: " + urlpath);



                            HtmlWeb detailwebsite = new HtmlWeb();
                            HtmlAgilityPack.HtmlDocument doc = website.Load(urlpath);
                            long categoryid = 0; //long.Parse(cat);

                            //one page
                            //HtmlNode company_rightNode = doc.DocumentNode.SelectSingleNode("//div[class='company_right']");
                            HtmlNode titleNode = null;
                            HtmlNode keywordNode = null;
                            HtmlNode emailNode = null;
                            HtmlNode contactno2Node = null;
                            HtmlNode contactno1Node = null;
                            HtmlNode siteNode = null;
                            HtmlNode districtNode = null;
                            HtmlNode memberaddressNode = null;
                            HtmlNodeCollection latLngNode = null;
                            HtmlNode des = null;
                            titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='node_title']");

                            if (titleNode != null)
                            {
                                titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='node_title']");//a
                                keywordNode = doc.DocumentNode.SelectSingleNode("//div[@class='field field-name-field-company-type field-type-taxonomy-term-reference field-label-inline clearfix']/div[@class='field-items']");//b
                                emailNode = doc.DocumentNode.SelectSingleNode("//div[@class='field field-name-field-email field-type-text field-label-inline clearfix']/div/div[@class='field-item even']");//c
                                contactno2Node = doc.DocumentNode.SelectSingleNode("//div[@class='field field-name-field-mobile field-type-text field-label-inline clearfix']/div/div[@class='field-item even']");//d
                                contactno1Node = doc.DocumentNode.SelectSingleNode("//div[@class='field field-name-field-company-tel field-type-text field-label-inline clearfix']/div/div[@class='field-item even']");//e
                                siteNode = doc.DocumentNode.SelectSingleNode("//div[@class='field field-name-field-company-url field-type-text field-label-inline clearfix']/div/div[@class='field-item even']");//f
                                districtNode = doc.DocumentNode.SelectSingleNode("//div[@class='field field-name-field-distirct field-type-taxonomy-term-reference field-label-inline clearfix']/div/div[@class='field-item even']");//g
                                memberaddressNode = doc.DocumentNode.SelectSingleNode("//div[@class='field field-name-field-company-address field-type-text field-label-inline clearfix']/div/div[@class='field-item even']");//h
                                latLngNode = doc.DocumentNode.SelectNodes("//div[@class='content']/script");//j //var myLatlng = new google.maps.LatLng(22.3568199, 114.1237311);
                            }
                            else
                            {
                                titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='cp-name']");
                                HtmlNode morenode = doc.DocumentNode.SelectSingleNode("//a[@class='more_btn']");
                                if (morenode != null)
                                {
                                    string aboutlink = "https://www.5metal.com.hk" + morenode.Attributes["href"].Value;
                                    HtmlWeb loadaboutWebsite = new HtmlWeb();
                                    HtmlAgilityPack.HtmlDocument aboutdoc = loadaboutWebsite.Load(aboutlink);
                                    des=aboutdoc.DocumentNode.SelectSingleNode("//div[@class='desc about_desc']");

                                    string others = "https://www.5metal.com.hk" + morenode.Attributes["href"].Value.Replace("about","contact");
                                    HtmlWeb othersWebsite = new HtmlWeb();
                                    HtmlAgilityPack.HtmlDocument othersdoc = othersWebsite.Load(others);
                                    HtmlNodeCollection mainnode=othersdoc.DocumentNode.SelectNodes("//div[@class='row']");
                                    foreach (HtmlNode otherinfoNode in mainnode)
                                    {
                                        if (otherinfoNode.InnerText.IndexOf("地址") != -1)
                                        {
                                            memberaddressNode = otherinfoNode.ChildNodes[3];
                                            continue;
                                        }
                                        if (otherinfoNode.InnerText.IndexOf("電話(公司)") != -1)
                                        {
                                            contactno1Node = otherinfoNode.ChildNodes[3];
                                            continue;
                                        }
                                        if (otherinfoNode.InnerText.IndexOf("電話(手提)") != -1)
                                        {
                                            contactno2Node = otherinfoNode.ChildNodes[3];
                                            continue;
                                        }
                                        if (otherinfoNode.InnerText.IndexOf("電郵") != -1)
                                        {
                                            emailNode = otherinfoNode.ChildNodes[3];
                                            continue;
                                        }
                                        if (otherinfoNode.InnerText.IndexOf("網址") != -1)
                                        {
                                            siteNode = otherinfoNode.ChildNodes[3];
                                            continue;
                                        }

                                        
                                    }

                                    latLngNode = othersdoc.DocumentNode.SelectNodes("//div[@id='cp-rightside']/script");
                                   
                                }
                               
                            }


                            if (titleNode == null)
                            {
                                continue;
                            }
                            if (titleNode != null)
                            {
                                title = titleNode.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                                //membername = title;
                            }


                            sourceRef = detailId;

                            DataTable dt = SQLHelper.ExecuteDt("SELECT [temppostid] FROM [grabpost] with(nolock) where sourceRefId='" + sourceRef + "' and sourceSite='" + sourceSite + "'");
                            if (dt.Rows.Count > 0)
                            {
                                //System.Threading.Thread.Sleep(3000);
                                continue;
                            }



                            if (keywordNode != null)
                            {
                                keyword = keywordNode.InnerHtml.Replace("<div class=\"field-item even\">", "").Replace("<div class=\"field-item odd\">", "").Replace("</div>", ",").Trim(',');

                            }

                            if (emailNode != null)
                            {
                                memberemail = emailNode.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                            }

                            if (contactno1Node != null)
                            {
                                postcontactno1 = contactno1Node.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                                if (!string.IsNullOrEmpty(postcontactno1))
                                {
                                    postcontactno1 = postcontactno1.Split(',')[0];
                                }
                                postcontactno1=System.Text.RegularExpressions.Regex.Replace(postcontactno1, "[\u4e00-\u9fa5]", "");
                            }
                            if (contactno2Node != null)
                            {
                                postcontactno2 = contactno2Node.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                                if (!string.IsNullOrEmpty(postcontactno2))
                                {
                                    postcontactno2 = postcontactno2.Split(',')[0];
                                }
                                postcontactno2 = System.Text.RegularExpressions.Regex.Replace(postcontactno2, "[\u4e00-\u9fa5]", "");
                            }

                            if (string.IsNullOrEmpty(postcontactno1))
                            {
                                postcontactno1 = postcontactno2;
                                postcontactno2 = "";
                            }

                            if (string.IsNullOrEmpty(memberemail))
                                continue;

                            if (siteNode != null)
                            {
                                memberwebsite = siteNode.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                            }

                            if (districtNode != null)
                            {
                                district = districtNode.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                            }
                            if (des != null)
                            {
                                postcontent = des.InnerText.Replace("\t","").Replace("\n","").Replace("\r","").Trim();
                                summary = postcontent;
                            }

                            if (latLngNode != null)
                            {
                                foreach (HtmlNode lat1 in latLngNode)
                                {
                                    string ctxt = lat1.InnerText;
                                    string[] ctxts=ctxt.Split('\n');
                                    for (int index1 = 0; index1 < ctxts.Length; index1++)
                                    {
                                        if (ctxts[index1].IndexOf("var myLatlng") != -1)
                                        {
                                            // //var myLatlng = new google.maps.LatLng(22.3568199, 114.1237311);
                                            string[] googleLatlng = ctxts[index1].Replace("var myLatlng = new google.maps.LatLng(", "").Replace(")", "").Replace(";","").Split(',');

                                          float.TryParse(googleLatlng[0], out flat);
                                          float.TryParse(googleLatlng[1], out flng);
                                            break;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }



                            if (memberaddressNode != null)
                            {
                                memberaddress = memberaddressNode.InnerText.Replace("\n", "").Replace("&nbsp;", "").Trim();
                                postaddress = memberaddress;
                            }



                            StringBuilder sbPostdataXml = new StringBuilder();
                            sbPostdataXml.Append("<fields></fields>");
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
                                     new SqlParameter("@averageprice", SqlDbType.Money){Value=price}
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
            sb.Append("<data>{2}</data>");
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


            if (System.Text.RegularExpressions.Regex.IsMatch(EmailAddress, strPattern))
            { return true; }
            return false;
        }
    }

}
