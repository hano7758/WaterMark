using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace WaterMark
{
    public class WaterMark
    {
        /// <summary>
        /// //水印图片保存地址
        /// </summary>
        private static string _wmImgSavePath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sWaterMarkContent">水印文字</param>
        /// <param name="imgPath">原图片地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="projectNo">项目编号</param>
        /// <returns></returns>
        public static string AddWaterMark(string sWaterMarkContent, string imgPath, string savePath, string projectNo)
        {
            string text = string.Format("仅供“{0}”使用", sWaterMarkContent);
            _wmImgSavePath = Path.Combine(savePath, projectNo + ".jpg");
            var imageName = Path.GetFileNameWithoutExtension(imgPath);
            //判断原图片是否有已转好的水印图片，有则直接返回不用再加水印
            if (Directory.Exists(savePath))
            {
                string[] filedir = Directory.GetFiles(savePath, imageName + "_Mark.jpg", SearchOption.AllDirectories);
                if (filedir.Length > 0)
                    return filedir[0];
            }
            else
            {
                Directory.CreateDirectory(savePath);
            }
            var sImgPath = string.Format(@"{0}\{1}_Mark.jpg", savePath, imageName);
            //载入原始图片
            //Image imgSrc = Image.FromFile(imgPath);

            using (var imgSrc = Image.FromFile(imgPath))
            {
                //根据拍摄方向旋转图片方向
                var exif = imgSrc.PropertyItems;
                byte orien = 0;
                var item = exif.Where(m => m.Id == 274).ToArray();
                if (item.Length > 0)
                    orien = item[0].Value[0];
                switch (orien)
                {
                    case 2:
                        imgSrc.RotateFlip(RotateFlipType.RotateNoneFlipX);//horizontal flip
                        break;
                    case 3:
                        imgSrc.RotateFlip(RotateFlipType.Rotate180FlipNone);//right-top
                        break;
                    case 4:
                        imgSrc.RotateFlip(RotateFlipType.RotateNoneFlipY);//vertical flip
                        break;
                    case 5:
                        imgSrc.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        imgSrc.RotateFlip(RotateFlipType.Rotate90FlipNone);//right-top
                        break;
                    case 7:
                        imgSrc.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        imgSrc.RotateFlip(RotateFlipType.Rotate270FlipNone);//left-bottom
                        break;
                    default:
                        break;
                }

                //新建原始普通大小的bmp
                Bitmap bmCanvas = new Bitmap(imgSrc.Width, imgSrc.Height, PixelFormat.Format24bppRgb);
                Graphics gCanvas = Graphics.FromImage(bmCanvas);
                gCanvas.Clear(Color.White);
                gCanvas.SmoothingMode = SmoothingMode.HighQuality;
                gCanvas.InterpolationMode = InterpolationMode.High;
                //将原始图片加载入画布
                gCanvas.DrawImage(imgSrc, 0, 0, imgSrc.Width, imgSrc.Height);
                //加入文字水印
                AddWatermarkText(gCanvas, text, imgSrc.Width, imgSrc.Height);
                bmCanvas.Save(sImgPath, ImageFormat.Jpeg);
                //释放资源
                bmCanvas.Dispose();
                imgSrc.Dispose();
                return sImgPath;
            }
        }

        /// <summary>
        /// 在图片中添加文字水印
        /// </summary>
        /// <param name="gSrcCanvas"></param>
        /// <param name="watermarkText"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static void AddWatermarkText(Graphics gSrcCanvas, string watermarkText, int width, int height)
        {
            //计算图片对角线长度
            double diagonal = Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));

            //计算对角线倾角
            double angle = Math.Asin(height / Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2))) / Math.PI * 180;

            // 确定水印文字的字体大小
            int[] sizes = new int[]
            {
                280, 276, 272, 268, 264, 260, 256, 252, 248, 244, 240, 236, 232, 228, 224, 220, 216, 212, 208,
                204, 200, 196, 192, 188, 184, 180, 176, 172, 168, 164, 160, 156, 152, 148, 144, 140, 136, 132,
                128, 124, 120, 116, 112, 108, 104, 100, 96, 92, 88, 84, 80, 76, 72, 68, 64, 60, 56, 52, 48, 44,
                40, 36, 32, 28, 24, 20, 16, 12, 8, 4
            };
            Font crFont = null;
            SizeF crSize = new SizeF();

            for (int i = 0; i < sizes.Length; i++)
            {
                crFont = new Font("微软雅黑", sizes[i], FontStyle.Bold);
                crSize = gSrcCanvas.MeasureString(watermarkText, crFont);
                if ((int)crSize.Width < (int)diagonal * 0.9)
                {
                    break;
                }
            }
            // 生成水印图片（将文字写到图片中）
            //Bitmap bmWaterMark = new Bitmap((int)crSize.Width + 3, (int)crSize.Height + 3, PixelFormat.Format32bppArgb);
            Bitmap bmWaterMark = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics gWaterMark = Graphics.FromImage(bmWaterMark);

            gWaterMark.TranslateTransform(width / 2, height / 2);
            //文字倾斜角度根据实际图片对角线长度计算
            gWaterMark.RotateTransform(-(int)angle);
            gWaterMark.TranslateTransform(-crSize.Width / 2, -crSize.Height / 2);

            PointF pt = new PointF(0, 0);
            // 画阴影文字
            Brush transparentBrush0 = new SolidBrush(Color.FromArgb(255, Color.Black));
            Brush transparentBrush1 = new SolidBrush(Color.FromArgb(255, Color.Black));
            gWaterMark.DrawString(watermarkText, crFont, transparentBrush0, pt.X, pt.Y + 1);
            gWaterMark.DrawString(watermarkText, crFont, transparentBrush0, pt.X + 1, pt.Y);
            gWaterMark.DrawString(watermarkText, crFont, transparentBrush1, pt.X + 1, pt.Y + 1);
            gWaterMark.DrawString(watermarkText, crFont, transparentBrush1, pt.X, pt.Y + 2);
            gWaterMark.DrawString(watermarkText, crFont, transparentBrush1, pt.X + 2, pt.Y);
            transparentBrush0.Dispose();
            transparentBrush1.Dispose();

            // 画文字
            gWaterMark.SmoothingMode = SmoothingMode.HighQuality;
            //Brush SolidBrush3 = new SolidBrush(Color.White);
            Brush solidBrush3 = new SolidBrush(Color.FromArgb(255, Color.White));
            gWaterMark.DrawString(watermarkText, crFont, solidBrush3, pt.X, pt.Y, StringFormat.GenericDefault);
            solidBrush3.Dispose();

            // 保存刚才的操作
            gWaterMark.Save();
            gWaterMark.Dispose();
            bmWaterMark.Save(_wmImgSavePath, ImageFormat.Jpeg);

            //// 将水印图片加到原图中
            //AddWatermarkImage(gSrcCanvas, new Bitmap(bmWaterMark), "WM_TOP_LEFT", width, height);

            using (var imageAttr = new ImageAttributes())
            {
                ColorMap colorMap = new ColorMap();
                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                ColorMap[] remapTable = { colorMap };
                imageAttr.SetRemapTable(remapTable, ColorAdjustType.Bitmap);
                float[][] colorMatrixElements =
                {
                    new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                    new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                    new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                    new float[] {0.0f, 0.0f, 0.0f, 0.3f, 0.0f},
                    new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                };
                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                imageAttr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                gSrcCanvas.DrawImage(bmWaterMark, new Rectangle(10, 10, bmWaterMark.Width, bmWaterMark.Height), 0, 0,
                    bmWaterMark.Width, bmWaterMark.Height, GraphicsUnit.Pixel, imageAttr);
                gSrcCanvas.Dispose();
            }
            bmWaterMark.Dispose();
        }

        /// <summary>
        /// 将水印图片叠加到原先的图片中
        /// </summary>
        /// <param name="gSrcCanvas"></param>
        /// <param name="imgWatermark"></param>
        /// <param name="watermarkPosition"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static void AddWatermarkImage(Graphics gSrcCanvas, Image imgWatermark, string watermarkPosition, int width, int height)
        {
            Image watermark = new Bitmap(imgWatermark);
            ImageAttributes imageAttr = new ImageAttributes();
            ColorMap colorMap = new ColorMap();
            colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
            colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
            ColorMap[] remapTable = { colorMap };
            imageAttr.SetRemapTable(remapTable, ColorAdjustType.Bitmap);
            float[][] colorMatrixElements =
            {
                    new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                    new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                    new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                    new float[] {0.0f, 0.0f, 0.0f, 0.3f, 0.0f},
                    new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                };
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            int xpos = 0;
            int ypos = 0;
            int watermarkWidth = watermark.Width;
            int watermarkHeight = watermark.Height;
            switch (watermarkPosition)
            {
                case "WM_TOP_LEFT":
                    xpos = 10;
                    ypos = 10;
                    break;
                case "WM_TOP_RIGHT":
                    xpos = width - watermarkWidth - 10;
                    ypos = 10;
                    break;
                case "WM_BOTTOM_RIGHT":
                    xpos = width - watermarkWidth - 10;
                    ypos = height - watermarkHeight - 10;
                    break;
                case "WM_BOTTOM_LEFT":
                    xpos = 10;
                    ypos = height - watermarkHeight - 10;
                    break;
            }
            gSrcCanvas.DrawImage(watermark, new Rectangle(xpos, ypos, watermarkWidth, watermarkHeight), 0, 0,
                watermark.Width, watermark.Height, GraphicsUnit.Pixel, imageAttr);
            gSrcCanvas.Dispose();
            watermark.Dispose();
            imageAttr.Dispose();
        }
    }
}