using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace 查题宝
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Token检测
            Q.token = OperateIniFile.ReadIniData("Setting", "Token", "", Directory.GetCurrentDirectory() + @"\Config.ini");
            if (Q.token == "")
            {
                OperateIniFile.WriteIniData("Setting", "Token", "", Directory.GetCurrentDirectory() + @"\Config.ini");
                //MessageBox.Show("请在文件Config.ini中添加Token！", "提示：", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static class Q
        {
            //定义变量
            public static JObject temp;
            public static string token;
            public static string answerData;
            public static string answer;
        }

        public class OperateIniFile
        {
            [DllImport("kernel32")]//返回0表示失败，非0为成功

            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")]//返回取得字符串缓冲区的长度

            private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

            public static string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
            {
                if (File.Exists(iniFilePath))
                {
                    StringBuilder temp = new StringBuilder(1024);
                    GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                    return temp.ToString();
                }
                else
                {
                    return String.Empty;
                }
            }

            public static bool WriteIniData(string Section, string Key, string Value, string iniFilePath)
            {
                if (File.Exists(iniFilePath))
                {
                    long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                    if (OpStation == 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public JObject ReadJson(string json)
        {
            JObject jsondata = (JObject)JsonConvert.DeserializeObject(json);
            return jsondata;
        }

        public string DoHttp(string url, string method, string postdata)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url);
            request.Method = method;
            request.ReadWriteTimeout = 5000;
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            request.Headers.Add("Cache-Control:no-cache;max-age=0");
            if (TogglePoint.SelectedIndex == 0)
            {
                request.Headers.Add("Authorization:" + Q.token);
                //request.Headers.Add("Authorization:HhznEmsAhXTsivoI;");
            }
            byte[] data = Encoding.UTF8.GetBytes(postdata);
            request.ContentLength = data.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            return retString;
        }

        private void CheckBtn_Click(object sender, RoutedEventArgs e)
        {
            //JObject temp;
            var api = new[] {
                new {url = "http://cx.icodef.com/wyn-nb?v=2", postIssueParam = "question", keyParam = "data", method = "post"},
                new {url = "http://exam.tk/search", postIssueParam = "question", keyParam = "data", method = "post"},
                new {url = "http://onlinecoursekiller.online/OnlineCourseKiller/killer", postIssueParam = "q", keyParam = "answer", method = "post"}
            };
            try
            {
                Q.answerData = DoHttp(api[TogglePoint.SelectedIndex].url, api[TogglePoint.SelectedIndex].method, api[TogglePoint.SelectedIndex].postIssueParam + "=" + QuestionBox.Text);
                Q.temp = ReadJson(Q.answerData);
                Q.answer = Convert.ToString(Q.temp[api[TogglePoint.SelectedIndex].keyParam]);
            }
            catch (Exception)
            {
                MessageBox.Show("服务器出错！", "错误：", MessageBoxButton.OK, MessageBoxImage.Error);
                //throw;
            }

            switch (ShowMethod.SelectedIndex)
            {
                case 0:
                    AnswerBox.Text = Q.answer;
                    break;
                case 1:
                    AnswerBox.Text = Q.answerData;
                    break;
                default:
                    break;
            }
            //{ "url":"http://onlinecoursekiller.online/OnlineCourseKiller/killer","postIssueParam":"q","keyParam":"answer","method":"post"},
            //{ "url":"http://cx.icodef.com/wyn-nb?v=2","postIssueParam":"question","keyParam":"data","method":"post"}
        }

        private void ShowMethod_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (ShowMethod.SelectedIndex)
            {
                case 0:
                    AnswerBox.Text = Q.answer;
                    break;
                case 1:
                    AnswerBox.Text = Q.answerData;
                    break;
                default:
                    break;
            }
        }
    }
}
