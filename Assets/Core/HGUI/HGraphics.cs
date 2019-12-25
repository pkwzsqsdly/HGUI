﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.HGUI
{
    public class HGraphics:AsyncScript
    {
        internal static Shader DefShader { get {
                if(shader==null)
                    shader = Shader.Find("Custom/UIDef");
                return shader;
            } }
        public static Shader shader;
        Color _color;
        public Color Color { get; set; }
        internal Vector3[] vertex;
        internal Vector2[] uv;
        internal int[] tris;
        internal int[][] SubMesh;
        internal Material[] materials;
        public Material material;
        internal int InstanceID;
        internal bool _vertexChange;
        public override void Initial()
        {
            material = new Material(DefShader);
        }
        public virtual void UpdateMesh()
        {
        }
        internal virtual Material GetMaterial(int index,HCanvas canvas)
        {
            return null;
        }
    }
}
