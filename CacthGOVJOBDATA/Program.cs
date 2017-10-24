using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;
using System.Data.SqlClient;
using System.Data;

namespace CacthGOVJOBDATA
{
    class Program
    {
        static string outputfolder = ConfigurationManager.AppSettings["outputfolder"].ToString();//@"C:\alljobs";
        static void Main(string[] args)
        {
            try
            {
                string url = ConfigurationManager.AppSettings["url"].Replace("#", "&");
                string downloadfile = string.Format(ConfigurationManager.AppSettings["downloadfile"].ToString(), DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss"));//@"C:\alljobs.zip";

                string jsonfile = outputfolder + "\\" + ConfigurationManager.AppSettings["jsonfilename"].ToString();//"\\all.json";            
                WebClient Client = new WebClient();
                Client.DownloadFile(url, downloadfile);
               
                ExtractFileToDirectory(downloadfile, outputfolder);

                String jsonstring = File.ReadAllText(jsonfile, Encoding.UTF8);
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonstring);
                if (data.common != null)
                {
                    dynamic englishJobs = data.common[0].English;
                    dynamic tcJobs = data.common[1].tc;
                    dynamic scJobs = data.common[2].sc;
                    insertjob(englishJobs, 1);
                    insertjob(tcJobs, 2);


                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(outputfolder + "\\" + ConfigurationManager.AppSettings["logfile"].ToString(), ex.ToString());
            }
            
        }

        public static void ExtractFileToDirectory(string zipFileName, string outputDirectory)
        {
            ZipFile zip = ZipFile.Read(zipFileName);
            Directory.CreateDirectory(outputDirectory);
            foreach (ZipEntry e in zip)
            {
                // check if you want to extract e or not
                //if (e.FileName == "TheFileToExtract")
                e.Extract(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        public static void insertjob(dynamic jobs,int pameter_langid)
        {
            int langid = 1;
            string startdate = null;
            long jobid = 0;
            string cenqaddr = null;
            string cdeptnamejve = null;
            string cbenefit = null;
            string expto = null;
            string cdivision = null;
            string ispsc = null;
            string iscs = null;
            int expfrom = 0;
            string enddate = null;
            string attachfilename = null;
            string isgf340 = null;
            string cappnotes = null;
            string ccyh = null;
            string cduties = null;
            string ccyd = null;
            string cernotes = null;
            string centreq = null;
            string cappmethod = null;
            string cjobname = null;
            string cenqtel = null;
            string centrypay = null;
            int minpayd = 0;
            string cdepturl = null;
            string cacademic = null;
            string ccym = null;
            int minpayh = 0;
            int minpaym = 0;
            string cjobnature = null;
            string cappterm = null;
            string pubdate = null;

            for (int jobindex = 0; jobindex < jobs.Count; jobindex++)
            {
                try
                {
                    dynamic job = jobs[jobindex];
                    langid = pameter_langid;
                    jobid = job.jobid;
                    startdate = job.startdate;
                    cenqaddr = job.cenqaddr ?? job.enqaddr;
                    cdeptnamejve = job.cdeptnamejve ?? job.deptnamejve; ;
                    cbenefit = job.cbenefit ?? job.benefit; ;
                    expto = job.expto ?? job.cexpto;
                    cdivision = job.cdivision ?? job.division;
                    ispsc = job.ispsc;
                    iscs = job.iscs;
                    expfrom = job.expfrom;
                    enddate = job.enddate;
                    attachfilename = job.attachfilename;
                    isgf340 = job.isgf340;
                    cappnotes = job.cappnotes ?? job.appnotes;
                    ccyh = job.ccyh ?? job.cyh;
                    cduties = job.cduties ?? job.duties;
                    ccyd = job.ccyd ?? job.cyd;
                    cernotes = job.cernotes ?? job.ernotes;
                    centreq = job.centreq ?? job.entreq; ;
                    cappmethod = job.cappmethod ?? job.appmethod;
                    cjobname = job.cjobname ?? job.jobname;
                    cenqtel = job.cenqtel ?? job.enqtel;
                    centrypay = job.centrypay ?? job.entrypay;
                    if (job.minpayd.Value != null)
                    {
                        int int_minpayd = (int)job.minpayd.Value;
                        minpayd = int_minpayd;
                    }
                    //minpayd = Math.Ceiling(job.minpayd.Value) ?? 0;
                    cdepturl = job.cdepturl ?? job.depturl;
                    dynamic cacademic_dynamic = job.cacademic ?? job.academic;
                    cacademic = cacademic_dynamic.ToString();
                    ccym = job.ccym ?? job.cym;
                    if (job.minpayh.Value != null)
                    {
                        int int_minpayh = (int)job.minpayh.Value;
                        minpayh = int_minpayh;
                    }
                    if (job.minpaym.Value != null)
                    {
                        int int_minpaym = (int)job.minpaym.Value;
                        minpaym = int_minpaym;
                    }
                    cjobnature = job.cjobnature ?? job.jobnature;
                    cappterm = job.cappterm ?? job.appterm;
                    if (job.pubdate != null)
                    {
                        pubdate = job.pubdate;
                    }

                    SQLHelper sqlHelper = new SQLHelper();
                    SqlParameter[] parameters = {
                                  new SqlParameter("@langid", SqlDbType.TinyInt){Value=langid},
                                  new SqlParameter("@startdate", SqlDbType.DateTime){Value=startdate},
                                  new SqlParameter("@jobid", SqlDbType.BigInt){Value=jobid},
                                  new SqlParameter("@cenqaddr", SqlDbType.NVarChar,256){Value=cenqaddr},
                                  new SqlParameter("@cdeptnamejve", SqlDbType.NVarChar,32){Value=cdeptnamejve},
                                  new SqlParameter("@cbenefit", SqlDbType.NVarChar,1024){Value=cbenefit },
                                  new SqlParameter("@expto", SqlDbType.NVarChar,32){Value=expto },
                                  new SqlParameter("@cdivision ", SqlDbType.NVarChar,32){Value=cdivision },
                                  new SqlParameter("@ispsc", SqlDbType.Char,1){Value=ispsc},
                                  new SqlParameter("@iscs", SqlDbType.Char,1){Value=iscs },
                                  new SqlParameter("@expfrom", SqlDbType.Int){Value=expfrom},
                                  new SqlParameter("@enddate", SqlDbType.DateTime){Value=enddate},
                                  new SqlParameter("@attachfilename", SqlDbType.NVarChar,256){Value=attachfilename },
                                  new SqlParameter("@isgf340", SqlDbType.Char,1){Value=isgf340},
                                  new SqlParameter("@cappnotes", SqlDbType.NVarChar,1024){Value=cappnotes},
                                  new SqlParameter("@ccyh", SqlDbType.NVarChar,32){Value=ccyh},
                                  new SqlParameter("@cduties", SqlDbType.NVarChar,1024){Value=cduties},
                                  new SqlParameter("@ccyd", SqlDbType.NVarChar,32){Value=ccyd },
                                  new SqlParameter("@cernotes", SqlDbType.NVarChar,32){Value=cernotes },
                                  new SqlParameter("@centreq", SqlDbType.NVarChar,1024){Value=centreq },
                                  new SqlParameter("@cappmethod", SqlDbType.NVarChar,1024){Value=cappmethod },
                                  new SqlParameter("@cjobname", SqlDbType.NVarChar,128){Value=cjobname },
                                  new SqlParameter("@cenqtel", SqlDbType.NVarChar,32){Value=cenqtel },
                                  new SqlParameter("@centrypay", SqlDbType.NVarChar,64){Value=centrypay },
                                  new SqlParameter("@minpayd", SqlDbType.Int){Value=minpayd  },
                                  new SqlParameter("@cdepturl", SqlDbType.NVarChar,256){Value=cdepturl  },
                                  new SqlParameter("@cacademic", SqlDbType.NVarChar,256){Value=cacademic  },

                                  new SqlParameter("@ccym", SqlDbType.NVarChar,32){Value=ccym   },
                                  new SqlParameter("@minpayh", SqlDbType.Int){Value=minpayh   },
                                  new SqlParameter("@minpaym", SqlDbType.Int){Value=minpaym  },
                                  new SqlParameter("@cjobnature", SqlDbType.NVarChar,128){Value=cjobnature   },
                                  new SqlParameter("@cappterm", SqlDbType.NVarChar,1024){Value=cappterm   },
                                  new SqlParameter("@pubdate", SqlDbType.DateTime){Value=pubdate   },

                            };
                    sqlHelper.RunProc("InsertGovJobData", parameters);
                }
                catch(Exception ex)
                {
                    File.WriteAllText(outputfolder + "\\" + ConfigurationManager.AppSettings["logfile"].ToString(), ex.ToString());
                }
                 
            }
        }
    }
}
