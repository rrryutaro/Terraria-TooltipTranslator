	using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.GameInput;
using Terraria.ModLoader;
using FKTModSettings;

namespace TooltipTranslator
{
	class TooltipTranslator : Mod
	{
		private static string DictionaryFilePath = $@"{Main.SavePath}\Mods\Cache\TooltipTranslator.txt";

		internal static TooltipTranslator instance;

		internal ModHotKey ToggleHotKeyUI;
		internal Translat translat;
		internal TooltipTranslatorTool tool;

        public bool LoadedFKTModSettings = false;

        public string Translation(string str)
        {
            string result = string.Empty;
			try
			{
				if (translat == null)
				{
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

			if (!Main.dedServ)
			{
				ToggleHotKeyUI = RegisterHotKey("Toggle Show Translator UI", "Z");
				tool = new TooltipTranslatorTool();

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
		}

        public void CreateTranslat()
        {
			if (translat == null)
			{
				translat = new Translat(GetTranslatingSite(), LangIDToLangStr(Config.sourceLangID), LangIDToLangStr(Config.resultLangID));
				LoadTranslatDictionary();
			}
			else
			{
				translat.Reset(GetTranslatingSite(), LangIDToLangStr(Config.sourceLangID), LangIDToLangStr(Config.resultLangID));
			}
		}

		public void LoadTranslatDictionary()
		{
			if (Config.isLoadTranslat && System.IO.File.Exists(DictionaryFilePath))
			{
				try
				{
					foreach (var line in System.IO.File.ReadAllLines(DictionaryFilePath, Encoding.UTF8))
					{
						var keyValue = line.Split('\t');
						TooltipTranslator.instance.translat.Add(keyValue[0], keyValue[1]);
					}
				}
				catch { }
			}
		}

		public void SaveTranslatDictionary()
		{
			if (Config.isSaveTranslat)
			{
				try
				{
					using (var fs = new FileStream(DictionaryFilePath, FileMode.Create))
					using (var sw = new StreamWriter(fs, Encoding.UTF8))
					{
						sw.Write(string.Join(Environment.NewLine, TooltipTranslator.instance.translat.TranslatDictionary.Select(x => $"{x.Key}\t{x.Value}")));
					}
				}
				catch { }
			}
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

		private TranslatingSite GetTranslatingSite()
		{
			TranslatingSite result = TranslatingSite.Google;
			switch (Config.translatingSite)
			{
				case 1:
					result = TranslatingSite.Google;
					break;

				case 2:
					result = TranslatingSite.Baidu;
					break;
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
			setting.AddBool("isLoadTranslat", "Load translat file", false);
			setting.AddBool("isSaveTranslat", "Save translat file", false);
			setting.AddInt("translatingSite", "Translating Site", 1, 2, false);
			setting.AddComment($"Translating Site: 1:Google 2:Baidu{Environment.NewLine}Baidu url:{Environment.NewLine}{Config.urlBaidu}");
			setting.AddInt("sourceLangID", "Source language no", 0, 11, false);
            setting.AddInt("resultLangID", "Result language no", 1, 11, false);
			setting.AddComment("LanguageNo:" + Environment.NewLine + "0:Auto  1:English[en]  2:Japanese[ja]  3:German[de]  4:Italian[it]  5:French[fr]  6:Spanish[es]  7:Russian[ru]  8:Chinese(simplified)[zh-CN]  9:Chinese(Traditional)[zh-TW]  10:Portuguese[pt]  11:Polish[pl]");
        }

        private void UpdateModSettings()
        {
            ModSetting setting;
            if (ModSettingsAPI.TryGetModSetting(this, out setting))
            {
                setting.Get("isAnnounce", ref Config.isAnnounce);
                setting.Get("isTranslat", ref Config.isTranslat);
				setting.Get("isLoadTranslat", ref Config.isLoadTranslat);
				setting.Get("isSaveTranslat", ref Config.isSaveTranslat);
				setting.Get("translatingSite", ref Config.translatingSite);
				setting.Get("sourceLangID", ref Config.sourceLangID);
                setting.Get("resultLangID", ref Config.resultLangID);
            }
        }

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int layerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 1"));

			layers.Insert(layerIndex, new LegacyGameInterfaceLayer(
				"TooltipTranslator: UI",
				delegate
				{
					try
					{
						tool.UIUpdate();
						tool.UIDraw();
					}
					catch { }
					return true;
				},
				InterfaceScaleType.UI)
			);

			layerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (layerIndex != -1)
			{
				layers.Insert(layerIndex, new LegacyGameInterfaceLayer(
					"TooltipTranslator: Tooltip",
					delegate
					{
						try
						{
							tool.TooltipDraw();
						}
						catch { }
						return true;
					},
					InterfaceScaleType.UI)
				);
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
