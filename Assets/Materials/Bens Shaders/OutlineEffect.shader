Shader "Hidden/OutlineEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineThickness ("Outline Thickness", Range(0, 5)) = 1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _DepthThreshold ("Depth Threshold", Range(0.01, 0.5)) = 0.05
        _NormalThreshold ("Normal Threshold", Range(0.1, 1)) = 0.5
    }
    
    SubShader
    {
        // No culling or depth
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
            float _OutlineThickness;
            float4 _OutlineColor;
            float _DepthThreshold;
            float _NormalThreshold;
            
            // Edge detection using depth and normals
            float DetectEdges(float2 uv)
            {
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
                return (depthDiff > _DepthThreshold || normalDiff > _NormalThreshold) ? 1.0 : 0.0;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                
                // Detect edges
                float edge = DetectEdges(i.uv);
                
                // Blend original color with outline
                return lerp(col, _OutlineColor, edge * _OutlineColor.a);
            }
            ENDCG
        }
    }
}