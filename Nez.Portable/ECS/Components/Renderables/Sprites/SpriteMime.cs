namespace Nez.Sprites
{
	/// <summary>
	/// this component will draw the same frame of spriteToMime every frame. The only difference in rendering is that SpriteMime uses its own
	/// localOffset and color. This allows you to use it for the purpose of shadows (by offsetting via localPosition) or silhouettes (with a
	/// Material that has a stencil read).
	/// </summary>
	public class SpriteMime : RenderableComponent
	{
		public override float Width { get { return _spriteToMime.Width; } }
		public override float Height { get { return _spriteToMime.Height; } }
		public override RectangleF Bounds { get { return _spriteToMime.Bounds; } }

		Sprite _spriteToMime;


		public SpriteMime()
		{}


		public SpriteMime( Sprite spriteToMime )
		{
			_spriteToMime = spriteToMime;
		}


		public override void OnAddedToEntity()
		{
			if( _spriteToMime == null )
				_spriteToMime = this.getComponent<Sprite>();
		}


		public override void Render( Graphics graphics, Camera camera )
		{
			graphics.batcher.draw( _spriteToMime.Subtexture, Entity.transform.position + localOffset, Color, Entity.transform.rotation, _spriteToMime.Origin, Entity.transform.scale, _spriteToMime.SpriteEffects, layerDepth );
		}
	}
}

