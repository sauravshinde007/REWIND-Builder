using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour, IMechanic
{

    public bool Connect(GameObject other)
    {
        return false;
    }

    public void ChangeValues(IMechanic newMechanic)
    {
        
    }
}
