using UnityEngine;
using System.Collections;

public class AgentConfig : MonoBehaviour {

	public float radiusCohesion;
	public float radiusSeparation;
	public float radiusAlignment;

	public float weightCohesion;
	public float weightSeparation;
	public float weightAlignment;

    public float maxAcceleration;
    public float maxVelocity;

    public float maxBound;

	public float maxFieldOfViewAngle = 90;

	// Wander behaviour
	public float wanderWeight;
	public float wanderRadius;
	public float wanderJitter;
	public float wanderDistance;

	// Avoid enemies
	public float radiusAvoid;
	public float weightAvoid;
}
