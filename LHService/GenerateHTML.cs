using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LandingHTML
{
    public partial class GenerateHTML : ServiceBase
    {

        #region Initialization Data
        private System.Threading.Timer tmr = null;
        IFormatProvider theCultureInfo = new System.Globalization.CultureInfo("en-US", true);
        DateTime Getdate = (DateTime)Convert.ToDateTime(System.DateTime.Now);
        DateTime requiredTime;
        string PortalRootPath = ConfigurationSettings.AppSettings["PortalRootPath"].ToString();
        string PageSettingPath = ConfigurationSettings.AppSettings["PageSettingPath"].ToString();
        int interval = int.Parse( ConfigurationSettings.AppSettings["Interval"].ToString());
        # endregion

        public GenerateHTML()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                WriteMessageToLog("Service Started");
                requiredTime = DateTime.Now.AddSeconds(10);
                SetTimerValue();
            }
            catch (Exception ex)
            {
                WriteMessageToLog(ex.Message);
            }
            finally
            {

            }
        }

        protected override void OnStop()
        {
            WriteMessageToLog("Service Stopped");
            if (tmr != null)
            {
                tmr.Dispose();
            }
        }

        public void generateHTML(string targetURL, string filename, string path, string fileDesc)
        {

            string strResult = string.Empty;

            string strWebPageToDownload = PortalRootPath + targetURL;
            string strHTMLPageToBeSaved = path + filename + ".html";

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;

                    strResult = client.DownloadString(strWebPageToDownload);
                }
                if (!strResult.Contains("Woopsi"))
                {
                    using (StreamWriter writer = new StreamWriter(strHTMLPageToBeSaved))
                    {
                        writer.WriteLine(strResult);
                    }
                    WriteMessageToLog(fileDesc + " generated.");
                }
                else
                {
                    WriteMessageToLog("Error in Webclient response for " + fileDesc);
                }
            }
            catch (Exception ex)
            {
                WriteMessageToLog("Error in WebClient response : " + ex);
            }

        }

        protected void WriteMessageToLog(string message)
        {
            string strMonth = (DateTime.Now.Month.ToString()).Length == 1 ? "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string strDay = (DateTime.Now.Day.ToString()).Length == 1 ? "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string strYear = DateTime.Now.Year.ToString();
            string strFileName = ConfigurationSettings.AppSettings["LogFileSuffix"].ToString() + ".txt";
            string strLogFileDirectory = ConfigurationSettings.AppSettings["LogDirectoryPath"].ToString();

            if (!(Directory.Exists(@"" + strLogFileDirectory + "\\" + strYear)))
                Directory.CreateDirectory(@"" + strLogFileDirectory + "\\" + strYear);
            if (!(Directory.Exists(@"" + strLogFileDirectory + "\\" + strYear + "\\" + strMonth)))
                Directory.CreateDirectory(@"" + strLogFileDirectory + "\\" + strYear + "\\" + strMonth);
            if (!(Directory.Exists(@"" + strLogFileDirectory + "\\" + strYear + "\\" + strMonth + "\\" + strDay)))
                Directory.CreateDirectory(@"" + strLogFileDirectory + "\\" + strYear + "\\" + strMonth + "\\" + strDay);

            strLogFileDirectory += strYear + "\\" + strMonth + "\\" + strDay;
            strFileName = @"" + strLogFileDirectory + "\\" + strFileName.Trim();
            bool flag = false;
            if (!File.Exists(strFileName))
            {
                flag = true;

            }
            using (StreamWriter strmTextWriter = new StreamWriter(strFileName, true))
            {
                strmTextWriter.WriteLine("<Log>");
                strmTextWriter.WriteLine("<Date_Time><!CDATA[" + DateTime.Now + "]]></Date_Time>");
                strmTextWriter.WriteLine("<Msg><!CDATA[" + message + "]]></Msg>");
                strmTextWriter.WriteLine("</Log>");
                strmTextWriter.Close();
            }
        }

        public void SetTimerValue()
        {
            try
            {
                tmr = new System.Threading.Timer(new TimerCallback(timerAction));
                tmr.Change((int)(requiredTime - DateTime.Now).TotalMilliseconds, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                WriteMessageToLog(ex.Message);

            }
        }

        public void timerAction(object e)
        {
            string targetURL, path, isActive;
            string filename = string.Empty;
            string fileDesc = string.Empty;
            int pageid, frequency;
            //DateTime generationTime;
            ArrayList list = new ArrayList();

            requiredTime = requiredTime.AddMinutes(interval);
            SetTimerValue();

            // WriteMessageToLog("File generation start");

            string strWidgetXML = PageSettingPath;// xml path

            if (!String.IsNullOrEmpty(strWidgetXML))
            {
                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument.Load(strWidgetXML);
                if (objXmlDocument.HasChildNodes)
                {
                    XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/pages/page");
                    if (objXmlNodeList.Count > 0)
                    {
                        foreach (XmlNode objXmlNode in objXmlNodeList)
                        {
                            pageid = Convert.ToInt16(objXmlNode.Attributes["id"].Value);
                            targetURL = objXmlNode.Attributes["url"].Value;
                            filename = objXmlNode.Attributes["name"].Value;
                            fileDesc = objXmlNode.Attributes["namedesc"].Value;
                            path = objXmlNode.Attributes["path"].Value;
                            isActive = objXmlNode.Attributes["active"].Value;
                            frequency = Convert.ToInt16(objXmlNode.Attributes["frequency"].Value);

                            try
                            {
                                if (isActive == "Y")
                                {
                                    WriteMessageToLog("Generating file for " + fileDesc);
                                    generateHTML(targetURL, filename, path, fileDesc);

                                }
                            }
                            catch
                            {
                                WriteMessageToLog("Error while generating file " + fileDesc + "");
                                continue;
                            }
                        }
                    }
                }

            }

            WriteMessageToLog("File generation complete at " + System.DateTime.Now);
        }
    }
}
