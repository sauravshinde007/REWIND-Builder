using System;
using UnityEngine;

public class MechanicConnection : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    public Mechanic fromMechanic;
    public Mechanic toMechanic;
    
    private bool _setupDone = false;

    public void ConnectMechanics()
    {
        var connectionSuccess =
            fromMechanic.actualMechanicGO.GetComponent<IMechanic>().Connect(toMechanic.actualMechanicGO);

        if (connectionSuccess) SetupLineRenderer();
        else
        {
            Debug.Log("Failed Connection");
            Destroy(gameObject);
        }
    }

    public bool SetupConnection(Mechanic fromMechanic, Mechanic toMechanic)
    {
        if (!fromMechanic || !toMechanic )
        {
            Debug.LogWarning("Mechanic connection could not be setup (from or to is null)");
            return false;
        }
        this.fromMechanic = fromMechanic;
        this.toMechanic = toMechanic;
        
        ConnectMechanics();
        
        _setupDone = true;
        return true;
    }

    private void SetupLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, fromMechanic.transform.position);
        lineRenderer.SetPosition(1, toMechanic.transform.position);
    }

    private void Update()
    {
        if (!_setupDone) return;
        
        if(!toMechanic || !fromMechanic) Destroy(gameObject);
    }
}