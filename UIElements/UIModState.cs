using Terraria.UI;
using Terraria.ModLoader.IO;

namespace TooltipTranslator.UIElements
{
    class UIModState : UIState
	{
		internal UserInterface userInterface;

		public UIModState(UserInterface userInterface)
		{
			this.userInterface = userInterface;
		}

        public virtual TagCompound Save()
        {
            TagCompound result = new TagCompound();
            return result;
        }
        public virtual void Load(TagCompound tag)
        {
        }
    }
}
