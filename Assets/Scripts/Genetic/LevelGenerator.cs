using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class LevelGenerator : MonoBehaviour, ILevelGenerator
{
    private GeneticAlgorithm m_ga;
    private Thread m_gaThread;

    private bool startedEvolution = false;

    [SerializeField] private Transform moduleParent;
    [SerializeField] private Vector3Int size;

    private Pathfinding pathfinding;
    public List<int2> unwalkable = new();
    public IList<PureSlot> slots;

    public List<Module> availableModules = new();
    private bool CanGenerate = false;

    public int maxEpochs = 200;
    public WriteToCSV WriteToCSV;
    public float desiredNovelty = 0.8f;
    public float desiredHeight = 1f;

    // Start is called before the first frame update
    void Start()
    {
        slots = new List<PureSlot>();
        WriteToCSV = GetComponent<WriteToCSV>();
        WriteToCSV.StartCSV();
        for (int i = 0; i < moduleParent.childCount; i++)
        {
            availableModules.Add(moduleParent.GetChild(i).GetComponent<Module>());
        }

        pathfinding = GetComponent<Pathfinding>();
        
        var chromosome = new PureChromosome(availableModules, size.x * size.y * size.z);
        var fitness = new FuncFitness((c) =>
        {
            var fc = c as PureChromosome;
            var genes = fc.GetGenes();

            List<PureSlot> newSlots = new();
            int i = 0;
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        var slot = new PureSlot { position = new Vector3Int(x, y, z), module = genes[i].Value as Module };
                        newSlots.Add(slot);
                        i++;
                    }
                }
            }

            float fitness = 1 - (Mathf.Abs(desiredNovelty - CheckNovelty(newSlots)) + Mathf.Abs(desiredHeight - CheckHeight(newSlots))) / 2;
            fc.Novelty = CheckNovelty(newSlots);
            return fitness;
        });

        var crossover = new UniformCrossover(0.5f);
        var mutation = new WFCMutation();
        var selection = new EliteSelection();

        var population = new Population(50, 100, chromosome);

        m_ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
        m_ga.Termination = new OrTermination(new GenerationNumberTermination(maxEpochs), new FitnessThresholdTermination(0.97f));

        // The fitness evaluation of whole population will be running on parallel.
        m_ga.TaskExecutor = new ParallelTaskExecutor
        {
            MinThreads = 100,
            MaxThreads = 200
        };

        // Everty time a generation ends, we log the best solution.
        m_ga.GenerationRan += delegate
        {
            var novelty = ((PureChromosome)m_ga.BestChromosome).Novelty;
            WriteToCSV.WriteLineCSV((float)novelty, (float)m_ga.BestChromosome.Fitness.Value);
            Debug.Log($"Generation: {m_ga.GenerationsNumber} - Enclosement: ${novelty}");
        };

        m_ga.TerminationReached += OnTermination;

        StartEvolving();
    }

    private void OnTermination(object sender, System.EventArgs e)
    {
        startedEvolution = false;

        PureChromosome fittest = m_ga.Population.CurrentGeneration.BestChromosome as PureChromosome;
        if (fittest == null) return;
        int i = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var slot = new PureSlot { position = new Vector3Int(x, y, z), module = fittest.GetGene(i).Value as Module };
                    slots.Add(slot);
                    i++;
                }
            }
        }

        unwalkable = new();

        for (i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;
            if (slot.position.y > 0) continue;

            if (!slot.module.isWalkable) unwalkable.Add(new int2(slot.position.x, slot.position.z));
        }

        CanGenerate = true;
    }

    public void StartEvolving()
    {
        startedEvolution = true;
        // Starts the genetic algorithm in a separate thread.
        m_gaThread = new Thread(() => m_ga.Start());
        m_gaThread.Start();
    }

    public void VisualizeMap()
    {
        ClearMap();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            var module = Instantiate(slot.module, slot.position * 2, slot.module.transform.rotation, transform);
            if (slot.module.name != "Empty") 
            {
                BoxCollider col = module.AddComponent<BoxCollider>();
                col.size = new Vector3(2, 2, 2);
            }
        }
    }

    public void ClearMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject); 
#endif
        }
    }

    public void StopExecution()
    {
        if (!startedEvolution) return;
        // When the script is destroyed we stop the genetic algorithm and abort its thread too.
        m_ga.Stop();
        m_gaThread.Abort();
        startedEvolution = false;

        PureChromosome fittest = m_ga.Population.CurrentGeneration.BestChromosome as PureChromosome;
        if (fittest == null) return;
        int i = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var slot = new PureSlot { position = new Vector3Int(x, y, z), module = fittest.GetGene(i).Value as Module };
                    slots.Add(slot);
                    i++;
                }
            }
        }

        unwalkable = new();

        for (i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;
            if (slot.position.y > 0) continue;

            if (!slot.module.isWalkable) unwalkable.Add(new int2(slot.position.x, slot.position.z));
        }
    }

    private void Update()
    {
        if (CanGenerate)
        {
            VisualizeMap();
            CanGenerate = false;
        }
    }

    private void OnDestroy()
    {
        StopExecution();
    }

    public List<Vector3> FindPath(int2 startPos, int2 endPos)
    {
        if (pathfinding != null)
        {
            return ChangeToListVector(pathfinding.FindPath(size.x, size.z, startPos, endPos, unwalkable.ToArray()));
        }
        else
        {
            return new List<Vector3>();
        }
    }

    private List<Vector3> ChangeToListVector(List<int2> originList)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < originList.Count - 1; i++)
        {
            result.Add(new Vector3(originList[i].x * 2, 0, originList[i].y * 2));
        }
        result.Reverse();

        return result;
    }

    public void CheckConnect()
    {
        Debug.Log(CheckConnectivity.CheckIsAll(size.x, size.z, new int2(0, 0), unwalkable.ToArray()));
    }

    public void SetXSize(int x)
    {
        size.x = x;
    }

    public void SetZSize(int z)
    {
        size.z = z;
    }

    public float CheckNovelty(List<PureSlot> slots)
    {
        float novelty = 0f;
        int walkablecount = 0;
        Module previous = null;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].module.isWalkable)
            {
                if (slots[i].module != previous)
                {
                    previous = slots[i].module;
                    novelty++;
                }
                walkablecount++;
            }

        }
        float result = (float)novelty / walkablecount;
        return result;
    }

    public float CheckHeight(List<PureSlot> slots)
    {
        float height = 0f;
        int walkablecount = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].module.isWalkable)
            {
                height += slots[i].position.y + 1;
                walkablecount++;
            }

        }

        float result = (float)height / walkablecount;

        return result;
    }

    public void RestartEvo()
    {
        m_gaThread = new Thread(() => m_ga.Start());
        m_gaThread.Start();
        startedEvolution = true;
    }

    public void SetDesiredNovelty(float desiredNovelty)
    {
        this.desiredNovelty = desiredNovelty;
    }

    public void SetDesiredHeight(float desiredHeight)
    {
        this.desiredHeight = desiredHeight;
    }
}
