using UnityEngine;

[ExecuteAlways]
public class EnableParticleGPUInstancing : MonoBehaviour
{
    public bool enableInstancing = true;
    
    private void OnEnable()
    {
        var ps = GetComponent<ParticleSystemRenderer>();
        if(ps)
            ps.enableGPUInstancing = enableInstancing;
    }
}
