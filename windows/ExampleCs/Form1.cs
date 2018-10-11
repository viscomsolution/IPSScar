﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IPSS;

namespace Example
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //khởi tạo đối tượng
            IPSScar detector = new IPSScar();

            //CÁC OPTION

            //đường dẫn chứa file output, nếu set null hoặc rỗng thì sẽ không save ảnh output
            detector.OutputFoler = @"D:\Result";
           

            //engine nhận tham số là ảnh hoặc đường dẫn đến ảnh
            CarPlate result = detector.ReadPlate(@"Test98.JPG");

            //kết quả trả về là class IPSSresult gồm nhiều thuộc tính
            //text là các ký tự biển số
            label1.Text = result.text;

            //bitmap là hình ảnh kết quả
            Bitmap bmp = result.bitmap;
            pictureBox1.Image = bmp;

            //thời gian xử lý
            int ms = result.elapsedMilisecond;

            //mã lỗi (nếu có)
            string error = result.error;

            //nếu có biển số giá trị là true
            bool hasPlate = result.hasPlate;

            //nếu đọc đủ các ký tự thì kết quả là true
            bool isValid = result.isValid;

            //biển số vuông hay biển số dài            
            label2.Text = result.type.ToString();
        }
    }
}
