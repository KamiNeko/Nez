namespace Nez
{
	/// <summary>
	/// Renderer that only renders all but one renderLayer. Useful to keep UI rendering separate from the rest of the game when used in conjunction
	/// with a RenderLayerRenderer. Note that UI would most likely want to be rendered in screen space so the camera matrix shouldn't be passed to
	/// Batcher.Begin.
	/// </summary>
	public class RenderLayerExcludeRenderer : Renderer
	{
		public int[] excludedRenderLayers;


		public RenderLayerExcludeRenderer( int renderOrder, params int[] excludedRenderLayers ) : base( renderOrder, null )
		{
			this.excludedRenderLayers = excludedRenderLayers;
		}


		public override void render( Scene scene )
		{
			var cam = camera ?? scene.Camera;
			beginRender( cam );

			for( var i = 0; i < scene.RenderableComponents.count; i++ )
			{
				var renderable = scene.RenderableComponents[i];
				if( !excludedRenderLayers.contains( renderable.RenderLayer ) && renderable.Enabled && renderable.IsVisibleFromCamera( cam ) )
					renderAfterStateCheck( renderable, cam );
			}

			if( shouldDebugRender && Core.debugRenderEnabled )
				debugRender( scene, cam );

			endRender();
		}


		protected override void debugRender( Scene scene, Camera cam )
		{
			base.debugRender( scene, cam );

			for( var i = 0; i < scene.RenderableComponents.count; i++ )
			{
				var renderable = scene.RenderableComponents[i];
				if( !excludedRenderLayers.contains( renderable.RenderLayer ) && renderable.Enabled && renderable.IsVisibleFromCamera( cam ) )
					renderable.DebugRender( Graphics.instance );
			}
		}

	}
}

