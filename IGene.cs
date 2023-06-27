namespace Genetic
{
	public interface IGene
	{
		public abstract object FuzeDecode(IGene gene);
		public abstract bool IsDominant();
		public abstract IGene Clone();
		public abstract IGene Replicate();
	}
}
