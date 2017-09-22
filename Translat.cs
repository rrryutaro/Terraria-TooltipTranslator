using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TooltipTranslator
{
    public class Translat
    {
        private WebBrowser web;
        private string sourceLang;
        private string resultLang;
        private Dictionary<string, string> dic;
        private List<string> list;
        private bool isLoaded;
        private bool isRunning;

        public Translat(string sourceLang, string resultLang)
        {
            this.sourceLang = sourceLang;
            this.resultLang = resultLang;

            web = new WebBrowser();
            web.ScriptErrorsSuppressed = true;
            web.Navigate($"https://translate.google.co.jp/#{sourceLang}/{resultLang}");
            web.DocumentCompleted += (a, b) => isLoaded = true;

            dic = new Dictionary<string, string>();
            list = new List<string>();
        }

        public void Reset()
        {
            if (0 < list.Count && isRunning)
            {
                isRunning = false;
            }
        }

        public string Translation(string src)
        {
            string result = string.Empty;

            if (!dic.ContainsKey(src))
            {
                dic.Add(src, "");
                list.Add(src);
            }
            else
            {
                result = dic[src];
            }

            if (isLoaded && 0 < list.Count && !isRunning)
            {
                isRunning = true;
                Task.Run(() =>
                {
                    try
                    {
                        while (0 < list.Count)
                        {
                            string str = list[0];

                            var source = GetElementById("source");
                            var submit = GetElementById("gt-submit");
                            var result_box = GetElementById("result_box");

                            source.InnerText = str;
                            submit.InvokeMember("click");

                            while (true)
                            {
                                Task.Delay(10);
                                if (result_box.InnerText != null && !result_box.InnerText.Equals("翻訳しています..."))
                                {
                                    dic[str] = result_box.InnerText;
                                    result_box.InnerText = null;
                                    list.RemoveAt(0);
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    isRunning = false;
                });
            }

            return result;
        }

        private HtmlElement GetElementById(string id)
        {
            HtmlElement result = null;
            if (web.InvokeRequired)
            {
                web.Invoke((MethodInvoker)delegate () { result = GetElementById(id); });
            }
            else
            {
                result = web.Document.GetElementById(id);
            }
            return result;
        }
    }
}
