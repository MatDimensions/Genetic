using UnityEngine;

namespace Genetic
{
	public class Genome<Genotype> where Genotype : struct, IGenotype
	{
		#region Fields
		private IGene[] fatherGenes;
		private IGene[] motherGenes;
		#endregion

		#region Methods

		public static void LogError(string msg)
			=> Debug.LogError("Genome Error : " + msg);

		#region Constructors
		private Genome(int size)
		{
			fatherGenes = new IGene[size];
			motherGenes = new IGene[size];
		}

		public Genome()
		{
			fatherGenes = new IGene[0];
			motherGenes = new IGene[0];
		}

		public Genome(IGene[] genes) : this(genes.Length, genes) { }

		public Genome(int size, IGene[] genes) : this(size)
		{
			for (int i = 0; i < genes.Length; i++)
			{
				fatherGenes[i] = genes[i].Clone();
				motherGenes[i] = genes[i].Clone();
			}
		}

		public Genome(IGene[] fatherGenes, IGene[] motherGenes)
		{
			this.fatherGenes = fatherGenes;
			this.motherGenes = motherGenes;
		}

		public Genome(Genome<Genotype> father, Genome<Genotype> mother) : this(father.Gamete(), mother.Gamete()) { }
		#endregion

		public IGene[] Gamete()
		{
			IGene[] gamete = new IGene[fatherGenes.Length];

			for (int i = 0; i < fatherGenes.Length; i++)
			{
				gamete[i] = (UnityEngine.Random.value <= 0.5f
					? fatherGenes[i].Replicate()
					: motherGenes[i].Replicate());
			}

			return gamete;
		}

		public Genotype Decode()
		{
			object[] Data = new object[fatherGenes.Length];
			for (int i = 0; i < fatherGenes.Length; i++)
			{
				if (fatherGenes[i].IsDominant() && !motherGenes[i].IsDominant())
				{
					System.Type childType = fatherGenes[i].GetType().UnderlyingSystemType;
					System.Reflection.MethodInfo f = childType.GetMethod("Decode");
					if (f == null)
					{
						LogError("Can't find Decode Method on fatherGene n°" + i);
						throw new System.MissingMethodException("Decode on fatherGene n°" + i);
					}
					object dt = f.Invoke(fatherGenes[i], new object[0]);
					Data[i] = dt;
				}
				else if (!fatherGenes[i].IsDominant() && motherGenes[i].IsDominant())
				{
					System.Type childType = motherGenes[i].GetType().UnderlyingSystemType;
					System.Reflection.MethodInfo f = childType.GetMethod("Decode");
					if (f == null)
					{
						LogError("Can't find Decode Method on motherGene n°" + i);
						throw new System.MissingMethodException("Decode on motherGene n°" + i);
					}
					object dt = f.Invoke(motherGenes[i], new object[0]);
					Data[i] = dt;
				}
				else
				{
					Data[i] = fatherGenes[i].FuzeDecode(motherGenes[i]);
				}
			}
			Genotype genotype = new Genotype();
			genotype.Setup(Data);
			return genotype;
		}
		#endregion
	}
}
