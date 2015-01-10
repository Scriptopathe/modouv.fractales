// Copyright (C) 2013, 2014 Alvarez Josué
//
// This code is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or (at
// your option) any later version.
//
// This code is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
// License (LICENSE.txt) for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation,
// Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// The developer's email is jUNDERSCOREalvareATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
// capital letters by the corresponding character)
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
namespace Modouv.Fractales.Forms
{
    public partial class StartForm : Form
    {
        public Mode StartMode
        {
            get;
            set;
        }
        public Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind TreeKind
        {
            get;
            set;
        }

        public bool FullScreen
        {
            get;
            set;
        }

        public Microsoft.Xna.Framework.Vector2 Resolution
        {
            get;
            set;
        }

        Dictionary<int, Tuple<string, Vector2>> m_resolutions = new Dictionary<int, Tuple<string, Vector2>>()
            {
                {0, new Tuple<string, Vector2>("HD MAGGLE (1920x1080)", new Vector2(1920, 1080))},
                {1, new Tuple<string, Vector2>("qHD TAPETE (1600x1900)", new Vector2(1600, 900))},
                {2, new Tuple<string, Vector2>("Medium (1200x675)", new Vector2(1200, 675))},
                {3, new Tuple<string, Vector2>("Shitty (800x450)", new Vector2(800, 450))},
                {4, new Tuple<string, Vector2>("Ultra Bad (400x275)", new Vector2(400, 275))},
            };

        void CollectData()
        {
            FullScreen = m_fullScreenCheckbox.Checked;
            Resolution = m_resolutions[m_resolutionCombo.SelectedIndex].Item2;
        }

        public StartForm()
        {
            InitializeComponent();
            foreach(var tup in m_resolutions.Values)
            {
                m_resolutionCombo.Items.Add(tup.Item1);
            }
            m_resolutionCombo.SelectedIndex = 0;
            m_picture.Image = new Bitmap("Forms\\start.png");
            m_startCustom.Click += delegate(object o, EventArgs a)
            {
                StartMode = Mode.Custom;
                CollectData();
                Close();
            };
            m_startDemo.Click += delegate(object o, EventArgs a)
            {
                StartMode = Mode.TreeDemo;
                TreeKind = Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Demo;
                CollectData();
                Close();
            };
            m_demoTree2.Click += delegate(object o, EventArgs a)
            {
                StartMode = Mode.TreeDemo;
                TreeKind = Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Demo2;
                CollectData();
                Close();
            };
            m_demoTree3.Click += delegate(object o, EventArgs a)
            {
                StartMode = Mode.TreeDemo;
                TreeKind = Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Demo3;
                CollectData();
                Close();
            };
            m_demoTree4.Click += delegate(object o, EventArgs a)
            {
                StartMode = Mode.TreeDemo;
                TreeKind = Generation.Populations.WorldFantasy.FractalTreePopulator.TreeKind.Demo4;
                CollectData();
                Close();
            };
            m_startWorld.Click += delegate(object o, EventArgs a)
            {
                StartMode = Mode.WorldDemo;
                CollectData();
                Close();
            };
        }
    }
}
