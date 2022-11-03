using Microsoft.Xna.Framework;
using Unify2D.Core;

namespace GameAssembly
{

	public class TestComponent : Component
	{
		public int A { get; set; }

		public override void Update(GameCore core)
		{
			_gameObject.Position += Vector2.UnitX * 12;

		}

	}


}
