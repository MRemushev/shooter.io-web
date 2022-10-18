using UnityEngine;

public static class MaterialObjectFade
{
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

    // ReSharper disable StringLiteralTypo
    public static void MakeFade(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt(ZWrite, 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
    
    public static void MakeOpaque(Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt(ZWrite, 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }
}
