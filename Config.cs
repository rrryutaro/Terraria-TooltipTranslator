using Terraria;
using Terraria.IO;

namespace TooltipTranslator
{
    public static class Config
    {
        private static string ConfigPath = $@"{Main.SavePath}\Mod Configs\TooltipTranslator.json";
		private static Preferences config;
        private static int version = 1;
        public static void LoadConfig()
        {
            config = new Preferences(ConfigPath);

            if (config.Load())
            {
                config.Get("version", ref version);
                config.Get("isAnnounce", ref isAnnounce);
                config.Get("isTranslat", ref isTranslat);
				config.Get("isLoadTranslat", ref isLoadTranslat);
				config.Get("isSaveTranslat", ref isSaveTranslat);
				config.Get("sourceLangID", ref sourceLangID);
                config.Get("resultLangID", ref resultLangID);
				config.Get("translatingSite", ref translatingSite);
				config.Get("urlBaidu", ref urlBaidu);
			}
            else
            {
                SaveValues();
            }
        }

        internal static void SaveValues()
        {
            config.Put("version", version);
            config.Put("isAnnounce", isAnnounce);
            config.Put("isTranslat", isTranslat);
			config.Put("isLoadTranslat", isLoadTranslat);
			config.Put("isSaveTranslat", isSaveTranslat);
			config.Put("sourceLangID", sourceLangID);
            config.Put("resultLangID", resultLangID);
			config.Put("translatingSite", translatingSite);
			config.Put("urlBaidu", urlBaidu);
			config.Save();

			TooltipTranslator.instance.SaveTranslatDictionary();
		}

        public static bool isAnnounce = true;
        public static bool isTranslat = true;
		public static bool isLoadTranslat = true;
		public static bool isSaveTranslat = true;
		public static int sourceLangID = 1;
        public static int resultLangID = 2;
		public static int translatingSite = 1;
		public static string urlBaidu = "http://fanyi.baidu.com/?aldtype=16047#auto/zh";
    }
}
