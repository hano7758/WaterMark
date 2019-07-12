using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace WaterMark
{
    class WaterMark
    {
        public static string AddWaterMark(string imgPath, string savePath)
        {
            var text = "仅供“XXXXXXXXXXXXXXXXXXXXX项目”使用";
            var imageName = Path.GetFileNameWithoutExtension(imgPath);
            var sImgPath = string.Format(@"{0}\{1}_Mark.jpg", savePath, imageName);
            using (Image image = Image.FromFile(imgPath))
            {
                try
                {
                    Bitmap bitmap = new Bitmap(image);
                    //图片的宽度与高度
                    int width = bitmap.Width, height = bitmap.Height;
                    //水印文字
                    Graphics g = Graphics.FromImage(bitmap);
                    g.DrawImage(bitmap, 0, 0);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
                    Font crFont = new Font("微软雅黑", 22, FontStyle.Bold);
                    SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(255, 137, 131, 131));
                    //将原点移动 到图片中点
                    g.TranslateTransform(-80, 150);
                    //以原点为中心 转 -45度
                    g.RotateTransform(-45);
                    for (int i = 0; i < 50; i++)
                    {
                        g.DrawString(text, crFont, semiTransBrush, new PointF(0 - i * 300, 100 + i * 300));
                        g.DrawString(text, crFont, semiTransBrush, new PointF(150 - i * 300, 500 + i * 300));
                        g.DrawString(text, crFont, semiTransBrush, new PointF(650 - i * 300, 600 + i * 300));
                    }
                    //保存文件
                    bitmap.Save(sImgPath, ImageFormat.Jpeg);
                    return sImgPath;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


    }
}