using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;

namespace TooltipTranslator.UIElements
{
    internal class UIImageListButton : UIElement
    {
        private List<Texture2D> _textures;
        private List<string> _hoverTexts;
        private List<object> _values;
        internal float visibilityActive = 1f;
        internal float visibilityInactive = 0.4f;
        internal int Index { get; set; }

        public T GetValue<T>()
        {
            T result = (T)_values[Index];
            return result;

        }

        public UIImageListButton(List<Texture2D> textures, List<object> values, List<string> hoverTexts, int defaultIndex = 0)
        {
            this._textures = textures;
            this._values = values;
            this._hoverTexts = hoverTexts;
            this.Width.Set((float)this._textures[0].Width, 0f);
            this.Height.Set((float)this._textures[0].Height, 0f);
            Index = defaultIndex;
        }

        public void AddImage(Texture2D texture)
        {
            this._textures.Add(texture);
        }

        private static Rectangle GetCenterPosition(Rectangle rect, Texture2D texture)
        {
            Rectangle result = rect;
            if (texture.Width < rect.Width || texture.Height < rect.Height)
            {
                result.X += rect.Width / 2 - texture.Width / 2;
                result.Y += rect.Height / 2 - texture.Height / 2;
                result.Width = texture.Width;
                result.Height = texture.Height;
            }
            return result;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this._textures[Index], GetCenterPosition(GetDimensions().ToRectangle(), this._textures[Index]), Color.White * (base.IsMouseHovering ? this.visibilityActive : this.visibilityInactive));
            if (IsMouseHovering)
            {
				if (_hoverTexts.Count == 1)
					Tool.tooltip = _hoverTexts[Index];
				else
					Tool.tooltip = $"Current:{_hoverTexts[Index]}{Environment.NewLine}Next:{GetNextTooltip()}";

			}
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            Main.PlaySound(12, -1, -1, 1, 1f, 0f);
        }

        public void SetVisibility(float whenActive, float whenInactive)
        {
            this.visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
            this.visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
        }

        public int NextIamge()
        {
            Index = GetNextImageIndex();
            return Index;
        }

        public int PrevIamge()
        {
            Index = GetPrevImageIndex();
            return Index;
        }

        private int GetNextImageIndex()
        {
            int result = Index + 1;
            if (_textures.Count <= result)
            {
                result = 0;
            }
            return result;
        }

        private int GetPrevImageIndex()
        {
            int result = Index - 1;
            if (result < 0)
            {
                result = _textures.Count - 1;
            }
            return result;
        }

        private string GetNextTooltip()
        {
            string result = _hoverTexts[GetNextImageIndex()];
            return result;
        }
    }
}
