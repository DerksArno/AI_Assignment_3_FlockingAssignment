using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour {

	public AgentConfig settings;

	public Transform agentPrefab;
	public Transform predatorPrefab;

	public int nAgents;
	public int nPredators;

	public List<Agent> agents;
	public List<Predator> predators;

    public float spawnRange;

	void Start () {
		settings = FindObjectOfType<AgentConfig>();
		spawnRange = settings.maxBound / 2;

		agents = new List<Agent>();
		predators = new List<Predator>();

		spawn(agentPrefab, nAgents);
		agents.AddRange(FindObjectsOfType<Agent>());

		spawn(predatorPrefab, nPredators);
		predators.AddRange(FindObjectsOfType<Predator>());
	}
	
	void Update () {
	
	}

	void spawn(Transform prefab, int n){

		for(int i=0; i< n; i++){

			var obj = Instantiate(prefab,
			                      new Vector3(Random.Range(-spawnRange, spawnRange),0, Random.Range(-spawnRange, spawnRange)),
			                      Quaternion.identity);
		}
	}

	public List<Agent> getNeigh(Agent agent, float radius){

        List<Agent> agentList = new List<Agent>();

        foreach (Agent otherAgent in agents)
        {
            if (otherAgent!=agent && Vector3.Distance(agent.xPosition, otherAgent.xPosition) < radius)
            {
                agentList.Add(otherAgent);
            }
        }
		return agentList;
	}

	public List<Predator> getPredators(Agent agent, float radius)
	{
		List<Predator> predatorList = new List<Predator>();

		foreach (Predator predator in predators)
		{
			if (Vector3.Distance(agent.xPosition, predator.xPosition) < radius)
			{
				predatorList.Add(predator);
			}
		}
		return predatorList;
	}
    
}
