using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

public class MapBehaviour : MonoBehaviour, ILevelGenerator
{
    private GeneticAlgorithm m_ga;
    private Thread m_gaThread;
    [SerializeField] private Transform moduleParent;
    [SerializeField] private Vector3Int size;
    [SerializeField] private GameObject player;
    public int maxEpochs = 100;
    public WriteToCSV WriteToCSV;

    private List<Slot> slots;
    private Module spawnPoint = null;
    private Pathfinding pathfinding;
    public List<int2> unwalkable = new();
    private List<Module> modules = new();
    public float desiredNovelty = 0.8f;
    public float desiredHeight = 1;

    private bool startedEvolution = false;
    private bool canRegenerate = false;

    private float timer = 0.0f;

    private void Awake()
    {
        pathfinding = GetComponent<Pathfinding>();

        foreach (Transform module in moduleParent)
        {
            modules.Add(module.GetComponent<Module>());
        }

        slots = new List<Slot>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    if(x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1)
                    {
                        slots.Add(new Slot(new Vector3Int(x, y, z), new Module[] { modules[0] }, false));
                    }
                    else
                    {
                        slots.Add(new Slot(new Vector3Int(x, y, z), modules.ToArray(), false));
                    }
                }
            }
        }
    }

    private void Start()
    {
        //CheckEntropy();
        //StartCoroutine(VisualizeMap());

        WriteToCSV = GetComponent<WriteToCSV>();
        WriteToCSV.StartCSV();

        var chromosome = new WFCChromosome(3);
        var population = new Population(50, 100, chromosome);
        var selection = new EliteSelection();
        var crossover = new UniformCrossover(0.5f);
        var mutation = new WFCMutation();

        var fitness = new FuncFitness((c) =>
        {
            var fc = c as WFCChromosome;

            List<Slot> newSlots = new();

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1)
                        {
                            newSlots.Add(new Slot(new Vector3Int(x, y, z), new Module[] { modules[0] }, false));
                        }
                        else
                        {
                            newSlots.Add(new Slot(new Vector3Int(x, y, z), modules.ToArray(), false));
                        }
                    }
                }
            }

            CheckEntropy(newSlots);
            float fitness = 1 - (Mathf.Abs(desiredNovelty - CheckNovelty(newSlots)) + Mathf.Abs(desiredHeight - CheckHeight(newSlots))) / 2;
            fc.Novelty = CheckNovelty(newSlots);
            return fitness;
        });

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
            var novelty = ((WFCChromosome)m_ga.BestChromosome).Novelty;
            WriteToCSV.WriteLineCSV(novelty, (float)m_ga.BestChromosome.Fitness.Value - 0.2f);
            Debug.Log($"Generation: {m_ga.GenerationsNumber} - Novelty: {novelty} - Fitness: {m_ga.BestChromosome.Fitness}");
        };

        m_ga.TerminationReached += OnTermination;

        // Starts the genetic algorithm in a separate thread.
        m_gaThread = new Thread(() => m_ga.Start());
        m_gaThread.Start();
        startedEvolution = true;

    }

    private void OnTermination(object sender, System.EventArgs e)
    {
        startedEvolution = false;
        canRegenerate = true;

    }


    public void CheckEntropy(List<Slot> slots)
    {
        List<Slot> temp = new(slots);
        temp.RemoveAll(s => s.collapsed);
        temp.Sort((a, b) => a.modules.Length - b.modules.Length);
        temp.RemoveAll(a => a.modules.Length != temp[0].modules.Length);

        if(temp.Count > 0) CollapseSlot(temp, slots);
    }

    private void CollapseSlot(List<Slot> temp, List<Slot> newslots)
    {
        Slot slotToCollapse = temp[RandomizationProvider.Current.GetInt(0, temp.Count)];
        slotToCollapse.collapsed = true;

        Module selectedModule = slotToCollapse.modules[RandomizationProvider.Current.GetInt(0, slotToCollapse.modules.Length)];
        slotToCollapse.modules = new Module[] { selectedModule };

        UpdateGeneration(slotToCollapse, newslots);

    }

    private void UpdateGeneration(Slot collapsedSlot, List<Slot> newslots)
    {
        List<Slot> newGenerationSlots = new() { collapsedSlot };

        while (newGenerationSlots.Count > 0)
        {
            Slot cur = newGenerationSlots[0];
            newGenerationSlots.RemoveAt(0);

            foreach (var dir in ValidDirections(cur))
            {
                Slot otherSlot = FindSlotAtPosition(cur, dir);

                if (otherSlot == null) continue;

                List<Module> possibleModules = PossibleModules(cur, dir);

                foreach (var possibleModule in otherSlot.modules)
                {
                    if (!possibleModules.Contains(possibleModule))
                    {
                        List<Module> temp = otherSlot.modules.ToList();
                        temp.Remove(possibleModule);
                        otherSlot.modules = temp.ToArray();

                        if (!newGenerationSlots.Contains(otherSlot)) newGenerationSlots.Add(otherSlot);
                    }
                }
            }
        }

        CheckEntropy(newslots);
    }

    public void StopExecution()
    {
        if (!startedEvolution) return;
        m_ga.Stop();
        m_gaThread.Abort();
    }

    private void OnDestroy()
    {
        StopExecution();
    }

    private List<Vector3Int> ValidDirections(Slot slot)
    {
        List<Vector3Int> validDirections = new();

        if (slot.position.x < size.x - 1) validDirections.Add(new Vector3Int(1, 0, 0));
        if (slot.position.x > 0) validDirections.Add(new Vector3Int(-1, 0, 0));
        if (slot.position.z < size.z - 1) validDirections.Add(new Vector3Int(0, 0, 1));
        if (slot.position.z > 0) validDirections.Add(new Vector3Int(0, 0, -1));
        if (slot.position.y < size.y -1) validDirections.Add(new Vector3Int(0, 1, 0));
        if (slot.position.y > 0) validDirections.Add(new Vector3Int(0, -1, 0));

        return validDirections;
    }

    private Slot FindSlotAtPosition(Slot slot, Vector3Int dir)
    {
        return slots.Find(s => s.position == slot.position + dir && !s.collapsed);
    }

    private List<Module> PossibleModules(Slot slot, Vector3Int dir)
    {
        List<Module> modules = new();
        foreach (var module in slot.modules)
        {
            modules = modules.Union(module.GetNeighboursFromDirection(dir)).ToList();
        }

        return modules;
    }

    private IEnumerator VisualizeMap()
    {
        unwalkable = new();
        foreach (var slotToCollapse in slots)
        {
            if (slotToCollapse.modules[0].isWalkable)
            {

                var module = Instantiate(slotToCollapse.modules[0], slotToCollapse.position * 2, slotToCollapse.modules[0].transform.rotation, transform);
                BoxCollider col = module.gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(2, 2, 2);

                if (spawnPoint == null) spawnPoint = module;

                yield return new WaitForSeconds(0.025f);
            }
            else
            {
                if(slotToCollapse.position.y == 0) {
                    unwalkable.Add(new int2(slotToCollapse.position.x, slotToCollapse.position.z));
                }
            }
        }

        SpawnPlayer();
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

    private void SpawnPlayer()
    {
        player.transform.position = spawnPoint.transform.position;
    }

    public void SetXSize(int x)
    {
        size.x = x;
    }

    public void SetZSize(int z)
    {
        size.z = z;
    }

    public void ClearLevel()
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

    public void RegenerateLevel()
    {
        ClearLevel();

        slots = new List<Slot>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1)
                    {
                        slots.Add(new Slot(new Vector3Int(x, y, z), new Module[] { modules[0] }, false));
                    }
                    else
                    {
                        slots.Add(new Slot(new Vector3Int(x, y, z), modules.ToArray(), false));
                    }
                }
            }
        }

        timer = Time.time;
        CheckEntropy(slots);
        Debug.Log(Time.time - timer);
        StartCoroutine(VisualizeMap());
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (canRegenerate)
        {
            RegenerateLevel();
            canRegenerate = false;
        }
    }

    public float CheckNovelty(List<Slot> slots)
    {
        float novelty = 0f;
        int walkablecount = 0;
        Module previous = null;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].modules[0].isWalkable)
            {
                if (slots[i].modules[0] != previous)
                {
                    previous = slots[i].modules[0];
                    novelty++;
                }
                walkablecount++;
            }

        }
        float result = (float)novelty / walkablecount;
        return result;
    }

    public float CheckHeight(List<Slot> slots)
    {
        float height = 0f;
        int walkablecount = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].modules[0].isWalkable)
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
