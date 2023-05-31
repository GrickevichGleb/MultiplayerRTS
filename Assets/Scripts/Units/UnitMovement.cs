using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent nvAgent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;
    
    
    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }


    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();
        if (target != null)
        {
            if((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                nvAgent.SetDestination(target.transform.position);
            }
            else if(nvAgent.hasPath)
            {
                nvAgent.ResetPath();
            }
            
            return;
        }
        
        
        if (!nvAgent.hasPath) return;
        if (nvAgent.remainingDistance > nvAgent.stoppingDistance) return;
        
        nvAgent.ResetPath();
    }
    
    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }


    [Server]
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();
        
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;
        
        nvAgent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        nvAgent.ResetPath();
    }
    
    #endregion

}
