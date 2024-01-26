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
        const float oceanHumidity = 1f; //Mathf.Infinity;

        Color oceanColor = Color.blue;
        Color clearColor = Color.clear;
        Vector3Int maxIndexes;

        const float oneSixth = 1f / 6;

        //Unity assignments:
        [SerializeField] Texture3D linked3DTexture;

        public float updateTimeMs = 0;

        //Runtime data
        CellInformation[,,] cellData;
        Vector3Int gridSize;

        //Unity functions:
        void Start()
        {
            gridSize = new Vector3Int(linked3DTexture.width, linked3DTexture.height, linked3DTexture.depth);

            cellData = new CellInformation[gridSize.x, gridSize.y, gridSize.z];

            maxIndexes = gridSize - Vector3Int.one;

            RunFunctionForEachCell(SetupPointCellData);
        }

        void Update()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            //Update cells: New data should not affect other cells
            CellInformation[,,]  newCellData = new CellInformation[gridSize.x, gridSize.y, gridSize.z];

            totalTemperature = 0;

            RunFunctionForEachCell(delegate(int x, int y, int z)
            {
                newCellData[x, y, z] = DiffusionTest(new Vector3Int(x, y, z), cellData, maxIndexes);
            });

            averageTemperatureK = totalTemperature / (gridSize.x * gridSize.y * gridSize.z);

            cellData = newCellData;

            //Output result
            WriteTextureData();

            updateTimeMs = sw.ElapsedMilliseconds;
        }
        public float averageTemperatureK;
        static float totalTemperature;

        //Main functions:
        void SetupOceanFloorCellData(int x, int y, int z)
        {
            int groundLevel = 2;

            if (y < groundLevel) cellData[x, y, z].humidity = oceanHumidity;
        }

        void SetupSideWallCellData(int x, int y, int z)
        {
            int sideLevel = 20;

            if (x < sideLevel) cellData[x, y, z].humidity = oceanHumidity;
        }

        void SetupPointCellData(int x, int y, int z)
        {
            if (x == 25 && y == 12 && z == 25) cellData[x, y, z].temperatureKelvin = 17000000;
        }

        static CellInformation IterateCellDataTemplate(Vector3Int coordinate, CellInformation[,,] existingCellData, Vector3Int maxIndexes)
        {
            //Cell modification logic goes here
            CellInformation currentData = existingCellData[coordinate.x, coordinate.y, coordinate.z];

            return new CellInformation
            {
                pressurePascal = currentData.pressurePascal,
                humidity = currentData.humidity,
                temperatureKelvin = currentData.temperatureKelvin
            };
        }

        static CellInformation DiffusionTest(Vector3Int coordinate, CellInformation[,,] existingCellData, Vector3Int maxIndexes)
        {
            //Cell modification logic goes here
            CellInformation currentData = existingCellData[coordinate.x, coordinate.y, coordinate.z];

            Vector3Int leftCellIndex = (coordinate.x != 0) ? coordinate + Vector3Int.left : coordinate;
            Vector3Int rightIndex = (coordinate.x != maxIndexes.x) ? coordinate + Vector3Int.right : coordinate;
            Vector3Int upIndex = (coordinate.y != maxIndexes.y) ? coordinate + Vector3Int.up : coordinate;
            Vector3Int lowIndex = (coordinate.y != 0) ? coordinate + Vector3Int.down : coordinate;
            Vector3Int forwardIndex = (coordinate.z != maxIndexes.z) ? coordinate + Vector3Int.forward : coordinate;
            Vector3Int backIndex = (coordinate.z != 0) ? coordinate + Vector3Int.back : coordinate;

            float averageSurroundingPressure = (
                existingCellData[leftCellIndex.x, leftCellIndex.y, leftCellIndex.z].pressurePascal +
                existingCellData[rightIndex.x, rightIndex.y, rightIndex.z].pressurePascal +
                existingCellData[upIndex.x, upIndex.y, upIndex.z].pressurePascal +
                existingCellData[lowIndex.x, lowIndex.y, lowIndex.z].pressurePascal +
                existingCellData[forwardIndex.x, forwardIndex.y, forwardIndex.z].pressurePascal +
                existingCellData[backIndex.x, backIndex.y, backIndex.z].pressurePascal
                ) * oneSixth;

            float averageSurroundingHumidity = (
                existingCellData[leftCellIndex.x, leftCellIndex.y, leftCellIndex.z].humidity +
                existingCellData[rightIndex.x, rightIndex.y, rightIndex.z].humidity +
                existingCellData[upIndex.x, upIndex.y, upIndex.z].humidity +
                existingCellData[lowIndex.x, lowIndex.y, lowIndex.z].humidity +
                existingCellData[forwardIndex.x, forwardIndex.y, forwardIndex.z].humidity +
                existingCellData[backIndex.x, backIndex.y, backIndex.z].humidity
                ) * oneSixth;

            float averageSurroundingTemperaturey = (
                existingCellData[leftCellIndex.x, leftCellIndex.y, leftCellIndex.z].temperatureKelvin +
                existingCellData[rightIndex.x, rightIndex.y, rightIndex.z].temperatureKelvin +
                existingCellData[upIndex.x, upIndex.y, upIndex.z].temperatureKelvin +
                existingCellData[lowIndex.x, lowIndex.y, lowIndex.z].temperatureKelvin +
                existingCellData[forwardIndex.x, forwardIndex.y, forwardIndex.z].temperatureKelvin +
                existingCellData[backIndex.x, backIndex.y, backIndex.z].temperatureKelvin
                ) * oneSixth;

            float newPressurePascal = currentData.pressurePascal + 0.1f * (averageSurroundingPressure - currentData.pressurePascal);
            float newHumidity = currentData.humidity + 1f * (averageSurroundingHumidity - currentData.humidity);
            float newTemperature = currentData.temperatureKelvin + 1f * (averageSurroundingTemperaturey - currentData.temperatureKelvin);

            totalTemperature += newTemperature;

            return new CellInformation
            {
                pressurePascal = newPressurePascal,
                humidity = newHumidity,
                temperatureKelvin = newTemperature
            };
        }

        Color32 GetVisualColorOfCell(int x, int y, int z)
        {
            CellInformation currentInformation = cellData[x, y, z];

            if (currentInformation.humidity == oceanHumidity) return oceanColor;
            else return clearColor;
        }

        Color32 GetDebugColorOfCell(int x, int y, int z)
        {
            CellInformation currentInformation = cellData[x, y, z];

            Color32 returnColor = new Color32(
                (byte)(Mathf.Clamp01((currentInformation.temperatureKelvin - 273.15f) / 100) * byte.MaxValue),
                (byte)(Mathf.Clamp01(currentInformation.pressurePascal / 100000) * byte.MaxValue),
                (byte)(Mathf.Clamp01(currentInformation.humidity) * byte.MaxValue),
                byte.MaxValue
                );

            return returnColor;
        }

        //Support functions:


        //Runtime functions:
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
                        colors[x + yOffset + zOffset] = GetDebugColorOfCell(x, y, z);
                    }
                }
            }

            linked3DTexture.SetPixels32(colors); //Apparently using SetPixels32 is faster than SetPixels according to warning 

            linked3DTexture.Apply();
        }
    }
}