using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicWeatherSystem.RuntimeTests
{
    public class WeatherSystemTest : MonoBehaviour
    {
        //Basic definitions:
        delegate void CellFunction(int x, int y, int z);

        struct CellInformation
        {
            public float pressurePascal;
            public float humidity;
            public float temperatureKelvin;
        }

        //Basic parameters:
        const float rockPressure = Mathf.Infinity;
        const float waterHumidity = 2f;
        const float oceanHumidity = Mathf.Infinity;

        Color oceanColor = Color.blue;
        Color clearColor = Color.clear;

        //Unity assignments:
        [SerializeField] Texture3D linked3DTexture;

        public float updateTimeMs = 0;

        //Runtime data
        CellInformation[,,] cellData;

        //Unity functions:
        void Start()
        {
            cellData = new CellInformation[linked3DTexture.width, linked3DTexture.height, linked3DTexture.depth];

            RunFunctionForEachCell(SetupCellData);
        }

        void Update()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            RunFunctionForEachCell(IterateCellData);

            WriteTextureData();

            updateTimeMs = sw.ElapsedMilliseconds;
        }

        //Main functions:
        void SetupCellData(int x, int y, int z)
        {
            int groundLevel = 5;

            if (y < groundLevel) cellData[x, y, z].humidity = oceanHumidity;
        }

        void IterateCellData(int x, int y, int z)
        {
            CellInformation currentInformation = cellData[x, y, z];

            //Cell modification logic goes here

        }

        Color32 GetColorOfCell(int x, int y, int z)
        {
            CellInformation currentInformation = cellData[x, y, z];

            if (currentInformation.humidity == oceanHumidity) return oceanColor;
            else return clearColor;
        }

        void WriteTextureData()
        {
            Vector3Int size = new Vector3Int(
                cellData.GetLength(0),
                cellData.GetLength(1),
                cellData.GetLength(2));

            Color32[] colors = new Color32[size.x * size.y * size.z];

            for (int z = 0; z < size.z; z++)
            {
                int zOffset = z * size.x * size.y;
                for (int y = 0; y < size.y; y++)
                {
                    int yOffset = y * size.x;
                    for (int x = 0; x < size.x; x++)
                    {
                        colors[x + yOffset + zOffset] = GetColorOfCell(x, y, z);
                    }
                }
            }

            linked3DTexture.SetPixels32(colors); //Apparently using SetPixels32 is faster than SetPixels according to warning 

            linked3DTexture.Apply();
        }

        void RunFunctionForEachCell(CellFunction cellFunction)
        {
            for (int x = 0; x < cellData.GetLength(0); x++)
            {
                for (int y = 0; y < cellData.GetLength(1); y++)
                {
                    for (int z = 0; z < cellData.GetLength(2); z++)
                    {
                        cellFunction(x, y, z);
                    }
                }
            }
        }

        //Support functions:

    }
}