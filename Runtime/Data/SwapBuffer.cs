﻿using System;

namespace huqiang.Data
{
    public class SwapBuffer<T, U> where T : class
    {
        T[] A;//源
        T[] B;//目标
        int length;
        int maxA = 0;
        int maxB = 0;
        public SwapBuffer(int len)
        {
            length = len;
            A = new T[len];
            B = new T[len];
        }
        /// <summary>
        /// 将一个符合条件的源项目移动到目标缓存,并返回
        /// </summary>
        /// <param name="condition">判定委托</param>
        /// <param name="u"></param>
        /// <returns></returns>
        public T Exchange(Func<T, U, bool> condition, U u)
        {
            for (int i = 0; i < maxA; i++)
            {
                var t = A[i];
                if (condition(t, u))
                {
                    B[maxB] = t;
                    maxB++;
                    maxA--;
                    A[i] = A[maxA];
                    return t;
                }
            }
            return null;
        }
        /// <summary>
        /// 给目标添加一个项目
        /// </summary>
        /// <param name="t"></param>
        public void Push(T t)
        {
            if (maxB < length)
            {
                B[maxB] = t;
                maxB++;
            }
        }
        /// <summary>
        /// 从源数据中移除一条数据
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (maxA > 0)
            {
                maxA--;
                var r = A[maxA];
                A[maxA] = null;
                return r;
            }
            return null;
        }
        /// <summary>
        /// 本次所有交换操作完毕
        /// </summary>
        public void Done()
        {
            var t = A;
            A = B;
            B = t;//A和B交换
            var l = maxA;
            maxA = maxB;
            maxB = l;
        }
        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < length; i++)
            {
                A[i] = null;
                B[i] = null;
            }
            maxA = 0;
            maxB = 0;
        }
        /// <summary>
        /// 源数组的长度
        /// </summary>
        public int Length { get { return maxA; } }
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                if (index < 0)
                    return null;
                if (index >= maxA)
                    return null;
                return A[index];
            }
        }
    }
}
