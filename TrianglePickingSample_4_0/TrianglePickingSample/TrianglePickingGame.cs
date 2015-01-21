#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
#endregion

namespace TrianglePicking
{
    /// <summary>
    /// Sample showing how to implement per-triangle picking. This uses a custom
    /// content pipeline processor to attach a list of vertex position data to each
    /// model as part of the build process, and then implements a ray-to-triangle
    /// intersection method to collide against this vertex data.
    /// </summary>
    /// public class TrianglePickingGame : Microsoft.Xna.Framework.Game
    class TrianglePickingGame : GraphicsDeviceControl
    {
        #region Variables

        BasicEffect effect;
        Stopwatch timer;
        Stopwatch timer2;

        ContentManager content;

        VertexPositionColor[][] mSelectedVertices = new VertexPositionColor[1][]{new VertexPositionColor[3]};
        VertexPositionColor[][] mSelectedVerticesBefore;

        Color mColorStandard = Color.Lerp(Color.LightGray, Color.Black, 0.1f);
        int mSelectedTriangles = 0;

        MainForm.EditMode mEditMode = MainForm.EditMode.NORMAL;

        public float mYaw = 0;
        public float mPitch = 0;
        public float mRoll = 0;

        System.Drawing.Size mOldSize;
        private int mPreviousScrollValue;

        Point mMousePositionLast = new Point(MousePosition.X, MousePosition.Y);
        VertexPositionColorTexture[] mVertexList = new VertexPositionColorTexture[24];

        const string TEXTURE_NAME    = "Green-gel-x";  // http://upload.wikimedia.org/wikipedia/commons/9/99/Green-gel-x.png
        const int TOP_LEFT_BACK      = 0;
        const int TOP_RIGHT_BACK     = 1;
        const int BOTTOM_RIGHT_BACK  = 2;
        const int BOTTOM_LEFT_BACK   = 3;

        const int TOP_LEFT_FORTH     = 4;
        const int TOP_RIGHT_FORTH    = 5;
        const int BOTTOM_RIGHT_FORTH = 6;
        const int BOTTOM_LEFT_FORTH  = 7;
        RasterizerState WIREFRAME_RASTERIZER_STATE = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        Texture2D texture;
        VertexPositionColorTexture[] vertexData;
        Int16[] indexData;

        #endregion

        #region Constants

        // ModelFilenames is the list of models that we will be putting on top of the
        // table. These strings will be used as arguments to content.Load<Model> and
        // will be drawn when the cursor is over an object.
        //static readonly string[] ModelFilenames =
        //{
        //    "Sphere",
        //    "Cats",
        //    "P2Wedge",
        //    "Cylinder",
        //};

        private string[] mVertexModelFilenames = 
        {
            "Cube",
        };
        
        // the following constants control the speed at which the camera moves
        // how fast does the camera move up, down, left, and right?
        const float CameraRotateSpeed = .1f;
        // how fast does the camera zoom in and out?
        const float CameraZoomSpeed = .01f;
        // the camera can't be further away than this distance
        const float CameraMaxDistance = 20.0f;
        // and it can't be closer than this
        const float CameraMinDistance = 1.2f;

        // the following constants control how the camera's default position
        const float CameraDefaultArc = -30.0f;
        const float CameraDefaultRotation = 225;
        const float CameraDefaultDistance = 4.5f;

        #endregion

        #region Fields

        //GraphicsDeviceManager graphics;
        GraphicsDevice graphics;

        // the current input states.  These are updated in the HandleInput function,
        // and used primarily in the UpdateCamera function.
        KeyboardState currentKeyboardState;
        MouseState currentMouseState;
        //GamePadState currentGamePadState;

        // a SpriteBatch and SpriteFont, which we will use to draw the objects' names
        // when they are selected.
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        // The cursor is used to tell what the user's pointer/mouse is over. The cursor
        // is moved with the left thumbstick. On windows, the mouse can be used as well.
        Cursor cursor;

        // the table that all of the objects are drawn on, and table model's 
        // absoluteBoneTransforms. Since the table is not animated, these can be 
        // calculated once and saved.
        Model table;
        Matrix[] tableAbsoluteBoneTransforms;

        // these are the models that we will draw on top of the table. we'll store them
        // and their bone transforms in arrays. Again, since these models aren't
        // animated, we can calculate their bone transforms once and save the result.
        //Model[] models = new Model[4];
        VertexModel[] mVertexModels = new VertexModel[1];
        VertexModel[] mVertexModelsBefore = new VertexModel[1];
        //Matrix[][] modelAbsoluteBoneTransforms = new Matrix[ModelFilenames.Length][];
        Matrix[][] mVertexModelAbsoluteBoneTransforms = new Matrix[1][];
        // each model will need one more matrix: a world transform. This matrix will be
        // used to place each model at a different location in the world.
        //Matrix[] modelWorldTransforms = new Matrix[ModelFilenames.Length];
        Matrix[] mVertexModelWorldTransforms = new Matrix[1];

        // The next set of variables are used to control the camera used in the sample. 
        // It is an arc ball camera, so it can rotate in a sphere around the target, and
        // zoom in and out.
        float cameraArc = CameraDefaultArc;
        float cameraRotation = CameraDefaultRotation;
        float cameraDistance = CameraDefaultDistance;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        // To keep things efficient, the picking works by first applying a bounding
        // sphere test, and then only bothering to test each individual triangle
        // if the ray intersects the bounding sphere. This allows us to trivially
        // reject many models without even needing to bother looking at their triangle
        // data. This field keeps track of which models passed the bounding sphere
        // test, so you can see the difference between this approximation and the more
        // accurate triangle picking.
        List<string> insideBoundingSpheres = new List<string>();

        // Store the name of the model underneath the cursor (or null if there is none).
        string pickedModelName;

        // Vertex array that stores exactly which triangle was picked.
        VertexPositionColor[] pickedTriangle =
        {
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
            new VertexPositionColor(Vector3.Zero, Color.Magenta),
        };

        int mPickedTriangleModelIndex = -1;

        VertexPositionColor[] helpGrid = new VertexPositionColor[48];
        
        // Effect and vertex declaration for drawing the picked triangle.
        BasicEffect lineEffect;

        // Custom rasterizer state for drawing in wireframe.
        static RasterizerState WireFrame = new RasterizerState
        {
            FillMode = FillMode.WireFrame,
            CullMode = CullMode.None
        };

        #endregion

        #region Initialization


        public TrianglePickingGame()
        {
            
        }


        protected override void Initialize()
        {
            Mouse.WindowHandle = this.Parent.Handle;
            mOldSize = this.Size;
            mPreviousScrollValue = currentMouseState.ScrollWheelValue;
            //this.MouseWheel += new MouseEventHandler(this.Scroll);
            
            // Create our effect.
            effect = new BasicEffect(GraphicsDevice);

            effect.VertexColorEnabled = true;

            //graphics = new GraphicsDeviceManager(this);
            graphics = GraphicsDevice;
            content = new ContentManager(Services, "Content");
            //content.RootDirectory = "Content";

            // Set up the world transforms that each model will use. They'll be
            // positioned in a line along the x axis.
            //for (int i = 0; i < modelWorldTransforms.Length; i++)
            //{
            //    float x = i - modelWorldTransforms.Length / 2;
            //    modelWorldTransforms[i] =
            //        Matrix.CreateTranslation(new Vector3(x, 0, 0));
            //}

            mVertexModelWorldTransforms[0] =
                    Matrix.CreateTranslation(new Vector3(0, 0, 0));

            cursor = new Cursor(GraphicsDevice, content);
            //components.Add(cursor);

            helpGrid[0] = new VertexPositionColor(new Vector3(-5, 0,  5), Color.Gray);
            helpGrid[1] = new VertexPositionColor(new Vector3( 5, 0,  5), Color.Gray);
            helpGrid[2] = new VertexPositionColor(new Vector3( 5, 0,  5), Color.Gray);
            helpGrid[3] = new VertexPositionColor(new Vector3( 5, 0, -5), Color.Gray);
            helpGrid[4] = new VertexPositionColor(new Vector3( 5, 0, -5), Color.Gray);
            helpGrid[5] = new VertexPositionColor(new Vector3(-5, 0, -5), Color.Gray);
            helpGrid[6] = new VertexPositionColor(new Vector3(-5, 0, -5), Color.Gray);
            helpGrid[7] = new VertexPositionColor(new Vector3(-5, 0,  5), Color.Gray);

            for (int x = 8; x < 28; x += 2)
            {
                if (x != 18)
                {
                    helpGrid[x    ] = new VertexPositionColor(new Vector3(-5, 0, -5 + (x - 8) / 2), Color.Gray);
                    helpGrid[x + 1] = new VertexPositionColor(new Vector3( 5, 0, -5 + (x - 8) / 2), Color.Gray);
                }
                else
                {
                    helpGrid[x    ] = new VertexPositionColor(new Vector3(-5, 0, -5 + (x - 8) / 2), Color.Red);
                    helpGrid[x + 1] = new VertexPositionColor(new Vector3( 5, 0, -5 + (x - 8) / 2), Color.Red);
                }
            }

            for (int z = 28; z < 48; z += 2)
            {
                if (z != 38)
                {
                    helpGrid[z    ] = new VertexPositionColor(new Vector3(-5 + (z - 28) / 2, 0, -5), Color.Gray);
                    helpGrid[z + 1] = new VertexPositionColor(new Vector3(-5 + (z - 28) / 2, 0,  5), Color.Gray);
                }
                else
                {
                    helpGrid[z    ] = new VertexPositionColor(new Vector3(-5 + (z - 28) / 2, 0, -5), Color.Green);
                    helpGrid[z + 1] = new VertexPositionColor(new Vector3(-5 + (z - 28) / 2, 0,  5), Color.Green);
                }
            }

            // now that the GraphicsDevice has been created, we can create the projection matrix.
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), GraphicsDevice.Viewport.AspectRatio, .01f, 1000);

            SetUpVertices(mColorStandard);
            SetUpIndices();
            SetUpVertexModel(mColorStandard);

            //base.Initialize();

            // Start the animation timer.
            timer = Stopwatch.StartNew();
            timer2 = Stopwatch.StartNew();

            LoadContent();

            // Hook the idle event to constantly redraw our animation.
            Application.Idle += delegate { Invalidate(); };
        }

        private void SetUpVertexModel(Color color)
        {
            const float HALF_SIDE = 1.0f;
            const float Z = 0.0f;
            VertexModelMesh vertexModelMesh = new VertexModelMesh();

            mVertexModels[0] = new VertexModel();

            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE,  HALF_SIDE,  HALF_SIDE), color, new Vector2(0, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE,  HALF_SIDE,  HALF_SIDE), color, new Vector2(1, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE, -HALF_SIDE,  HALF_SIDE), color, new Vector2(1, 1));
            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE, -HALF_SIDE,  HALF_SIDE), color, new Vector2(0, 1));

            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE,  HALF_SIDE, -HALF_SIDE), color, new Vector2(0, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE,  HALF_SIDE, -HALF_SIDE), color, new Vector2(1, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE, -HALF_SIDE, -HALF_SIDE), color, new Vector2(1, 1));
            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE, -HALF_SIDE, -HALF_SIDE), color, new Vector2(0, 1));

            //  BOTTOM
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);

            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);
                                    
            //  LEFT
            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);

            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);
            
            //  FORTH
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);

            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            
            //  RIGHT
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);
            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);

            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);
                        
            //  BACK
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);

            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);
            
            //  TOP
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);

            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            
            mVertexModels[mVertexModels.Length - 1].AddMesh(vertexModelMesh);
        }

        private void SetUpVertices(Color color)
        {
            const float HALF_SIDE = 1.0f;
            const float Z = 0.0f;

            vertexData = new VertexPositionColorTexture[4];
            vertexData[TOP_LEFT_BACK] = new VertexPositionColorTexture(new Vector3(-HALF_SIDE, HALF_SIDE, Z), color, new Vector2(0, 0));
            vertexData[TOP_RIGHT_BACK] = new VertexPositionColorTexture(new Vector3(HALF_SIDE, HALF_SIDE, Z), color, new Vector2(1, 0));
            vertexData[BOTTOM_RIGHT_BACK] = new VertexPositionColorTexture(new Vector3(HALF_SIDE, -HALF_SIDE, Z), color, new Vector2(1, 1));
            vertexData[BOTTOM_LEFT_BACK] = new VertexPositionColorTexture(new Vector3(-HALF_SIDE, -HALF_SIDE, Z), color, new Vector2(0, 1));
        }

        private void SetUpIndices()
        {
            indexData = new Int16[6];
            indexData[0] = TOP_LEFT_BACK;
            indexData[1] = BOTTOM_RIGHT_BACK;
            indexData[2] = BOTTOM_LEFT_BACK;

            indexData[3] = TOP_LEFT_BACK;
            indexData[4] = TOP_RIGHT_BACK;
            indexData[5] = BOTTOM_RIGHT_BACK;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        /// protected override void LoadContent()
        protected void LoadContent()
        {
            // load all of the models that will appear on the table:
            //for (int i = 0; i < ModelFilenames.Length; i++)
            //{
            //    // load the actual model, using ModelFilenames to determine what
            //    // file to load.
            //    models[i] = content.Load<Model>(ModelFilenames[i]);

            //    // create an array of matrices to hold the absolute bone transforms,
            //    // calculate them, and copy them in.
            //    modelAbsoluteBoneTransforms[i] = new Matrix[models[i].Bones.Count];
            //    models[i].CopyAbsoluteBoneTransformsTo(
            //        modelAbsoluteBoneTransforms[i]);
            //}

            // now that we've loaded in the models that will sit on the table, go
            // through the same procedure for the table itself.
            table = content.Load<Model>("Table");
            tableAbsoluteBoneTransforms = new Matrix[table.Bones.Count];
            table.CopyAbsoluteBoneTransformsTo(tableAbsoluteBoneTransforms);

            // create a spritebatch and load the font, which we'll use to draw the
            // models' names.
            //spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("hudFont");

            // create the effect and vertex declaration for drawing the
            // picked triangle.
            //lineEffect = new BasicEffect(graphics.GraphicsDevice);
            lineEffect = new BasicEffect(GraphicsDevice);
            lineEffect.VertexColorEnabled = true;

            texture = content.Load<Texture2D>(TEXTURE_NAME);
        }


        #endregion

        #region Update and Draw
    

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        /// protected override void Update(GameTime gameTime)
        protected void Update()
        {
            HandleInput();

            UpdateCamera();

            UpdatePicking();

            if (mEditMode == MainForm.EditMode.MOVEX ||
                mEditMode == MainForm.EditMode.MOVEY ||
                mEditMode == MainForm.EditMode.MOVEZ)
            {
                UpdateMovement(mEditMode);
            }

            //base.Update(gameTime);
        }

        protected override void Draw()
        {
            Update();
            //GraphicsDevice device = graphics.GraphicsDevice;
            GraphicsDevice device = GraphicsDevice;

            device.Clear(Color.Lerp(Color.LightGray, Color.White, 0.2f));

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            // Draw help floor
            DrawHelpFloor();

            // Draw the table.
            //DrawModel(table, Matrix.Identity, tableAbsoluteBoneTransforms);

            // Use the same DrawModel function to draw all of the models on the table.
            //for (int i = 0; i < models.Length; i++)
            //{
            //    DrawModel(models[i], modelWorldTransforms[i],
            //                         modelAbsoluteBoneTransforms[i]);
            //}

            // Use the same DrawModel function to draw all of the models
            for (int i = 0; i < mVertexModels.Length; i++)
            {
                DrawVertexModel(mVertexModels[i], mVertexModelWorldTransforms[i],
                                     mVertexModelAbsoluteBoneTransforms[i], device, false);
            }

            //DrawVerticeModel();

            DrawSelectedTriangles();

            // Use the same DrawModel function to draw all wireframes of the models
            for (int i = 0; i < mVertexModels.Length; i++)
            {
                DrawVertexModel(mVertexModels[i], mVertexModelWorldTransforms[i],
                                     mVertexModelAbsoluteBoneTransforms[i], device, true);
            }

            // Draw the outline of the triangle under the cursor.
            DrawPickedTriangle();
            
            // Draw text describing the picking results.
            DrawText();
        }


        /// <summary>
        /// Runs a per-triangle picking algorithm over all the models in the scene,
        /// storing which triangle is currently under the cursor.
        /// </summary>
        void UpdatePicking()
        {
            // Look up a collision ray based on the current cursor position. See the
            // Picking Sample documentation for a detailed explanation of this.
            cursor.Update(DateTime.Now);
            Ray cursorRay = cursor.CalculateCursorRay(PointToClient(MousePosition), projectionMatrix, viewMatrix);
            Ray cursorRayVertex = cursor.CalculateCursorRay(PointToClient(MousePosition), projectionMatrix, viewMatrix);

            // Clear the previous picking results.
            insideBoundingSpheres.Clear();

            pickedModelName = null;
            mPickedTriangleModelIndex = -1;
            
            // Keep track of the closest object we have seen so far, so we can
            // choose the closest one if there are several models under the cursor.
            float closestIntersection = float.MaxValue;

            // Loop over all our models.
            for (int i = 0; i < mVertexModels.Length; i++)
            {
                bool insideBoundingSphere;
                Vector3 vertex1, vertex2, vertex3;

                // Perform the ray to model intersection test.
                float? intersection = RayIntersectsModel(cursorRayVertex, mVertexModels[i], 
                                                         mVertexModelWorldTransforms[i],
                                                         out insideBoundingSphere,
                                                         out vertex1, out vertex2,
                                                         out vertex3);

                // If this model passed the initial bounding sphere test, remember
                // that so we can display it at the top of the screen.
                if (insideBoundingSphere)
                    insideBoundingSpheres.Add(mVertexModelFilenames[i]);

                // Do we have a per-triangle intersection with this model?
                if (intersection != null)
                {
                    // If so, is it closer than any other model we might have
                    // previously intersected?
                    if (intersection < closestIntersection)
                    {
                        // Store information about this model.
                        closestIntersection = intersection.Value;

                        pickedModelName = mVertexModelFilenames[i];
                        mPickedTriangleModelIndex = i;

                        // Store vertex positions so we can display the picked triangle.
                        pickedTriangle[0].Position = vertex1;
                        pickedTriangle[1].Position = vertex2;
                        pickedTriangle[2].Position = vertex3;
                    }
                }
            }
        }


        /// <summary>
        /// Checks whether a ray intersects a model. This method needs to access
        /// the model vertex data, so the model must have been built using the
        /// custom TrianglePickingProcessor provided as part of this sample.
        /// Returns the distance along the ray to the point of intersection, or null
        /// if there is no intersection.
        /// </summary>
        static float? RayIntersectsModel(Ray rayVertex, VertexModel vertexModel, Matrix vertexModelTransform,
                                         out bool insideBoundingSphere,
                                         out Vector3 vertex1, out Vector3 vertex2,
                                         out Vector3 vertex3)
        {
            vertex1 = vertex2 = vertex3 = Vector3.Zero;

            // The input ray is in world space, but our model data is stored in object
            // space. We would normally have to transform all the model data by the
            // modelTransform matrix, moving it into world space before we test it
            // against the ray. That transform can be slow if there are a lot of
            // triangles in the model, however, so instead we do the opposite.
            // Transforming our ray by the inverse modelTransform moves it into object
            // space, where we can test it directly against our model data. Since there
            // is only one ray but typically many triangles, doing things this way
            // around can be much faster.

            //Matrix inverseTransform = Matrix.Invert(modelTransform);
            Matrix inverseVertexTransform = Matrix.Invert(vertexModelTransform);

            //ray.Position = Vector3.Transform(ray.Position, inverseTransform);
            //ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);
            rayVertex.Position = Vector3.Transform(rayVertex.Position, inverseVertexTransform);
            rayVertex.Direction = Vector3.TransformNormal(rayVertex.Direction, inverseVertexTransform);

            // Look up our custom collision data from the Tag property of the model.
            //Dictionary<string, object> tagData = (Dictionary<string, object>)model.Tag;

            //if (tagData == null)
            //{
            //    throw new InvalidOperationException(
            //        "Model.Tag is not set correctly. Make sure your model " +
            //        "was built using the custom TrianglePickingProcessor.");
            //}

            // Start off with a fast bounding sphere test.
            //BoundingSphere boundingSphere = (BoundingSphere)tagData["BoundingSphere"];
            BoundingSphere boundingSphere = vertexModel.GetBoundingSphere();
            
            if (boundingSphere.Intersects(rayVertex) == null)
            {
                // If the ray does not intersect the bounding sphere, we cannot
                // possibly have picked this model, so there is no need to even
                // bother looking at the individual triangle data.
                insideBoundingSphere = false;

                return null;
            }
            else
            {
                // The bounding sphere test passed, so we need to do a full
                // triangle picking test.
                insideBoundingSphere = true;

                // Keep track of the closest triangle we found so far,
                // so we can always return the closest one.
                float? closestIntersection = null;

                // Loop over the vertex data, 3 at a time (3 vertices = 1 triangle).
                //Vector3[] vertices = (Vector3[])tagData["Vertices"];
                //MessageBox.Show(vertices[0].ToString());
                Vector3[] verticesVertex = vertexModel.GetMeshes()[0].GetVertexVectorsFromIndice();

                for (int i = 0; i < verticesVertex.Length; i += 3)
                {
                    // Perform a ray to triangle intersection test.
                    float? intersection;

                    RayIntersectsTriangle(ref rayVertex,
                                          ref verticesVertex[i],
                                          ref verticesVertex[i + 1],
                                          ref verticesVertex[i + 2],
                                          out intersection);

                    // Does the ray intersect this triangle?
                    if (intersection != null)
                    {
                        // If so, is it closer than any other previous triangle?
                        if ((closestIntersection == null) ||
                            (intersection < closestIntersection))
                        {
                            // Store the distance to this triangle.
                            closestIntersection = intersection;

                            // Transform the three vertex positions into world space,
                            // and store them into the output vertex parameters.
                            Vector3.Transform(ref verticesVertex[i],
                                              ref vertexModelTransform, out vertex1);

                            Vector3.Transform(ref verticesVertex[i + 1],
                                              ref vertexModelTransform, out vertex2);

                            Vector3.Transform(ref verticesVertex[i + 2],
                                              ref vertexModelTransform, out vertex3);
                        }
                    }
                }

                return closestIntersection;
            }
        }


        /// <summary>
        /// Checks whether a ray intersects a triangle. This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        /// 
        /// This method is implemented using the pass-by-reference versions of the
        /// XNA math functions. Using these overloads is generally not recommended,
        /// because they make the code less readable than the normal pass-by-value
        /// versions. This method can be called very frequently in a tight inner loop,
        /// however, so in this particular case the performance benefits from passing
        /// everything by reference outweigh the loss of readability.
        /// </summary>
        static void RayIntersectsTriangle(ref Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;
            
            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }

        void DrawSelectedTriangles()
        {
            if (mSelectedVertices != null && mSelectedTriangles > 0)
            {
                //GraphicsDevice device = graphics.GraphicsDevice;
                GraphicsDevice device = GraphicsDevice;

                // Set line drawing renderstates. We disable backface culling
                // and turn off the depth buffer because we want to be able to
                // see the picked triangle outline regardless of which way it is
                // facing, and even if there is other geometry in front of it.

                // Activate the line drawing BasicEffect.

                // Draw textured box
                device.RasterizerState = RasterizerState.CullNone;  // vertex order doesn't matter
                device.BlendState = BlendState.NonPremultiplied;    // use alpha blending
                //device.DepthStencilState    = DepthStencilState.None;  // don't bother with the depth/stencil buffer

                lineEffect.View = viewMatrix;
                lineEffect.Projection = projectionMatrix;
                //lineEffect.Texture          = texture;
                //lineEffect.TextureEnabled   = true;
                lineEffect.DiffuseColor = Color.White.ToVector3();
                lineEffect.Alpha = 0.9f;
                lineEffect.CurrentTechnique.Passes[0].Apply();
                
                // Draw the triangle.
                for (int i = 0; i < mSelectedVertices.Length; i++)
                {
                    device.DrawUserPrimitives(PrimitiveType.TriangleList,
                                              mSelectedVertices[i], 0, mSelectedVertices[i].Length / 3);
                }

                // Reset renderstates to their default values.
                device.RasterizerState = RasterizerState.CullCounterClockwise;
                device.DepthStencilState = DepthStencilState.Default;
                lineEffect.Alpha        = 1f;
            }
        }


        /// <summary>
        /// Helper for drawing the outline of the triangle currently under the cursor.
        /// </summary>
        void DrawPickedTriangle()
        {
            if (pickedModelName != null)
            {
                //GraphicsDevice device = graphics.GraphicsDevice;
                GraphicsDevice device = GraphicsDevice;

                // Set line drawing renderstates. We disable backface culling
                // and turn off the depth buffer because we want to be able to
                // see the picked triangle outline regardless of which way it is
                // facing, and even if there is other geometry in front of it.
                device.RasterizerState = WireFrame;
                device.DepthStencilState = DepthStencilState.None;

                // Activate the line drawing BasicEffect.
                lineEffect.Projection = projectionMatrix;
                lineEffect.View = viewMatrix;
                lineEffect.TextureEnabled = false;

                lineEffect.CurrentTechnique.Passes[0].Apply();

                // Draw the triangle.
                device.DrawUserPrimitives(PrimitiveType.TriangleList,
                                          pickedTriangle, 0, 1);

                // Reset renderstates to their default values.
                device.RasterizerState = RasterizerState.CullCounterClockwise;
                device.DepthStencilState = DepthStencilState.Default;
            }
        }

        /// <summary>
        /// Helper for drawing the outline of the triangle currently under the cursor.
        /// </summary>
        void DrawHelpFloor()
        {
            //GraphicsDevice device = graphics.GraphicsDevice;
            GraphicsDevice device = GraphicsDevice;

            // Set line drawing renderstates. We disable backface culling
            // and turn off the depth buffer because we want to be able to
            // see the picked triangle outline regardless of which way it is
            // facing, and even if there is other geometry in front of it.

            // Activate the line drawing BasicEffect.

            // Draw textured box
            device.RasterizerState = RasterizerState.CullNone;  // vertex order doesn't matter
            device.BlendState = BlendState.NonPremultiplied;    // use alpha blending
            //device.DepthStencilState    = DepthStencilState.None;  // don't bother with the depth/stencil buffer

            lineEffect.View = viewMatrix;
            lineEffect.Projection = projectionMatrix;
            //lineEffect.Texture          = texture;
            //lineEffect.TextureEnabled   = true;
            lineEffect.DiffuseColor = Color.White.ToVector3();
            lineEffect.CurrentTechnique.Passes[0].Apply();

            // Draw the triangle.
            device.DrawUserPrimitives(PrimitiveType.LineList,
                                        helpGrid, 0, 24);

            // Reset renderstates to their default values.
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
        }

        void DrawVerticeModel()
        {
            //GraphicsDevice device = graphics.GraphicsDevice;
            GraphicsDevice device = GraphicsDevice;

            // Set line drawing renderstates. We disable backface culling
            // and turn off the depth buffer because we want to be able to
            // see the picked triangle outline regardless of which way it is
            // facing, and even if there is other geometry in front of it.

            // Activate the line drawing BasicEffect.

            // Draw textured box
            device.RasterizerState      = RasterizerState.CullNone;  // vertex order doesn't matter
            device.BlendState           = BlendState.NonPremultiplied;    // use alpha blending
            //device.DepthStencilState    = DepthStencilState.None;  // don't bother with the depth/stencil buffer

            lineEffect.View             = viewMatrix;
            lineEffect.Projection       = projectionMatrix;
            //lineEffect.Texture          = texture;
            //lineEffect.TextureEnabled   = true;
            lineEffect.DiffuseColor     = Color.White.ToVector3();
            lineEffect.CurrentTechnique.Passes[0].Apply();

            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, 4, indexData, 0, 2);

            // Reset renderstates to their default values.
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
        }


        /// <summary>
        /// Helper for drawing text showing the current picking results.
        /// </summary>
        void DrawText()
        {
            // Draw the text twice to create a drop-shadow effect, first in black one
            // pixel down and to the right, then again in white at the real position.
            Vector2 shadowOffset = new Vector2(1, 1);

            spriteBatch.Begin();

            // Draw a list of which models passed the initial bounding sphere test.
            if (insideBoundingSpheres.Count > 0)
            {
                string text = "Inside bounding sphere: " +
                                string.Join(", ", insideBoundingSpheres.ToArray());

                Vector2 position = new Vector2(50, 50);

                spriteBatch.DrawString(spriteFont, text,
                                       position + shadowOffset, Color.Black);
                
                spriteBatch.DrawString(spriteFont, text,
                                       position, Color.White);
            }

            // Draw the name of the model that passed the per-triangle picking test.
            if (pickedModelName != null)
            {
                System.Drawing.Point mousePosition = PointToClient(MousePosition);
                Vector2 position = new Vector2(mousePosition.X, mousePosition.Y);//cursor.Position;

                // Draw the text below the cursor position.
                position.X += 32;
                position.Y += 32;

                // Center the string.
                position -= spriteFont.MeasureString(pickedModelName) / 2;
                
                spriteBatch.DrawString(spriteFont, pickedModelName,
                                       position + shadowOffset, Color.Black);
                
                spriteBatch.DrawString(spriteFont, pickedModelName,
                                       position, Color.White);
            }

            string text2 = "Edit mode: " +
                                mEditMode.ToString();

            Vector2 position2 = new Vector2(8, 8);

            spriteBatch.DrawString(spriteFont, text2,
                                   position2 + shadowOffset, Color.Black);

            spriteBatch.DrawString(spriteFont, text2,
                                   position2, Color.White);

            spriteBatch.End();
        }


        /// <summary>
        /// DrawModel is a helper function that takes a model, world matrix, and
        /// bone transforms. It does just what its name implies, and draws the model.
        /// </summary>
        private void DrawModel(Model model, Matrix worldTransform,
                                            Matrix[] absoluteBoneTransforms)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] *
                                                                        worldTransform;
                }

                mesh.Draw();
            }
        }

        private void DrawVertexModel(VertexModel model, Matrix worldTransform,
                                            Matrix[] absoluteBoneTransforms, GraphicsDevice device, bool wireframe)
        {
            foreach (VertexModelMesh mesh in model.GetMeshes())
            {
                foreach (BasicEffect effect in mesh.GetEffects())
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                    //effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] *
                    //                                                    worldTransform;
                }

                // Activate the line drawing BasicEffect
                //  Send with temporary effect
                lineEffect.Projection = projectionMatrix;
                lineEffect.View = viewMatrix;
                lineEffect.TextureEnabled = false;

                lineEffect.CurrentTechnique.Passes[0].Apply();

                if (!wireframe)
                {
                    mesh.Draw(device, lineEffect);
                }
                else
                {
                    mesh.DrawWireFrame(device, lineEffect);
                }
            }
        }
        
        public void CreateCube()
        {
            const float HALF_SIDE = 1.0f;
            VertexModelMesh vertexModelMesh = new VertexModelMesh();

            Color color = mColorStandard;

            Array.Resize(ref mVertexModels, mVertexModels.Length + 1);
            mVertexModels[mVertexModels.Length - 1] = new VertexModel();

            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE,  HALF_SIDE,  HALF_SIDE), color, new Vector2(0, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE,  HALF_SIDE,  HALF_SIDE), color, new Vector2(1, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE, -HALF_SIDE,  HALF_SIDE), color, new Vector2(1, 1));
            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE, -HALF_SIDE,  HALF_SIDE), color, new Vector2(0, 1));
            
            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE,  HALF_SIDE, -HALF_SIDE), color, new Vector2(0, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE,  HALF_SIDE, -HALF_SIDE), color, new Vector2(1, 0));
            vertexModelMesh.AddVertex(new Vector3( HALF_SIDE, -HALF_SIDE, -HALF_SIDE), color, new Vector2(1, 1));
            vertexModelMesh.AddVertex(new Vector3(-HALF_SIDE, -HALF_SIDE, -HALF_SIDE), color, new Vector2(0, 1));

            //  BOTTOM
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);

            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);

            //  LEFT
            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);

            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);

            //  FORTH
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);

            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);

            //  RIGHT
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);
            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);

            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);

            //  BACK
            vertexModelMesh.AddIndexData(BOTTOM_LEFT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);

            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);
            vertexModelMesh.AddIndexData(BOTTOM_RIGHT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);

            //  TOP
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);

            vertexModelMesh.AddIndexData(TOP_LEFT_BACK);
            vertexModelMesh.AddIndexData(TOP_LEFT_FORTH);
            vertexModelMesh.AddIndexData(TOP_RIGHT_FORTH);

            mVertexModels[mVertexModels.Length - 1].AddMesh(vertexModelMesh);

            Array.Resize(ref mVertexModelWorldTransforms, mVertexModelWorldTransforms.Length + 1);
            mVertexModelWorldTransforms[mVertexModelWorldTransforms.Length - 1] = Matrix.CreateTranslation(new Vector3(0, 0, 0));

            Array.Resize(ref mVertexModelAbsoluteBoneTransforms, mVertexModelAbsoluteBoneTransforms.Length + 1);

            Array.Resize(ref mSelectedVertices, mSelectedVertices.Length + 1);
            mSelectedVertices[mSelectedVertices.Length - 1] = new VertexPositionColor[3];

            Array.Resize(ref mVertexModelFilenames, mVertexModelFilenames.Length + 1);
            mVertexModelFilenames[mVertexModelFilenames.Length - 1] = "Cube";
        }

        public void CreateCylinder()
        {
            VertexModelMesh vertexModelMesh = new VertexModelMesh();

            Color color = mColorStandard;

            Array.Resize(ref mVertexModels, mVertexModels.Length + 1);
            mVertexModels[mVertexModels.Length - 1] = new VertexModel();

            CylinderPrimitive temp = new CylinderPrimitive(vertexModelMesh, 1f, 1f, 32, color);

            mVertexModels[mVertexModels.Length - 1].AddMesh(vertexModelMesh);

            Array.Resize(ref mVertexModelWorldTransforms, mVertexModelWorldTransforms.Length + 1);
            mVertexModelWorldTransforms[mVertexModelWorldTransforms.Length - 1] = Matrix.CreateTranslation(new Vector3(0, 0, 0));

            Array.Resize(ref mVertexModelAbsoluteBoneTransforms, mVertexModelAbsoluteBoneTransforms.Length + 1);

            Array.Resize(ref mSelectedVertices, mSelectedVertices.Length + 1);
            mSelectedVertices[mSelectedVertices.Length - 1] = new VertexPositionColor[3];

            Array.Resize(ref mVertexModelFilenames, mVertexModelFilenames.Length + 1);
            mVertexModelFilenames[mVertexModelFilenames.Length - 1] = "Cylinder";
        }
        
        public void CreateSphere()
        {
            VertexModelMesh vertexModelMesh = new VertexModelMesh();

            Color color = mColorStandard;

            Array.Resize(ref mVertexModels, mVertexModels.Length + 1);
            mVertexModels[mVertexModels.Length - 1] = new VertexModel();

            SpherePrimitive temp = new SpherePrimitive(vertexModelMesh, 1f, 16, color);

            mVertexModels[mVertexModels.Length - 1].AddMesh(vertexModelMesh);

            Array.Resize(ref mVertexModelWorldTransforms, mVertexModelWorldTransforms.Length + 1);
            mVertexModelWorldTransforms[mVertexModelWorldTransforms.Length - 1] = Matrix.CreateTranslation(new Vector3(0, 0, 0));

            Array.Resize(ref mVertexModelAbsoluteBoneTransforms, mVertexModelAbsoluteBoneTransforms.Length + 1);

            Array.Resize(ref mSelectedVertices, mSelectedVertices.Length + 1);
            mSelectedVertices[mSelectedVertices.Length - 1] = new VertexPositionColor[3];

            Array.Resize(ref mVertexModelFilenames, mVertexModelFilenames.Length + 1);
            mVertexModelFilenames[mVertexModelFilenames.Length - 1] = "Sphere";
        }

        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        void HandleInput()
        {
            currentKeyboardState    = Keyboard.GetState();
            currentMouseState       = Mouse.GetState();
            //currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            //if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
            //    currentGamePadState.Buttons.Back == ButtonState.Pushed)
            //{
            //    //Exit();
            //}
        }


        /// <summary>
        /// Handles input for moving the camera.
        /// </summary>
        void UpdateCamera()
        {
            //float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            float time = (float)timer.ElapsedMilliseconds;
            timer.Restart();
            
            // should we reset the camera?
            /*if (currentKeyboardState.IsKeyDown(Keys.R) ||
                currentGamePadState.Buttons.RightStick == ButtonState.Pushed)
            {
                cameraArc = CameraDefaultArc;
                cameraDistance = CameraDefaultDistance;
                cameraRotation = CameraDefaultRotation;
            }

            // Mouse.GetState().X

            // Check for input to rotate the camera up and down around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W)  || 
                (MouseButtons == System.Windows.Forms.MouseButtons.Right && mMousePositionLast.Y < MousePosition.Y))
            {
                cameraArc += time * CameraRotateSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S)    ||
                (MouseButtons == System.Windows.Forms.MouseButtons.Right && mMousePositionLast.Y > MousePosition.Y))
            {
                cameraArc -= time * CameraRotateSpeed;
            }

            cameraArc += currentGamePadState.ThumbSticks.Right.Y * time *
                CameraRotateSpeed;

            // Limit the arc movement.
            //cameraArc = MathHelper.Clamp(cameraArc, -90.0f, 90.0f);

            // Check for input to rotate the camera around the model.
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                cameraRotation += time * CameraRotateSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                cameraRotation -= time * CameraRotateSpeed;
            }*/

            if (MouseButtons == System.Windows.Forms.MouseButtons.Middle)
            {
                cameraRotation += MousePosition.X - mMousePositionLast.X;
                cameraArc      += MousePosition.Y - mMousePositionLast.Y;
            }
            
            /*cameraRotation += currentGamePadState.ThumbSticks.Right.X * time *
                CameraRotateSpeed;

            // Check for input to zoom camera in and out.
            if (currentKeyboardState.IsKeyDown(Keys.Z))
                cameraDistance += time * CameraZoomSpeed;

            if (currentKeyboardState.IsKeyDown(Keys.X))
                cameraDistance -= time * CameraZoomSpeed;

            cameraDistance += currentGamePadState.Triggers.Left * time
                * CameraZoomSpeed;
            cameraDistance -= currentGamePadState.Triggers.Right * time
                * CameraZoomSpeed;*/

            // Check for input to zoom camera in and out.

            if (currentMouseState.ScrollWheelValue < mPreviousScrollValue)
            {
                cameraDistance += time * CameraZoomSpeed * 10;
            }
            else if (currentMouseState.ScrollWheelValue > mPreviousScrollValue)
            {
                cameraDistance -= time * CameraZoomSpeed * 10;
            }

            mPreviousScrollValue = currentMouseState.ScrollWheelValue;

            // clamp the camera distance so it doesn't get too close or too far away.
            cameraDistance = MathHelper.Clamp(cameraDistance,
                CameraMinDistance, CameraMaxDistance);

            Matrix unrotatedView = Matrix.CreateLookAt(
                new Vector3(0, 0, -cameraDistance), Vector3.Zero, Vector3.Up);

            viewMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          unrotatedView;

            if (this.Size != mOldSize)
            {
                UpdateSize();
                mOldSize = this.Size;
            }
            
            mMousePositionLast = new Point(MousePosition.X, MousePosition.Y);
        }

        public void UpdateSelection()
        {
            UpdatePicking();

            if (pickedModelName != null)
            {
                int triangleFoundID = -1;

                for (int i = 0; i < mSelectedVertices[mPickedTriangleModelIndex].Length; i += 3)
                {
                    if (mSelectedVertices[mPickedTriangleModelIndex][i    ].Position == pickedTriangle[0].Position &&
                        mSelectedVertices[mPickedTriangleModelIndex][i + 1].Position == pickedTriangle[1].Position &&
                        mSelectedVertices[mPickedTriangleModelIndex][i + 2].Position == pickedTriangle[2].Position)
                    {
                        triangleFoundID = i;
                    }
                }

                if (triangleFoundID == -1)
                {
                    mSelectedTriangles++;

                    if (mSelectedTriangles > 1)
                    {
                        Array.Resize(ref mSelectedVertices[mPickedTriangleModelIndex], mSelectedVertices[mPickedTriangleModelIndex].Length + 3);
                    }

                    mSelectedVertices[mPickedTriangleModelIndex][mSelectedVertices[mPickedTriangleModelIndex].Length - 3] = new VertexPositionColor(pickedTriangle[0].Position, Color.Red);
                    mSelectedVertices[mPickedTriangleModelIndex][mSelectedVertices[mPickedTriangleModelIndex].Length - 2] = new VertexPositionColor(pickedTriangle[1].Position, Color.Red);
                    mSelectedVertices[mPickedTriangleModelIndex][mSelectedVertices[mPickedTriangleModelIndex].Length - 1] = new VertexPositionColor(pickedTriangle[2].Position, Color.Red);
                }
                else
                {
                    if (mSelectedVertices[mPickedTriangleModelIndex].Length > 3)
                    {
                        for (int i = triangleFoundID; i < mSelectedVertices[mPickedTriangleModelIndex].Length - 5; i += 3)
                        {
                            mSelectedVertices[mPickedTriangleModelIndex][i    ] = mSelectedVertices[mPickedTriangleModelIndex][i + 3];
                            mSelectedVertices[mPickedTriangleModelIndex][i + 1] = mSelectedVertices[mPickedTriangleModelIndex][i + 4];
                            mSelectedVertices[mPickedTriangleModelIndex][i + 2] = mSelectedVertices[mPickedTriangleModelIndex][i + 5];
                        }

                        Array.Resize(ref mSelectedVertices[mPickedTriangleModelIndex], mSelectedVertices[mPickedTriangleModelIndex].Length - 3);
                    }
                    else
                    {
                        Array.Clear(mSelectedVertices[mPickedTriangleModelIndex], 0, mSelectedVertices[mPickedTriangleModelIndex].Length);
                    }

                    mSelectedTriangles--;
                }
            }
        }

        public void ClearSelection()
        {
            mSelectedTriangles = 0;

            foreach (VertexPositionColor[] vertexArray in mSelectedVertices)
            {
                Array.Clear(vertexArray, 0, vertexArray.Length);
            }                        
        }
        
        private void UpdateSize()
        {
            // now that the GraphicsDevice has been created, we can create the projection matrix.
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), GraphicsDevice.Viewport.AspectRatio, .01f, 1000);
        }

        public void SetEditMode(MainForm.EditMode editMode)
        {
            mEditMode = editMode;
        }

        public void StoreToVertexModelBefore()
        {
            mVertexModelsBefore = new VertexModel[mVertexModels.Length];

            for (int i = 0; i < mVertexModels.Length; i++)
            {
                mVertexModelsBefore[i] = mVertexModels[i].Clone();
            }

            //  Clone mSelectedVertices to mSelectedVerticesBefore
            Array.Resize(ref mSelectedVerticesBefore, mSelectedVertices.Length);

            for (int i = 0; i < mSelectedVertices.Length; i++)
            {
                mSelectedVerticesBefore[i] = new VertexPositionColor[mSelectedVertices[i].Length];

                for (int j = 0; j < mSelectedVertices[i].Length; j++)
                {
                    mSelectedVerticesBefore[i][j] = new VertexPositionColor(mSelectedVertices[i][j].Position, mSelectedVertices[i][j].Color);
                }
            }
        }

        private void UpdateMovement(MainForm.EditMode editMode)
        {
            
            VertexPositionColor[][] selectedVerticesTemp = new VertexPositionColor[mSelectedVertices.Length][];
            mSelectedVertices.CopyTo(selectedVerticesTemp, 0);

            Vector3 addToPosition = Vector3.Zero;

            if (editMode == MainForm.EditMode.MOVEX)
            {
                addToPosition = Vector3.UnitX / 100 * ((this.PointToClient(new System.Drawing.Point(Mouse.GetState().X, Mouse.GetState().Y)).X - this.Size.Width / 2));
            }
            else if (editMode == MainForm.EditMode.MOVEY)
            {
                addToPosition = - Vector3.UnitY / 100 * (float)((this.PointToClient(new System.Drawing.Point(Mouse.GetState().X, Mouse.GetState().Y)).Y - this.Size.Height / 2));
            }
            else if (editMode == MainForm.EditMode.MOVEZ)
            {
                addToPosition = Vector3.UnitZ / 100 * (float)((this.PointToClient(new System.Drawing.Point(Mouse.GetState().X, Mouse.GetState().Y)).X - this.Size.Width / 2));
            }

            for (int i = 0; i < mVertexModels.Length; i++)
            {
                List<VertexModelMesh> vertexModelMesh = mVertexModels[i].GetMeshes();
                List<int> movedVerticesIndex = new List<int>();

                for (int j = 0; j < vertexModelMesh.Count; j++)
                {
                    Vector3[] vertices = vertexModelMesh[j].GetVertexVectorsFromIndice();

                    for (int k = 0; k < vertexModelMesh[j].GetVertexVectorsFromIndice().Length; k += 3)
                    {
                        for (int l = 0; l < mSelectedVertices[i].Length; l += 3)
                        {
                            if (vertices[k] == selectedVerticesTemp[i][l].Position &&
                                vertices[k + 1] == selectedVerticesTemp[i][l + 1].Position &&
                                vertices[k + 2] == selectedVerticesTemp[i][l + 2].Position)
                            {
                                if (!movedVerticesIndex.Contains(vertexModelMesh[j].GetVertexIndex(k)))
                                {
                                    vertexModelMesh[j].SetVertex(k, mVertexModelsBefore[i].GetMeshes()[j].GetVertexVectorsFromIndice()[k] + addToPosition);

                                    movedVerticesIndex.Add(vertexModelMesh[j].GetVertexIndex(k));
                                }
                                if (!movedVerticesIndex.Contains(vertexModelMesh[j].GetVertexIndex(k + 1)))
                                {
                                    vertexModelMesh[j].SetVertex(k + 1, mVertexModelsBefore[i].GetMeshes()[j].GetVertexVectorsFromIndice()[k + 1] + addToPosition);

                                    movedVerticesIndex.Add(vertexModelMesh[j].GetVertexIndex(k + 1));
                                }
                                if (!movedVerticesIndex.Contains(vertexModelMesh[j].GetVertexIndex(k + 2)))
                                {
                                    vertexModelMesh[j].SetVertex(k + 2, mVertexModelsBefore[i].GetMeshes()[j].GetVertexVectorsFromIndice()[k + 2] + addToPosition);

                                    movedVerticesIndex.Add(vertexModelMesh[j].GetVertexIndex(k + 2));
                                }

                                mSelectedVertices[i][l    ].Position = vertexModelMesh[j].GetVertexVectorsFromIndice()[k    ];
                                mSelectedVertices[i][l + 1].Position = vertexModelMesh[j].GetVertexVectorsFromIndice()[k + 1];
                                mSelectedVertices[i][l + 2].Position = vertexModelMesh[j].GetVertexVectorsFromIndice()[k + 2];
                            }
                        }
                    }
                }
            }

            //mEditMode = MainForm.EditMode.NORMAL;
        }

        public void ResetMovement()
        {
            mVertexModels = new VertexModel[mVertexModelsBefore.Length];

            for (int i = 0; i < mVertexModelsBefore.Length; i++)
            {
                mVertexModels[i] = mVertexModelsBefore[i].Clone();
            }

            //  Clone mSelectedVerticesBefore to mSelectedVertices

            Array.Resize(ref mSelectedVertices, mSelectedVerticesBefore.Length);

            for (int i = 0; i < mSelectedVerticesBefore.Length; i++)
            {
                mSelectedVertices[i] = new VertexPositionColor[mSelectedVerticesBefore[i].Length];

                for (int j = 0; j < mSelectedVerticesBefore[i].Length; j++)
                {
                    mSelectedVertices[i][j] = new VertexPositionColor(mSelectedVerticesBefore[i][j].Position, mSelectedVerticesBefore[i][j].Color);
                }
            }
        }

        #endregion
    }

    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    //static class Program
    //{
    //    static void Main()
    //    {
    //        using (TrianglePickingGame game = new TrianglePickingGame())
    //        {
    //            game.Run();
    //        }
    //    }
    //}

    #endregion
}
