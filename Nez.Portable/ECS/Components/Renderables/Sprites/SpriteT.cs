using System;
using Nez.Textures;
using System.Collections.Generic;

namespace Nez.Sprites
{
    /// <summary>
    /// Sprite class handles the display and animation of a sprite. It uses a suggested Enum as a key (you can use an int as well if you
    /// prefer). If you do use an Enum it is recommended to pass in a IEqualityComparer when using an enum like CoreEvents does. See also
    /// the EnumEqualityComparerGenerator.tt T4 template for automatically generating the IEqualityComparer.
    /// </summary>
    public class Sprite<TEnum> : Sprite, IUpdatable where TEnum : struct, IComparable, IFormattable
    {
        public event Action<TEnum> OnAnimationCompletedEvent;
        public bool IsPlaying { get; private set; }
        public int CurrentFrame { get; private set; }

        /// <summary>
        /// gets/sets the currently playing animation
        /// </summary>
        /// <value>The current animation.</value>
        public TEnum CurrentAnimation
        {
            get { return currentAnimationKey; }
            set { Play(value); }
        }

        /// <summary>
        /// Returns all animation keys
        /// </summary>
        public IEnumerable<TEnum> AnimationKeys
        {
            get
            {
                return animations.Keys;
            }
        }

        /// <summary>
        /// beware the beast man! If you use this constructor you must set the subtexture or set animations so that this sprite has proper bounds
        /// when the Scene is running.
        /// </summary>
        /// <param name="customComparer">Custom comparer.</param>
        public Sprite(IEqualityComparer<TEnum> customComparer = null) : base(Graphics.instance.pixelTexture)
        {
            animations = new Dictionary<TEnum, SpriteAnimation>(customComparer);
        }

        public Sprite(IEqualityComparer<TEnum> customComparer, Subtexture subtexture) : base(subtexture)
        {
            animations = new Dictionary<TEnum, SpriteAnimation>(customComparer);
        }

        /// <summary>
        /// Sprite needs a Subtexture at constructor time so that it knows how to size itself
        /// </summary>
        /// <param name="subtexture">Subtexture.</param>
        public Sprite(Subtexture subtexture) : this(null, subtexture)
        {
        }

        /// <summary>
        /// Sprite needs a Subtexture at constructor time so the first frame of the passed in animation will be used for this constructor
        /// </summary>
        /// <param name="animationKey">Animation key.</param>
        /// <param name="animation">Animation.</param>
        public Sprite(TEnum animationKey, SpriteAnimation animation) : this(null, animation.frames[0])
        {
            AddAnimation(animationKey, animation);
        }

        void IUpdatable.update()
        {
            if (currentAnimation == null || !IsPlaying)
                return;

            // handle delay
            if (!delayComplete && elapsedDelay < currentAnimation.delay)
            {
                elapsedDelay += Time.deltaTime;
                if (elapsedDelay >= currentAnimation.delay)
                    delayComplete = true;

                return;
            }

            // count backwards if we are going in reverse
            if (isReversed)
                totalElapsedTime -= Time.deltaTime;
            else
                totalElapsedTime += Time.deltaTime;


            totalElapsedTime = Mathf.clamp(totalElapsedTime, 0f, currentAnimation.totalDuration);
            completedIterations = Mathf.floorToInt(totalElapsedTime / currentAnimation.iterationDuration);
            isLoopingBackOnPingPong = false;


            // handle ping pong loops. if loop is false but pingPongLoop is true we allow a single forward-then-backward iteration
            if (currentAnimation.pingPong)
            {
                if (currentAnimation.loop || completedIterations < 2)
                    isLoopingBackOnPingPong = completedIterations % 2 != 0;
            }


            var elapsedTime = 0f;
            if (totalElapsedTime < currentAnimation.iterationDuration)
            {
                elapsedTime = totalElapsedTime;
            }
            else
            {
                elapsedTime = totalElapsedTime % currentAnimation.iterationDuration;

                // if we arent looping and elapsedTime is 0 we are done. Handle it appropriately
                if (!currentAnimation.loop && elapsedTime == 0)
                {
                    // the animation is done so fire our event
                    OnAnimationCompletedEvent?.Invoke(currentAnimationKey);

                    IsPlaying = false;

                    switch (currentAnimation.completionBehavior)
                    {
                        case AnimationCompletionBehavior.RemainOnFinalFrame:
                            return;
                        case AnimationCompletionBehavior.RevertToFirstFrame:
                            SetSubtexture(currentAnimation.frames[0]);
                            return;
                        case AnimationCompletionBehavior.HideSprite:
                            subtexture = null;
                            currentAnimation = null;
                            return;
                    }
                }
            }


            // if we reversed the animation and we reached 0 total elapsed time handle un-reversing things and loop continuation
            if (isReversed && totalElapsedTime <= 0)
            {
                isReversed = false;

                if (currentAnimation.loop)
                {
                    totalElapsedTime = 0f;
                }
                else
                {
                    // the animation is done so fire our event
                    OnAnimationCompletedEvent?.Invoke(currentAnimationKey);

                    IsPlaying = false;
                    return;
                }
            }

            // time goes backwards when we are reversing a ping-pong loop
            if (isLoopingBackOnPingPong)
                elapsedTime = currentAnimation.iterationDuration - elapsedTime;


            // fetch our desired frame
            var desiredFrame = Mathf.floorToInt(elapsedTime / currentAnimation.secondsPerFrame);
            if (desiredFrame != CurrentFrame)
            {
                CurrentFrame = desiredFrame;
                SetSubtexture(currentAnimation.frames[CurrentFrame]);
                HandleFrameChanged();

                // ping-pong needs special care. we don't want to double the frame time when wrapping so we man-handle the totalElapsedTime
                if (currentAnimation.pingPong && (CurrentFrame == 0 || CurrentFrame == currentAnimation.frames.Count - 1))
                {
                    if (isReversed)
                        totalElapsedTime -= currentAnimation.secondsPerFrame;
                    else
                        totalElapsedTime += currentAnimation.secondsPerFrame;
                }
            }
        }

        public Sprite<TEnum> AddAnimation(TEnum key, SpriteAnimation animation)
        {
            // if we have no subtexture use the first frame we find
            if (subtexture == null && animation.frames.Count > 0)
                SetSubtexture(animation.frames[0]);
            animations[key] = animation;

            return this;
        }

        public SpriteAnimation GetAnimation(TEnum key)
        {
            Assert.isTrue(animations.ContainsKey(key), "{0} is not present in animations", key);
            return animations[key];
        }

        /// <summary>
        /// plays the animation at the given index. You can cache the indices by calling animationIndexForAnimationName.
        /// </summary>
        /// <param name="animationKey">Animation key.</param>
        /// <param name="startFrame">Start frame.</param>
        public SpriteAnimation Play(TEnum animationKey, int startFrame = 0)
        {
            Assert.isTrue(animations.ContainsKey(animationKey), "Attempted to play an animation that doesnt exist");

            var animation = animations[animationKey];
            animation.prepareForUse();

            currentAnimationKey = animationKey;
            currentAnimation = animation;
            IsPlaying = true;
            isReversed = false;
            CurrentFrame = startFrame;
            SetSubtexture(currentAnimation.frames[CurrentFrame]);

            totalElapsedTime = (float)startFrame * currentAnimation.secondsPerFrame;
            return animation;
        }

        public bool IsAnimationPlaying(TEnum animationKey)
        {
            return currentAnimation != null && currentAnimationKey.Equals(animationKey);
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void UnPause()
        {
            IsPlaying = true;
        }

        public void ReverseAnimation()
        {
            isReversed = !isReversed;
        }

        public void Stop()
        {
            IsPlaying = false;
            currentAnimation = null;
        }

        void HandleFrameChanged()
        {
            // TODO: add animation frame triggers
        }

        private Dictionary<TEnum, SpriteAnimation> animations;

        // playback state
        private SpriteAnimation currentAnimation;
        private TEnum currentAnimationKey;
        private float totalElapsedTime;
        private float elapsedDelay;
        private int completedIterations;
        private bool delayComplete;
        private bool isReversed;
        private bool isLoopingBackOnPingPong;
    }
}

