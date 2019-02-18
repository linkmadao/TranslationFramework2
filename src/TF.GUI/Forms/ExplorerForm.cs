﻿using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using TF.Core.Entities;
using WeifenLuo.WinFormsUI.Docking;

namespace TF.GUI.Forms
{
    public partial class ExplorerForm : DockContent
    {
        private class NodeSorter : IComparer
        {
            // Puts folders before files. Sort alphabetically
            public int Compare(object x, object y)
            {
                var tx = x as TreeNode;
                var ty = y as TreeNode;

                var tfc1 = tx.Tag as TranslationFileContainer;
                var tfc2 = ty.Tag as TranslationFileContainer;

                if (tfc1 != null && tfc2 == null)
                {
                    return -1;
                }

                if (tfc1 == null && tfc2 != null)
                {
                    return 1;
                }

                return string.CompareOrdinal(tx.Text, ty.Text);
            }
        }
        
        public delegate bool FileChangedHandler(TranslationFile selectedFile);

        public event FileChangedHandler FileChanged;

        public ExplorerForm()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            tvGameFiles.TreeViewNodeSorter = new NodeSorter();
        }

        public void LoadTree(IList<TranslationFileContainer> containers)
        {
            tvGameFiles.Nodes.Clear();

            var nodes = new List<TreeNode>(containers.Count);
            foreach (var fileContainer in containers)
            {
                var node = new TreeNode(fileContainer.Path, 0, 0)
                {
                    Tag = fileContainer
                };

                foreach (var file in fileContainer.Files)
                {
                    var tnChild = new TreeNode(file.RelativePath, (int)file.Type, (int)file.Type)
                    {
                        Tag = file
                    };

                    node.Nodes.Add(tnChild);
                }

                nodes.Add(node);
            }

            tvGameFiles.Nodes.AddRange(nodes.ToArray());
            tvGameFiles.Sort();
        }

        protected virtual bool OnFileChanged(TranslationFile selectedFile)
        {
            if (FileChanged != null)
            {
                var cancel = FileChanged.Invoke(selectedFile);
                return cancel;
            }

            return false;
        }

        private void tvGameFiles_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
            {
                var result = OnFileChanged(null);
                e.Cancel = result;
            }
            else
            {
                var item = e.Node.Tag as TranslationFile;
                var result = OnFileChanged(item);
                e.Cancel = result;
            }
        }
    }
}
