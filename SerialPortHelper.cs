using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

//导入注册表的命名空间
using Microsoft.Win32;

namespace GPRS_DTU
{
    public class SerialPortHelper
    {
        /// <summary>
        /// 可用串口扫描
        /// </summary>
        /// <returns>可用串口列表</returns>
        public static List<SerialPort> ScanSerialPort()
        {
            List<SerialPort> splist = new List<SerialPort>();

            //是否检测到端口
            bool flag = false;

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    //创建串口
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());

                    //打开端口的串口连接
                    sp.Open();

                    //关闭串口连接
                    sp.Close();

                    //添加到可用串口列表中
                    splist.Add(sp);

                    //设置标志
                    flag = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            if (!flag)
            {
                return null;
            }

            return splist;
        }

        /// <summary>
        /// 初始化信息
        /// </summary>
        /// <param name="sp"></param>
        public static void PortInit(SerialPort sp)
        {
            sp.BaudRate = 115200;
            sp.DataBits = 8;
            sp.StopBits = StopBits.One;
            sp.Parity = Parity.None;
        }

        /// <summary>
        /// 通过注册表获取串口列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetComListByRegistryKey()
        {
            List<string> list = null;

            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");

            //查找到相应的表项
            if (keyCom != null)
            {
                list = new List<string>();

                //获取注册表当前路径下的所有键名
                string[] sSubKeys = keyCom.GetValueNames();

                foreach (string sName in sSubKeys)
                {
                    //根据键名，查找系统中对应的端口值
                    list.Add((string)keyCom.GetValue(sName));
                }
            }

            return list;
        }

        /// <summary>
        /// 通过系统函数，获取系统的可用串口
        /// </summary>
        /// <returns></returns>
        public static string[] GetComListByApi()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static void OpenPort(SerialPort sp)
        {
            //根据串口状态打开或关闭
            if (sp.IsOpen)
            {
                //检测当前com口的状态,如果依然打开，就先关闭
                sp.Close();
            }

            sp.Open();
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static void ClosePort(SerialPort sp)
        {
            //设置端口名称
            sp.Close();
        }
    }
}
