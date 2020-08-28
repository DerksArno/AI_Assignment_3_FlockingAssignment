using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Predator : Agent
{
    protected override Vector3 combine() 
    {
        return config.wanderWeight * wander();
    }
}
