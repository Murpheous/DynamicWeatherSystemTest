using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class WaveUI : MonoBehaviour
{
    [Header("Custom Render Texture (Simulation Behaviour)")]
    // The CRT reference gives access to start/stop/update modes of the simulation.
    [SerializeField, Tooltip("The Simulation Custom Render Texture")] CustomRenderTexture simCRT;
    // The material reference gives access to the properties of the simulation render texture.
    [SerializeField, Tooltip("CRT material (overwritten runtime)")] Material matSimulation;

    [Header("Display Material (Needed for Rendering Behaviour)")]
    [SerializeField] Material matSurface;

    private bool iHaveCRT = false;
    private bool iHaveSurface = false;
    private bool crtUpdateNeeded = false;

    void UpdateSimulation()
    {
        crtUpdateNeeded = false;
        if (!iHaveCRT)
            return;
        simCRT.Update(1);
    }

    [SerializeField, Range(10, 100)]
    private float lambdaPixels = 42;
    [SerializeField]
    private bool displayReal = false;
    [SerializeField]
    private bool displayImaginary = false;
    [SerializeField]
    private bool displayEnergy = false;

    float LambdaPixels
    {
        get => lambdaPixels;
        set
        {
            if (iHaveCRT && lambdaPixels != value)
            {
                matSimulation.SetFloat("_LambdaPx", lambdaPixels);
                crtUpdateNeeded = true;
            }
            lambdaPixels = value;
        }
    }
    bool DisplayReal
    {
        get => displayReal;
        set
        {
            if (iHaveSurface && value != displayReal)
                matSurface.SetFloat("_ShowReal", value ? 1f : 0f);
            displayReal = value;
        }
    }

    bool DisplayImaginary
    {
        get => displayImaginary;
        set
        {
            if (iHaveSurface && value != displayImaginary)
                matSurface.SetFloat("_ShowImaginary", value ? 1f : 0f);
            displayImaginary = value;
        }
    }
    bool DisplayEnergy
    {
        get => displayEnergy;
        set
        {
            if (iHaveSurface && value != displayEnergy)
                matSurface.SetFloat("_ShowSquare", value ? 1f : 0f);
            displayEnergy = value;
        }
    }

    private void OnGUI()
    {
        string txtDisplayMode = "Not Set";
        if (displayReal && displayImaginary)
            txtDisplayMode = "Amplitude";
        else if (displayReal)
        {
            txtDisplayMode = "Real";
        }
        else if (displayImaginary)
        {
            txtDisplayMode = "Imaginary";
        }
        if (displayEnergy)
            txtDisplayMode += "-Squared";
        GUILayout.Box("Showing: " + txtDisplayMode);
        DisplayReal = GUILayout.Toggle(DisplayReal, "Use Real Component");
        DisplayImaginary = GUILayout.Toggle(DisplayImaginary, "Use Imaginary Component");
        DisplayEnergy = GUILayout.Toggle(DisplayEnergy, "Show Energy");
        GUILayout.Label("Wavelength");
        LambdaPixels = GUILayout.HorizontalSlider(LambdaPixels, 10, 100);
    }


    void Start()
    {
        if (simCRT != null)
            matSimulation = simCRT.material;
        matSurface = GetComponent<MeshRenderer>().material;
        iHaveCRT = matSimulation != null && simCRT != null;
        iHaveSurface = matSurface != null;
        if (iHaveSurface)
        {
            displayReal = matSurface.GetFloat("_ShowReal") > 0.1f;
            displayImaginary = matSurface.GetFloat("_ShowImaginary") > 0.1f;
            displayEnergy = matSurface.GetFloat("_ShowSquare") > 0.1f;
        }
        if (iHaveCRT)
        {
            lambdaPixels = matSimulation.GetFloat("_LambdaPx");
        }
    }

    private void Update()
    {
        if (!iHaveCRT)
            return;
        if (crtUpdateNeeded)
            UpdateSimulation();
    }
}
