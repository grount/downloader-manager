﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace download_manager
{
    public class FileDownload : IDownloader
    {
        public Stopwatch m_StopWatch;
        public Queue<string> m_UrlQueue { get; set; }
        DownloadStatus e_DownloadStatus;
        public int m_BufferSize { get; set; }
        public DataGridView m_DataGrid { private get; set; }
        public string m_DownloadDestination { get; set; }
        public long m_TotalSize { get; set; }
        public int m_TotalBytesRecieved { get; set; }
        private int m_bytesRead { get; set; }
        private int m_ThreadIndex { get; set; }
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;


        public FileDownload()
        {
            m_UrlQueue = new Queue<string>();
            m_StopWatch = new Stopwatch();
            e_DownloadStatus = new DownloadStatus();
            m_BufferSize = 1024;
            m_TotalBytesRecieved = 0;

        }

        public void Start(string i_DownloadPath, int i_currentThreadIndex)
        {
            m_ThreadIndex = i_currentThreadIndex;
            m_DownloadDestination = i_DownloadPath;
            Download();
        }

        public void Download()
        {
            WebRequest webRequest = null;
            WebResponse webResponse = null;
            Stream remoteStream = null;
            Stream localStream = null;

            try
            {
                webRequest = WebRequest.Create(m_UrlQueue.Dequeue());
                e_DownloadStatus = DownloadStatus.Downloading;
                //webRequest.Credentials = CredentialCache.DefaultCredentials;
                webResponse = webRequest.GetResponse();

                m_TotalSize = webResponse.ContentLength;
                if (m_TotalSize <= 0)
                {
                    throw new ApplicationException("The file that you want to download doesn't exists");
                }

                m_StopWatch.Start(); // TODO when completed stop watch..
                remoteStream = webResponse.GetResponseStream();
                localStream = File.Create(m_DownloadDestination);

                byte[] buffer = new byte[m_BufferSize];
                m_bytesRead = 0;

                do
                {
                    if (remoteStream != null) m_bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
                    localStream.Write(buffer, 0, m_bytesRead);

                    m_TotalBytesRecieved += m_bytesRead;
                    InternalDownloadProgressChanged();
                } while (m_bytesRead > 0);

            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
            InternalDownloadCompleted();
            webResponse?.Close();
            remoteStream?.Close();
            localStream?.Close();
            }

        }

        private void InternalDownloadCompleted()
        {
            OnDownloadCompleted(new DownloadCompletedEventArgs(m_ThreadIndex));
        }

        private void OnDownloadCompleted(DownloadCompletedEventArgs e)
        {
            DownloadCompleted?.Invoke(this, e);
        }

        private void InternalDownloadProgressChanged()
        {
            OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(m_TotalBytesRecieved, m_TotalSize,
                (int) (m_TotalBytesRecieved / 1024d / m_StopWatch.Elapsed.TotalSeconds), m_ThreadIndex));
        }

        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChanged?.Invoke(this, e);
        }
    }
}