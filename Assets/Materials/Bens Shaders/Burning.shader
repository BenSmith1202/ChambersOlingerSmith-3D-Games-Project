Shader "Custom/BurningGround"
{
    Properties
    {
        _AlbedoTex ("Albedo Texture", 2D) = "white" {}
        _CrackMask ("Crack Mask", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (1,0.5,0,1)
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 3
        _ScrollSpeed ("Scroll Speed", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _AlbedoTex;
        sampler2D _CrackMask;
        sampler2D _NoiseTex;
        float4 _EmissionColor;
        float _EmissionStrength;
        float4 _ScrollSpeed;

        struct Input
        {
            float2 uv_AlbedoTex;
            float2 uv_CrackMask;
            float2 uv_NoiseTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 noiseUV = IN.uv_NoiseTex + _Time.y * _ScrollSpeed.xy;

            // Sample base textures
            fixed4 albedo = tex2D(_AlbedoTex, IN.uv_AlbedoTex);
            fixed crack = tex2D(_CrackMask, IN.uv_CrackMask).r;
            fixed noise = tex2D(_NoiseTex, noiseUV).r;

            // Base color
            o.Albedo = albedo.rgb;

            // Emission based on cracks * animated noise
            float emissionMask = crack * noise;
            o.Emission = _EmissionColor.rgb * emissionMask * _EmissionStrength;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
