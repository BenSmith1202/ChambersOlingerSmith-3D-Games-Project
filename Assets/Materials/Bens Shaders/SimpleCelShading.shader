Shader "Hidden/SimpleCelShading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLevels ("Color Levels", Range(2, 16)) = 4
        _EnableOutline ("Enable Outline", Float) = 1
        _OutlineThickness ("Outline Thickness", Range(0, 5)) = 1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _EdgeThreshold ("Edge Threshold", Range(0.01, 0.5)) = 0.05
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            sampler2D _CameraDepthNormalsTexture;
            float4 _MainTex_TexelSize;
            float _ColorLevels;
            float _EnableOutline;
            float _OutlineThickness;
            float4 _OutlineColor;
            float _EdgeThreshold;
            
            // Simple color posterization/banding function
            float3 PosterizeColors(float3 color)
            {
                return floor(color * _ColorLevels) / (_ColorLevels - 1.0);
            }
            
            // Edge detection using depth and normals
            float DetectEdges(float2 uv)
            {
                if (_EnableOutline < 0.5) return 0;
                
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineThickness;
                
                // Sample depth-normals at surrounding pixels
                float4 center = tex2D(_CameraDepthNormalsTexture, uv);
                float4 up = tex2D(_CameraDepthNormalsTexture, uv + float2(0, texelSize.y));
                float4 down = tex2D(_CameraDepthNormalsTexture, uv - float2(0, texelSize.y));
                float4 left = tex2D(_CameraDepthNormalsTexture, uv - float2(texelSize.x, 0));
                float4 right = tex2D(_CameraDepthNormalsTexture, uv + float2(texelSize.x, 0));
                
                // Decode normals and depth
                float3 centerNormal;
                float centerDepth;
                DecodeDepthNormal(center, centerDepth, centerNormal);
                
                float3 upNormal, downNormal, leftNormal, rightNormal;
                float upDepth, downDepth, leftDepth, rightDepth;
                
                DecodeDepthNormal(up, upDepth, upNormal);
                DecodeDepthNormal(down, downDepth, downNormal);
                DecodeDepthNormal(left, leftDepth, leftNormal);
                DecodeDepthNormal(right, rightDepth, rightNormal);
                
                // Calculate differences
                float depthDiff = max(
                    abs(centerDepth - upDepth),
                    max(abs(centerDepth - downDepth),
                    max(abs(centerDepth - leftDepth),
                    abs(centerDepth - rightDepth)))
                );
                
                float normalDiff = max(
                    1.0 - dot(centerNormal, upNormal),
                    max(1.0 - dot(centerNormal, downNormal),
                    max(1.0 - dot(centerNormal, leftNormal),
                    1.0 - dot(centerNormal, rightNormal)))
                );
                
                // Return 1.0 if we detect an edge, 0.0 otherwise
                return (depthDiff > _EdgeThreshold || normalDiff > 0.5) ? 1.0 : 0.0;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Sample the original color
                float4 col = tex2D(_MainTex, i.uv);
                
                // Apply color banding
                float3 bandedColor = PosterizeColors(col.rgb);
                
                // Detect edges for outlines
                float edge = DetectEdges(i.uv);
                
                // Combine effects - apply outline over the banded colors
                return float4(lerp(bandedColor, _OutlineColor.rgb, edge * _OutlineColor.a), col.a);
            }
            ENDCG
        }
    }
}