﻿using System;
using System.IO;
using System.Windows.Forms;

namespace download_manager
{
    public partial class OptionsForm : Form
    {
        public int m_simultaneousDownloadsCount { get; set; }
        public string m_serializationPath;
        public OptionsForm()
        {
            InitializeComponent();
            m_serializationPath = Environment.CurrentDirectory + "\\save.bin";
            deserializePreviousState();
        }

        private void maxThreadCount_ValueChanged(object sender, EventArgs e)
        {
            m_simultaneousDownloadsCount = maxThreadCountTrackBar.Value;
            threadCountTextBox.Text = maxThreadCountTrackBar.Value.ToString();
        }

        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }


        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T) binaryFormatter.Deserialize(stream);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            MainWindow.m_numberOfSimultaneousDownloads = m_simultaneousDownloadsCount;

            WriteToBinaryFile(m_serializationPath, m_simultaneousDownloadsCount);
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void deserializePreviousState()
        {
            if (File.Exists(m_serializationPath))
            {
                m_simultaneousDownloadsCount = ReadFromBinaryFile<int>(m_serializationPath);
                maxThreadCountTrackBar.Value = m_simultaneousDownloadsCount;
            }
        }
    }
}