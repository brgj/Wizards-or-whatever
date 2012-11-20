using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.DebugViews;

namespace WizardsOrWhatever
{

    /// <summary>
    /// Class to draw and maintain terrain.
    /// </summary>
    public class Terrain
    {
        /// <summary>
        /// The maximum number of vertices that should be used in a polygon.
        /// </summary>
        private const int MAX_VERTICES = 8;

        /// <summary>
        /// World to manage terrain in.
        /// </summary>
        public World World;

        /// <summary>
        /// Center of terrain in world units.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Width of terrain in world units.
        /// </summary>
        public float Width;

        /// <summary>
        /// Height of terrain in world units.
        /// </summary>
        public float Height;

        /// <summary>
        /// Points per each world unit used to define the terrain in the point cloud.
        /// </summary>
        public int PointsPerUnit;

        /// <summary>
        /// Points per cell.
        /// </summary>
        public int CellSize;

        /// <summary>
        /// Points per sub cell.
        /// </summary>
        public int SubCellSize;

        /// <summary>
        /// Point cloud defining the terrain.
        /// </summary>
        private sbyte[,] _terrainMap;

        /// <summary>
        /// Generated bodies.
        /// </summary>
        private List<Body>[,] _bodyMap;

        /// <summary>
        /// The Graphics Device accessor used to draw the terrain.
        /// </summary>
        private PrimitiveBatch _primitiveBatch;

        /// <summary>
        /// A temp vector array used to draw polygons.
        /// </summary>
        private Vector2[] tempVertices = new Vector2[MAX_VERTICES];

        /// <summary>
        /// The width of the point cloud.
        /// </summary>
        private float _localWidth;

        /// <summary>
        /// The height of the point cloud.
        /// </summary>
        private float _localHeight;

        /// <summary>
        /// The width in cells.
        /// </summary>
        private int _xnum;

        /// <summary>
        /// The height in cells.
        /// </summary>
        private int _ynum;

        /// <summary>
        /// The area to be redrawn.
        /// </summary>
        private AABB _dirtyArea;

        /// <summary>
        /// The top left of the terrain object.
        /// </summary>
        private Vector2 _topLeft;

        /// <summary>
        /// Terrain constructor
        /// </summary>
        /// <param name="world">The world to add the terrain to</param>
        /// <param name="area">The Axis-aligned Bounding box that encapsulates the terrain</param>
        public Terrain(World world, AABB area)
        {
            World = world;
            Width = area.Extents.X * 2;
            Height = area.Extents.Y * 2;
            Center = area.Center;
        }

        /// <summary>
        /// Creates a primitive batch used for drawing the polygons with a small amount of resource usage
        /// </summary>
        /// <param name="device"></param>
        public void LoadContent(GraphicsDevice device)
        {
            _primitiveBatch = new PrimitiveBatch(device, 1000);
        }

        /// <summary>
        /// Initialize the terrain for use.
        /// </summary>
        public void Initialize()
        {
            // find top left of terrain in world space
            _topLeft = new Vector2(Center.X - (Width * 0.5f), Center.Y - (Height * 0.5f));

            // convert the terrains size to a point cloud size
            _localWidth = Width * PointsPerUnit;
            _localHeight = Height * PointsPerUnit;

            _terrainMap = new sbyte[(int)_localWidth + 1, (int)_localHeight + 1];

            for (int x = 0; x < _localWidth; x++)
            {
                for (int y = 0; y < _localHeight; y++)
                {
                    _terrainMap[x, y] = 1;
                }
            }

            _xnum = (int)(_localWidth / CellSize);
            _ynum = (int)(_localHeight / CellSize);
            _bodyMap = new List<Body>[_xnum, _ynum];

            // make sure to mark the dirty area to an infinitely small box
            _dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
        }

        /// <summary>
        /// Randomizes the terrain using three sine waves with random seeds
        /// </summary>
        /// <returns></returns>
        public int[] RandomizeTerrain()
        {
            int[] terrainContour = new int[(int)_localWidth*10];
            Random randomizer = new Random();
            double rand1 = randomizer.NextDouble() + 1;
            double rand2 = randomizer.NextDouble() + 2;
            double rand3 = randomizer.NextDouble() + 3;

            float offset = Height*2;
            float peakHeight = Height;
            float flatness = 100;

            for (int x = 0; x < (int)_localWidth*10; x++)
            {
                double height = peakHeight / rand1 * Math.Sin((float)x / flatness * rand1 + rand1);
                height += peakHeight / rand2 * Math.Sin((float)x / flatness * rand2 + rand2);
                height += peakHeight / rand3 * Math.Sin((float)x / flatness * rand3 + rand3);
                height += offset;
                terrainContour[x] = (int)height;
            }
            return terrainContour;
        }

        /// <summary>
        /// Uses RandomizeTerrain to create a contour, set the terrainMap, and generate the terrain.
        /// </summary>
        /// <param name="texture"></param>
        public void CreateRandomTerrain(Vector2 position)
        {
            int[] terrainContour = RandomizeTerrain();
            int width = (int)_localWidth;
            int height = (int)_localHeight;

            for (int x = (int)position.X; x < width + (int)position.X; x++)
            {
                for (int y = (int)position.Y; y < height + (int)position.Y; y++)
                {
                    if (y > terrainContour[x-(int)position.X])
                    {
                        _terrainMap[x, y] = -1;
                    }
                    else
                    {
                        _terrainMap[x, y] = 1;
                    }
                }
            }
            // generate terrain
            for (int gy = 0; gy < _ynum; gy++)
            {
                for (int gx = 0; gx < _xnum; gx++)
                {
                    //remove old terrain object at grid cell
                    if (_bodyMap[gx, gy] != null)
                    {
                        for (int i = 0; i < _bodyMap[gx, gy].Count; i++)
                        {
                            World.RemoveBody(_bodyMap[gx, gy][i]);
                        }
                    }

                    _bodyMap[gx, gy] = null;

                    //generate new one
                    GenerateTerrain(gx, gy);
                }
            }
        }

        /// <summary>
        /// Modify a single point in the terrain.
        /// </summary>
        /// <param name="location">World location to modify. Automatically clipped.</param>z
        /// <param name="value">-1 = inside terrain, 1 = outside terrain</param>
        public void ModifyTerrain(Vector2 location, sbyte value)
        {
            // find local position
            // make position local to map space
            Vector2 p = location - _topLeft;

            // find map position for each axis
            p.X = p.X * _localWidth / Width;
            p.Y = p.Y * _localHeight / Height;

            if (p.X >= 0 && p.X < _localWidth && p.Y >= 0 && p.Y < _localHeight)
            {
                _terrainMap[(int)p.X, (int)p.Y] = value;

                // expand dirty area
                if (p.X < _dirtyArea.LowerBound.X) _dirtyArea.LowerBound.X = p.X;
                if (p.X > _dirtyArea.UpperBound.X) _dirtyArea.UpperBound.X = p.X;

                if (p.Y < _dirtyArea.LowerBound.Y) _dirtyArea.LowerBound.Y = p.Y;
                if (p.Y > _dirtyArea.UpperBound.Y) _dirtyArea.UpperBound.Y = p.Y;
            }
        }

        /// <summary>
        /// Regenerate the terrain.
        /// </summary>
        public void RegenerateTerrain()
        {
            //iterate effected cells
            var gx0 = (int)(_dirtyArea.LowerBound.X / CellSize);
            var gx1 = (int)(_dirtyArea.UpperBound.X / CellSize) + 1;
            if (gx0 < 0) gx0 = 0;
            if (gx1 > _xnum) gx1 = _xnum;
            var gy0 = (int)(_dirtyArea.LowerBound.Y / CellSize);
            var gy1 = (int)(_dirtyArea.UpperBound.Y / CellSize) + 1;
            if (gy0 < 0) gy0 = 0;
            if (gy1 > _ynum) gy1 = _ynum;

            for (int gx = gx0; gx < gx1; gx++)
            {
                for (int gy = gy0; gy < gy1; gy++)
                {
                    //remove old terrain object at grid cell
                    if (_bodyMap[gx, gy] != null)
                    {
                        for (int i = 0; i < _bodyMap[gx, gy].Count; i++)
                        {
                            World.RemoveBody(_bodyMap[gx, gy][i]);
                        }
                    }

                    _bodyMap[gx, gy] = null;

                    //generate new one
                    GenerateTerrain(gx, gy);
                }
            }

            _dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
        }

        /// <summary>
        /// Generates the terrain using the MarchingSquares algorithm. Stores polygons in body lists that are associated with points
        /// </summary>
        /// <param name="gx">the x coordinate of the body</param>
        /// <param name="gy">the y coordinate of the body</param>
        private void GenerateTerrain(int gx, int gy)
        {
            float ax = gx * CellSize;
            float ay = gy * CellSize;

            List<Vertices> polys = MarchingSquares.DetectSquares(new AABB(new Vector2(ax, ay), new Vector2(ax + CellSize, ay + CellSize)), SubCellSize, SubCellSize, _terrainMap, 2, true);
            if (polys.Count == 0) return;

            _bodyMap[gx, gy] = new List<Body>();

            // create the scale vector
            Vector2 scale = new Vector2(1f / PointsPerUnit, 1f / PointsPerUnit);

            // create physics object for this grid cell
            foreach (var item in polys)
            {
                item.Scale(ref scale);
                item.Translate(ref _topLeft);
                item.ForceCounterClockWise();
                Vertices p = FarseerPhysics.Common.PolygonManipulation.SimplifyTools.CollinearSimplify(item);
                List<Vertices> decompPolys = EarclipDecomposer.ConvexPartition(p);

                foreach (Vertices poly in decompPolys)
                {
                    if (poly.Count > 2)
                    {
                        _bodyMap[gx, gy].Add(BodyFactory.CreatePolygon(World, poly, 1));
                    }
                }
            }
        }

        /// <summary>
        /// Draws the terrain by cycling through all coordinates and displaying the polygons at each.
        /// </summary>
        public void DrawTerrain()
        {
            for (int gy = 0; gy < _ynum; gy++)
            {
                for (int gx = 0; gx < _xnum; gx++)
                {
                    if (_bodyMap[gx, gy] == null)
                        continue;
                    foreach (Body b in _bodyMap[gx, gy])
                    {
                        DrawBody(b);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a single body
        /// </summary>
        /// <param name="body">the body to draw</param>
        public void DrawBody(Body body)
        {
            Transform xf;
            body.GetTransform(out xf);
            foreach (Fixture f in body.FixtureList)
            {
                PolygonShape poly = (PolygonShape)f.Shape;
                int vertexCount = poly.Vertices.Count;

                if (vertexCount > MAX_VERTICES)
                    throw new Exception("Num vertices in polygon exceeds maximum number allowed");

                for (int i = 0; i < vertexCount; i++)
                    tempVertices[i] = MathUtils.Multiply(ref xf, poly.Vertices[i]);

                DrawPolygon(tempVertices, vertexCount, Color.SaddleBrown);
            }
        }

        /// <summary>
        /// Draws a linear or triangular polygon
        /// </summary>
        /// <param name="vertices">The vertices list</param>
        /// <param name="count">The number of vertices to draw</param>
        /// <param name="color">The color used to draw the vertices</param>
        public void DrawPolygon(Vector2[] vertices, int count, Color color)
        {
            if (!_primitiveBatch.IsReady())
            {
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");
            }
            if (count == 2)
            {
                _primitiveBatch.AddVertex(vertices[0], color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(vertices[1], color, PrimitiveType.LineList);
                return;
            }

            for (int i = 1; i < count - 1; i++)
            {
                _primitiveBatch.AddVertex(vertices[0], color, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(vertices[i], color, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(vertices[i + 1], color, PrimitiveType.TriangleList);
            }
        }

        /// <summary>
        /// Renders the Terrain.
        /// </summary>
        /// <param name="projection">The projection of the camera</param>
        /// <param name="view">The camera view</param>
        public void RenderTerrain(ref Matrix projection, ref Matrix view)
        {
            _primitiveBatch.Begin(ref projection, ref view);
            DrawTerrain();
            _primitiveBatch.End();
        }
    }
}
