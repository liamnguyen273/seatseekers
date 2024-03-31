using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Adapted from RainbowArt.CleanFlatUI.GradientModifier, original content is provided below

namespace com.brg.UnityComponents
{
    public class GradientModifier : BaseMeshEffect
    {
        public enum Style
        {
            Horizontal,
            Vertical,
            Radial,
            Diamond
        }

        public enum Blend
        {
            Override,
            Add,
            Multiply
        }

        [FormerlySerializedAs("gradientStyle")] [SerializeField] private Style _gradientStyle = Style.Horizontal;
        [FormerlySerializedAs("blend")] [SerializeField] private Blend _blend = Blend.Override;
        [FormerlySerializedAs("moreVertices")] [SerializeField] private bool _moreVertices = true;
        [FormerlySerializedAs("offset")] [SerializeField] [Range(-1, 1)] private float _offset = 0f;
        [FormerlySerializedAs("scale")] [SerializeField] [Range(0.1f, 10)] private float _scale = 1f;
        [FormerlySerializedAs("gradient")] [SerializeField] private Gradient _gradient = new()
        {
            colorKeys = new[]
            {
                new GradientColorKey(Color.black, 0), 
                new GradientColorKey(Color.white, 1)
            }
        };

        private readonly List<UIVertex> _vertexList = new List<UIVertex>();
        private readonly List<float> _gradientKeysPos = new List<float>();
        private readonly List<int> _originIndices = new List<int>(3);
        private readonly List<UIVertex> _starts = new List<UIVertex>(3);
        private readonly List<UIVertex> _ends = new List<UIVertex>(2);
        private readonly float[] _cachedVertexPositions = new float[3];

        public Style GradientStyle
        {
            get => _gradientStyle;
            set
            {
                if(_gradientStyle == value)
                {
                    return;
                }
                _gradientStyle = value;
                graphic.SetVerticesDirty();
            }
        }

        public Blend BlendMode
        {
            get => _blend;
            set
            {
                if(_blend == value)
                {
                    return;
                }
                _blend = value;
                graphic.SetVerticesDirty();
            }
        }

        public bool MoreVertices
        {            
            get => _moreVertices;
            set
            {
                if (_moreVertices == value)
                {
                    return;
                }
                _moreVertices = value;
                graphic.SetVerticesDirty();
            }
        }

        public float Offset
        {
            get => _offset;
            set
            {
                if (_offset == value)
                {
                    return;
                }
                _offset = Mathf.Clamp(value, -1f, 1f);
                graphic.SetVerticesDirty();
            }
        }

        public float Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                {
                    return;
                }
                _scale = Mathf.Clamp(value, 0.1f, 10f);
                graphic.SetVerticesDirty();
            }
        }

        public UnityEngine.Gradient Gradient
        {
            get => _gradient;
            set
            {
                _gradient = value;
                graphic.SetVerticesDirty();
            }
        }

        private Color BlendColor(Color colorA, Color colorB)
        {
            switch (BlendMode)
            {
                case Blend.Add: 
                {
                    return colorA + colorB;
                }                    
                case Blend.Multiply: 
                {
                    return colorA * colorB;
                }
                default: 
                    return colorB;
            }
        }

        public override void ModifyMesh(VertexHelper helper)
        {
            if (!IsActive() || helper.currentVertCount == 0)
            {
                return;
            }

            switch (GradientStyle)
            {
                case Style.Horizontal:
                {
                    ModifyMeshForHorizontal(helper);
                    break;
                }                
                case Style.Vertical:
                {
                    ModifyMeshForVertical(helper);
                    break;
                }  
                case Style.Diamond:
                {
                    ModifyMeshForDiamond(helper);
                    break;
                }                
                case Style.Radial:
                {
                    ModifyMeshForRadial(helper);
                    break;
                }                
            }
        }

        private void ModifyMeshForHorizontal(VertexHelper helper)
        {
            _vertexList.Clear();
            helper.GetUIVertexStream(_vertexList);
            var bounds = GetVertsBounds(_vertexList);
            var min = bounds.xMin;
            var w = bounds.width;
            var width = w == 0f ? 0f : 1f / w / Scale;
            var zoomOffset = (1 - (1 / Scale)) * 0.5f;
            var offset = (Offset * (1 - zoomOffset)) - zoomOffset;

            if (MoreVertices)
            {
                SplitTrianglesAtGradientKeys(_vertexList, bounds, zoomOffset, helper);
            }

            UIVertex vertex = new UIVertex();
            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                vertex.color = BlendColor(vertex.color, Gradient.Evaluate((vertex.position.x - min) * width - offset));
                helper.SetUIVertex(vertex, i);
            }
        }

        private void ModifyMeshForVertical(VertexHelper helper)
        {
            _vertexList.Clear();
            helper.GetUIVertexStream(_vertexList);
            Rect bounds = GetVertsBounds(_vertexList);
            float min = bounds.yMin;
            float h = bounds.height;

            float height = h == 0f ? 0f : 1f / h / Scale;
            float zoomOffset = (1 - (1 / Scale)) * 0.5f;
            float offset = (Offset * (1 - zoomOffset)) - zoomOffset;

            if (MoreVertices)
            {
                SplitTrianglesAtGradientKeys(_vertexList, bounds, zoomOffset, helper);
            }

            UIVertex vertex = new UIVertex();
            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                vertex.color = BlendColor(vertex.color, Gradient.Evaluate((vertex.position.y - min) * height - offset));
                helper.SetUIVertex(vertex, i);
            }
        }

        private void ModifyMeshForDiamond(VertexHelper helper)
        {
            _vertexList.Clear();
            helper.GetUIVertexStream(_vertexList);
            int nCount = _vertexList.Count;
            Rect bounds = GetVertsBounds(_vertexList);

            float height = bounds.height == 0f ? 0f : 1f / bounds.height / Scale;
            float radius = bounds.center.y / 2f;
            Vector3 center = (Vector3.right + Vector3.up) * radius + Vector3.forward * _vertexList[0].position.z;

            if (MoreVertices)
            {
                helper.Clear();
                for (int i = 0; i < nCount; i++)
                {
                    helper.AddVert(_vertexList[i]);
                }
                UIVertex centralVertex = new UIVertex();
                centralVertex.position = center;
                centralVertex.normal = _vertexList[0].normal;
                centralVertex.uv0 = new Vector2(0.5f, 0.5f);
                centralVertex.color = Color.white;
                helper.AddVert(centralVertex);
                for (int i = 1; i < nCount; i++)
                {
                    helper.AddTriangle(i - 1, i, nCount);
                }
                helper.AddTriangle(0, nCount - 1, nCount);
            }

            UIVertex vertex = new UIVertex();

            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                vertex.color = BlendColor(vertex.color, Gradient.Evaluate(
                    Vector3.Distance(vertex.position, center) * height - Offset));
                helper.SetUIVertex(vertex, i);
            }
        }

        private void ModifyMeshForRadial(VertexHelper helper)
        {
            _vertexList.Clear();
            helper.GetUIVertexStream(_vertexList);
            Rect bounds = GetVertsBounds(_vertexList);

            float width = bounds.width == 0f ? 0f : 1f / bounds.width / Scale;
            float height = bounds.height == 0f ? 0f : 1f / bounds.height / Scale;

            if (MoreVertices)
            {
                helper.Clear();

                float radiusX = bounds.width / 2f;
                float radiusY = bounds.height / 2f;
                UIVertex centralVertex = new UIVertex();
                centralVertex.position = Vector3.right * bounds.center.x + Vector3.up * bounds.center.y + Vector3.forward * _vertexList[0].position.z;
                centralVertex.normal = _vertexList[0].normal;
                centralVertex.uv0 = new Vector2(0.5f, 0.5f);
                centralVertex.color = Color.white;

                int steps = 64;
                for (int i = 0; i < steps; i++)
                {
                    UIVertex curVertex = new UIVertex();
                    float angle = (float)i * 360f / (float)steps;
                    float cosX = Mathf.Cos(Mathf.Deg2Rad * angle);
                    float cosY = Mathf.Sin(Mathf.Deg2Rad * angle);

                    curVertex.position = Vector3.right * cosX * radiusX + Vector3.up * cosY * radiusY + Vector3.forward * _vertexList[0].position.z;
                    curVertex.normal = _vertexList[0].normal;
                    curVertex.uv0 = new Vector2((cosX + 1) * 0.5f, (cosY + 1) * 0.5f);
                    curVertex.color = Color.white;
                    helper.AddVert(curVertex);
                }

                helper.AddVert(centralVertex);

                for (int i = 1; i < steps; i++)
                {
                    helper.AddTriangle(i - 1, i, steps);
                }
                helper.AddTriangle(0, steps - 1, steps);
            }

            UIVertex vertex = new UIVertex();

            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);

                vertex.color = BlendColor(vertex.color, Gradient.Evaluate(
                    Mathf.Sqrt(
                        Mathf.Pow(Mathf.Abs(vertex.position.x - bounds.center.x) * width, 2f) +
                        Mathf.Pow(Mathf.Abs(vertex.position.y - bounds.center.y) * height, 2f)) * 2f - Offset));

                helper.SetUIVertex(vertex, i);
            }
        }

        private Rect GetVertsBounds(List<UIVertex> vertices)
        {
            float left = vertices[0].position.x;
            float right = left;
            float bottom = vertices[0].position.y;
            float top = bottom;

            for (int i = vertices.Count - 1; i >= 1; --i)
            {
                float x = vertices[i].position.x;
                float y = vertices[i].position.y;

                if (x > right)
                {
                    right = x;
                }
                else if (x < left)
                {
                    left = x;
                }

                if (y > top)
                {
                    top = y;
                }
                else if (y < bottom)
                {
                    bottom = y;
                }
            }

            return new Rect(left, bottom, right - left, top - bottom);
        }

        private void SplitOneTriangle(List<UIVertex> vertexList, VertexHelper helper,int triangleIndex)
        {
            int i = triangleIndex * 3;
            float[] positions = GetVertexPositions(vertexList, i);
            _originIndices.Clear();
            _starts.Clear();
            _ends.Clear();

            for (int s = 0; s < _gradientKeysPos.Count; s++)
            {
                int initialCount = helper.currentVertCount;
                bool hadEnds = _ends.Count > 0;
                bool earlyStart = false;
                for (int p = 0; p < 3; p++)
                {
                    if (!_originIndices.Contains(p) && positions[p] < _gradientKeysPos[s])
                    {
                        int p1 = (p + 1) % 3;
                        var start = vertexList[p + i];
                        if (positions[p1] > _gradientKeysPos[s])
                        {
                            _originIndices.Insert(0, p);
                            _starts.Insert(0, start);
                            earlyStart = true;
                        }
                        else
                        {
                            _originIndices.Add(p);
                            _starts.Add(start);
                        }
                    }
                }

                if (_originIndices.Count == 0)
                {
                    continue;
                }
                if (_originIndices.Count == 3)
                {
                    break;
                }
                foreach (var start in _starts)
                {
                    helper.AddVert(start);
                }
                _ends.Clear();
                foreach (int index in _originIndices)
                {
                    int oppositeIndex = (index + 1) % 3;
                    if (positions[oppositeIndex] < _gradientKeysPos[s])
                    {
                        oppositeIndex = (oppositeIndex + 1) % 3;
                    }
                    _ends.Add(CreateSplitVertex(vertexList[index + i], vertexList[oppositeIndex + i], _gradientKeysPos[s]));
                }
                if (_ends.Count == 1)
                {
                    int oppositeIndex = (_originIndices[0] + 2) % 3;
                    _ends.Add(CreateSplitVertex(vertexList[_originIndices[0] + i], vertexList[oppositeIndex + i], _gradientKeysPos[s]));
                }

                foreach (var end in _ends)
                {
                    helper.AddVert(end);
                }

                if (hadEnds)
                {
                    helper.AddTriangle(initialCount - 2, initialCount, initialCount + 1);
                    helper.AddTriangle(initialCount - 2, initialCount + 1, initialCount - 1);
                    if (_starts.Count > 0)
                    {
                        if (earlyStart)
                        {
                            helper.AddTriangle(initialCount - 2, initialCount + 3, initialCount);
                        }
                        else
                        {
                            helper.AddTriangle(initialCount + 1, initialCount + 3, initialCount - 1);
                        }
                    }
                }
                else
                {
                    int vertexCount = helper.currentVertCount;
                    helper.AddTriangle(initialCount, vertexCount - 2, vertexCount - 1);
                    if (_starts.Count > 1)
                    {
                        helper.AddTriangle(initialCount, vertexCount - 1, initialCount + 1);
                    }
                }

                _starts.Clear();
            }

            if (_ends.Count > 0)
            {
                if (_starts.Count == 0)
                {
                    for (int p = 0; p < 3; p++)
                    {
                        if (!_originIndices.Contains(p) && positions[p] > _gradientKeysPos[_gradientKeysPos.Count - 1])
                        {
                            int p1 = (p + 1) % 3;
                            UIVertex end = vertexList[p + i];
                            if (positions[p1] > _gradientKeysPos[_gradientKeysPos.Count - 1])
                            {
                                _starts.Insert(0, end);
                            }
                            else
                            {
                                _starts.Add(end);
                            }
                        }
                    }
                }

                foreach (var start in _starts)
                {
                    helper.AddVert(start);
                }

                int vertexCount = helper.currentVertCount;
                if (_starts.Count > 1)
                {
                    helper.AddTriangle(vertexCount - 4, vertexCount - 2, vertexCount - 1);
                    helper.AddTriangle(vertexCount - 4, vertexCount - 1, vertexCount - 3);
                }
                else if (_starts.Count > 0)
                {
                    helper.AddTriangle(vertexCount - 3, vertexCount - 1, vertexCount - 2);
                }
            }
            else
            {
                helper.AddVert(vertexList[i]);
                helper.AddVert(vertexList[i + 1]);
                helper.AddVert(vertexList[i + 2]);
                int vertexCount = helper.currentVertCount;
                helper.AddTriangle(vertexCount - 3, vertexCount - 2, vertexCount - 1);
            }
        }

        private void SplitTrianglesAtGradientKeys(List<UIVertex> vertexList, Rect bounds, float zoomOffset, VertexHelper helper)
        {
            FindGradientKeysPos(zoomOffset, bounds);
            if (_gradientKeysPos.Count == 0)
            {
                return;
            }
            helper.Clear();
            int count = vertexList.Count/3;
            for (int i = 0; i < count; ++i)
            {
                SplitOneTriangle(vertexList, helper, i);
            }
        }

        private float[] GetVertexPositions(List<UIVertex> vertexList, int index)
        {
            if (GradientStyle == Style.Horizontal)
            {
                _cachedVertexPositions[0] = vertexList[index].position.x;
                _cachedVertexPositions[1] = vertexList[index + 1].position.x;
                _cachedVertexPositions[2] = vertexList[index + 2].position.x;
            }
            else
            {
                _cachedVertexPositions[0] = vertexList[index].position.y;
                _cachedVertexPositions[1] = vertexList[index + 1].position.y;
                _cachedVertexPositions[2] = vertexList[index + 2].position.y;
            }
            return _cachedVertexPositions;
        }

        private void FindGradientKeysPos(float zoomOffset, Rect bounds)
        {
            _gradientKeysPos.Clear();
            var offset = Offset * (1 - zoomOffset);
            var startBoundary = zoomOffset - offset;
            var endBoundary = (1 - zoomOffset) - offset;

            foreach (var color in Gradient.colorKeys)
            {
                if (color.time >= endBoundary)
                {
                    break;
                }
                if (color.time > startBoundary)
                {
                    _gradientKeysPos.Add((color.time - startBoundary) * Scale);
                }
            }
            foreach (var alpha in Gradient.alphaKeys)
            {
                if (alpha.time >= endBoundary)
                {
                    break;
                }
                if (alpha.time > startBoundary)
                {
                    _gradientKeysPos.Add((alpha.time - startBoundary) * Scale);
                }
            }

            var min = bounds.xMin;
            var size = bounds.width;
            if (GradientStyle == Style.Vertical)
            {
                min = bounds.yMin;
                size = bounds.height;
            }

            _gradientKeysPos.Sort();
            for (var i = 0; i < _gradientKeysPos.Count; i++)
            {
                _gradientKeysPos[i] = (_gradientKeysPos[i] * size) + min;

                if (i <= 0 || !(Math.Abs(_gradientKeysPos[i] - _gradientKeysPos[i - 1]) < 2)) continue;
                
                _gradientKeysPos.RemoveAt(i);
                --i;
            }
        }

        private UIVertex CreateSplitVertex(UIVertex vertex1, UIVertex vertex2, float stop)
        {
            if (GradientStyle == Style.Horizontal)
            {
                var sx = vertex1.position.x - stop;
                var dx = vertex1.position.x - vertex2.position.x;
                var dy = vertex1.position.y - vertex2.position.y;
                var uvx = vertex1.uv0.x - vertex2.uv0.x;
                var uvy = vertex1.uv0.y - vertex2.uv0.y;
                var ratio = sx / dx;
                var splitY = vertex1.position.y - (dy * ratio);

                var splitVertex = new UIVertex();
                splitVertex.position = new Vector3(stop, splitY, vertex1.position.z);
                splitVertex.normal = vertex1.normal;
                splitVertex.uv0 = new Vector2(vertex1.uv0.x - (uvx * ratio), vertex1.uv0.y - (uvy * ratio));
                splitVertex.color = Color.white;
                return splitVertex;
            }
            else
            {
                var sy = vertex1.position.y - stop;
                var dy = vertex1.position.y - vertex2.position.y;
                var dx = vertex1.position.x - vertex2.position.x;
                var uvx = vertex1.uv0.x - vertex2.uv0.x;
                var uvy = vertex1.uv0.y - vertex2.uv0.y;
                var ratio = sy / dy;
                var splitX = vertex1.position.x - (dx * ratio);

                var splitVertex = new UIVertex();
                splitVertex.position = new Vector3(splitX, stop, vertex1.position.z);
                splitVertex.normal = vertex1.normal;
                splitVertex.uv0 = new Vector2(vertex1.uv0.x - (uvx * ratio), vertex1.uv0.y - (uvy * ratio));
                splitVertex.color = Color.white;
                return splitVertex;
            }
        } 
    }
}

// namespace RainbowArt.CleanFlatUI
// {
//     public class GradientModifier : BaseMeshEffect
//     {
//         public enum Style
//         {
//             Horizontal,
//             Vertical,
//             Radial,
//             Diamond
//         }
//
//         public enum Blend
//         {
//             Override,
//             Add,
//             Multiply
//         }
//
//         [SerializeField]
//         Style gradientStyle = Style.Horizontal;
//
//         [SerializeField]
//         Blend blend = Blend.Override;
//
//         [SerializeField]
//         bool moreVertices = true;
//
//         [SerializeField]
//         [Range(-1, 1)]
//         float offset = 0f;
//
//         [SerializeField]
//         [Range(0.1f, 10)]
//         float scale = 1f;
//
//         [SerializeField]
//         UnityEngine.Gradient gradient = new UnityEngine.Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) } };
//
//         List<UIVertex> vertexList = new List<UIVertex>();
//         List<float> gradientKeysPos = new List<float>();
//         List<int> originIndices = new List<int>(3);
//         List<UIVertex> starts = new List<UIVertex>(3);
//         List<UIVertex> ends = new List<UIVertex>(2);
//         float[] cachedVertexPositions = new float[3];
//
//         public Style GradientStyle
//         {
//             get => gradientStyle;
//             set
//             {
//                 if(gradientStyle == value)
//                 {
//                     return;
//                 }
//                 gradientStyle = value;
//                 graphic.SetVerticesDirty();
//             }
//         }
//
//         public Blend BlendMode
//         {
//             get => blend;
//             set
//             {
//                 if(blend == value)
//                 {
//                     return;
//                 }
//                 blend = value;
//                 graphic.SetVerticesDirty();
//             }
//         }
//
//         public bool MoreVertices
//         {            
//             get => moreVertices;
//             set
//             {
//                 if (moreVertices == value)
//                 {
//                     return;
//                 }
//                 moreVertices = value;
//                 graphic.SetVerticesDirty();
//             }
//         }
//
//         public float Offset
//         {
//             get => offset;
//             set
//             {
//                 if (offset == value)
//                 {
//                     return;
//                 }
//                 offset = Mathf.Clamp(value, -1f, 1f);
//                 graphic.SetVerticesDirty();
//             }
//         }
//
//         public float Scale
//         {
//             get => scale;
//             set
//             {
//                 if (scale == value)
//                 {
//                     return;
//                 }
//                 scale = Mathf.Clamp(value, 0.1f, 10f);
//                 graphic.SetVerticesDirty();
//             }
//         }
//
//         public UnityEngine.Gradient Gradient
//         {
//             get => gradient;
//             set
//             {
//                 gradient = value;
//                 graphic.SetVerticesDirty();
//             }
//         }
//
//         Color BlendColor(Color colorA, Color colorB)
//         {
//             switch (BlendMode)
//             {
//                 case Blend.Add: 
//                 {
//                     return colorA + colorB;
//                 }                    
//                 case Blend.Multiply: 
//                 {
//                     return colorA * colorB;
//                 }
//                 default: 
//                     return colorB;
//             }
//         }
//
//         public override void ModifyMesh(VertexHelper helper)
//         {
//             if (!IsActive() || helper.currentVertCount == 0)
//             {
//                 return;
//             }
//
//             switch (GradientStyle)
//             {
//                 case Style.Horizontal:
//                 {
//                     ModifyMeshForHorizontal(helper);
//                     break;
//                 }                
//                 case Style.Vertical:
//                 {
//                     ModifyMeshForVertical(helper);
//                     break;
//                 }  
//                 case Style.Diamond:
//                 {
//                     ModifyMeshForDiamond(helper);
//                     break;
//                 }                
//                 case Style.Radial:
//                 {
//                     ModifyMeshForRadial(helper);
//                     break;
//                 }                
//             }
//         }
//
//         void ModifyMeshForHorizontal(VertexHelper helper)
//         {
//             vertexList.Clear();
//             helper.GetUIVertexStream(vertexList);
//             Rect bounds = GetVertsBounds(vertexList);
//             float min = bounds.xMin;
//             float w = bounds.width;
//             float width = w == 0f ? 0f : 1f / w / Scale;
//             float zoomOffset = (1 - (1 / Scale)) * 0.5f;
//             float offset = (Offset * (1 - zoomOffset)) - zoomOffset;
//
//             if (MoreVertices)
//             {
//                 SplitTrianglesAtGradientKeys(vertexList, bounds, zoomOffset, helper);
//             }
//
//             UIVertex vertex = new UIVertex();
//             for (int i = 0; i < helper.currentVertCount; i++)
//             {
//                 helper.PopulateUIVertex(ref vertex, i);
//                 vertex.color = BlendColor(vertex.color, Gradient.Evaluate((vertex.position.x - min) * width - offset));
//                 helper.SetUIVertex(vertex, i);
//             }
//         }
//
//         void ModifyMeshForVertical(VertexHelper helper)
//         {
//             vertexList.Clear();
//             helper.GetUIVertexStream(vertexList);
//             Rect bounds = GetVertsBounds(vertexList);
//             float min = bounds.yMin;
//             float h = bounds.height;
//
//             float height = h == 0f ? 0f : 1f / h / Scale;
//             float zoomOffset = (1 - (1 / Scale)) * 0.5f;
//             float offset = (Offset * (1 - zoomOffset)) - zoomOffset;
//
//             if (MoreVertices)
//             {
//                 SplitTrianglesAtGradientKeys(vertexList, bounds, zoomOffset, helper);
//             }
//
//             UIVertex vertex = new UIVertex();
//             for (int i = 0; i < helper.currentVertCount; i++)
//             {
//                 helper.PopulateUIVertex(ref vertex, i);
//                 vertex.color = BlendColor(vertex.color, Gradient.Evaluate((vertex.position.y - min) * height - offset));
//                 helper.SetUIVertex(vertex, i);
//             }
//         }
//
//         void ModifyMeshForDiamond(VertexHelper helper)
//         {
//             vertexList.Clear();
//             helper.GetUIVertexStream(vertexList);
//             int nCount = vertexList.Count;
//             Rect bounds = GetVertsBounds(vertexList);
//
//             float height = bounds.height == 0f ? 0f : 1f / bounds.height / Scale;
//             float radius = bounds.center.y / 2f;
//             Vector3 center = (Vector3.right + Vector3.up) * radius + Vector3.forward * vertexList[0].position.z;
//
//             if (MoreVertices)
//             {
//                 helper.Clear();
//                 for (int i = 0; i < nCount; i++)
//                 {
//                     helper.AddVert(vertexList[i]);
//                 }
//                 UIVertex centralVertex = new UIVertex();
//                 centralVertex.position = center;
//                 centralVertex.normal = vertexList[0].normal;
//                 centralVertex.uv0 = new Vector2(0.5f, 0.5f);
//                 centralVertex.color = Color.white;
//                 helper.AddVert(centralVertex);
//                 for (int i = 1; i < nCount; i++)
//                 {
//                     helper.AddTriangle(i - 1, i, nCount);
//                 }
//                 helper.AddTriangle(0, nCount - 1, nCount);
//             }
//
//             UIVertex vertex = new UIVertex();
//
//             for (int i = 0; i < helper.currentVertCount; i++)
//             {
//                 helper.PopulateUIVertex(ref vertex, i);
//                 vertex.color = BlendColor(vertex.color, Gradient.Evaluate(
//                     Vector3.Distance(vertex.position, center) * height - Offset));
//                 helper.SetUIVertex(vertex, i);
//             }
//         }
//
//         void ModifyMeshForRadial(VertexHelper helper)
//         {
//             vertexList.Clear();
//             helper.GetUIVertexStream(vertexList);
//             Rect bounds = GetVertsBounds(vertexList);
//
//             float width = bounds.width == 0f ? 0f : 1f / bounds.width / Scale;
//             float height = bounds.height == 0f ? 0f : 1f / bounds.height / Scale;
//
//             if (MoreVertices)
//             {
//                 helper.Clear();
//
//                 float radiusX = bounds.width / 2f;
//                 float radiusY = bounds.height / 2f;
//                 UIVertex centralVertex = new UIVertex();
//                 centralVertex.position = Vector3.right * bounds.center.x + Vector3.up * bounds.center.y + Vector3.forward * vertexList[0].position.z;
//                 centralVertex.normal = vertexList[0].normal;
//                 centralVertex.uv0 = new Vector2(0.5f, 0.5f);
//                 centralVertex.color = Color.white;
//
//                 int steps = 64;
//                 for (int i = 0; i < steps; i++)
//                 {
//                     UIVertex curVertex = new UIVertex();
//                     float angle = (float)i * 360f / (float)steps;
//                     float cosX = Mathf.Cos(Mathf.Deg2Rad * angle);
//                     float cosY = Mathf.Sin(Mathf.Deg2Rad * angle);
//
//                     curVertex.position = Vector3.right * cosX * radiusX + Vector3.up * cosY * radiusY + Vector3.forward * vertexList[0].position.z;
//                     curVertex.normal = vertexList[0].normal;
//                     curVertex.uv0 = new Vector2((cosX + 1) * 0.5f, (cosY + 1) * 0.5f);
//                     curVertex.color = Color.white;
//                     helper.AddVert(curVertex);
//                 }
//
//                 helper.AddVert(centralVertex);
//
//                 for (int i = 1; i < steps; i++)
//                 {
//                     helper.AddTriangle(i - 1, i, steps);
//                 }
//                 helper.AddTriangle(0, steps - 1, steps);
//             }
//
//             UIVertex vertex = new UIVertex();
//
//             for (int i = 0; i < helper.currentVertCount; i++)
//             {
//                 helper.PopulateUIVertex(ref vertex, i);
//
//                 vertex.color = BlendColor(vertex.color, Gradient.Evaluate(
//                     Mathf.Sqrt(
//                         Mathf.Pow(Mathf.Abs(vertex.position.x - bounds.center.x) * width, 2f) +
//                         Mathf.Pow(Mathf.Abs(vertex.position.y - bounds.center.y) * height, 2f)) * 2f - Offset));
//
//                 helper.SetUIVertex(vertex, i);
//             }
//         }
//
//         Rect GetVertsBounds(List<UIVertex> vertices)
//         {
//             float left = vertices[0].position.x;
//             float right = left;
//             float bottom = vertices[0].position.y;
//             float top = bottom;
//
//             for (int i = vertices.Count - 1; i >= 1; --i)
//             {
//                 float x = vertices[i].position.x;
//                 float y = vertices[i].position.y;
//
//                 if (x > right)
//                 {
//                     right = x;
//                 }
//                 else if (x < left)
//                 {
//                     left = x;
//                 }
//
//                 if (y > top)
//                 {
//                     top = y;
//                 }
//                 else if (y < bottom)
//                 {
//                     bottom = y;
//                 }
//             }
//
//             return new Rect(left, bottom, right - left, top - bottom);
//         }
//
//         void SplitOneTriangle(List<UIVertex> vertexList, VertexHelper helper,int triangleIndex)
//         {
//             int i = triangleIndex * 3;
//             float[] positions = GetVertexPositions(vertexList, i);
//             originIndices.Clear();
//             starts.Clear();
//             ends.Clear();
//
//             for (int s = 0; s < gradientKeysPos.Count; s++)
//             {
//                 int initialCount = helper.currentVertCount;
//                 bool hadEnds = ends.Count > 0;
//                 bool earlyStart = false;
//                 for (int p = 0; p < 3; p++)
//                 {
//                     if (!originIndices.Contains(p) && positions[p] < gradientKeysPos[s])
//                     {
//                         int p1 = (p + 1) % 3;
//                         var start = vertexList[p + i];
//                         if (positions[p1] > gradientKeysPos[s])
//                         {
//                             originIndices.Insert(0, p);
//                             starts.Insert(0, start);
//                             earlyStart = true;
//                         }
//                         else
//                         {
//                             originIndices.Add(p);
//                             starts.Add(start);
//                         }
//                     }
//                 }
//
//                 if (originIndices.Count == 0)
//                 {
//                     continue;
//                 }
//                 if (originIndices.Count == 3)
//                 {
//                     break;
//                 }
//                 foreach (var start in starts)
//                 {
//                     helper.AddVert(start);
//                 }
//                 ends.Clear();
//                 foreach (int index in originIndices)
//                 {
//                     int oppositeIndex = (index + 1) % 3;
//                     if (positions[oppositeIndex] < gradientKeysPos[s])
//                     {
//                         oppositeIndex = (oppositeIndex + 1) % 3;
//                     }
//                     ends.Add(CreateSplitVertex(vertexList[index + i], vertexList[oppositeIndex + i], gradientKeysPos[s]));
//                 }
//                 if (ends.Count == 1)
//                 {
//                     int oppositeIndex = (originIndices[0] + 2) % 3;
//                     ends.Add(CreateSplitVertex(vertexList[originIndices[0] + i], vertexList[oppositeIndex + i], gradientKeysPos[s]));
//                 }
//
//                 foreach (var end in ends)
//                 {
//                     helper.AddVert(end);
//                 }
//
//                 if (hadEnds)
//                 {
//                     helper.AddTriangle(initialCount - 2, initialCount, initialCount + 1);
//                     helper.AddTriangle(initialCount - 2, initialCount + 1, initialCount - 1);
//                     if (starts.Count > 0)
//                     {
//                         if (earlyStart)
//                         {
//                             helper.AddTriangle(initialCount - 2, initialCount + 3, initialCount);
//                         }
//                         else
//                         {
//                             helper.AddTriangle(initialCount + 1, initialCount + 3, initialCount - 1);
//                         }
//                     }
//                 }
//                 else
//                 {
//                     int vertexCount = helper.currentVertCount;
//                     helper.AddTriangle(initialCount, vertexCount - 2, vertexCount - 1);
//                     if (starts.Count > 1)
//                     {
//                         helper.AddTriangle(initialCount, vertexCount - 1, initialCount + 1);
//                     }
//                 }
//
//                 starts.Clear();
//             }
//
//             if (ends.Count > 0)
//             {
//                 if (starts.Count == 0)
//                 {
//                     for (int p = 0; p < 3; p++)
//                     {
//                         if (!originIndices.Contains(p) && positions[p] > gradientKeysPos[gradientKeysPos.Count - 1])
//                         {
//                             int p1 = (p + 1) % 3;
//                             UIVertex end = vertexList[p + i];
//                             if (positions[p1] > gradientKeysPos[gradientKeysPos.Count - 1])
//                             {
//                                 starts.Insert(0, end);
//                             }
//                             else
//                             {
//                                 starts.Add(end);
//                             }
//                         }
//                     }
//                 }
//
//                 foreach (var start in starts)
//                 {
//                     helper.AddVert(start);
//                 }
//
//                 int vertexCount = helper.currentVertCount;
//                 if (starts.Count > 1)
//                 {
//                     helper.AddTriangle(vertexCount - 4, vertexCount - 2, vertexCount - 1);
//                     helper.AddTriangle(vertexCount - 4, vertexCount - 1, vertexCount - 3);
//                 }
//                 else if (starts.Count > 0)
//                 {
//                     helper.AddTriangle(vertexCount - 3, vertexCount - 1, vertexCount - 2);
//                 }
//             }
//             else
//             {
//                 helper.AddVert(vertexList[i]);
//                 helper.AddVert(vertexList[i + 1]);
//                 helper.AddVert(vertexList[i + 2]);
//                 int vertexCount = helper.currentVertCount;
//                 helper.AddTriangle(vertexCount - 3, vertexCount - 2, vertexCount - 1);
//             }
//         }
//
//         void SplitTrianglesAtGradientKeys(List<UIVertex> vertexList, Rect bounds, float zoomOffset, VertexHelper helper)
//         {
//             FindGradientKeysPos(zoomOffset, bounds);
//             if (gradientKeysPos.Count == 0)
//             {
//                 return;
//             }
//             helper.Clear();
//             int count = vertexList.Count/3;
//             for (int i = 0; i < count; ++i)
//             {
//                 SplitOneTriangle(vertexList, helper, i);
//             }
//         }
//
//         float[] GetVertexPositions(List<UIVertex> vertexList, int index)
//         {
//             if (GradientStyle == Style.Horizontal)
//             {
//                 cachedVertexPositions[0] = vertexList[index].position.x;
//                 cachedVertexPositions[1] = vertexList[index + 1].position.x;
//                 cachedVertexPositions[2] = vertexList[index + 2].position.x;
//             }
//             else
//             {
//                 cachedVertexPositions[0] = vertexList[index].position.y;
//                 cachedVertexPositions[1] = vertexList[index + 1].position.y;
//                 cachedVertexPositions[2] = vertexList[index + 2].position.y;
//             }
//             return cachedVertexPositions;
//         }
//
//         void FindGradientKeysPos(float zoomOffset, Rect bounds)
//         {
//             gradientKeysPos.Clear();
//             var offset = Offset * (1 - zoomOffset);
//             var startBoundary = zoomOffset - offset;
//             var endBoundary = (1 - zoomOffset) - offset;
//
//             foreach (var color in Gradient.colorKeys)
//             {
//                 if (color.time >= endBoundary)
//                 {
//                     break;
//                 }
//                 if (color.time > startBoundary)
//                 {
//                     gradientKeysPos.Add((color.time - startBoundary) * Scale);
//                 }
//             }
//             foreach (var alpha in Gradient.alphaKeys)
//             {
//                 if (alpha.time >= endBoundary)
//                 {
//                     break;
//                 }
//                 if (alpha.time > startBoundary)
//                 {
//                     gradientKeysPos.Add((alpha.time - startBoundary) * Scale);
//                 }
//             }
//
//             float min = bounds.xMin;
//             float size = bounds.width;
//             if (GradientStyle == Style.Vertical)
//             {
//                 min = bounds.yMin;
//                 size = bounds.height;
//             }
//
//             gradientKeysPos.Sort();
//             for (int i = 0; i < gradientKeysPos.Count; i++)
//             {
//                 gradientKeysPos[i] = (gradientKeysPos[i] * size) + min;
//
//                 if (i > 0 && Math.Abs(gradientKeysPos[i] - gradientKeysPos[i - 1]) < 2)
//                 {
//                     gradientKeysPos.RemoveAt(i);
//                     --i;
//                 }
//             }
//         }
//
//         UIVertex CreateSplitVertex(UIVertex vertex1, UIVertex vertex2, float stop)
//         {
//             if (GradientStyle == Style.Horizontal)
//             {
//                 float sx = vertex1.position.x - stop;
//                 float dx = vertex1.position.x - vertex2.position.x;
//                 float dy = vertex1.position.y - vertex2.position.y;
//                 float uvx = vertex1.uv0.x - vertex2.uv0.x;
//                 float uvy = vertex1.uv0.y - vertex2.uv0.y;
//                 float ratio = sx / dx;
//                 float splitY = vertex1.position.y - (dy * ratio);
//
//                 UIVertex splitVertex = new UIVertex();
//                 splitVertex.position = new Vector3(stop, splitY, vertex1.position.z);
//                 splitVertex.normal = vertex1.normal;
//                 splitVertex.uv0 = new Vector2(vertex1.uv0.x - (uvx * ratio), vertex1.uv0.y - (uvy * ratio));
//                 splitVertex.color = Color.white;
//                 return splitVertex;
//             }
//             else
//             {
//                 float sy = vertex1.position.y - stop;
//                 float dy = vertex1.position.y - vertex2.position.y;
//                 float dx = vertex1.position.x - vertex2.position.x;
//                 float uvx = vertex1.uv0.x - vertex2.uv0.x;
//                 float uvy = vertex1.uv0.y - vertex2.uv0.y;
//                 float ratio = sy / dy;
//                 float splitX = vertex1.position.x - (dx * ratio);
//
//                 UIVertex splitVertex = new UIVertex();
//                 splitVertex.position = new Vector3(splitX, stop, vertex1.position.z);
//                 splitVertex.normal = vertex1.normal;
//                 splitVertex.uv0 = new Vector2(vertex1.uv0.x - (uvx * ratio), vertex1.uv0.y - (uvy * ratio));
//                 splitVertex.color = Color.white;
//                 return splitVertex;
//             }
//         } 
//     }
// }