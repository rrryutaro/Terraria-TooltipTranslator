using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;

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
                config.Get("sourceLangID", ref sourceLangID);
                config.Get("resultLangID", ref resultLangID);
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
            config.Put("sourceLangID", sourceLangID);
            config.Put("resultLangID", resultLangID);
            config.Save();
        }

        public static bool isAnnounce = true;
        public static bool isTranslat = true;
        public static int sourceLangID = 1;
        public static int resultLangID = 2;

        public static bool isReset = false;
    }
}
