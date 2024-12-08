using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FMPostProcessingController : MonoBehaviour
{
    [SerializeField] VolumeProfile volumeProfile;
    [SerializeField] float blurValue = 0f;
    [SerializeField] float bloomValue = 0f;
    [SerializeField] float lutValue = 0f;
    [SerializeField] float chromaticValue = 0f;
    [SerializeField] float vignetteValue = 0f;

    private UniversalRenderPipelineAsset universalRenderPipelineAsset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
