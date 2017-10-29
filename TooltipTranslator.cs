using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using FKTModSettings;

namespace TooltipTranslator
{
	class TooltipTranslator : Mod
	{
        internal static TooltipTranslator instance;

        internal ModHotKey ToggleHotKeyTranslat;
        internal Translat translat;
        private int sourceLangID;
        private int resultLangID;

        public bool LoadedFKTModSettings = false;

        public string Translation(string str)
        {
            string result = string.Empty;
			try
			{
				if (translat == null || sourceLangID != Config.sourceLangID || resultLangID != Config.resultLangID)
				{
					sourceLangID = Config.sourceLangID;
					resultLangID = Config.resultLangID;
					CreateTranslat();
				}
				result = translat.Translation(str);
			}
			catch { }
            return result;
        }

        public TooltipTranslator()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}

        public override void Load()
        {
            instance = this;
            ToggleHotKeyTranslat = RegisterHotKey("Toggle Translator", "Z");

            Config.LoadConfig();
            LoadedFKTModSettings = ModLoader.GetMod("FKTModSettings") != null;
            try
            {
                if (LoadedFKTModSettings)
                {
                    LoadModSettings();
                }
            }
            catch { }
        }

        private void CreateTranslat()
        {
            translat = new Translat(LangIDToLangStr(Config.sourceLangID), LangIDToLangStr(Config.resultLangID));
        }

        private string LangIDToLangStr(int langID)
        {
            string result = "en";
            switch (langID)
            {
                case 0: result = "auto"; break;
                case 1: result = "en"; break;
                case 2: result = "ja"; break;
                case 3: result = "de"; break;
                case 4: result = "it"; break;
                case 5: result = "fr"; break;
                case 6: result = "es"; break;
                case 7: result = "ru"; break;
                case 8: result = "zh-CN"; break;
                case 9: result = "zh-TW"; break;
                case 10: result = "pt"; break;
                case 11: result = "pl"; break;
            }
            return result;
        }

        public override void PreSaveAndQuit()
        {
            Config.SaveValues();
        }

        public override void PostUpdateInput()
        {
            try
            {
                if (LoadedFKTModSettings && !Main.gameMenu)
                {
                    UpdateModSettings();
                }
            }
            catch { }
        }

        private void LoadModSettings()
        {
            ModSetting setting = ModSettingsAPI.CreateModSettingConfig(this);
            setting.AddComment($"Tooltip Translator v{TooltipTranslator.instance.Version}");
            setting.AddBool("isAnnounce", "Switching announce", false);
            setting.AddBool("isTranslat", "Translat", false);
            setting.AddInt("sourceLangID", "Source language no", 0, 11, false);
            setting.AddInt("resultLangID", "Result language no", 1, 11, false);
            setting.AddComment("LanguageNo: 0:Auto 1:English[en] 2:Japanese[ja] 3:German[de] 4:Italian[it] 5:French[fr] 6:Spanish[es] 7:Russian[ru] 8:Chinese(simplified)[zh-CN] 9:Chinese(Traditional)[zh-TW] 10:Portuguese[pt] 11:Polish[pl]".Replace(" ", Environment.NewLine));
            setting.AddBool("isReset", "If it has not been translated," + Environment.NewLine + "translat it when switching", false);
        }

        private void UpdateModSettings()
        {
            ModSetting setting;
            if (ModSettingsAPI.TryGetModSetting(this, out setting))
            {
                setting.Get("isAnnounce", ref Config.isAnnounce);
                setting.Get("isTranslat", ref Config.isTranslat);
                setting.Get("sourceLangID", ref Config.sourceLangID);
                setting.Get("resultLangID", ref Config.resultLangID);
                setting.Get("isReset", ref Config.isReset);
            }
        }
    }

    class TranslatorPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (TooltipTranslator.instance.ToggleHotKeyTranslat.JustPressed)
            {
                Config.isTranslat = !Config.isTranslat;
                if (Config.isAnnounce)
                {
                    Main.NewText("Translation " + (Config.isTranslat ? "On" : "Off"));
                }
                if (Config.isTranslat && Config.isTranslat)
                {
                    TooltipTranslator.instance.translat.Reset();
                }
            }
        }
    }
    class TranslatorGlobalItem : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!Config.isTranslat)
                return;

            foreach (var tooltip in tooltips)
            {
                string text = TooltipTranslator.instance.Translation(tooltip.text);
                if (!string.IsNullOrEmpty(text))
                    tooltip.text = text;
            }
        }
    }
}
