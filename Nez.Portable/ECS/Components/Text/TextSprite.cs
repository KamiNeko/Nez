﻿using Microsoft.Xna.Framework;
using Nez.Sprites;


namespace Nez
{
	public class TextSprite : Sprite
	{
		public override RectangleF Bounds
		{
			get
			{
				if( areBoundsDirty )
				{
					bounds.CalculateBounds( Entity.transform.position, localOffset, origin, Entity.transform.scale, Entity.transform.rotation, _size.X, _size.Y );
					areBoundsDirty = false;
				}

				return bounds;
			}
		}

		/// <summary>
		/// text to draw
		/// </summary>
		/// <value>The text.</value>
		public string Text
		{
			get { return _text; }
			set { SetText( value ); }
		}

		/// <summary>
		/// horizontal alignment of the text
		/// </summary>
		/// <value>The horizontal origin.</value>
		public HorizontalAlign HorizontalOrigin
		{
			get { return _horizontalAlign; }
			set { SetHorizontalAlign( value ); }
		}

		/// <summary>
		/// vertical alignment of the text
		/// </summary>
		/// <value>The vertical origin.</value>
		public VerticalAlign VerticalOrigin
		{
			get { return _verticalAlign; }
			set { SetVerticalAlign( value ); }
		}


		protected HorizontalAlign _horizontalAlign;
		protected VerticalAlign _verticalAlign;
		protected IFont _font;
		protected string _text;
		Vector2 _size;


		public TextSprite( IFont font, string text, Vector2 localOffset, Color color )
		{
			_font = font;
			_text = text;
			this.LocalOffset = localOffset;
			this.Color = color;
			_horizontalAlign = HorizontalAlign.Left;
			_verticalAlign = VerticalAlign.Top;

			UpdateSize();
		}


		#region Fluent setters

		public TextSprite SetFont( IFont font )
		{
			_font = font;
			UpdateSize();

			return this;
		}


		public TextSprite SetText( string text )
		{
			_text = text;
			UpdateSize();
			UpdateCentering();

			return this;
		}


		public TextSprite SetHorizontalAlign( HorizontalAlign hAlign )
		{
			_horizontalAlign = hAlign;
			UpdateCentering();

			return this;
		}


		public TextSprite SetVerticalAlign( VerticalAlign vAlign )
		{
			_verticalAlign = vAlign;
			UpdateCentering();

			return this;
		}

		#endregion


		void UpdateSize()
		{
			_size = _font.measureString( _text );
			UpdateCentering();
		}


		void UpdateCentering()
		{
			var oldOrigin = origin;

			if( _horizontalAlign == HorizontalAlign.Left )
				oldOrigin.X = 0;
			else if( _horizontalAlign == HorizontalAlign.Center )
				oldOrigin.X = _size.X / 2;
			else
				oldOrigin.X = _size.X;

			if( _verticalAlign == VerticalAlign.Top )
				oldOrigin.Y = 0;
			else if( _verticalAlign == VerticalAlign.Center )
				oldOrigin.Y = _size.Y / 2;
			else
				oldOrigin.Y = _size.Y;

			Origin = new Vector2( (int)oldOrigin.X, (int)oldOrigin.Y );
		}


		public override void Render( Graphics graphics, Camera camera )
		{
			graphics.batcher.drawString( _font, _text, Entity.transform.position + localOffset, Color, Entity.transform.rotation, Origin, Entity.transform.scale, SpriteEffects, layerDepth );
		}

	}
}

