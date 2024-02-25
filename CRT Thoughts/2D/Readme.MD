
# Configure CRT demo

A quick overview of the files and their relationships to get to see something.

## CRT 2D Diffusion.shader

This is the source for the diffusion shader. The compiled shader is the section "Weather/CRT Diffusion 2D"
In this case it is not used as the shader for the display, it is used on a material for the rendertexture.

## M_CRT_2Diffusion.mat

This is an slightly unusual intermediate material that will be attached to the render texture.
The properties of this material can be set from code to control the operating parameters in the simulation in real time.

## CRT2d 512x512

This is the render texture. 
This is where:
    - Set dimension (2D in this case) and the size in "pixels" is selected (512x512) for now.
    - Set the 'Color Format': I chose R32G32B32A32_SFLOAT. Each float represents a value pressure, temp, humidity, and one channel spare for now.
    - The material is the material M_CRT_Diffusion.
    - I have set the initialization Mode to 'On Demand' (for testeing with the code module CRT Monitor.cs), to run manually set to 'On Load'
    - The Update mode 'Realtime' will run the CRT every Frame. Choosing 'On Load' will run it once, and 'On Demand' as triggered by code. 
    - I won't go into update zones at this point.

## M_Display.mat

Just for debug this is the Standard shader with the Albedo set to the Rendertexture I put it on a cube.

## CRT Monitor

This is a simple code module that illustrates how to manipulate the CRT at runtime. One of the issues with running CRTs at full update speed (especially if double buffered as this one is) is that it burns GPU resources and there is no control of the cadence of updates - we need a definite time interval, and depending on the simulation granularity the diffusion might only be needed a few times a second.

The module shows how to control the update cadence, and also how to modify parameters through the material properties.
