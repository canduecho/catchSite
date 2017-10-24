using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using HtmlAgilityPack;
using System.Configuration;

namespace SendExcelToRiverConsole
{

    class Program
    {
        static void Main(string[] args)
        {
            /*
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("canduecho@gmail.com", "!ap0206107"),
                EnableSsl = true
            };
            client.Send("canduecho@gmail.com", "myusername@gmail.com", "test", "testbody");
            Console.WriteLine("Sent");
            Console.ReadLine();
             */
            StringBuilder contents = new StringBuilder();
            //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            String baselink = "http://www.alexa.com/topsites/countries;{0}/HK";
            for (int i = 0; i < 20; i++)
            {
                String getbaselink = String.Format(baselink, i);
                HtmlWeb website = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = website.Load(getbaselink);

                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//p[@class='desc-paragraph']//a"))
                {
                    String url = link.Attributes["href"].Value;
                    String text = link.InnerText;
                    contents.Append(text).Append(",").Append(url).AppendLine();
                }
            }
            
            String files = string.Format(@"D:\data_{0}.csv", DateTime.Now.Date.ToString("dd-mmm-yyyy"));
            System.IO.File.WriteAllText(files, contents.ToString());
            string to = ConfigurationSettings.AppSettings["to"].ToString();
            string from = ConfigurationSettings.AppSettings["from"].ToString();
            string cc = ConfigurationSettings.AppSettings["cc"].ToString();

            SendMail(to, from, cc, "'Alexa - Top Hong Kong Sites", "hi kevin, send data to you", files);
        }

        public static string SendMail(string toList, string from, string ccList, string subject, string body,string filepath)
        {

            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();
            string msg = string.Empty;
            try
            {
                MailAddress fromAddress = new MailAddress(from);
                message.From = fromAddress;
                message.To.Add(toList);
                if (ccList != null && ccList != string.Empty)
                    message.CC.Add(ccList);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                message.Attachments.Add(new Attachment(filepath));
                smtpClient.Host = "smtp.gmail.com";   // We use gmail as our smtp client
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential("canduecho@gmail.com", "!ap0206107");

                smtpClient.Send(message);
                msg = "Successful<BR>";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }
    }
}

