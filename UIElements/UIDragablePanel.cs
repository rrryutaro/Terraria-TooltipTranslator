using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.UI;
using Newtonsoft.Json;

namespace TooltipTranslator.UIElements
{
	class UIDragablePanel : UIPanel
	{
		private static Texture2D dragTexture;
		private Vector2 offset;
		private bool dragable;
		private bool dragging;
		private bool resizeableX;
		private bool resizeableY;
		private bool resizeable => resizeableX || resizeableY;
		private bool resizeing;
		private List<UIElement> additionalDragTargets;

        public string caption;

		public UIDragablePanel(bool dragable = true, bool resizeableX = false, bool resizeableY = false)
		{
			this.dragable = dragable;
			this.resizeableX = resizeableX;
			this.resizeableY = resizeableY;
			if (dragTexture == null)
			{
				dragTexture = TextureManager.Load("Images/UI/PanelBorder");
			}
			additionalDragTargets = new List<UIElement>();
		}

		public void AddDragTarget(UIElement element)
		{
			additionalDragTargets.Add(element);
		}
        public void DragTargetClear()
        {
            additionalDragTargets.Clear();
        }

        public virtual bool IsLock()
        {
            bool result = false;
            return result;
        }

        public override void MouseDown(UIMouseEvent evt)
		{
			DragStart(evt);
			base.MouseDown(evt);
		}

		public override void MouseUp(UIMouseEvent evt)
		{
			DragEnd(evt);
			base.MouseUp(evt);
		}

		private void DragStart(UIMouseEvent evt)
		{
			CalculatedStyle innerDimensions = GetInnerDimensions();
			if (!IsLock() && (evt.Target == this || additionalDragTargets.Contains(evt.Target)))
			{
				if (resizeable && new Rectangle((int)(innerDimensions.X + innerDimensions.Width - 12), (int)(innerDimensions.Y + innerDimensions.Height - 12), 12 + 6, 12 + 6).Contains(evt.MousePosition.ToPoint()))
				{
					offset = new Vector2(evt.MousePosition.X - innerDimensions.X - innerDimensions.Width - 6, evt.MousePosition.Y - innerDimensions.Y - innerDimensions.Height - 6);
					resizeing = true;
				}
				else if (dragable)
				{
					offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
					dragging = true;
				}
			}
		}

		private void DragEnd(UIMouseEvent evt)
		{
			dragging = false;
			resizeing = false;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = base.GetOuterDimensions();
			if (ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.showItemIcon = false;
				Main.ItemIconCacheUpdate(0);
			}
			if (dragging)
			{
				Left.Set(Main.MouseScreen.X - offset.X, 0f);
				Top.Set(Main.MouseScreen.Y - offset.Y, 0f);
				Recalculate();
			}
			if (resizeing)
			{
				if (resizeableX)
				{
					Width.Pixels = Main.MouseScreen.X - dimensions.X - offset.X;
				}
				if (resizeableY)
				{
					Height.Pixels = Main.MouseScreen.Y - dimensions.Y - offset.Y;
				}
				Recalculate();
			}
            base.DrawSelf(spriteBatch);
			if (resizeable)
			{
				DrawDragAnchor(spriteBatch, dragTexture, this.BorderColor);
			}

            //DrawCaption
            if (!string.IsNullOrEmpty(caption))
            {
                dimensions = base.GetOuterDimensions();
                float fontHeight = Main.fontMouseText.MeasureString(caption).Y;
                spriteBatch.DrawString(Main.fontMouseText, caption, dimensions.Position().Offset(7, dimensions.Height - fontHeight - 4), Color.Wheat);
            }
        }

        private void DrawDragAnchor(SpriteBatch spriteBatch, Texture2D texture, Color color)
		{
			CalculatedStyle dimensions = GetDimensions();

			Point point = new Point((int)(dimensions.X + dimensions.Width - 12), (int)(dimensions.Y + dimensions.Height - 12));
			spriteBatch.Draw(texture, new Rectangle(point.X - 2, point.Y - 2, 12 - 2, 12 - 2), new Rectangle(12 + 4, 12 + 4, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X - 4, point.Y - 4, 12 - 4, 12 - 4), new Rectangle(12 + 4, 12 + 4, 12, 12), color);
			spriteBatch.Draw(texture, new Rectangle(point.X - 6, point.Y - 6, 12 - 6, 12 - 6), new Rectangle(12 + 4, 12 + 4, 12, 12), color);
		}

        public string SavePositionJsonString()
        {
            string result = string.Empty;

            var pos = new CalculatedStyle();
            pos.Y = Top.Pixels;
            pos.X = Left.Pixels;
            pos.Width = Width.Pixels;
            pos.Height = Height.Pixels;
            result = JsonConvert.SerializeObject(pos);

            return result;
        }
        public void LoadPositionJsonString(string jsonString)
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                var pos = JsonConvert.DeserializeObject<CalculatedStyle>(jsonString);
                Top.Pixels = pos.Y;
                Left.Pixels = pos.X;
                Width.Pixels = pos.Width;
                Height.Pixels = pos.Height;

                if (Main.screenHeight < Top.Pixels)
                {
                    Top.Pixels = Main.screenHeight - Height.Pixels;
                }
                if (Main.screenWidth < Left.Pixels)
                {
                    Left.Pixels = Main.screenWidth - Width.Pixels;
                }

                Recalculate();
            }
        }
    }
}
