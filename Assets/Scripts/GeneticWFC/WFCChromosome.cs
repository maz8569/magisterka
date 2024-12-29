using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

public class WFCChromosome : ChromosomeBase
{
    private readonly int numberOfTiles;

    public WFCChromosome(int numberOfTiles) : base(numberOfTiles)
    {
        this.numberOfTiles = numberOfTiles;

        for (int i = 0; i < numberOfTiles; i++)
        {
            ReplaceGene(i, new Gene(RandomizationProvider.Current.GetFloat(0.01f, 1)));
        }
    }

    public float Novelty { get; internal set; }

    public override IChromosome CreateNew()
    {
        return new WFCChromosome(numberOfTiles);
    }

    public override Gene GenerateGene(int geneIndex)
    {
        return new Gene(RandomizationProvider.Current.GetFloat(0.01f, 1));
    }

    public override IChromosome Clone()
    {
        var clone = base.Clone() as WFCChromosome;
        clone.Novelty = Novelty;
        return clone;
    }
}
