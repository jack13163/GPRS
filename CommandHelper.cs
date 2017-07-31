using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace GPRS_DTU
{
    public class CommandHelper : ICommandHelper
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="CenterNum"></param>
        /// <param name="msgTxt"></param>
        /// <returns></returns>
        public string SendToCom(SerialPort sp, string mobile, string CenterNum, string msgTxt)//发送信息 
        {
            try
            {
                sp.Write("AT+CSCA=+86" + CenterNum + ";&W" + "\r");
                Thread.Sleep(200);
                sp.Write("AT+CMGF=0" + "\r");
                Thread.Sleep(200);
                byte[] buffer = new byte[sp.BytesToRead];

                return System.Text.Encoding.ASCII.GetString(buffer);
            }
            catch
            {
                return "发送失败！";
            }
        }

        /// <summary>
        /// 发送AT指令
        /// </summary>
        /// <param name="ATCmd"></param>
        /// <returns></returns>
        public string ATCommand(SerialPort sp, string ATCmd)//发送AT指令 
        {
            string result = "";
            try
            {
                sp.Write(ATCmd);

                if (ATCmd.Contains("AT+CIPSTART=\"TCP\",\"b17664507c.51mypc.cn\",\"29437\""))
                {
                    Thread.Sleep(6000);
                }
                else
                {
                    Thread.Sleep(500);
                }

                byte[] buffer = new byte[sp.BytesToRead];
                sp.Read(buffer, 0, buffer.Length);

                result = System.Text.Encoding.UTF8.GetString(buffer);

                //更新窗体
                MainFrm.update(result);

                return result;
            }
            catch (Exception ex)
            {
                return "发送AT指令失败！:" + ex.Message;
            }
        }

        /// <summary>
        /// 发送字节数据
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="ATCmd"></param>
        /// <returns></returns>
        public void SendEnd(SerialPort sp)
        {
            try
            {
                byte[] bs = new byte[1];
                bs[0] = 26;

                sp.Write(bs, 0, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("失败！:" + ex.Message);
            }
        }
    }
}
