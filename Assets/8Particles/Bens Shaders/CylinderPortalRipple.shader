// Shader for a rippling, glowing portal effect on a cylinder mesh
// Ripples expand outwards from the object center (0,0,0)
// Includes two scrolling noise textures using cylindrical coordinate mapping
// Designed for Unity's Built-in Render Pipeline
Shader "Custom/CylinderPortalRippleCylindricalNoise" // Updated name
{
    Properties
    {
        _Color ("Primary Color", Color) = (0, 1, 1, 1)
        _Color2 ("Secondary Color", Color) = (1, 0, 1, 1)
        _GlowColor("Glow Color", Color) = (0.5, 0.5, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 2.0

        [Header(Ripples)]
        _RippleSpeed ("Speed", Float) = 1.0
        _RippleFrequency ("Frequency", Float) = 5.0
        _RippleAmplitude ("Amplitude", Range(0, 1)) = 0.3

        [Header(Noise Texture 1)]
        _NoiseTex1 ("Texture 1 (Grayscale)", 2D) = "white" {}
        _NoiseScale1 ("Scale 1 (Wrap, Height)", Vector) = (1.0, 1.0, 0, 0) // Scale for angle (X) and height (Y)
        _NoiseScrollSpeed1 ("Scroll Speed 1 (Angle, Height)", Vector) = (0.1, 0.05, 0, 0)

        [Header(Noise Texture 2)]
        _NoiseTex2 ("Texture 2 (Grayscale)", 2D) = "white" {}
        _NoiseScale2 ("Scale 2 (Wrap, Height)", Vector) = (1.2, 1.2, 0, 0)
        _NoiseScrollSpeed2 ("Scroll Speed 2 (Angle, Height)", Vector) = (-0.05, 0.08, 0, 0)

        [Header(Transparency)]
        _BaseAlpha ("Base Alpha", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // Define PI for calculations
            #define UNITY_PI 3.14159265359

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 objPos : TEXCOORD0; // Pass object space position
            };

            // Properties
            fixed4 _Color;
            fixed4 _Color2;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _RippleSpeed;
            float _RippleFrequency;
            float _RippleAmplitude;
            sampler2D _NoiseTex1;
            float4 _NoiseScale1; // Use float4 for Vector property (X=Wrap Scale, Y=Height Scale)
            float4 _NoiseScrollSpeed1;
            sampler2D _NoiseTex2;
            float4 _NoiseScale2;
            float4 _NoiseScrollSpeed2;
            float _BaseAlpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // --- Ripple Calculation (same as before) ---
                float distanceFromCenter = length(i.objPos.xyz);
                float sineWave = sin((distanceFromCenter * _RippleFrequency) - (_Time.y * _RippleSpeed));
                float ripple01 = (sineWave * 0.5) + 0.5;
                float rippleModulator = lerp(1.0, ripple01, _RippleAmplitude);

                // --- Cylindrical Noise UV Calculation ---
                // Assumes cylinder length runs along the LOCAL Z-AXIS.
                // If your cylinder runs along Y, use atan2(i.objPos.z, i.objPos.x) and i.objPos.y for height.
                // If your cylinder runs along X, use atan2(i.objPos.y, i.objPos.z) and i.objPos.x for height.

                // Calculate angle around the Z-axis (using X and Y object positions)
                float angle = atan2(i.objPos.y, i.objPos.x); // Output is -PI to +PI

                // Normalize angle to 0-1 range for UV coordinate (U)
                float wrapCoord = (angle / (2.0 * UNITY_PI)) + 0.5;

                // Use the position along the cylinder axis (Z) as the other UV coordinate (V)
                float heightCoord = i.objPos.z;

                // Combine into base cylindrical coordinates
                float2 cylindricalCoords = float2(wrapCoord, heightCoord);

                // Calculate final UVs with scale and scroll for both textures
                // _NoiseScale.x scales the wrap (angle), _NoiseScale.y scales the height
                float2 noiseUV1 = cylindricalCoords * _NoiseScale1.xy + _NoiseScrollSpeed1.xy * _Time.y;
                float2 noiseUV2 = cylindricalCoords * _NoiseScale2.xy + _NoiseScrollSpeed2.xy * _Time.y;

                // Sample noise textures (ensure Wrap Mode is "Repeat")
                fixed noiseVal1 = tex2D(_NoiseTex1, noiseUV1).r;
                fixed noiseVal2 = tex2D(_NoiseTex2, noiseUV2).r;

                // Combine the noise values
                fixed combinedNoise = noiseVal1 * noiseVal2;

                // Blend between _Color and _Color2 based on the combined noise value
                fixed3 blendedColor = lerp(_Color.rgb, _Color2.rgb, combinedNoise);

                // --- Final Color and Emission ---
                fixed4 finalColor;
                finalColor.rgb = blendedColor * rippleModulator; // Apply ripple modulation
                fixed3 emission = _GlowColor.rgb * finalColor.rgb * _GlowIntensity; // Calculate emission
                finalColor.a = _BaseAlpha; // Apply base alpha

                // --- Output ---
                finalColor.rgb += emission; // Add emission glow
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
