using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {


	public Vector3 xPosition;
	public Vector3 velocity;
	public Vector3 acceleration;
	public World world;
    public AgentConfig config;

    private Vector3 wanderTarget;

    private void Start () {
	
		world = FindObjectOfType<World>();

        config = FindObjectOfType<AgentConfig>();

        xPosition = transform.position;
	}

    private void Update () {
        float deltaTime = Time.deltaTime;

        acceleration = combine();

        // Never exceed a max value
        acceleration = Vector3.ClampMagnitude(acceleration, config.maxAcceleration);

        velocity = velocity + acceleration * deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, config.maxVelocity);

        xPosition = xPosition + velocity * deltaTime;

        // Wrap around if out of bounds
        wrapAround(ref xPosition, -config.maxBound / 2, config.maxBound / 2);

        transform.position = xPosition;

        if (velocity.magnitude > 0)
        {
            transform.LookAt(xPosition + velocity);
        }
    }

    // cohesion behavior
    private Vector3 cohesion() {
        // return a vector that will steer our current velocity
        // towards the center of mass of all nearby neighbours
        Vector3 steerVector = new Vector3(0, 0, 0);

        // agents
        int countAgents = 0;

        // get all my nearby neighbours inside radius Rc of this current agent
        List<Agent> neighbours = world.getNeigh(this, config.radiusCohesion);

        // no neighbours means no cohesion desire
        if (neighbours.Count == 0)
            return steerVector;

        // find the center of mass of all neighbours by summing all positions and 
        // divide by total number of agents
        foreach (Agent agent in neighbours)
        {
            if (isInFieldOfView(agent.xPosition))
            {
                steerVector += agent.xPosition;
                countAgents++;
            }
        }

        if (countAgents == 0)
            return steerVector;

        steerVector /= countAgents;

        // steer our velocity towards the center of mass
        steerVector = steerVector - this.xPosition;

        // make r have the length 1 (so just direction).
        steerVector.Normalize();

        return steerVector;
	}

    // separation behavior
    private Vector3 separation() {
        // steer in the opposite direction from each of our nearby neigbhours
        Vector3 seperationVector = new Vector3(0, 0, 0);

        // get all my neighbours
        List<Agent> neighs = world.getNeigh(this, config.radiusSeparation);

        // no neighbours then no separation desire
        if (neighs.Count == 0)
            return seperationVector;
        
		// add the contribution of each neighbor towards me
		foreach (Agent agent in neighs) {

            if (isInFieldOfView(agent.xPosition))
            {
                // towards me means that we have to differentiate between my position and the neighbours position
                Vector3 towardsMe = this.xPosition - agent.xPosition;

                if (Vector3.Magnitude(towardsMe) != 0)
                {

                    // force contribution will vary inversly proportional 
                    // to distance or even the square of the distance
                    // so the closer a neighbour is the more force will be applied.
                    // Divide the normalized vector by the squared distance.
                    seperationVector += Vector3.Normalize(towardsMe) / Vector3.Magnitude(towardsMe) / Vector3.Magnitude(towardsMe);

                }
            }
		}

        seperationVector.Normalize();

        return seperationVector;
	}

    private Vector3 alignment() {
        //Alignment behaviour
        //steer agent to match the direction and speed of neighbours
        Vector3 alignmentVector = new Vector3(0, 0, 0);

        //get all neighbours
        List<Agent> neighs = world.getNeigh(this, config.radiusAlignment);

        //no neighbours means no one to align too
        if (neighs.Count == 0)
            return alignmentVector;

        //match direction and speed == match velocity
        //do this for all neighbours
        foreach (Agent agent in neighs)
        {
            if (isInFieldOfView(agent.xPosition))
                alignmentVector += agent.velocity;
        }

        alignmentVector.Normalize();

        return alignmentVector;
    }

    protected virtual Vector3 combine() {
        //return alignment();
        //return Vector3.Normalize( cohesion() + separation() );


        // combine behaviors in different proportions
        // return our acceleration
        Vector3 combinedVector = Vector3.zero;

        // cohesion rule normalized
        Vector3 cohere = cohesion();

        // separation rule normalized
        Vector3 separate = separation();

        // alinment rule normalized
        Vector3 align = alignment();

        // wander
        Vector3 wanderVector = wander();

        // avoid enemies
        Vector3 avoidVector = avoidEnemies();

        // combine all rules as weighted sum (include the coëfficients)
        combinedVector =
            config.weightCohesion * cohere +
            config.weightSeparation * separate + 
            config.weightAlignment * align +
            config.wanderWeight * wanderVector +
            config.weightAvoid * avoidVector;

        // return acceleration
        return combinedVector;
    }

    //create bounds so the agents keep in a certain area shaped like a donut
    //when it disappeares at the top it wil appear at the bottom and viceversa (same for left and right)
    //this will keep them on the screen while running the game.
    //in order to change the vector we have to use a reference to the vector.
    private void wrapAround(ref Vector3 v, float min, float max)
    {
        v.x = wrapAroundFloat(v.x, min, max);
        v.y = wrapAroundFloat(v.y, min, max);
        v.z = wrapAroundFloat(v.z, min, max);
    }

    private float wrapAroundFloat(float value, float min, float max)
    {
        //  min ------value-------- max
        if (value > max)
            value = min;
        else if (value < min)
            value = max;
        return value;
    }

    private bool isInFieldOfView(Vector3 agent)
    {
        // is the stuff inside my field of view?
        bool inFieldOfView = Vector3.Angle(this.velocity, agent - this.xPosition) <= config.maxFieldOfViewAngle;
        return inFieldOfView;
    }

    protected Vector3 wander()
    {
        // wander steer behavior that looks purposeful
        float jitter = config.wanderJitter * Time.deltaTime;

        // add a small random vector to the target's position so it jitters on the circle
        // RandomBinomial) returns a number between 0 and 1
        // The vector is only changing on the X and Z plane (2D)
        wanderTarget = new Vector3(RandomBinomial() * jitter, 0, RandomBinomial() * jitter);

        // reproject the vector back to unit circle
        wanderTarget = Vector3.Normalize(wanderTarget);

        // increase length to be the same as the radius of the wander circle
        wanderTarget *= config.wanderRadius;

        // position the target (circle) in front of the agent
        Vector3 target = wanderTarget + new Vector3(0, 0, config.wanderDistance);

        // project the target from local space to world space
        Vector3 targetInWorld = transform.TransformPoint(target);

        // steer towards it
        targetInWorld -= this.xPosition;

        targetInWorld.Normalize();

        return targetInWorld;
    }

    private float RandomBinomial()
    {
        return Random.Range(0f, 1f) - Random.Range(0f, 1f);
    }

    // Avoid enemies in your radius
    private Vector3 avoidEnemies()
    {
        List<Predator> predators = world.getPredators(this, config.radiusAvoid);
        if (predators.Count == 0)
            return Vector3.zero;

        Vector3 fleeVector = Vector3.zero;
        foreach (Predator predator in predators)
        {
            fleeVector += flee(predator.xPosition);
        }

        return fleeVector.normalized;
    }

    // Get a new vector to flee from the target
    private Vector3 flee(Vector3 target)
    {
        Vector3 newVelocity = xPosition - target;
        newVelocity = newVelocity.normalized * config.maxVelocity;
        return newVelocity - velocity;
    }

}
