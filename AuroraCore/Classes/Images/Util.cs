using Aurora;
using AuroraCoreLib.Enums;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AuroraCore.Classes.Images
{
    public class Util
    {
        private readonly CreateImageOptions _cIO;
        public Util(IWebHostEnvironment env)
        {
            _cIO = new(env);
        }
        public Util(CreateImageOptions cIO)
        {
            _cIO = cIO;
        }

        public void Start(NanoleafJsonPanelLayoutLayout pl, string name)
        {
            XYMax xy = GetXyMax(pl);
            Bitmap bmp = CreateBitmap(xy.XMax, xy.YMax);
            SaveImage(bmp, pl, name);
        }
        public XYMax GetXyMax(NanoleafJsonPanelLayoutLayout pl)
        {
            XYMax xy = new()
            {
                XMax = pl.PositionData.Select(x => x.X).Max(),
                YMax = pl.PositionData.Select(x => x.Y).Max()
            };
            return xy;
        }



        public Bitmap CreateBitmap(int width, int heigth)
        {
            Bitmap bmp = new(width + _cIO.BorderResized, heigth + _cIO.BorderResized);
            using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(_cIO.Background); }
            return bmp;
        }

        public void DrawHexaGon(NanoLeafJsonPositionData pd, Graphics graphicIm)
        {
            var graphics = graphicIm;
            //Get the middle of the panel
            int x_0 = pd.X + _cIO.Border;
            int y_0 = pd.Y + _cIO.Border;

            var shape = new PointF[6];

            var r = pd.SideLenght;

            //Create 6 points
            for (int a = 0; a < 6; a++)
            {
                shape[a] = new PointF(x_0 + r * (float)Math.Cos(a * 60 * Math.PI / 180f), y_0 + r * (float)Math.Sin(a * 60 * Math.PI / 180f));
            }
            Pen pen = new(_cIO.BorderColor, 3);
            graphics.DrawPolygon(pen, shape);
        }

        public void DrawTriangle2(NanoLeafJsonPositionData pd, Graphics graphicIm)
        {
            Point[] shape = new Point[3];
            var xmiddle = pd.X + _cIO.Border;
            var ymiddle = pd.Y + _cIO.Border;
            var sidelenght = pd.SideLenght;
            var halfsidelenght = sidelenght / 2;
            var h = (int)Math.Round(sidelenght * Math.Sqrt(3) / 2);
            var xdreieck = h / 3 * 2;
            var ydreieck = h / 3;
            switch (pd.Orientation)
            {
                case 0:
                case 360:
                    shape[0] = new Point(xmiddle, ymiddle + xdreieck);
                    shape[1] = new Point(xmiddle - halfsidelenght, ymiddle - ydreieck);
                    shape[2] = new Point(xmiddle + halfsidelenght, ymiddle - ydreieck);
                    break;
                case 60:
                    shape[0] = new Point(xmiddle + xdreieck, ymiddle + halfsidelenght);
                    shape[1] = new Point(xmiddle - xdreieck, ymiddle + ydreieck);
                    shape[2] = new Point(xmiddle + ydreieck, ymiddle - xdreieck);
                    break;
                case 120:
                    shape[0] = new Point(xmiddle + ydreieck, ymiddle + xdreieck);
                    shape[1] = new Point(xmiddle + halfsidelenght, ymiddle - ydreieck);
                    shape[2] = new Point(xmiddle - xdreieck, ymiddle - ydreieck);

                    break;
                case 180:
                    shape[0] = new Point(xmiddle - halfsidelenght, ymiddle + ydreieck);
                    shape[1] = new Point(xmiddle + halfsidelenght, ymiddle + ydreieck);
                    shape[2] = new Point(xmiddle, ymiddle - xdreieck);
                    break;
                case 240:
                    shape[0] = new Point(xmiddle - ydreieck, ymiddle + xdreieck);
                    shape[1] = new Point(xmiddle - xdreieck, ymiddle - halfsidelenght);
                    shape[2] = new Point(xmiddle + xdreieck, ymiddle - ydreieck);
                    break;
                case 300:
                    shape[0] = new Point(xmiddle - halfsidelenght, ymiddle + xdreieck);
                    shape[1] = new Point(xmiddle - ydreieck, ymiddle - xdreieck);
                    shape[2] = new Point(xmiddle + xdreieck, ymiddle);
                    break;
            }
            //shape[0] = new Point(xmiddle, ymiddle + xdreieck);
            //shape[1] = new Point(xmiddle - sidelenght / 2, ymiddle - ydreieck);
            //shape[2] = new Point(xmiddle + sidelenght / 2, ymiddle - ydreieck);
            Pen pen = new(_cIO.BorderColor, 3);
            graphicIm.DrawPolygon(pen, shape);
            graphicIm.DrawString(pd.PanelId.ToString(), new Font("Arial", 10, FontStyle.Bold), Brushes.White, new PointF(pd.X, pd.Y));
        }
        public void DrawTriangle(NanoLeafJsonPositionData pd, Graphics graphicIm)
        {

            Point[] shape = new Point[3];
            var xmiddle = pd.X + _cIO.Border;
            var ymiddle = pd.Y + _cIO.Border;
            var sidelenght = pd.SideLenght;
            var halfsidelenght = sidelenght / 2;
            var h = (int)Math.Round(sidelenght * Math.Sqrt(3) / 2);
            var xdreieck = h / 3 * 2;
            var ydreieck = h / 3;
            switch (pd.Orientation)
            {
                case 0:
                case 360:
                case 120:
                case 240:
                    shape[0] = new Point(xmiddle, ymiddle + xdreieck);
                    shape[1] = new Point(xmiddle - halfsidelenght, ymiddle - ydreieck);
                    shape[2] = new Point(xmiddle + halfsidelenght, ymiddle - ydreieck);
                    break;
                case 180:
                case 60:
                case 300:
                    shape[0] = new Point(xmiddle - halfsidelenght, ymiddle + ydreieck);
                    shape[1] = new Point(xmiddle + halfsidelenght, ymiddle + ydreieck);
                    shape[2] = new Point(xmiddle, ymiddle - xdreieck);
                    break;
            }
            Pen pen = new(_cIO.BorderColor, 3);
            graphicIm.DrawPolygon(pen, shape);
            //graphicIm.DrawString(pd.PanelId.ToString(), new Font("Arial", 10, FontStyle.Bold), Brushes.White, new PointF(pd.X, pd.Y));
        }
        public void SaveImage(Bitmap image, NanoleafJsonPanelLayoutLayout pl, string imagename)
        {
            Graphics graphicIm = Graphics.FromImage(image);
            var liste = pl.PositionData.ToList();
            Pen pen = new(_cIO.BorderColor, 3);

            for (int i = 0; i < liste.Count; i++)
            {
                if (liste[i].ShapeType != ShapeType.Triangle && liste[i].ShapeType != ShapeType.HexagonShapes) continue;
                if (liste[i].ShapeType == ShapeType.HexagonShapes)
                {
                    DrawHexaGon(liste[i], graphicIm);
                }
                if (liste[i].ShapeType == ShapeType.Triangle)
                {
                    DrawTriangle(liste[i], graphicIm);
                }
                graphicIm.DrawEllipse(pen, liste[i].X + _cIO.Border, liste[i].Y + _cIO.Border, 7, 7);

                //graphicIm.DrawString("X", new Font("Arial", 10, FontStyle.Bold), Brushes.Red, RedxX[i], RedxY[i]);
            }
            Directory.CreateDirectory(_cIO.Path);
            image.Save(_cIO.Path + imagename + "." + _cIO.Extension, _cIO.Type);
            graphicIm.Dispose();
            image.Dispose();
        }
    }
}
