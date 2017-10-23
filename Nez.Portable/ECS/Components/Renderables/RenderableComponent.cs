using System;
using Microsoft.Xna.Framework;


namespace Nez
{
    /// <summary>
    /// Concrete implementation of IRenderable. Contains convenience 
    /// 
    /// Subclasses MUST either override width/height or bounds!
    /// </summary>
    public abstract class RenderableComponent : Component, IRenderable, IComparable<RenderableComponent>
    {
        /// <summary>
        /// width of the RenderableComponent. subclasses that do not override the bounds property must implement this!
        /// </summary>
        /// <value>The width.</value>
        public virtual float Width { get { return Bounds.width; } }

        /// <summary>
        /// height of the RenderableComponent. subclasses that do not override the bounds property must implement this!
        /// </summary>
        /// <value>The height.</value>
        public virtual float Height { get { return Bounds.height; } }

        /// <summary>
        /// the AABB that wraps this object. Used for camera culling.
        /// </summary>
        /// <value>The bounds.</value>
        public virtual RectangleF Bounds
        {
            get
            {
                if (areBoundsDirty)
                {
                    bounds.CalculateBounds(Entity.transform.position, localOffset, Vector2.Zero, Entity.transform.scale, Entity.transform.rotation, Width, Height);
                    areBoundsDirty = false;
                }

                return bounds;
            }
        }

        /// <summary>
        /// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
        /// list on the scene.
        /// </summary>
        public float LayerDepth
        {
            get { return layerDepth; }
            set { SetLayerDepth(value); }
        }

        /// <summary>
        /// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
        /// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
        /// </summary>
        /// <value>The render layer.</value>
        public int RenderLayer
        {
            get { return renderLayer; }
            set { SetRenderLayer(value); }
        }

        /// <summary>
        /// color passed along to the Batcher when rendering
        /// </summary>
        public Color Color { get => color; set => color = value; }

        /// <summary>
        /// used by Renderers to specify how this sprite should be rendered
        /// </summary>
        public virtual Material Material { get; set; }

        /// <summary>
        /// offset from the parent entity. Useful for adding multiple Renderables to an Entity that need specific positioning.
        /// </summary>
        /// <value>The local position.</value>
        public Vector2 LocalOffset
        {
            get { return localOffset; }
            set { SetLocalOffset(value); }
        }

        /// <summary>
        /// the visibility of this Renderable. Changes in state end up calling the onBecameVisible/onBecameInvisible methods.
        /// </summary>
        /// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
        public bool IsVisible
        {
            get { return isVisible; }
            private set
            {
                if (isVisible != value)
                {
                    isVisible = value;

                    if (isVisible)
                    {
                        OnBecameVisible();
                    }
                    else
                    {
                        OnBecameInvisible();
                    }
                }
            }
        }
        
        public override void OnEntityTransformChanged(Transform.Component comp)
        {
            areBoundsDirty = true;
        }
        
        /// <summary>
        /// called by a Renderer. The Camera can be used for culling and the Graphics instance to draw with.
        /// </summary>
        /// <param name="graphics">Graphics.</param>
        /// <param name="camera">Camera.</param>
        public abstract void Render(Graphics graphics, Camera camera);
        
        /// <summary>
        /// renders the bounds only if there is no collider. Always renders a square on the origin.
        /// </summary>
        /// <param name="graphics">Graphics.</param>
        public override void DebugRender(Graphics graphics)
        {
            // if we have no collider draw our bounds
            if (Entity.getComponent<Collider>() == null)
            {
                graphics.batcher.drawHollowRect(Bounds, Debug.Colors.renderableBounds);
            }

            // draw a square for our pivot/origin
            graphics.batcher.drawPixel(Entity.transform.position + localOffset, Debug.Colors.renderableCenter, 4);
        }
        
        /// <summary>
        /// called when the Renderable enters the camera frame. Note that these methods will not be called if your Renderer does not use
        /// isVisibleFromCamera for its culling check. All default Renderers do.
        /// </summary>
        protected virtual void OnBecameVisible()
        {
        }
        
        /// <summary>
        /// called when the renderable exits the camera frame. Note that these methods will not be called if your Renderer does not use
        /// isVisibleFromCamera for its culling check. All default Renderers do.
        /// </summary>
        protected virtual void OnBecameInvisible()
        {
        }
        
        public override void OnRemovedFromEntity()
        {
        }
        
        /// <summary>
        /// returns true if the Renderables bounds intersects the Camera.bounds. Handles state switches for the isVisible flag. Use this method
        /// in your render method to see decide if you should render or not.
        /// </summary>
        /// <returns><c>true</c>, if visible from camera was ised, <c>false</c> otherwise.</returns>
        /// <param name="camera">Camera.</param>
        public virtual bool IsVisibleFromCamera(Camera camera)
        {
            isVisible = camera.bounds.Intersects(Bounds);
            return isVisible;
        }
        
        public RenderableComponent SetMaterial(Material material)
        {
            this.Material = material;
            if (Entity != null && Entity.scene != null)
                Entity.scene.RenderableComponents.setRenderLayerNeedsComponentSort(renderLayer);
            return this;
        }


        /// <summary>
        /// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
        /// </summary>
        /// <returns>The layer depth.</returns>
        /// <param name="value">Value.</param>
        public RenderableComponent SetLayerDepth(float value)
        {
            layerDepth = Mathf.clamp01(value);

            if (Entity != null && Entity.scene != null)
            {
                Entity.scene.RenderableComponents.setRenderLayerNeedsComponentSort(renderLayer);
            }

            return this;
        }
        
        /// <summary>
        /// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
        /// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
        /// </summary>
        /// <returns>The render layer.</returns>
        /// <param name="value">Render layer.</param>
        public RenderableComponent SetRenderLayer(int value)
        {
            if (renderLayer != value)
            {
                var oldRenderLayer = value;
                renderLayer = value;

                // if we have an entity then we are being managed by a ComponentList so we need to let it know that we changed renderLayers
                if (Entity != null && Entity.scene != null)
                {
                    Entity.scene.RenderableComponents.updateRenderableRenderLayer(this, oldRenderLayer, value);
                }
            }

            return this;
        }
        
        /// <summary>
        /// color passed along to the Batcher when rendering
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="value">Color.</param>
        public RenderableComponent SetColor(Color value)
        {
            Color = value;
            return this;
        }
        
        /// <summary>
        /// offset from the parent entity. Useful for adding multiple Renderables to an Entity that need specific positioning.
        /// </summary>
        /// <returns>The local offset.</returns>
        /// <param name="value">Offset.</param>
        public RenderableComponent SetLocalOffset(Vector2 value)
        {
            if (localOffset != value)
            {
                localOffset = value;
                areBoundsDirty = true;
            }

            return this;
        }
        
        /// <summary>
        /// helper for retrieving a Material subclass already casted
        /// </summary>
        /// <returns>The material.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetMaterial<T>() where T : Material
        {
            return Material as T;
        }
        
        /// <Docs>To be added.</Docs>
        /// <para>Returns the sort order of the current instance compared to the specified object.</para>
        /// <summary>
        /// sorted first by renderLayer, then layerDepth and finally material
        /// </summary>
        /// <returns>The to.</returns>
        /// <param name="other">Other.</param>
        public int CompareTo(RenderableComponent other)
        {
            var res = other.renderLayer.CompareTo(renderLayer);

            if (res == 0)
            {
                res = other.layerDepth.CompareTo(layerDepth);

                if (res == 0)
                {
                    // both null or equal
                    if (ReferenceEquals(Material, other.Material))
                    {
                        return 0;
                    }

                    if (other.Material == null)
                    {
                        return -1;
                    }

                    return 1;
                }
            }

            return res;
        }
        
        public override string ToString()
        {
            return $"[RenderableComponent] {GetType()}, renderLayer: {renderLayer}]";
        }

        protected Vector2 localOffset;
        protected float layerDepth;
        protected int renderLayer;
        protected RectangleF bounds;
        protected bool isVisible;
        protected bool areBoundsDirty = true;

        private Color color = Color.White;
    }
}

