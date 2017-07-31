using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;


namespace GPRS_DTU
{
    //异步更新窗体
    public delegate void UpdateText(string content);

    public partial class MainFrm : Form
    {
        //可用串口信息表
        private List<SerialPort> list = new List<SerialPort>();

        //对外访问
        public static UpdateText update = null;

        //更新控件
        public void Update(string content)
        {
            this.txtContentArea.Text += content;
            //光标重定位
            this.txtContentArea.Select(this.txtContentArea.TextLength, 0);
            this.txtContentArea.ScrollToCaret();
        }

        public MainFrm()
        {
            InitializeComponent();

            //绑定下拉框数据
            bindToCombobox();

            //注册监听
            update = new UpdateText(Update);

            this.btnStartGPRS.Enabled = false;

            this.lblNotify.Visible = false;
        }

        /// <summary>
        /// 当程序加载时，设置串口信息
        /// </summary>
        private void LoadPortInfo(string[] ports)
        {
            //初始化信息
            foreach (var item in ports)
            {
                //新建串口对象
                SerialPort sp = new SerialPort();

                //设置串口信息
                sp.PortName = item;

                //添加到串口信息表中
                list.Add(sp);
            }

        }

        /// <summary>
        /// 查找可用的串口，并将可用信息绑定到下拉列表框
        /// </summary>
        private void bindToCombobox()
        {
            string[] ps = SerialPortHelper.GetComListByApi();

            //获取串口信息
            LoadPortInfo(ps);

            //绑定到控件
            this.cbReadyPortList.DataSource = ps;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenPort_Click(object sender, EventArgs e)
        {
            //获取当前串口信息
            SerialPort sp = GetCurrentPort();

            //设置端口名称
            try
            {
                //根据串口状态打开或关闭
                if (!sp.IsOpen)
                {
                    //打开串口
                    SerialPortHelper.OpenPort(sp);

                    this.btnOpenPort.Text = "关闭串口";

                    this.btnFlag.BackColor = Color.Red;
                    this.btnStartGPRS.Enabled = true;
                }
                else
                {
                    //关闭串口
                    SerialPortHelper.ClosePort(sp);

                    this.btnOpenPort.Text = "打开串口";

                    this.btnFlag.BackColor = Color.Green;
                    this.btnGPRSFlag.BackColor = Color.Green;
                    this.btnStartGPRS.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("串口操作失败：" + ex.Message);
            }

        }

        /// <summary>
        /// 串口选择变化时，自动查询串口状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbReadyPortList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //获取当前串口信息
            SerialPort sp = GetCurrentPort();

            //根据串口状态打开或关闭
            if (!sp.IsOpen)
            {
                this.btnOpenPort.Text = "打开串口";
                this.btnFlag.BackColor = Color.Green;
                this.btnGPRSFlag.BackColor = Color.Green;
            }
            else
            {
                this.btnOpenPort.Text = "关闭串口";
                this.btnFlag.BackColor = Color.Red;
            }
        }

        /// <summary>
        /// 发送GPRS数据或者命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            ICommandHelper helper = new CommandHelper();

            //获取当前串口
            SerialPort sp = GetCurrentPort();

            if (!sp.IsOpen)
            {
                MessageBox.Show("请先打开对应的串口！");
                return;
            }
            //获取内容
            string content = this.txtToBeSended.Text;

            if (content.Length > 0)
            {
                //判断是否发送GPRS数据
                if (this.btnGPRSFlag.BackColor == Color.Red)
                {
                    //发送GPRS消息
                    helper.ATCommand(sp, "AT+CIPSEND\r");

                    helper.ATCommand(sp, content);

                    helper.SendEnd(sp);
                }
                else if (content.ToUpper().StartsWith("AT"))
                {
                    //发送AT命令
                    helper.ATCommand(sp, content + "\r");
                }
                else
                {
                    //发送失败
                    MessageBox.Show("AT命令发送失败！格式错误，必须以AT开头！");
                    return;
                }
            }
            else
            {
                MessageBox.Show("发送内容不允许为空！");
                return;
            }
        }

        /// <summary>
        /// 返回当前下拉列表选中的串口的信息
        /// </summary>
        /// <returns></returns>
        private SerialPort GetCurrentPort()
        {
            //获取端口名称
            string pname = this.cbReadyPortList.SelectedValue.ToString();
            //MessageBox.Show(pname);

            //查找当前的串口信息表中的信息，获取当前串口信息
            SerialPort sp = list.Where(item => item.PortName.Equals(pname)).FirstOrDefault();

            return sp;
        }

        /// <summary>
        /// 打开GPRS连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartGPRS_Click(object sender, EventArgs e)
        {
            Task t = new Task(Run);

            t.Start();

            this.lblNotify.Visible = true;
        }

        private void Run()
        {
            ICommandHelper helper = new CommandHelper();

            SerialPort sp = GetCurrentPort();
            if (this.btnGPRSFlag.BackColor != Color.Red)
            {
                //获取需要执行的AT指令
                string[] cmd = new string[] { 
                    "AT+CIPSHUT",
                    "AT", "AT+CGCLASS=\"B\"", 
                    "AT+CIPHEAD=0",
                    "AT+CGDCONT=1,\"IP\",\"uninet\"", "AT+CGATT=1" ,
                    "AT+CLPORT=\"TCP\",\"2000\"",
                    "AT+CIPSTART=\"TCP\",\"b17664507c.51mypc.cn\",\"29437\""
                };
                
                //判断是否加入回车符号
                for (int i = 0; i < cmd.Length; i++)
                {
                    //发送
                    helper.ATCommand(sp, cmd[i] + "\r");
                }

                //设置信号灯
                this.btnGPRSFlag.BackColor = Color.Red;
                this.btnStartGPRS.Text = "关闭GPRS";
            }
            else
            {
                helper.ATCommand(sp, "AT+CIPCLOSE\r");
                this.btnGPRSFlag.BackColor = Color.Green;
                this.btnStartGPRS.Text = "打开GPRS";
            }
        }

        /// <summary>
        /// 快捷发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtToBeSended_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                btnSend_Click(null, null);
            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SerialPort sp = GetCurrentPort();
            ICommandHelper helper = new CommandHelper();

            try
            {
                helper.ATCommand(sp, "AT+CIPCLOSE\r");

                sp.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("窗口关闭失败：" + ex.Message);
            }
        }
    }
}
