using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez.Shadows
{
	/// <summary>
	/// Point light that also casts shadows
	/// </summary>
	public class PolyLight : RenderableComponent
	{
		/// <summary>
		/// layer mask of all the layers this light should interact with. defaults to all layers.
		/// </summary>
		public int collidesWithLayers = Physics.allLayers;

		public override RectangleF Bounds
		{
			get
			{
				if( areBoundsDirty )
				{
					bounds.CalculateBounds( Entity.transform.position, localOffset, new Vector2( _radius, _radius ), Vector2.One, 0, _radius * 2f, _radius * 2f );
					areBoundsDirty = false;
				}

				return bounds;
			}
		}

		/// <summary>
		/// Radius of influence of the light
		/// </summary>
		public float radius
		{
			get { return _radius; }
			set { setRadius( value ); }
		}

		/// <summary>
		/// Power of the light, from 0 (turned off) to 1 for maximum brightness        
		/// </summary>
		public float power;

		protected float _radius;
		protected VisibilityComputer _visibility;

		Effect _lightEffect;
		FastList<short> _indices = new FastList<short>( 50 );
		FastList<VertexPositionTexture> _vertices = new FastList<VertexPositionTexture>( 20 );

		// shared Collider cache used for querying for nearby geometry. Maxes out at 10 Colliders.
		static protected Collider[] _colliderCache = new Collider[10];


		public PolyLight( float radius ) : this( radius, Color.White )
		{ }


		public PolyLight( float radius, Color color ) : this( radius, color, 1.0f )
		{ }


		public PolyLight( float radius, Color color, float power )
		{
			this.radius = radius;
			this.power = power;
			this.Color = color;
			computeTriangleIndices();
		}


		#region Fluent setters

		public virtual PolyLight setRadius( float radius )
		{
			if( radius != _radius )
			{
				_radius = radius;
				areBoundsDirty = true;

				if( _lightEffect != null )
					_lightEffect.Parameters["lightRadius"].SetValue( radius );
			}

			return this;
		}


		public PolyLight setPower( float power )
		{
			this.power = power;
			return this;
		}

		#endregion


		/// <summary>
		/// fetches any Colliders that should be considered for occlusion. Subclasses with a shape other than a circle can override this.
		/// </summary>
		/// <returns>The overlapped components.</returns>
		protected virtual int getOverlappedColliders()
		{
			return Physics.overlapCircleAll( Entity.position + localOffset, _radius, _colliderCache, collidesWithLayers );
		}


		/// <summary>
		/// override point for calling through to VisibilityComputer that allows subclasses to setup their visibility boundaries for
		/// different shaped lights.
		/// </summary>
		protected virtual void loadVisibilityBoundaries()
		{
			_visibility.loadRectangleBoundaries();
		}


		#region Component and RenderableComponent

		public override void OnAddedToEntity()
		{
			_lightEffect = Entity.scene.ContentManager.loadEffect<Effect>( "polygonLight", EffectResource.polygonLightBytes );
			_lightEffect.Parameters["lightRadius"].SetValue( radius );
			_visibility = new VisibilityComputer();
		}


		public override void Render( Graphics graphics, Camera camera )
		{
			if( power > 0 && IsVisibleFromCamera( camera ) )
			{
				var totalOverlaps = getOverlappedColliders();

				// compute the visibility mesh
				_visibility.begin( Entity.transform.position + localOffset, _radius );
				loadVisibilityBoundaries();
				for( var i = 0; i < totalOverlaps; i++ )
				{
					if( !_colliderCache[i].isTrigger )
						_visibility.addColliderOccluder( _colliderCache[i] );
				}
				System.Array.Clear( _colliderCache, 0, totalOverlaps );

				// generate a triangle list from the encounter points
				var encounters = _visibility.end();
				generateVertsFromEncounters( encounters );
				ListPool<Vector2>.free( encounters );

				var primitiveCount = _vertices.length / 2;
				if( primitiveCount == 0 )
					return;

				Core.graphicsDevice.BlendState = BlendState.Additive;
				Core.graphicsDevice.RasterizerState = RasterizerState.CullNone;

				// wireframe debug
				//var rasterizerState = new RasterizerState();
				//rasterizerState.FillMode = FillMode.WireFrame;
				//rasterizerState.CullMode = CullMode.None;
				//Core.graphicsDevice.RasterizerState = rasterizerState;

				// Apply the effect
				_lightEffect.Parameters["viewProjectionMatrix"].SetValue( Entity.scene.Camera.viewProjectionMatrix );
				_lightEffect.Parameters["lightSource"].SetValue( Entity.transform.position );
				_lightEffect.Parameters["lightColor"].SetValue( Color.ToVector3() * power );
				_lightEffect.Techniques[0].Passes[0].Apply();

				Core.graphicsDevice.DrawUserIndexedPrimitives( PrimitiveType.TriangleList, _vertices.buffer, 0, _vertices.length, _indices.buffer, 0, primitiveCount );
			}
		}


		public override void DebugRender( Graphics graphics )
		{
			// draw a square for our pivot/origin and draw our bounds
			graphics.batcher.drawPixel( Entity.transform.position + localOffset, Debug.Colors.renderableCenter, 4 );
			graphics.batcher.drawHollowRect( Bounds, Debug.Colors.renderableBounds );
		}

		#endregion


		/// <summary>
		/// adds a vert to the list
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="texCoord">Tex coordinate.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		void addVert( Vector2 position )
		{
			var index = _vertices.length;
			_vertices.ensureCapacity();
			_vertices.buffer[index].Position = position.toVector3();
			_vertices.buffer[index].TextureCoordinate = position;
			_vertices.length++;
		}


		void computeTriangleIndices( int totalTris = 20 )
		{
			_indices.reset();

			// compute the indices to form triangles
			for( var i = 0; i < totalTris; i += 2 )
			{
				_indices.add( 0 );
				_indices.add( (short)( i + 2 ) );
				_indices.add( (short)( i + 1 ) );
			}
		}


		void generateVertsFromEncounters( List<Vector2> encounters )
		{
			_vertices.reset();

			// add a vertex for the center of the mesh
			addVert( Entity.transform.position );

			// add all the other encounter points as vertices storing their world position as UV coordinates
			for( var i = 0; i < encounters.Count; i++ )
				addVert( encounters[i] );

			// if we dont have enough tri indices add enough for our encounter list
			var triIndices = _indices.length / 3;
			if( encounters.Count > triIndices )
				computeTriangleIndices( encounters.Count );
		}

	}
}
