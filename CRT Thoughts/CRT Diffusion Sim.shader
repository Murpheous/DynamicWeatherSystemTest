﻿Shader "Weather/CRT Simulation"
{
    Properties
    {
        _dt("TimeStep", Range(0.0, 0.5)) = 1 
        _PressWeight("Pressure Rate", Range(0.01, 5.0)) = 1
        _HumidWeight("Humidity Rate", Range(0.01, 5.0)) = 1
        _TempWeight("Temperature Rate", Range(0.01, 5.0)) = 0.1
    }

    CGINCLUDE

    #include "UnityCustomRenderTexture.cginc"
    
    #define A(U)  tex3D(_SelfTexture3D, float3(U))

    float _dt;
    float _PressWeight;
    float _HumidWeight;
    float _TempWeight;

    float4 frag(v2f_customrendertexture i) : SV_Target
    {
        float3 uvw = i.globalTexcoord;

        float du = 1.0 / _CustomRenderTextureWidth;
        float dv = 1.0 / _CustomRenderTextureHeight;
        float dw = 1.0 / _CustomRenderTextureDepth;
        float4 duvw = float4(du, dv, dw,0);

        // Cell contains r= press, g= humid, b=temp
        float4 cell = A(uvw); 
        float4 cellUp = A(uvw + duvw.wyw);
        float4 cellDwn = A(uvw - duvw.wyw);
        float4 cellRgt = A(uvw + duvw.xww);
        float4 cellLft = A(uvw - duvw.xww);
        float4 cellFwd = A(uvw + duvw.wwz);
        float4 cellBak = A(uvw - duvw.wwz);

        /*  float newPressurePascal = currentData.pressurePascal + 0.1f * (averageSurroundingPressure - currentData.pressurePascal);
            float newHumidity = currentData.humidity + 1f * (averageSurroundingHumidity - currentData.humidity);
            float newTemperature = currentData.temperatureKelvin + 1f * (averageSurroundingTemperature - currentData.temperatureKelvin); */

        float4 average = (cellUp + cellDwn + cellLft + cellRgt + cellFwd + cellBak)/6.0; //*0.16666666 ?;
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