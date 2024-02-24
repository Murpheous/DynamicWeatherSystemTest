using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class CRTMonitor : MonoBehaviour
{
    [Header("Custom Render Texture (Simulation Behaviour)")]
    // The CRT reference gives access to start/stop/update modes of the simulation.
    [SerializeField, Tooltip("The Simulation Custom Render Texture")] CustomRenderTexture simCRT;
    // The material reference gives access to the properties of the simulation render texture.
    [SerializeField, Tooltip("CRT material (overwritten runtime)")] Material matSimulation;

    // Realtime is good for dynamic things like water manual may be best for calculation heavy jobs that run less frequently
    [SerializeField, Tooltip("Set CRT on demand vs realtime")] bool crtOnDemandMode;

    // Cadence of updates in manual mode
    [SerializeField,Tooltip("CRT Update Interval (seconds)"),Range(0.01f,1f)] float updateInterval;

    [Header("Display Material (Needed for Rendering Behaviour)")]
    [SerializeField] Material matDisplay;

    private bool iHaveCRT = false;
    private bool crtUpdateNeeded = false;
    private bool crtResetNeeded = false;

    void ResetSimulation()
    {
        crtResetNeeded = false;
        currentUpdateMode = crtOnDemandMode;
        if (!iHaveCRT)
            return;
        //matSimulation.SetFloat("_dt", crtOnDemandMode ? updateInterval : Time.deltaTime);
        simCRT.updateMode = crtOnDemandMode ? CustomRenderTextureUpdateMode.OnDemand : CustomRenderTextureUpdateMode.Realtime;
        nextUpdateTime = 0;
        simCRT.Initialize();
    }
    void UpdateSimulation()
    {
        crtUpdateNeeded = false;
        if (!iHaveCRT)
            return;
        simCRT.Update(1);
    }

    private void OnGUI()
    {
        string txtCrtStatus = iHaveCRT ? "CRT Initialized" : "CRT not present!!";
        GUILayout.Box(txtCrtStatus);
        crtResetNeeded |= GUILayout.Button("Reset\nSimulation");
    }


    void Start()
    {
        if (simCRT != null)
            matSimulation = simCRT.material;
        iHaveCRT = matSimulation != null && simCRT != null;
        ResetSimulation();
    }


    float nextUpdateTime = 0;
    float delta;
    bool currentUpdateMode = false;
    private void Update()
    {
        if (!iHaveCRT)
            return;
        if (currentUpdateMode != crtOnDemandMode || crtResetNeeded)
            ResetSimulation();
        if (crtOnDemandMode)
        {
            delta = Time.deltaTime;
            nextUpdateTime -= delta;
            while (nextUpdateTime < 0) // A bit dodgy, but handles Update interval slower than configured interval
            {
                nextUpdateTime += updateInterval;
                UpdateSimulation();
            }
        }
        if (crtUpdateNeeded)
            UpdateSimulation();
    }
}
