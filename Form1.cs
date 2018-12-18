using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Machine_Speed
{
    public partial class Form1 : Form
    {
        private Queue<double> dataQueue0 = new Queue<double>(100);
        private int num = 5;   // 每次移动的单位数
        int y = 0;
        string str;

        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            CBPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serial_Data_Received);   // 反复调用
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (btnOpen.Text == "开启")
            {
                try
                {
                    serialPort1.PortName = CBPort.Text;
                    serialPort1.BaudRate = Convert.ToInt32(CBBaud.Text);
                    serialPort1.Open();
                    btnOpen.Text = "关闭";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                serialPort1.Close();
                btnOpen.Text = "开启";
            }
        }

        #region 图表初始化

        private void InitChart()
        {
            this.chart1.ChartAreas.Clear();
            ChartArea chartArea0 = new ChartArea("Chart_1");
            this.chart1.ChartAreas.Add(chartArea0);

            this.chart1.Series.Clear();
            Series series0 = new Series("Series_0");
            series0.ChartArea = "Chart_1";
            this.chart1.Series.Add(series0);

            this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            this.chart1.ChartAreas[0].AxisY.Maximum = 12000;
            this.chart1.ChartAreas[0].AxisX.Interval = 5;
            this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Black;
            this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Black;

            this.chart1.Titles.Clear();
            this.chart1.Titles.Add("series0");
            this.chart1.Titles[0].Text = "电机转速";
            this.chart1.Titles[0].ForeColor = Color.Blue;
            this.chart1.Titles[0].Font = new System.Drawing.Font("Consolas", 12F);

            this.chart1.Series[0].Color = Color.Red;
            this.chart1.Series[0].Name = "电机转速 r / s";
            this.chart1.Series[0].ChartType = SeriesChartType.Spline;
            this.chart1.Series[0].Points.Clear();
        }

        #endregion

        #region 队列的设置

        private void updateQueueValue()
        {
            int i;

            if (dataQueue0.Count > 95)
            {
                for (i = 0; i < num; i++)
                {
                    dataQueue0.Dequeue();
                }
            }

            for (i = 0; i < num; i++)
            {
                dataQueue0.Enqueue(y);
            }
        }



        #endregion

        #region 绘图开关

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.timer1.Start();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
        }

        #endregion

        #region 数据处理

        private void serial_Data_Received(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] temp = new byte[3];
                int data_temp;
                try
                {
                    while (serialPort1.IsOpen)
                    {
                        data_temp = serialPort1.ReadByte();
                        if (data_temp == 0xAA)
                        {
                            serialPort1.Read(temp, 0, 3); // AA data0 data1 55 在判断过程中已经被读取，然后释放，读取的三个数据为有效数据的高低两位和帧尾
                            y = temp[0] * 256 + temp[1];
                            textBox1.AppendText(y.ToString() + " ");
                        }
                    }
                }
                catch { }
            }
            catch{ }
        }

        #endregion

        private void btnInit_Click(object sender, EventArgs e)
        {
            InitChart();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            updateQueueValue();
            this.chart1.Series[0].Points.Clear();

            for (int i = 0; i < dataQueue0.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY((i + 1), dataQueue0.ElementAt(i));
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
