using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TooltipTranslator
{
	public enum TranslatingSite
	{
		Google = 1,
		Baidu = 2
	};

	public class Translat
    {
		private static Regex regKeyString = new Regex(@".* \((\d*)\)$");

		private WebBrowser web;
        private string sourceLang;
        private string resultLang;
		private TranslatingSite translatingSite;
		private Dictionary<string, string> dic;
        private List<string> list;
		private List<string> sortList;
		private bool isLoaded;
        private bool isRunning;
		private bool isCancel;

        public Translat(TranslatingSite translatingSite, string sourceLang, string resultLang)
        {
			Reset(translatingSite, sourceLang, resultLang);
		}

		public void Reset(TranslatingSite translatingSite, string sourceLang, string resultLang)
		{
			if (this.translatingSite != translatingSite || this.sourceLang != sourceLang || this.resultLang != resultLang)
			{
				this.translatingSite = translatingSite;
				this.sourceLang = sourceLang;
				this.resultLang = resultLang;

				web = new WebBrowser();
				web.ScriptErrorsSuppressed = true;
				string url = $"https://translate.google.co.jp/#{sourceLang}/{resultLang}";
				if (translatingSite == TranslatingSite.Baidu)
				{
					url = Config.urlBaidu;
				}
				web.Navigate(url);
				web.DocumentCompleted += (a, b) => isLoaded = true;

				dic = new Dictionary<string, string>();
				list = new List<string>();
				sortList = new List<string>();
			}
		}

		public Dictionary<string, string> TranslatDictionary
		{
			get
			{
				return dic;
			}
		}

		public List<string> SortList
		{
			get
			{
				return sortList;
			}
		}

		public bool IsRunning
		{
			get
			{
				return isRunning;
			}
		}

		public int TranslatingCount
		{
			get
			{
				return list.Count;
			}
		}

		public string TranslatingString
		{
			get
			{
				string result = string.Empty;
				if (0 < list.Count)
				{
					result = list[0];
				}
				return result;
			}
		}

		public void Add(string key, string value)
		{
			sortList.Add(key);
			dic.Add(key, value);
			TooltipTranslatorUI.instance.updateNeeded = true;
		}

		public void Remove(string key)
		{
			if (dic.ContainsKey(key))
			{
				dic.Remove(key);
				list.Remove(key);
				sortList.Remove(key);
				TooltipTranslatorUI.instance.updateNeeded = true;
			}
		}

		public void Cancel()
		{
			list.Clear();
			isCancel = false;
		}

		public void Reload()
		{
			if (isRunning)
			{
				isCancel = true;
				int count = 0;
				while (true)
				{
					Task.Delay(10);
					if ((!isRunning && !isCancel) || 100 < count++)
					{
						break;
					}
				}
			}
			if (translatingSite == TranslatingSite.Baidu)
			{
				try
				{
					var result_box = GetElementById("main-outer");
					result_box = result_box.Children[0].Children[0].Children[0].Children[1].Children[0].Children[2].Children[0].Children[0].Children[0].Children[1];
					result_box.InnerText = null;
				}
				catch { }
			}

			var nullValues = dic.Where(x => string.IsNullOrEmpty(x.Value));
			if (nullValues != null)
			{
				list.Clear();
				foreach (var keyValue in nullValues)
				{
					sortList.Remove(keyValue.Key);
					sortList.Add(keyValue.Key);
					list.Add(keyValue.Key);
				}
				RunTranslating();
			}
			TooltipTranslatorUI.instance.updateNeeded = true;
		}

		private void TranslationCompleted(string key, string value)
		{
			dic[key] = value;
			TooltipTranslatorUI.instance.updateNeeded = true;
		}

		private string ConvertKeyString(string key)
		{
			string result = key;
			if (regKeyString.IsMatch(key))
			{
				result = key.Substring(0, key.LastIndexOf(" "));
			}
			return result;
		}

		private string GetValue(string key)
		{
			string result;
			if (regKeyString.IsMatch(key))
			{
				result = $"{dic[ConvertKeyString(key)]} ({regKeyString.Match(key).Groups[1].Value})";
			}
			else
			{
				result = dic[key];
			}
			return result;
		}

		public string Translation(string src)
		{
			string result = string.Empty;
			string key = ConvertKeyString(src);

			if (!dic.ContainsKey(key))
			{
				Add(key, "");
				list.Add(key);
				RunTranslating();
			}
			else
			{
				result = GetValue(src);
			}

			return result;
		}

		public void RunTranslating()
		{
			if (isLoaded && 0 < list.Count && !isRunning)
			{
				isRunning = true;
				if (translatingSite == TranslatingSite.Baidu)
				{
					TranslationBaidu();
				}
				else
				{
					TranslationGoogle();
				}
			}
		}

		public void TranslationGoogle()
		{
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
							if (isCancel)
							{
								Cancel();
								break;
							}

							Task.Delay(10);
							if (result_box.InnerText != null && !result_box.InnerText.Equals("翻訳しています..."))
							{
								TranslationCompleted(str, result_box.InnerText);
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

		public void TranslationBaidu()
		{
			Task.Run(() =>
			{
				try
				{
					while (0 < list.Count)
					{
						string str = list[0];

						var source = GetElementById("baidu_translate_input");
						var submit = GetElementById("translate-button");

						try
						{
							GetElementById("main-outer").Children[0].Children[0].Children[0].Children[1].Children[0].Children[2].Children[0].Children[0].Children[0].Children[1].InnerText = null; ;
						}
						catch { }
						
						source.InnerText = str;
						submit.InvokeMember("click");

						//isLoaded = false;
						//web.Navigate($"{Config.urlBaidu}/{str}");
						//while (true)
						//{
						//	if (isLoaded)
						//		break;
						//	else
						//		Task.Delay(10);
						//}
						//var result_box = GetElementById("main-outer");

						int count = 0;
						while (true)
						{
							if (isCancel)
							{
								Cancel();
								break;
							}

							try
							{
								Task.Delay(10);
								var result_box = GetElementById("main-outer").Children[0].Children[0].Children[0].Children[1].Children[0].Children[2].Children[0].Children[0].Children[0].Children[1];
								var text = result_box.InnerText;
								if (!string.IsNullOrEmpty(text))
								{
									if (0 <= text.IndexOf(Environment.NewLine))
										text = text.Replace(Environment.NewLine, " ");
									if (text.Substring(text.Length - 1).Equals(" "))
										text = text.Substring(0, text.Length - 1);
									TranslationCompleted(str, text);
									result_box.InnerText = null;
									break;
								}
								else
								{
									Task.Delay(10);
									count++;
								}
							}
							catch
							{
								Task.Delay(10);
							}
							if (100 < count++)
								break;
						}
						list.RemoveAt(0);
					}
				}
				catch { }
				isRunning = false;
			});
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
