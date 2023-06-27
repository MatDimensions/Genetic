using UnityEngine;

namespace Genetic
{
	public abstract class BasGeneObject : ScriptableObject { }
	public abstract class GeneObject<Gene> : BasGeneObject
		where Gene : IGene
	{
		public abstract Gene GetGene();
	}
}
