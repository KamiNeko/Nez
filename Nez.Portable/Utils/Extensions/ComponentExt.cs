using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez
{
	public static class ComponentExt
	{
		#region Entity Component management
		
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T addComponent<T>( this Component self, T component ) where T : Component
		{
			return self.Entity.addComponent( component );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T addComponent<T>( this Component self ) where T : Component, new()
		{
			return self.Entity.addComponent<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T getComponent<T>( this Component self ) where T : Component
		{
			return self.Entity.getComponent<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void getComponents<T>( this Component self, List<T> componentList ) where T : class
		{
			self.Entity.getComponents<T>( componentList );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static List<T> getComponents<T>( this Component self ) where T : Component
		{
			return self.Entity.getComponents<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool removeComponent<T>( this Component self ) where T : Component
		{
			return self.Entity.removeComponent<T>();
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void removeComponent( this Component self, Component component )
		{
			self.Entity.removeComponent( component );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void removeComponent( this Component self )
		{
			self.Entity.removeComponent( self );
		}

		#endregion

	}
}

