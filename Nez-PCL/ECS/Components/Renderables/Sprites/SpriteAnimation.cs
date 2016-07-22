﻿using System.Collections.Generic;
using Nez.Textures;
using Microsoft.Xna.Framework;


namespace Nez.Sprites
{
	public enum AnimationCompletionBehavior
	{
		RemainOnFinalFrame,
		RevertToFirstFrame,
		HideSprite
	}


	public class SpriteAnimation
	{
		public float fps = 10;
		public bool loop = true;
		public bool pingPong;
		public float delay = 0f;
		public float totalDuration;
		public AnimationCompletionBehavior completionBehavior;
		public List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();

		internal float secondsPerFrame;
		internal float iterationDuration;

		bool _hasBeenPreparedForUse;


		public SpriteAnimation()
		{}


		public SpriteAnimation( Subtexture frame )
		{
			addFrame( frame );
		}


		public SpriteAnimation( List<Subtexture> frames )
		{
			addFrames( frames );
		}


		public void prepareForUse()
		{
			if( _hasBeenPreparedForUse )
				return;

			secondsPerFrame = 1f / fps;
			iterationDuration = secondsPerFrame * (float)frames.Count;

			if( loop )
				totalDuration = float.PositiveInfinity;
			else if( pingPong )
				totalDuration = iterationDuration * 2f;
			else
				totalDuration = iterationDuration;

			_hasBeenPreparedForUse = true;
		}


		/// <summary>
		/// sets the origin for all frames in this animation
		/// </summary>
		/// <param name="origin"></param>
		public void setOrigin( Vector2 origin )
		{
			for( var i = 0; i < frames.Count; i++ )
				frames[i].origin = origin;
		}


		/// <summary>
		/// adds a frame to this animation
		/// </summary>
		/// <param name="frame">Frame.</param>
		public void addFrame( SpriteAnimationFrame frame )
		{
			frames.Add( frame );
		}


		/// <summary>
		/// adds a frame to this animation with a 0,0 origin
		/// </summary>
		/// <param name="subtexture">Subtexture.</param>
		public void addFrame( Subtexture subtexture )
		{
			addFrame( new SpriteAnimationFrame( subtexture ) );
		}


		public void addFrames( List<Subtexture> subtextures )
		{
			for( var i = 0; i < subtextures.Count; i++ )
				addFrame( subtextures[i] );
		}

	}
}

