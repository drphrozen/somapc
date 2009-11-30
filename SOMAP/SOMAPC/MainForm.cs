using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tamir.SharpSsh;
using System.Diagnostics;
using System.Collections;
using Tamir.SharpSsh.jsch;

namespace SOMAPC
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            root = remoteTreeView.Nodes.Add("/", "/", "server", "server");
        }

        private Sftp sftp = null;
        private TreeNode root;

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (sftp != null)
                sftp.Close();
            LoginInfo info = LoginInfo.CreateFromString(loginTextBox.Text);
            sftp = new Sftp(info.Host, info.Username, info.Password);
            sftp.Connect();
            updateTree(info.Path);
        }

        private void createTreeRecurse(TreeNode root, string[] children)
        {
            foreach (string child in children)
                root = root.Nodes.Add(child);
        }

        private void updateTree(string path)
        {
            string[] nodes;
            {
                List<string> tmp = new List<string>(path.Split('/'));
                tmp.RemoveAll((str) => { return str == ""; });
                nodes = tmp.ToArray();
            }
            updateTree(path, nodes);
        }
        private void updateTree(string path, string[] nodes)
        {
            TreeNode currentNode = remoteTreeView.Nodes["/"];
            for (int i = 0; i < nodes.Length; i++)
            {
                string node = nodes[i];
                TreeNodeCollection currentNodes = currentNode.Nodes;
                if (currentNodes.ContainsKey(node))
                {
                    currentNode = currentNodes[node];
                }
                else
                {
                    string[] tmp = new string[nodes.Length - i];
                    Array.Copy(nodes, i, tmp, 0, tmp.Length);
                    createTreeRecurse(currentNode, tmp);
                    break;
                }
            }

            List<string> tmpList = new List<string>();
            foreach (TreeNode node in currentNode.Nodes)
                tmpList.Add(node.Text);
            tmpList.Sort();

            string[] tmpArray = tmpList.ToArray();
            List<ChannelSftp.LsEntry> list = sftp.GetEntryList(path);
            foreach (ChannelSftp.LsEntry entry in list)
            {
                string entryName = entry.getFilename().ToString();
                if (entryName == "." || entryName == "..")
                    continue;
                if (Array.BinarySearch<string>(tmpArray, entryName) < 0)
                {
                    // Add the new node if it didn't exist
                    string imageKey = (entry.getAttrs().isDir() ? "folder" : "file");
                    string selectedImageKey = (entry.getAttrs().isDir() ? "folder_open" : "file");
                    currentNode.Nodes.Add(entryName, entryName, imageKey, selectedImageKey);
                    Console.WriteLine("entry: " + entryName);
                }
                else
                {
                    // The node already existed, remove it from the garbage list
                    tmpList.Remove(entryName);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(sftp != null)
                sftp.Close();
        }

        private string buildTree(TreeNode node)
        {
            List<string> nodes = new List<string>();
            TreeNode current = node;
            while (current != root)
            {
                nodes.Add(current.Text);
                current = current.Parent;
            }
            nodes.Reverse();
            
            StringBuilder sb = new StringBuilder();
            sb.Append('/');
            foreach (string tmp in nodes)
            {
                sb.Append(tmp);
                sb.Append('/');
            }
            return sb.ToString();
        }

        private void remoteTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            remoteTreeView.SuspendLayout();
            if(e.Node != null)
            {
                if (sftp != null)
                {
                    updateTree(buildTree(e.Node));
                }
            }
            e.Node.Expand();
            remoteTreeView.ResumeLayout();
        }
    }
}
