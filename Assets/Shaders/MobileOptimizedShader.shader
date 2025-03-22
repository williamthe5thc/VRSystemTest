Shader "VRInterviewSystem/MobileOptimized" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [Toggle] _UseEmission ("Use Emission", Float) = 0
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        [Toggle] _VertexColorTint ("Vertex Color Tint", Float) = 0
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 150
        
        CGPROGRAM
        // Use physically based standard lighting model
        // We use "surface" shader with custom lighting model optimized for mobile
        #pragma surface surf MobileSpecular fullforwardshadows
        #pragma target 3.0
        #pragma multi_compile _ _USEEMISSION_ON
        #pragma multi_compile _ _VERTEXCOLORTINT_ON
        
        sampler2D _MainTex;
        
        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
        };
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _EmissionColor;
        
        // Simplified specular lighting model for mobile
        half4 LightingMobileSpecular(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
            half3 h = normalize(lightDir + viewDir);
            half diff = max(0, dot(s.Normal, lightDir));
            float nh = max(0, dot(s.Normal, h));
            float spec = pow(nh, s.Specular * 128.0) * s.Gloss;
            
            half4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
            c.a = s.Alpha;
            return c;
        }
        
        void surf(Input IN, inout SurfaceOutput o) {
            // Sample the texture
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Apply vertex color tinting if enabled
            #ifdef _VERTEXCOLORTINT_ON
                c *= IN.color;
            #endif
            
            o.Albedo = c.rgb;
            
            // We map metallic to specular power and glossiness to gloss
            o.Specular = _Metallic;
            o.Gloss = _Glossiness;
            o.Alpha = c.a;
            
            // Add emission if enabled
            #ifdef _USEEMISSION_ON
                o.Emission = _EmissionColor.rgb;
            #endif
        }
        ENDCG
    }
    
    // Fallback to a simpler shader for low-end devices
    FallBack "Mobile/Diffuse"
}

// Second shader for transparent materials
Shader "VRInterviewSystem/MobileTransparent" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 150
        
        CGPROGRAM
        // Physically based standard lighting model, alpha blended
        #pragma surface surf Lambert alpha:fade
        #pragma target 3.0
        
        sampler2D _MainTex;
        
        struct Input {
            float2 uv_MainTex;
        };
        
        half _Glossiness;
        fixed4 _Color;
        half _Alpha;
        
        void surf(Input IN, inout SurfaceOutput o) {
            // Sample the texture
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb;
            o.Alpha = c.a * _Alpha;
        }
        ENDCG
    }
    
    FallBack "Mobile/VertexLit"
}

// Third shader for highlighting interactive objects
Shader "VRInterviewSystem/Highlight" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _HighlightColor ("Highlight Color", Color) = (0,1,1,1)
        _HighlightIntensity ("Highlight Intensity", Range(0,1)) = 0.5
        _PulseSpeed ("Pulse Speed", Range(0,10)) = 1.0
    }
    
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 150
        
        CGPROGRAM
        #pragma surface surf Lambert
        #pragma target 3.0
        
        sampler2D _MainTex;
        
        struct Input {
            float2 uv_MainTex;
        };
        
        fixed4 _Color;
        fixed4 _HighlightColor;
        half _HighlightIntensity;
        half _PulseSpeed;
        
        void surf(Input IN, inout SurfaceOutput o) {
            // Sample the texture
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Calculate pulsing highlight effect
            half pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5) * _HighlightIntensity;
            
            // Mix base color with highlight color
            o.Albedo = lerp(c.rgb, _HighlightColor.rgb, pulse);
            o.Alpha = c.a;
            
            // Add some emission for the glow effect
            o.Emission = _HighlightColor.rgb * pulse;
        }
        ENDCG
    }
    
    FallBack "Mobile/Diffuse"
}

// Fourth shader for UI elements with better visibility in VR
Shader "VRInterviewSystem/UI" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EdgeFalloff ("Edge Visibility", Range(0,5)) = 1.0
    }
    
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float3 viewDir : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _EdgeFalloff;
            
            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                
                // Calculate view direction and normal for edge enhancement
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.normal = UnityObjectToWorldNormal(float3(0, 0, -1)); // Assuming UI facing camera
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                
                // Calculate edge enhancement based on view angle
                // This makes UI more visible when viewed from angles
                float edgeFactor = 1.0 - pow(abs(dot(normalize(i.viewDir), i.normal)), _EdgeFalloff);
                col.rgb = lerp(col.rgb, col.rgb * 1.5, edgeFactor * 0.5);
                
                return col;
            }
            ENDCG
        }
    }
}

// Fifth shader for world-space UI with edge visibility
Shader "VRInterviewSystem/WorldSpaceUI" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
    }
    
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _RimColor;
            float _RimPower;
            
            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Calculate view direction and normal for rim lighting
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.normal = v.normal;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Calculate rim effect (makes edges more visible)
                half rim = 1.0 - saturate(dot(normalize(i.viewDir), i.normal));
                rim = pow(rim, _RimPower);
                
                col.rgb = lerp(col.rgb, _RimColor.rgb, rim * _RimColor.a);
                
                return col;
            }
            ENDCG
        }
    }
}