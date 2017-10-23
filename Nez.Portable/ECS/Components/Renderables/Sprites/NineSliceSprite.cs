using Nez.Sprites;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class NineSliceSprite : Sprite
	{
		public new float Width
		{
			get { return _finalRenderRect.Width; }
			set
			{
				_finalRenderRect.Width = (int)value;
				_destRectsDirty = true;
			}
		}

		public new float Height
		{
			get { return _finalRenderRect.Height; }
			set
			{
				_finalRenderRect.Height = (int)value;
				_destRectsDirty = true;
			}
		}

		public override RectangleF Bounds
		{
			get
			{
				if( areBoundsDirty )
				{
					bounds.Location = Entity.transform.position + localOffset;
					bounds.width = Width;
					bounds.height = Height;
					areBoundsDirty = false;
				}

				return bounds;
			}
		}

		public new NinePatchSubtexture subtexture;


		/// <summary>
		/// full area in which we will be rendering
		/// </summary>
		Rectangle _finalRenderRect;
		Rectangle[] _destRects = new Rectangle[9];
		bool _destRectsDirty = true;


		public NineSliceSprite( NinePatchSubtexture subtexture ) : base( subtexture )
		{
			this.subtexture = subtexture;
		}


		public NineSliceSprite( Subtexture subtexture, int top, int bottom, int left, int right ) : this( new NinePatchSubtexture( subtexture, left, right, top, bottom ) )
		{}


		public NineSliceSprite( Texture2D texture, int top, int bottom, int left, int right ) : this( new NinePatchSubtexture( texture, left, right, top, bottom) )
		{}


		public override void Render( Graphics graphics, Camera camera )
		{
			if( _destRectsDirty )
			{
				subtexture.generateNinePatchRects( _finalRenderRect, _destRects, subtexture.left, subtexture.right, subtexture.top, subtexture.bottom);
				_destRectsDirty = false;
			}

			var pos = ( Entity.transform.position + localOffset ).ToPoint();

			for( var i = 0; i < 9; i++ )
			{
				// shift our destination rect over to our position
				var dest = _destRects[i];
				dest.X += pos.X;
				dest.Y += pos.Y;
				graphics.batcher.draw( subtexture, dest, subtexture.ninePatchRects[i], Color );
			}
		}
	}
}

