using UnityEngine;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using System.Collections.Generic;

public class PureChromosome : ChromosomeBase
{
    private List<Module> modules;
    private int mapSize;

    public double Novelty { get; internal set; }

    public PureChromosome(List<Module> modules, int mapSize) : base(mapSize)
    {
        this.modules = modules;
        this.mapSize = mapSize;   

        var moduleIndexes = RandomizationProvider.Current.GetInts(mapSize, 0, modules.Count);

        for( int i = 0; i < moduleIndexes.Length; i++)
        {
            ReplaceGene(i, new Gene(modules[moduleIndexes[i]]));
        }

    }

    public override IChromosome CreateNew()
    {
        return new PureChromosome(modules, mapSize);
    }

    public override Gene GenerateGene(int geneIndex)
    {
        return new Gene(modules[RandomizationProvider.Current.GetInt(0, modules.Count)]);
    }


    public override IChromosome Clone()
    {
        var clone = base.Clone() as PureChromosome;
        clone.Novelty = Novelty;

        return clone;
    }
}


