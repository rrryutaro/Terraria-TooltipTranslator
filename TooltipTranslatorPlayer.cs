using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TooltipTranslator
{
    class TooltipTranslatorPlayer : ModPlayer
    {
        private TagCompound uiData;

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
			if (TooltipTranslator.instance.ToggleHotKeyUI.JustPressed)
            {
				TooltipTranslatorUI.instance.ShowUI = !TooltipTranslatorUI.instance.ShowUI;
            }
        }

        public override TagCompound Save()
        {
            return new TagCompound
            {
                ["TooltipTranslatorUI"] = TooltipTranslatorUI.instance.Save(),
            };
        }

        public override void Load(TagCompound tag)
        {
            if (tag.ContainsKey("TooltipTranslatorUI"))
            {
                if (tag.Get<object>("TooltipTranslatorUI").GetType().Equals(typeof(TagCompound)))
                {
                    uiData = tag.Get<TagCompound>("TooltipTranslatorUI");
                }
            }
        }

        public override void OnEnterWorld(Player player)
        {
			TooltipTranslatorUI.instance.InitializeUI();
            if (uiData != null)
            {
				TooltipTranslatorUI.instance.Load(uiData);
            }
        }
    }
}
