using UnityEngine;

namespace VRGIN.Visuals
{
    public interface IMaterialPalette
    {
        Material Sprite { get; }

        Material Unlit { get; }

        Material UnlitTransparent { get; }

        Material UnlitTransparentCombined { get; }

        Shader StandardShader { get; }
    }
}
