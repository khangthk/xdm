﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;
using System.IO;
using Application = Gtk.Application;
using IoPath = System.IO.Path;
using XDM.Core.Lib.Common;
using XDM.Core.Lib.UI;
using XDM.GtkUI.Utils;
using Translations;
using UI = Gtk.Builder.ObjectAttribute;
using XDM.Core.Lib.Util;
using XDMApp;

namespace XDM.GtkUI.Dialogs.QueueScheduler
{
    internal class QueueSelectionDialog : Dialog, IQueueSelectionDialog
    {
        public event EventHandler<QueueSelectionEventArgs>? QueueSelected;
        public event EventHandler? ManageQueuesClicked;

        [UI] private Button BtnOK = null;
        [UI] private Button BtnCancel = null;
        [UI] private TreeView LbQueues = null;

        private string[] downloadIds = new string[0];
        private ListStore listStore; 
        private WindowGroup group;
        public bool Result { get; set; } = false;

        private QueueSelectionDialog(Builder builder, Window parent, WindowGroup group) : base(builder.GetRawOwnedObject("dialog"))
        {
            builder.Autoconnect(this);
            Modal = true;
            SetPosition(WindowPosition.Center);
            TransientFor = parent;
            this.group = group;
            this.group.AddWindow(this);

            GtkHelper.AttachSafeDispose(this);
            Title = TextResource.GetText("Q_MOVE_TO");
            SetDefaultSize(400, 300);
            LoadTexts();

            listStore = new ListStore(typeof(string));
            LbQueues.Model = listStore;

            var queueNameRendererText = new CellRendererText();
            var queueNameColumn = new TreeViewColumn("", queueNameRendererText, "text", 0)
            {
                Resizable = false,
                Reorderable = false,
                Sizing = TreeViewColumnSizing.Autosize,
                Expand = true
            };
            LbQueues.HeadersVisible = false;
            LbQueues.AppendColumn(queueNameColumn);

            BtnCancel.Clicked += BtnCancel_Clicked;
            BtnOK.Clicked += BtnOK_Clicked;
        }

        private void BtnOK_Clicked(object? sender, EventArgs e)
        {
            QueueSelected?.Invoke(this, new QueueSelectionEventArgs(GtkHelper.GetSelectedIndex(LbQueues), downloadIds));
            QueueSelected = null;
            Result = true;
            this.group.RemoveWindow(this);
            Visible = false;
        }

        private void BtnCancel_Clicked(object? sender, EventArgs e)
        {
            Result = false;
            this.group.RemoveWindow(this);
            Visible = false;
        }

        public void SetData(IEnumerable<string> items, string[] downloadIds)
        {
            this.downloadIds = downloadIds;
            foreach (var item in items)
            {
                listStore.AppendValues(item);
            }
            if (listStore.IterNChildren() > 0)
            {
                GtkHelper.SetSelectedIndex(LbQueues, 0);
            }
        }

        public void ShowWindow(IAppWinPeer peer)
        {
            this.Run();
            this.Destroy();
            this.Dispose();
        }

        private void LoadTexts()
        {
            BtnCancel.Label = TextResource.GetText("ND_CANCEL");
            BtnOK.Label = TextResource.GetText("MSG_OK");
        }

        public static QueueSelectionDialog CreateFromGladeFile(Window parent, WindowGroup group)
        {
            var builder = new Builder();
            builder.AddFromFile(IoPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "glade", "queue-selection-dialog.glade"));
            return new QueueSelectionDialog(builder, parent, group);
        }
    }
}
