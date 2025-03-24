using UnityEngine;

public static class MechanicUtils
{
    public static void ScaleUp(IMechanic mechanic, float scaleFactor = 1.2f)
    {
        if (mechanic is MonoBehaviour mb)
        {
            mb.transform.localScale *= scaleFactor;
        }
        else
        {
            Debug.LogWarning("IMechanic does not derive from MonoBehaviour, cannot scale.");
        }
    }
}