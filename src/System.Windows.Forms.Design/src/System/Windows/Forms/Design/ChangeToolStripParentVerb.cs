﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing.Design;

namespace System.Windows.Forms.Design
{
    /// <summary>
    ///  Internal class to provide 'Embed in ToolStripContainer" verb for ToolStrips and MenuStrips.
    /// </summary>
    internal class ChangeToolStripParentVerb
    {
        private readonly ToolStripDesigner _designer;
        private readonly IDesignerHost _host;
        private readonly IComponentChangeService _componentChangeSvc;
        private readonly IServiceProvider _provider;

        /// <summary>
        ///  Create one of these things...
        /// </summary>
        internal ChangeToolStripParentVerb(string text, ToolStripDesigner designer)
        {
            Debug.Assert(designer != null, "Can't have a StandardMenuStripVerb without an associated designer");
            _designer = designer;
            _provider = designer.Component.Site;
            _host = (IDesignerHost)_provider.GetService(typeof(IDesignerHost));
            _componentChangeSvc = (IComponentChangeService)_provider.GetService(typeof(IComponentChangeService));
        }

        /// <summary>
        ///  When the verb is invoked, change the parent of the ToolStrip.
        /// </summary>
        // This is actually called...
        public void ChangeParent()
        {
            Cursor current = Cursor.Current;
            // create a transaction so this happens as an atomic unit.
            DesignerTransaction changeParent = _host.CreateTransaction("Add ToolStripContainer Transaction");
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                //Add a New ToolStripContainer to the RootComponent ...
                Control root = _host.RootComponent as Control;
                if (_host.GetDesigner(root) is ParentControlDesigner rootDesigner)
                {
                    // close the DAP first - this is so that the autoshown panel on drag drop here is not conflicting with the currently opened panel
                    // if the verb was called from the panel
                    ToolStrip toolStrip = _designer.Component as ToolStrip;
                    if (toolStrip != null && _designer != null && _designer.Component != null && _provider != null)
                    {
                        DesignerActionUIService dapuisvc = _provider.GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
                        dapuisvc.HideUI(toolStrip);
                    }

                    // Get OleDragHandler ...
                    ToolboxItem tbi = new ToolboxItem(typeof(ToolStripContainer));
                    OleDragDropHandler ddh = rootDesigner.GetOleDragHandler();
                    if (ddh != null)
                    {
                        IComponent[] newComp = ddh.CreateTool(tbi, root, 0, 0, 0, 0, false, false);
                        if (newComp[0] is ToolStripContainer tsc)
                        {
                            if (toolStrip != null)
                            {
                                var changeService = _provider.GetService<IComponentChangeService>();
                                Control newParent = GetParent(tsc, toolStrip);
                                PropertyDescriptor controlsProp = TypeDescriptor.GetProperties(newParent)["Controls"];
                                Control oldParent = toolStrip.Parent;
                                if (oldParent != null)
                                {
                                    changeService.OnComponentChanging(oldParent, controlsProp);
                                    //remove control from the old parent
                                    oldParent.Controls.Remove(toolStrip);
                                }

                                if (newParent != null)
                                {
                                    changeService.OnComponentChanging(newParent, controlsProp);
                                    //finally add & relocate the control with the new parent
                                    newParent.Controls.Add(toolStrip);
                                }

                                //fire our comp changed events
                                if (changeService != null && oldParent != null && newParent != null)
                                {
                                    changeService.OnComponentChanged(oldParent, controlsProp);
                                    changeService.OnComponentChanged(newParent, controlsProp);
                                }

                                //Set the Selection on the new Parent ... so that the selection is restored to the new item,
                                if (_provider.GetService(typeof(ISelectionService)) is ISelectionService selSvc)
                                {
                                    selSvc.SetSelectedComponents(new IComponent[] { tsc });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException)
                {
                    IUIService uiService = (IUIService)_provider.GetService(typeof(IUIService));
                    uiService.ShowError(e.Message);
                }

                if (changeParent != null)
                {
                    changeParent.Cancel();
                    changeParent = null;
                }
            }
            finally
            {
                changeParent?.Commit();

                Cursor.Current = current;
            }
        }

        private static Control GetParent(ToolStripContainer container, Control c)
        {
            Control newParent = container.ContentPanel;
            DockStyle dock = c.Dock;
            if (c.Parent is ToolStripPanel)
            {
                dock = c.Parent.Dock;
            }

            foreach (Control panel in container.Controls)
            {
                if (panel is ToolStripPanel)
                {
                    if (panel.Dock == dock)
                    {
                        newParent = panel;
                        break;
                    }
                }
            }

            return newParent;
        }
    }
}
