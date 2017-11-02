using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System;
using Terraria.ID;
using System.Linq;
using System.Text;
using Terraria.UI.Chat;

namespace TooltipTranslator.UIElements
{
	public class UISlot : UIElement
	{
		public static string translatingString;
		public int sortOrder;
		public string key;
		public string value;
		public int height;

		public UISlot(int index, string key, string value)
		{
			this.sortOrder = index;
			this.key = key;
			this.value = value;
			height = (int)(Main.fontMouseText.MeasureString(key).Y * 2) + 8;
		}

        protected virtual void SetSlotSize()
        {
            this.Width.Set(TooltipTranslatorUI.instance.panelMain.Width.Pixels - 26, 0f);
			if (this.Width.Pixels < TooltipTranslatorUI.instance.panelMain.MinWidth.Pixels)
			{
				this.Width.Set(TooltipTranslatorUI.instance.panelMain.MinWidth.Pixels - 26, 0f);
			}
            this.Height.Set(height, 0f);
        }

        public override void Recalculate()
        {
            base.Recalculate();
			SetSlotSize();
		}

		public override int CompareTo(object obj)
        {
            int result = sortOrder < (obj as UISlot).sortOrder ? -1 : 1;
            return result;
        }

		public override void RightClick(UIMouseEvent evt)
		{
			TooltipTranslator.instance.translat.Remove(key);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
        {
			try
			{
				CalculatedStyle dimensions = base.GetInnerDimensions();
				Rectangle rect = dimensions.ToRectangle();
				Vector2 pos = dimensions.Position();
				if (IsMouseHovering)
				{
					rect.Height -= 3;
					spriteBatch.Draw(Main.magicPixel, rect, Color.Yellow * 0.6f);
				}
				else if (translatingString == key)
				{
					rect.Height -= 3;
					spriteBatch.Draw(Main.magicPixel, rect, Color.Green * 0.6f);
				}

				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, key, pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero, 1f);
				pos.Y += Main.fontMouseText.MeasureString(key).Y + 4;
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, value, pos.X, pos.Y, Color.White, Color.Black, Vector2.Zero, 1f);
				pos.Y += Main.fontMouseText.MeasureString(key).Y + 2;

				rect.Y = (int)pos.Y;
				rect.Height = 1;
				spriteBatch.Draw(Main.magicPixel, rect, Color.Black);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write(ex.Message);
			}
		}
	}
}
