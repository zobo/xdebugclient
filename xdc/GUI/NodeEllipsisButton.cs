using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using xdc.Properties;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;

namespace Aga.Controls.Tree.NodeControls
{
    public class NodeEllipsisButton : InteractiveControl
    {
        public const int ImageSize = 13;

        private Bitmap _normal;
        private Bitmap _pressed;

        private bool _pstate;

        public NodeEllipsisButton()
        {
            _normal = Resources.ellipsis_normal;
            _pressed = Resources.ellipsis_pressed;
            LeftMargin = 0;
        }

        public override Size MeasureSize(TreeNodeAdv node, DrawContext context)
        {
            if (GetValue(node).ToString() == "")
                return new Size(0, 0);
            else
                return new Size(_normal.Width,_normal.Height);
        }

        public override void Draw(TreeNodeAdv node, DrawContext context)
        {
            if (GetValue(node).ToString() != "")
            {
                Image img;
                if (_pstate)
                    img = _pressed;
                else
                    img = _normal;
                Rectangle bounds = GetBounds(node, context);
                context.Graphics.DrawImage(img, bounds.Location);
            }
        }

        public override void MouseDown(TreeNodeAdvMouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                _pstate = true;
                Parent.FullUpdate();
            }
        }

        public override void MouseUp(TreeNodeAdvMouseEventArgs args)
        {
            _pstate = false;
            Parent.FullUpdate();

            xdc.Forms.PropertyDetailsForm df = new xdc.Forms.PropertyDetailsForm(GetValue(args.Node).ToString());
            df.Show();
        }

        public override void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
        {
            MessageBox.Show("stop");

            object bla = GetValue(args.Node);



            args.Handled = true;
        }

    }
}
