﻿#if XNA_SHIMS
namespace Microsoft.Xna.Framework.Input
{
    public struct Mouse
    {
        public static MouseState GetState()
        {
            return default(MouseState);
        }
    }
}
#endif