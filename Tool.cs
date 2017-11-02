using System;
using Terraria;
using Terraria.UI;
using TooltipTranslator.UIElements;

namespace TooltipTranslator
{
    // UIState needs UserInterface for Scrollbar fixes
    // Tool should store data? does it even matter?
    abstract class Tool
    {
        internal bool visible;
        internal UserInterface userInterface;
        internal UIModState uistate;
        internal static string tooltip;

        public Tool(Type uistateType)
        {
            userInterface = new UserInterface();
            uistate = (UIModState)Activator.CreateInstance(uistateType, new object[] { userInterface });
            uistate.Activate();
            userInterface.SetState(uistate);
        }

        /// <summary>
        /// Initializes this Tool. Called during Load.
        /// Useful for initializing data.
        /// </summary>
        internal virtual void Initialize()
        {
        }

        /// <summary>
        /// Initializes this Tool. Called during Load after Initialize only on SP and Clients.
        /// Useful for initializing UI.
        /// </summary>
        internal virtual void ClientInitialize() { }

        internal virtual void ScreenResolutionChanged()
        {
            userInterface?.Recalculate();
        }

        internal virtual void UIUpdate()
        {
            if (visible)
            {
                tooltip = string.Empty;
                userInterface?.Update(Main._drawInterfaceGameTime);
            }
        }

        internal virtual void UIDraw()
        {
            if (visible)
            {
                uistate.Draw(Main.spriteBatch);
            }
        }

        internal virtual void TooltipDraw()
        {
            if (visible && !string.IsNullOrEmpty(tooltip))
            {
                Main.hoverItemName = tooltip;
            }
        }

        internal virtual void DrawUpdateToggle() { }

        internal virtual void Toggled() { }

        internal virtual void PostSetupContent()
        {
            if (!Main.dedServ)
            {

            }
        }
    }
}
