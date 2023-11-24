using UnityEditor;
using UnityEngine;

namespace DynamicWeatherSystem.EditorScripts
{
    public class Texture3DTools : EditorWindow
    {
        [MenuItem("Tools/DynamicWeatherSystem/3D texture toolbox")]

        public static void ShowWindow()
        {
            GetWindow(typeof(Texture3DTools));
        }

        Vector3Int size = new Vector3Int(100, 50, 100);
        Color defaultColor = Color.black;

        void OnGUI()
        {
            EditorGUILayout.LabelField("3D Texture tools:", EditorStyles.boldLabel);

            size = EditorGUILayout.Vector3IntField("Size", size);
            defaultColor = EditorGUILayout.ColorField("Default color", defaultColor);

            if (size.x <= 0) size.x = 1;
            if (size.y <= 0) size.y = 1;
            if (size.z <= 0) size.z = 1;

            if (GUILayout.Button(text: "Create 3D texture in Asset folder"))
            {
                CreateTexture3DInAssetFolder(size, defaultColor);
            }
        }

        void CreateTexture3DInAssetFolder(Vector3Int size, Color defaultColor)
        {
            //Source: https://docs.unity3d.com/Manual/class-Texture3D.html

            // Configure the texture

            TextureFormat format = TextureFormat.RGBA32;
            TextureWrapMode wrapMode = TextureWrapMode.Clamp;

            // Create the texture and apply the configuration
            Texture3D texture = new Texture3D(size.x, size.y, size.z, format, false);
            texture.wrapMode = wrapMode;

            // Create a 3-dimensional array to store color data
            Color[] colors = new Color[size.x * size.y * size.z];

            // Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
            for (int z = 0; z < size.z; z++)
            {
                int zOffset = z * size.x * size.y;
                for (int y = 0; y < size.y; y++)
                {
                    int yOffset = y * size.x;
                    for (int x = 0; x < size.x; x++)
                    {
                        colors[x + yOffset + zOffset] = defaultColor;
                    }
                }
            }

            // Copy the color values to the texture
            texture.SetPixels(colors);

            // Apply the changes to the texture and upload the updated texture to the GPU
            texture.Apply();

            // Save the texture to your Unity Project
            AssetDatabase.CreateAsset(texture, $"Assets/3DTexture_{size.x}x{size.y}x{size.z}.asset");
        }
    }
}