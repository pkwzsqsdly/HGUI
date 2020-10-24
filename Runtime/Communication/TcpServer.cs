﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace huqiang
{
    /// <summary>
    /// tcp监听服务器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TcpServer<T>  where T : TcpLink, new()
    {
        /// <summary>
        /// 每个线程的最大用户连接数
        /// </summary>
        public static int SingleCount = 2048;
        /// <summary>
        /// 线程缓存
        /// </summary>
        public LinkThread<T>[] linkBuff;
        Socket soc;
        /// <summary>
        /// 单例服务器实例
        /// </summary>
        public static TcpServer<T> Instance;

        Thread server;
        PackType packType;
        IPEndPoint endPoint;
        int tCount;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">服务器ip地址</param>
        /// <param name="port">服务器端口</param>
        /// <param name="type">数据封包类型</param>
        /// <param name="thread">线程数量</param>
        public TcpServer(string ip, int port,PackType type = PackType.Part, int thread = 8)
        {
            tCount = thread;
            packType = type;
      
            soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            soc.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //端点
            endPoint= new IPEndPoint(IPAddress.Parse(ip), port);
            //绑定
            try
            {
                soc.Bind(endPoint);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            soc.Listen(0);
            soc.ReceiveTimeout = 1000;
            Instance = this;
            if (tCount > 0)
            {
                linkBuff = new LinkThread<T>[tCount];
                for (int i = 0; i < tCount; i++)
                {
                    linkBuff[i] = new LinkThread<T>(SingleCount);
                    linkBuff[i].soc = soc;
                }
            }
            else
            {
                tCount = 1;
                linkBuff = new LinkThread<T>[1];
                linkBuff[0] = new LinkThread<T>(SingleCount);
                linkBuff[0].soc = soc;
            }
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            if(server==null)
            {
                server = new Thread(AcceptClient);
                server.Start();
            }
            if(threadTimer==null)
            {
                threadTimer = new ThreadTimer();
                threadTimer.Interal = 1000;
                threadTimer.Tick = (o, e) => {
                    try
                    {
                        Heartbeat();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                    }
                };
            }
        }
        ThreadTimer threadTimer;
        byte[] nil = { 0 };
        /// <summary>
        /// 资源释放
        /// </summary>
        public void Dispose()
        {
            soc.Disconnect(true);
            soc.Dispose();
            server.Abort();
            Instance = null;
            for (int i = 0; i < tCount; i++)
                linkBuff[i].running = false;
            if (threadTimer != null)
                threadTimer.Dispose();
        }
        void AcceptClient()
        {
            while (true)
            {
                try
                {
                    var client = soc.Accept();
                    CreateLink(client);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                }
            }
        }
        /// <summary>
        /// 给用户发送心跳
        /// </summary>
        void Heartbeat()
        {
            for (int i = 0; i <tCount; i++)
            {
                var links = linkBuff[i];
                for (int j = 0; j < links.Count; j++)
                {
                    var l = links[j];
                    if (l != null)
                        l.Send(nil);
                }
            }
        }
        /// <summary>
        /// 广播所有在线用户
        /// </summary>
        /// <param name="action"></param>
        public void Broadcasting(Action<T> action)
        {
            for (int i = 0; i < tCount; i++)
            {
                var links = linkBuff[i];
                for (int j = 0; j < links.Count; j++)
                {
                    var l = links[j];
                    if (l != null)
                        action(l);
                }
            }
        }
        /// <summary>
        /// 查询某个用户连接
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口</param>
        /// <returns></returns>
        public T FindLink(int ip, int port)
        {
            for (int i = 0; i < tCount; i++)
            {
                var l = linkBuff[i].Find(ip, port);
                if (l != null)
                    return l;
            }
            return null;
        }
         void CreateLink(Socket client)
        {
            var end =  client.RemoteEndPoint as IPEndPoint;
            var link = new T();
            link.SetSocket(client,end,packType);
            int s = 0;
            int c = linkBuff[0].Count;
            for (int i = 1; i < tCount; i++)
            {
                if (c > linkBuff[i].Count)
                {
                    s = i;
                    c = linkBuff[i].Count;
                }
            }
            link.buffIndex = s;
            linkBuff[s].Add(link);
        }
        /// <summary>
        /// 查询某个用户连接
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        public T FindLink(Int64 id)
        {
            for (int i = 0; i < tCount; i++)
            {
                var l = linkBuff[i].Find(id);
                if (l != null)
                    return l;
            }
            return null;
        }
    }
}