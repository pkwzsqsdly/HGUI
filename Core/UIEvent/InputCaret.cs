﻿using huqiang.Core.HGUI;
using UnityEngine;

namespace huqiang.UIEvent
{
    public class InputCaret
    {
        static HImage m_Caret;
        public static HImage Caret
        {
            get
            {
                if (m_Caret == null)
                {
                    var g = new GameObject("m_caret",typeof(HImage));
                    m_Caret = g.GetComponent<HImage>();
                    m_Caret.transform.SetParent(App.UIRoot);
                    m_Caret.SizeDelta = new Vector2(2,24);
                }
                else if (m_Caret.name == "buff")
                {
                    var g = new GameObject("m_caret", typeof(HImage));
                    m_Caret = g.GetComponent<HImage>();
                    m_Caret.SizeDelta = new Vector2(2, 24);
                }
                return m_Caret;
            }
        }
        static float time;
        public static int CaretStyle = 0;
        public static void UpdateCaret()
        {
            switch (CaretStyle)
            {
                case 1:
                    time += Time.deltaTime;
                    if (time > 2f)
                    {
                        time = 0;
                    }
                    else if (time > 1f)
                    {
                        Caret.gameObject.SetActive(false);
                    }
                    else
                    {
                        //if (Keyboard.InputEvent != null)
                        //    Keyboard.SetPressPointer();
                        Caret.gameObject.SetActive(true);
                    }
                    break;
                case 2:
                    Caret.gameObject.SetActive(true);
                    break;
                default:
                    Caret.gameObject.SetActive(false);
                    break;
            }
        }
        public static void SetParent(Transform rect)
        {
            var t = Caret.transform;
            t.SetParent(rect);
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
            t.localRotation = Quaternion.identity;
        }
        public static void Active()
        {
            if (m_Caret != null)
            {
                m_Caret.gameObject.SetActive(true);
            }
        }
        public static void Hide()
        {
            CaretStyle = 0;
            if (m_Caret != null)
            {
                m_Caret.gameObject.SetActive(false);
            }
        }
        public static void ChangeCaret(Vector3 pos,Vector2 size)
        {
            var trans = Caret.transform;
            trans.localPosition = pos;
            Caret.SizeDelta = size;
            Caret.m_vertexChange = true;
            CaretStyle = 1;
        }
        public static void ChangeCaret(float left, float right, float top, float down, Color32 color)
        {
            if (Caret.vertInfo.Size == 0)
            {
                Caret.vertInfo = HGUIMesh.blockBuffer.RegNew(4);
            }
            else if (Caret.vertInfo.Size < 4 | Caret.vertInfo.Size > 8)
            {
                HGUIMesh.blockBuffer.Release(ref Caret.vertInfo);
                Caret.vertInfo = HGUIMesh.blockBuffer.RegNew(4);
            }
            unsafe
            {
                HVertex* hv = (HVertex*)Caret.vertInfo.Addr;
                hv[0].position.x = left;
                hv[0].position.y = down;
                hv[0].color = color;

                hv[1].position.x = right;
                hv[1].position.y = down;
                hv[1].color = color;

                hv[2].position.x = left;
                hv[2].position.y = top;
                hv[2].color = color;

                hv[3].position.x = right;
                hv[3].position.y = top;
                hv[3].color = color;
            }
          
            Caret.tris = HGUIMesh.Rectangle;
            CaretStyle = 1;
        }
        public static void ChangeCaret(HVertex[] vertices,int[] tris)
        {
            int c = vertices.Length;
            if (Caret.vertInfo.Size > 0)
            {
                HGUIMesh.blockBuffer.Release(ref Caret.vertInfo);
            }
            Caret.vertInfo = HGUIMesh.blockBuffer.RegNew(c);
            unsafe
            {
                HVertex* hv = (HVertex*)Caret.vertInfo.Addr;
                for (int i = 0; i < c; i++)
                {
                    hv[i] = vertices[i];
                }
            }
            Caret.vertInfo.DataCount = c;
            Caret.tris = tris;
            CaretStyle = 2;
        }
    }
}
