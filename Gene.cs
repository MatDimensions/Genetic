namespace Genetic
{
	public abstract class Gene<T> : IGene
	{
		#region Fields
		protected bool isDominant;
		protected float chancesToMutate;
		#endregion

		#region Methods
		public abstract void Encode(T Data);
		public abstract T Decode();
		public abstract object FuzeDecode(IGene gene);

		protected abstract void Mutate();

		public bool IsDominant()
		{
			return isDominant;
		}
		public abstract IGene Clone();
		public abstract IGene Replicate();
		#endregion
	}
}
