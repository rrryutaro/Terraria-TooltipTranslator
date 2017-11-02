using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent.UI.Elements;
using TooltipTranslator.UIElements;

namespace TooltipTranslator
{
    class TooltipTranslatorUI : UIModState
	{
		static internal TooltipTranslatorUI instance;

		internal UIDragablePanel panelMain;
        internal UIHoverImageButton closeButton;
        internal UIPanel inlaidPanel;
		internal UIGrid grid;
		internal UIImageListButton btnTranslatOnOff;
		internal UIImageListButton btnReload;
		internal UIImageListButton btnSort;

		internal bool updateNeeded;

        internal string caption = $"TooltipTranslator v{TooltipTranslator.instance.Version} Count:0";

		static internal int menuIconSize = 28;
		static internal int menuMargin = 4;

		private bool showUI;
		public bool ShowUI
		{
			get { return showUI; }
			set
			{
				if (value)
				{
					Append(panelMain);
					if (TooltipTranslator.instance.translat == null)
					{
						TooltipTranslator.instance.CreateTranslat();
					}
					updateNeeded = true;
				}
				else
				{
					RemoveChild(panelMain);
				}
				showUI = value;

				TooltipTranslator.instance.tool.visible = value;
			}
		}

		public TooltipTranslatorUI(UserInterface ui) : base(ui)
		{
			instance = this;
		}

        public void InitializeUI()
        {
            RemoveAllChildren();

            panelMain = new UIDragablePanel(true, true, true);
            panelMain.caption = caption;
            panelMain.SetPadding(6);
            panelMain.Left.Set(400f, 0f);
            panelMain.Top.Set(400f, 0f);
            panelMain.Width.Set(350f, 0f);
			panelMain.MinWidth.Set(300f, 0f);
			panelMain.MaxWidth.Set(Main.screenWidth, 0f);
			panelMain.Height.Set(298, 0f);
			panelMain.MinHeight.Set(124, 0f);
			panelMain.MaxHeight.Set(Main.screenHeight, 0f);

			Texture2D texture = ModLoader.GetMod("TooltipTranslator").GetTexture("UIElements/closeButton");
			closeButton = new UIHoverImageButton(texture, "Close");
			closeButton.OnClick += (a, b) => ShowUI = false;
            closeButton.Left.Set(-20f, 1f);
			closeButton.Top.Set(6f, 0f);
			panelMain.Append(closeButton);

			inlaidPanel = new UIPanel();
			inlaidPanel.SetPadding(5);
			inlaidPanel.Top.Pixels = 32;
			inlaidPanel.Width.Set(0, 1f);
			inlaidPanel.Height.Set(-50, 1f);
			panelMain.Append(inlaidPanel);

			grid = new UIGrid();
			grid.Width.Set(-20f, 1f);
			grid.Height.Set(0, 1f);
			grid.ListPadding = 2f;
			inlaidPanel.Append(grid);

			var lootItemsScrollbar = new FixedUIScrollbar(userInterface);
			lootItemsScrollbar.SetView(100f, 1000f);
			lootItemsScrollbar.Height.Set(0, 1f);
			lootItemsScrollbar.Left.Set(-20, 1f);
			inlaidPanel.Append(lootItemsScrollbar);
			grid.SetScrollbar(lootItemsScrollbar);

			float topPos = 0;
			float leftPos = 0;

			texture = Main.itemTexture[ItemID.GPS].Resize(menuIconSize);
			btnTranslatOnOff = new UIImageListButton(
				new List<Texture2D>() { texture, texture },
				new List<object>() { true, false },
				new List<string>() { "Ascending order", "Descending order" },
				Config.isTranslat ? 0 : 1);
			btnTranslatOnOff.OnClick += (a, b) =>
			{
				btnTranslatOnOff.NextIamge();
				Config.isTranslat = btnTranslatOnOff.GetValue<bool>();
				if (Config.isAnnounce)
				{
					Main.NewText("Translation " + (Config.isTranslat ? "On" : "Off"));
				}
				btnTranslatOnOff.visibilityActive = btnTranslatOnOff.visibilityInactive = btnTranslatOnOff.GetValue<bool>() ? 1.0f : 0.4f;
			};
			btnTranslatOnOff.visibilityActive = btnTranslatOnOff.visibilityInactive = btnTranslatOnOff.GetValue<bool>() ? 1.0f : 0.4f;
			leftPos += menuMargin;
			btnTranslatOnOff.Left.Set(leftPos, 0f);
			btnTranslatOnOff.Top.Set(topPos, 0f);
			panelMain.Append(btnTranslatOnOff);

			texture = ModLoader.GetMod("TooltipTranslator").GetTexture("UIElements/reload").Resize(menuIconSize);
			btnReload = new UIImageListButton(
				new List<Texture2D>() { texture },
				new List<object>() { 0 },
				new List<string>() { "Reloading settings etc." },
				0);
			btnReload.OnClick += (a, b) =>
			{
				TooltipTranslator.instance.CreateTranslat();
				if (Config.isTranslat)
				{
					TooltipTranslator.instance.translat.Reload();
				}
				updateNeeded = true;
			};
			leftPos += menuIconSize + menuMargin;
			btnReload.Left.Set(leftPos, 0f);
			btnReload.Top.Set(topPos, 0f);
			panelMain.Append(btnReload);

			btnSort = new UIImageListButton(
				new List<Texture2D>() { Main.itemTexture[ItemID.AlphabetStatueA].Resize(menuIconSize), Main.itemTexture[ItemID.AlphabetStatueD].Resize(menuIconSize) },
				new List<object>() { SortOrder.Ascending, SortOrder.Descending },
				new List<string>() { "Ascending order", "Descending order" },
				1);
			btnSort.OnClick += (a, b) =>
			{
				btnSort.NextIamge();
				updateNeeded = true;
			};
			leftPos += menuIconSize + menuMargin;
			btnSort.Left.Set(leftPos, 0f);
			btnSort.Top.Set(topPos, 0f);
			panelMain.Append(btnSort);

			updateNeeded = true;
		}

		public SortOrder SortOrder
		{
			get
			{
				return btnSort.GetValue<SortOrder>();
			}
		}

		internal void UpdateGrid()
		{
			if (!updateNeeded) { return; }
			updateNeeded = false;

            grid.Clear();

			List<string> sortList = TooltipTranslator.instance.translat.SortList;
			Dictionary<string, string> dic = TooltipTranslator.instance.translat.TranslatDictionary;
			//for (int i = sortList.Count - 1; 0 <= i; i--)
			SortOrder sortOrder = SortOrder;
			for (int i = 0; i < sortList.Count; i++)
			{
				string key = sortList[i];
				int order = i;
				if (sortOrder == SortOrder.Descending)
					order = sortList.Count - i;
				var slot = new UISlot(order, key, dic[key]);
				grid._items.Add(slot);
				grid._innerList.Append(slot);
			}
			grid.UpdateOrder();
			grid._innerList.Recalculate();
			panelMain.caption = caption.Replace("Count:0", $"Count:{grid.Count}");
        }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
            UpdateGrid();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (TooltipTranslator.instance.translat.IsRunning)
			{
				var pos = btnSort.GetOuterDimensions().Position();
				pos.X += menuIconSize + menuMargin;
				string text = $"Translating: {TooltipTranslator.instance.translat.TranslatingCount}";
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero, 1f);

				UISlot.translatingString = TooltipTranslator.instance.translat.TranslatingString;
			}
			else
			{
				UISlot.translatingString = string.Empty;
			}
			base.Draw(spriteBatch);
		}

		public override TagCompound Save()
        {
            TagCompound result = base.Save();

            if (panelMain != null)
            {
                result.Add("position", panelMain.SavePositionJsonString());
            }
            return result;
        }

        public override void Load(TagCompound tag)
        {
            base.Load(tag);
            if (tag.ContainsKey("position"))
            {
                panelMain.LoadPositionJsonString(tag.GetString("position"));
            }
        }
    }

	public enum SortOrder
	{
		Ascending,
		Descending
	}
}
