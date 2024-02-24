﻿Shader "Weather/CRT Diffusion 2D"
{
    Properties
    {
        _dt("TimeStep", Range(0.0001, 1)) = 0.1 
        _PressWeight("Pressure Weight", Range(0.01, 5.0)) = 1
        _HumidWeight("Humidity Weight", Range(0.01, 5.0)) = 1
        _TempWeight("Temperature Weight", Range(0.01, 5.0)) = 0.1
    }

    CGINCLUDE

    #include "UnityCustomRenderTexture.cginc"
    
    #define A(U)  tex2D(_SelfTexture2D, float2(U))

    float _dt;
    float _PressWeight;
    float _HumidWeight;
    float _TempWeight;

    float4 frag(v2f_customrendertexture i) : SV_Target
    {
        float2 uv = i.globalTexcoord;

        float du = 1.0 / _CustomRenderTextureWidth;
        float dv = 1.0 / _CustomRenderTextureHeight;
        float4 duv = float4(du, dv, 0 ,0);

        // Cell contains r= press, g= humid, b=temp
        float4 cell = A(uv); 
        float4 cellUp = A(uv + duv.wy);
        float4 cellDwn = A(uv - duv.wy);
        float4 cellRgt = A(uv + duv.xw);
        float4 cellLft = A(uv - duv.xw);

        /*  float newPressurePascal = currentData.pressurePascal + 0.1f * (averageSurroundingPressure - currentData.pressurePascal);
            float newHumidity = currentData.humidity + 1f * (averageSurroundingHumidity - currentData.humidity);
            float newTemperature = currentData.temperatureKelvin + 1f * (averageSurroundingTemperature - currentData.temperatureKelvin); */

        float4 average = (cellUp + cellDwn + cellLft + cellRgt)*0.25;
        return float4(  cell.r + _dt * _PressWeight*(average.r - cell.r),
                        cell.g + _dt * _HumidWeight*(average.g - cell.g), 
                        cell.b + _dt * _TempWeight*(average.b - cell.b),
                        cell.a );
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "Update"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            ENDCG
        }
    }
}