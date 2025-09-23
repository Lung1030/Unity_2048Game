// 可以添加動態光效的腳本
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DynamicBackgroundLights : MonoBehaviour
{
    public Light2D[] backgroundLights;
    public float pulseDuration = 2f;
    public Color[] lightColors;
    
    void Start()
    {
        foreach (var light in backgroundLights)
        {
            StartCoroutine(PulseLight(light));
        }
    }
    
    System.Collections.IEnumerator PulseLight(Light2D light)
    {
        while (true)
        {
            // 改變光線強度
            for (float t = 0; t < 1; t += Time.deltaTime / pulseDuration)
            {
                light.intensity = Mathf.Lerp(0.5f, 2f, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
        }
    }
}