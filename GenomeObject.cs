using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Genetic
{
	public abstract class GenomeObject<Genome, Genotype> : ScriptableObject
		where Genome : Genome<Genotype>, new()
		where Genotype : struct, IGenotype
	{
		[SerializeField]
		private List<BasGeneObject> genes = new List<BasGeneObject>();

		public Genome GetGenome()
		{
			IGene[] iGenes = new IGene[genes.Count];

			for (int i = 0; i < genes.Count; i++)
			{
				Type type = genes[i].GetType();
				var f = type.GetMethod("GetGene");
				iGenes[i] = (IGene)f.Invoke(genes[i], new object[0]);
			}

			Genome a = new Genome();
			Type genomeType = a.GetType();
			ConstructorInfo constructorInfo = genomeType.GetConstructor(new Type[1] { iGenes.GetType() });

			return (Genome)constructorInfo.Invoke(new object[1] { iGenes });
		}
	}
}
