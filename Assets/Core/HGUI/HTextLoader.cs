﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using huqiang.Data;
using UnityEngine;

namespace Assets.Core.HGUI
{
    public unsafe struct HTextData
    {
        public HGraphicsData graphicsData;
        public Int32 text;
        public Int32 font;
        public Vector2 pivot;
        public HorizontalWrapMode m_hof;
        public VerticalWrapMode m_vof;
        public TextAnchor anchor;
        public bool m_richText;
        public float m_lineSpace;
        public int m_fontSize;
        public bool m_align;
        public static int Size = sizeof(HTextData);
        public static int ElementSize = Size / 4;
    }
    public class HTextLoader:HGraphicsLoader
    {
        public static List<Font> fonts = new List<Font>();
        public static Font FindFont(string str)
        {
            if (fonts == null)
                return null;
            for (int i = 0; i < fonts.Count; i++)
            {
                if (str == fonts[i].name)
                    return fonts[i];
            }
            if (fonts.Count == 0)
                fonts.Add(Font.CreateDynamicFontFromOSFont("Arial", 16));
            return fonts[0];
        }
        protected string fontName;
        protected unsafe void LoadHText(FakeStruct fake,HText tar)
        {
            HTextData* src = (HTextData*)fake.ip;
            var buffer = fake.buffer;
            tar.m_text = buffer.GetData(src->text)as string;
            fontName = buffer.GetData(src->font)as string;
            if (fontName != null)
                tar._font = FindFont(fontName);
            tar.TextPivot = src->pivot;
            tar.m_hof = src->m_hof;
            tar.m_vof = src->m_vof;
            tar.TextAnchor = src->anchor;
            tar.m_richText = src->m_richText;
            tar.m_lineSpace = src->m_lineSpace;
            tar.m_fontSize = src->m_fontSize;
            tar.m_align = src->m_align;
        }
        protected unsafe void SaveHText(FakeStruct fake, HText src)
        {
            HTextData* tar = (HTextData*)fake.ip;
            tar->text = fake.buffer.AddData(src.m_text);
            if (src._font != null)
                tar->font = fake.buffer.AddData(src._font.name);
            tar->pivot = src.TextPivot;
            tar->m_hof = src.m_hof;
            tar->m_vof = src.m_vof;
            tar->anchor = src.TextAnchor;
            tar->m_richText = src.m_richText;
            tar->m_lineSpace = src.m_lineSpace;
            tar->m_fontSize = src.m_fontSize;
            tar->m_align = src.m_align;
        }
        public unsafe override void LoadToObject(FakeStruct fake, Component com)
        {
            HText image = com.GetComponent<HText>();
            if (image == null)
                return;
            LoadScript(fake.ip, image);
            LoadHGraphics(fake, image);
            LoadHText(fake, image);
        }
        public unsafe override FakeStruct LoadFromObject(Component com, DataBuffer buffer)
        {
            var src = com as HText;
            if (src == null)
                return null;
            FakeStruct fake = new FakeStruct(buffer, HTextData.ElementSize);
            SaveScript(fake.ip, src);
            SaveHGraphics(fake, src);
            SaveHText(fake, src);
            return fake;
        }
    }
}
