using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Randomizations;

public class WFCMutation : MutationBase
{
    protected override void PerformMutate(IChromosome chromosome, float probability)
    {
        if (RandomizationProvider.Current.GetDouble() <= probability)
        {
            var indexes = RandomizationProvider.Current.GetUniqueInts(2, 0, chromosome.Length);
            foreach (var index in indexes)
            {
                chromosome.ReplaceGene(index, chromosome.GenerateGene(index));
            }
        }
    }
}
