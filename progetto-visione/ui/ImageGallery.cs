﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Vision.UI
{
    public partial class ImageGallery : UserControl
    {
        public ImageGallery()
        {
            InitializeComponent();
        }

        public void AddImage(string path, string imageLabel = "")
        {
            AddImage(new Image<Bgr, byte>(path), imageLabel);
        }

        public void AddImage<TColor, TDepth>(Image<TColor, TDepth> image, string imageLabel = "") where TColor : struct, IColor where TDepth : new()
        {
            var singleImageWidth = table.Size.Width / table.ColumnCount;
            var singleImageHeight = table.Size.Height / 2;

            var label = new Label {
                Dock = DockStyle.Top,
                Text = imageLabel,
                AutoEllipsis = true,
                AutoSize = false
            };
            var box = new ImageBox
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                SizeMode = PictureBoxSizeMode.Zoom,
                FunctionalMode = ImageBox.FunctionalModeOption.Minimum,
                Image = image
            };
            var layout = new Panel { Height = singleImageHeight, BorderStyle = BorderStyle.FixedSingle };
            
            layout.Controls.Add(label);
            layout.Controls.Add(box);
            table.Controls.Add(layout);
        }

        public void AddImages(string[] paths, string[] imageLabels = default(string[]))
        {
            for (int i = 0; i < paths.Length; i++)
            {
                AddImage(paths[i], imageLabels.ElementAtOrDefault(i));
            }
        }

        public void AddImages<TColor, TDepth>(Image<TColor, TDepth>[] images, string[] imageLabels = default(string[])) 
            where TColor : struct, IColor 
            where TDepth : new()
        {
            for (int i = 0; i < images.Length; i++)
            {
                AddImage(images[i], imageLabels.ElementAtOrDefault(i));
            }
        }

        public void ClearGallery()
        {
            foreach (Control box in table.Controls.OfType<Panel>().ToList())
            {
                table.Controls.Remove(box);
            }
        }
    }
}
