﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms
{
    public partial class DataGridViewButtonCell
    {
        private static class DataGridViewButtonCellRenderer
        {
            private static VisualStyleRenderer? s_visualStyleRenderer;

            public static VisualStyleRenderer DataGridViewButtonRenderer
            {
                get
                {
                    s_visualStyleRenderer ??= new VisualStyleRenderer(ButtonElement);

                    return s_visualStyleRenderer;
                }
            }

            public static void DrawButton(Graphics g, Rectangle bounds, int buttonState)
            {
                DataGridViewButtonRenderer.SetParameters(ButtonElement.ClassName, ButtonElement.Part, buttonState);
                DataGridViewButtonRenderer.DrawBackground(g, bounds, Rectangle.Truncate(g.ClipBounds));
            }
        }
    }
}
