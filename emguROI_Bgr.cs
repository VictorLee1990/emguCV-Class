using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;

namespace UDE_MachineVision
{
    public class emguROI_Bgr
    {
        #region 定義參數
        //公用的參數
        /// <summary>
        /// 原始資料
        /// </summary>
        public Image<Bgr, Byte> eROI_data = null; // ROI_原始資料
        /// <summary>
        /// 左上X座標
        /// </summary>
        public int OrgX; // 左上X座標
        /// <summary>
        /// 左上Y座標
        /// </summary>
        public int OrgY; // 左上Y座標
        /// <summary>
        /// ROI寬
        /// </summary>
        public int Width; // ROI寬
        /// <summary>
        /// ROI高
        /// </summary>
        public int Height; // ROI高

        //私有的參數
        private Rectangle eROI = Rectangle.Empty; // ROI座標(x,y,w,h)
        private Image<Bgr, Byte> eROI_parent = null; // ROI_對應的父Image
        private int mX, mY; // 滑鼠點下去的座標紀錄,用於改變ROI大小或是拖曳ROI
        private int Corner_Index = 0; // ROI第幾個角落
        private bool handle = false; // 是否有點擊到ROI
        private bool handle_cornor = false; // 是否有點擊到角落
        #endregion
        
        /// <summary>
        /// 建構子
        /// </summary>
        public emguROI_Bgr() // emguROI格式
        {
            eROI = new Rectangle();
        }
        /// <summary>
        /// 設定ROI座標及尺寸,座標是左上的點
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="w">ROI寬</param>
        /// <param name="h">ROI高</param>
        public void SetPlacement(int x, int y, int w, int h) // 設定ROI位置
        {
            eROI.X = x;
            eROI.Y = y;
            eROI.Width = w;
            eROI.Height = h;
            OrgX = x;
            OrgY = y;
            Width = w;
            Height = h;

            if (eROI_parent == null) { return; }
            eROI_data = new Image<Bgr, Byte>(eROI_parent.Bitmap);
            eROI_data.ROI = eROI;
        }
        /// <summary>
        /// 將ROI依附在Image上
        /// </summary>
        /// <param name="srcImage">被依附的Image</param>
        public void Attach(ref Image<Bgr, Byte> srcImage)
        {
            eROI_parent = srcImage;
        }
        /// <summary>
        /// 提供介面拖曳及改變ROI尺寸的方法
        /// </summary>
        /// <param name="eX">滑鼠X座標</param>
        /// <param name="eY">滑鼠Y座標</param>
        /// <param name="zoom">顯示縮放比例</param>
        public void Drag(int eX, int eY, double zoom)
        {
            if (handle == false) { return; }

            double moveX, moveY;
            moveX = (double)(eX - mX) / zoom;
            moveY = (double)(eY - mY) / zoom;

            if (handle_cornor == false)
            {
                if (Corner_Index != 0)
                {
                    switch (Corner_Index) // 1左上 2左下 3右上 4右下 5上 6左 7右 8下
                    {
                        case 1:
                            eROI.X = OrgX + (int)moveX;
                            eROI.Y = OrgY + (int)moveY;
                            eROI.Width = Width - (int)moveX;
                            eROI.Height = Height - (int)moveY;
                            break;
                        case 2:
                            eROI.X = OrgX + (int)moveX;
                            eROI.Width = Width - (int)moveX;
                            eROI.Height = Height + (int)moveY;
                            break;
                        case 3:
                            eROI.Y = OrgY + (int)moveY;
                            eROI.Width = Width + (int)moveX;
                            eROI.Height = Height - (int)moveY;
                            break;
                        case 4:
                            eROI.Width = Width + (int)moveX;
                            eROI.Height = Height + (int)moveY;
                            break;
                        case 5:
                            eROI.Y = OrgY + (int)moveY;
                            eROI.Height = Height - (int)moveY;
                            break;
                        case 6:
                            eROI.X = OrgX + (int)moveX;
                            eROI.Width = Width - (int)moveX;
                            break;
                        case 7:
                            eROI.Width = Width + (int)moveX;
                            break;
                        case 8:
                            eROI.Height = Height + (int)moveY;
                            break;
                        default:
                            break;
                    }


                }
                else
                {
                    eROI.X = OrgX + (int)moveX;
                    eROI.Y = OrgY + (int)moveY;
                }
            }
            if (handle_cornor == true)
            {
                switch (Corner_Index) // 1左上 2左下 3右上 4右下 5上 6左 7右 8下
                {
                    case 1:
                        eROI.X = OrgX + (int)moveX;
                        eROI.Y = OrgY + (int)moveY;
                        eROI.Width = Width - (int)moveX;
                        eROI.Height = Height - (int)moveY;
                        break;
                    case 2:
                        eROI.X = OrgX + (int)moveX;
                        eROI.Width = Width - (int)moveX;
                        eROI.Height = Height + (int)moveY;
                        break;
                    case 3:
                        eROI.Y = OrgY + (int)moveY;
                        eROI.Width = Width + (int)moveX;
                        eROI.Height = Height - (int)moveY;
                        break;
                    case 4:
                        eROI.Width = Width + (int)moveX;
                        eROI.Height = Height + (int)moveY;
                        break;
                    case 5:
                        eROI.Y = OrgY + (int)moveY;
                        eROI.Height = Height - (int)moveY;
                        break;
                    case 6:
                        eROI.X = OrgX + (int)moveX;
                        eROI.Width = Width - (int)moveX;
                        break;
                    case 7:
                        eROI.Width = Width + (int)moveX;
                        break;
                    case 8:
                        eROI.Height = Height + (int)moveY;
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 提供介面偵測ROI是否有被滑鼠點擊的方法
        /// </summary>
        /// <param name="eX">滑鼠X座標</param>
        /// <param name="eY">滑鼠Y座標</param>
        /// <param name="zoom">顯示縮放比例</param>
        /// <returns>回傳是否有被點擊,true=有</returns>
        public bool HitTest(int eX, int eY, double zoom)
        {
            Corner_Index = HitCorner(eX, eY, zoom);
            if ((double)eX / zoom > eROI.X - 10 && (double)eX / zoom < eROI.X + eROI.Width + 10 && (double)eY / zoom > eROI.Y - 10 && (double)eY / zoom < eROI.Y + eROI.Height + 10)
            {
                mX = eX;
                mY = eY;
                handle = true;
                return true;
            }
            else
            {
                handle = false;
                return false;
            }
        }
        /// <summary>
        /// 偵測ROI八個邊界角是否有被滑鼠點擊的方法,私有的,與HitTest共存
        /// </summary>
        /// <param name="eX">滑鼠X座標</param>
        /// <param name="eY">滑鼠Y座標</param>
        /// <param name="zoom">顯示縮放比例</param>
        /// <returns>回傳是否有被點擊,true=有</returns>
        private int HitCorner(int eX, int eY, double zoom)
        {
            int helfW, helfH;
            helfW = (int)((double)eROI.Width / 2.0);
            helfH = (int)((double)eROI.Height / 2.0);
            if (handle_cornor == true)
                return Corner_Index;

            if ((double)eX / zoom > eROI.X - 5.0 / zoom && (double)eX / zoom < eROI.X + 5.0 / zoom && (double)eY / zoom > eROI.Y - 5.0 / zoom && (double)eY / zoom < eROI.Y + 5.0 / zoom) // 左上
            {
                handle_cornor = true;
                Corner_Index = 1;
                return 1;
            }
            else if ((double)eX / zoom > eROI.X - 5.0 / zoom && (double)eX / zoom < eROI.X + 5.0 / zoom && (double)eY / zoom > eROI.Y + eROI.Height - 5.0 / zoom && (double)eY / zoom < eROI.Y + eROI.Height + 5.0 / zoom) // 左下
            {
                handle_cornor = true;
                Corner_Index = 2;
                return 2;
            }
            else if ((double)eX / zoom > eROI.X + eROI.Width - 5.0 / zoom && (double)eX / zoom < eROI.X + eROI.Width + 5.0 / zoom && (double)eY / zoom > eROI.Y - 5.0 / zoom && (double)eY / zoom < eROI.Y + 5.0 / zoom) // 右上
            {
                handle_cornor = true;
                Corner_Index = 3;
                return 3;
            }
            else if ((double)eX / zoom > eROI.X + eROI.Width - 5.0 / zoom && (double)eX / zoom < eROI.X + eROI.Width + 5.0 / zoom && (double)eY / zoom > eROI.Y + eROI.Height - 5.0 / zoom && (double)eY / zoom < eROI.Y + eROI.Height + 5.0 / zoom) // 右下
            {
                handle_cornor = true;
                Corner_Index = 4;
                return 4;
            }
            else if ((double)eX / zoom > eROI.X + helfW - 5.0 / zoom && (double)eX / zoom < eROI.X + helfW + 5.0 / zoom && (double)eY / zoom > eROI.Y - 5.0 / zoom && (double)eY / zoom < eROI.Y + 5.0 / zoom) // 上
            {
                handle_cornor = true;
                Corner_Index = 5;
                return 5;
            }
            else if ((double)eX / zoom > eROI.X - 5.0 / zoom && (double)eX / zoom < eROI.X + 5.0 / zoom && (double)eY / zoom > eROI.Y + helfH - 5.0 / zoom && (double)eY / zoom < eROI.Y + helfH + 5.0 / zoom) // 左
            {
                handle_cornor = true;
                Corner_Index = 6;
                return 6;
            }
            else if ((double)eX / zoom > eROI.X + eROI.Width - 5.0 / zoom && (double)eX / zoom < eROI.X + eROI.Width + 5.0 / zoom && (double)eY / zoom > eROI.Y + helfH - 5.0 / zoom && (double)eY / zoom < eROI.Y + helfH + 5.0 / zoom) // 右
            {
                handle_cornor = true;
                Corner_Index = 7;
                return 7;
            }
            else if ((double)eX / zoom > eROI.X + helfW - 5.0 / zoom && (double)eX / zoom < eROI.X + helfW + 5.0 / zoom && (double)eY / zoom > eROI.Y + eROI.Height - 5.0 / zoom && (double)eY / zoom < eROI.Y + eROI.Height + 5.0 / zoom) // 下
            {
                handle_cornor = true;
                Corner_Index = 8;
                return 8;
            }
            else
            {
                handle_cornor = false;
                return 0;
            }
        }
        /// <summary>
        /// 提供介面再滑鼠放開ROI時使用,將拖曳或縮放的ROI資訊存入此Class
        /// </summary>
        public void MouseUp()
        {
            if (eROI_parent == null) { return; }
            if (eROI.Width < 50 || eROI.Height < 50)
            {
                eROI.Width = 55;
                eROI.Height = 55;
                return;
            }

            handle = false;
            handle_cornor = false;
            OrgX = eROI.X;
            OrgY = eROI.Y;
            Width = eROI.Width;
            Height = eROI.Height;

            eROI_data = new Image<Bgr, Byte>(eROI_parent.Bitmap);
            eROI_data.ROI = eROI;
        }
        /// <summary>
        /// 提供介面畫ROI框的方法
        /// </summary>
        /// <param name="g">繪圖介面物件</param>
        /// <param name="pen">繪圖筆</param>
        /// <param name="handles">是否顯示ROI八個邊界方框</param>
        /// <param name="zoom">顯示縮放比例</param>
        public void DrawFrame(Graphics g, Pen pen, bool handles, double zoom)
        {
            int x, y, w, h;
            x = (int)((double)eROI.X * zoom);
            y = (int)((double)eROI.Y * zoom);
            w = (int)((double)eROI.Width * zoom);
            h = (int)((double)eROI.Height * zoom);
            g.DrawRectangle(pen, x, y, w, h);
            if (handles == true) // 八個邊界方框，大小為10*10
            {
                int helfW, helfH;
                helfW = (int)((double)w / 2.0);
                helfH = (int)((double)h / 2.0);
                g.DrawRectangle(pen, x - 5, y - 5, 10, 10);
                g.DrawRectangle(pen, x - 5, y + helfH - 5, 10, 10);
                g.DrawRectangle(pen, x - 5, y + h - 5, 10, 10);
                g.DrawRectangle(pen, x + helfW - 5, y - 5, 10, 10);
                g.DrawRectangle(pen, x + helfW - 5, y + h - 5, 10, 10);
                g.DrawRectangle(pen, x + w - 5, y - 5, 10, 10);
                g.DrawRectangle(pen, x + w - 5, y + helfH - 5, 10, 10);
                g.DrawRectangle(pen, x + w - 5, y + h - 5, 10, 10);
            }
        }
        /// <summary>
        /// 讀取指定路徑的圖片
        /// </summary>
        /// <param name="path">指定路徑</param>
        public void Load(string path)
        {
            eROI_data = new Image<Bgr, Byte>(path);
        }
        /// <summary>
        /// 存檔圖片至指定路徑
        /// </summary>
        /// <param name="path">指定路徑</param>
        public void Save(string path)
        {
            if (eROI_parent == null) { return; }
            eROI_data = new Image<Bgr, Byte>(eROI_parent.Bitmap);
            eROI_data.ROI = eROI;
            eROI_data.Save(path);
        }
    }
}
