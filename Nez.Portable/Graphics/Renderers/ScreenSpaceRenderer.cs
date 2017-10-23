using System;


namespace Nez
{
	/// <summary>
	/// Renderer that renders using its own Camera which doesnt move.
	/// </summary>
	public class ScreenSpaceRenderer : Renderer
	{
		public int[] renderLayers;


		public ScreenSpaceRenderer( int renderOrder, params int[] renderLayers ) : base( renderOrder, null )
		{
			Array.Sort( renderLayers );
			Array.Reverse( renderLayers );
			this.renderLayers = renderLayers;
			wantsToRenderAfterPostProcessors = true;
		}


		public override void render( Scene scene )
		{
			beginRender( camera );

			for( var i = 0; i < renderLayers.Length; i++ )
			{
				var renderables = scene.RenderableComponents.componentsWithRenderLayer( renderLayers[i] );
				for( var j = 0; j < renderables.length; j++ )
				{
					var renderable = renderables.buffer[j];
					if( renderable.Enabled && renderable.IsVisibleFromCamera( camera ) )
						renderAfterStateCheck( renderable, camera );
				}
			}

			if( shouldDebugRender && Core.debugRenderEnabled )
				debugRender( scene, camera );

			endRender();
		}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			base.onSceneBackBufferSizeChanged( newWidth, newHeight );

			// this is a bit of a hack. we maybe should take the Camera in the constructor
			if( camera == null )
				camera = Core.scene.CreateEntity( "screenspace camera" ).addComponent<Camera>();
		}

	}
}

