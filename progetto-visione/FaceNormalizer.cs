﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using System.Drawing;
using Vision.Model;

namespace Vision
{
    class FaceNormalizer
    {
        private CascadeClassifier eyesDector;

        /// percentage that control how much of the face is visible
        private const double DESIDERED_LEFT_EYE_X = 0.35;
        private const double DESIDERED_LEFT_EYE_Y = 0.5;

        public FaceNormalizer(CascadeClassifier eyesDector)
        {
            this.eyesDector = eyesDector;
        }

        public Image<Gray, TDepth> Normalize<TDepth>(Image<Bgr, TDepth> originalImage, int targetWidth=512, int targetHeight = 512) where TDepth : new()
        {
            var eyes = EyesDetector.GetEyes(eyesDector, originalImage);
            if (eyes == null)
            {
                //ImageViewer.Show(originalImage);
                return null;
            }
            //DrawEyesRect(originalImage, eyes);
            // detect angle respect to horizontal line of eyes
            var dy = eyes.Right.GetCenter().Y - eyes.Left.GetCenter().Y;
            var dx = eyes.Right.GetCenter().X - eyes.Left.GetCenter().X;
            var rotationAngle = RadToDegree(Math.Atan2(dy, dx)); // angle
            var eyesCenterX = (eyes.Left.GetCenter().X + eyes.Right.GetCenter().X) / 2;
            var eyesCenterY = (eyes.Left.GetCenter().Y + eyes.Right.GetCenter().Y) / 2;

            // determine the right target scale
            var dist = Math.Sqrt(dx * dx + dy * dy);
            var targetDist = (1.0 - 2 * DESIDERED_LEFT_EYE_X) * targetWidth;
            var targetScale = targetDist / dist;

            // determine the translation for cropping image
            var tx = targetWidth * 0.5;
            var ty = targetHeight * DESIDERED_LEFT_EYE_Y;

            // create affine matrix
            var rotationMatrix = new RotationMatrix2D(new PointF(eyesCenterX, eyesCenterY), rotationAngle, targetScale);
            var affineMatrix = new Matrix<double>(rotationMatrix.Rows, rotationMatrix.Cols, rotationMatrix.DataPointer);
            //rotationMatrix.Dispose();

            affineMatrix.SetCellValue(0, 2, affineMatrix.GetCellValue(0, 2) + (tx - eyesCenterX));
            affineMatrix.SetCellValue(1, 2, affineMatrix.GetCellValue(1, 2) + (ty - eyesCenterY));

            var resized = originalImage.WarpAffine(affineMatrix.Mat,
                targetWidth,
                targetHeight,
                Inter.Area,
                Warp.Default,
                BorderType.Default,
                new Bgr()
            );

            return resized.Convert<Gray, TDepth>();
        }

        private void DrawEyesRect<TDepth>(Image<Bgr, TDepth> img, Eyes eyes) where TDepth : new()
        {
            img.Draw(eyes.Left, new Bgr(Color.Blue));
            img.Draw(eyes.Right, new Bgr(Color.Green));
            img.DrawPolyline(new Point[] { eyes.Left.GetCenter(), eyes.Right.GetCenter() }, false, new Bgr(Color.Red));
            new System.Threading.Thread(() =>
            {
                ImageViewer.Show(img);
            }).Start();
        }

        private double RadToDegree(double rad)
        {
            return rad * (180 / Math.PI);
        }
    }

    static class Ext
    {
        public static void SetCellValue<TDepth>(this Matrix<TDepth> m, int row, int col, double value) where TDepth : new()
        {
            m.GetRow(row).GetCol(col).SetValue(value);
        }
        public static TDepth GetCellValue<TDepth>(this Matrix<TDepth> m, int row, int col) where TDepth : new()
        {
            return m.Data[row, col];
        }
    }
}
