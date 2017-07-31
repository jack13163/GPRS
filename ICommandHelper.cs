using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace GPRS_DTU
{
    /// <summary>
    /// GPRS的AT指令执行类
    /// </summary>
    public interface ICommandHelper
    {
        /// <summary>
        /// 发送AT命令
        /// </summary>
        string ATCommand(SerialPort sp, string ATCmd);
        void SendEnd(SerialPort sp);

        /// <summary>
        /// 发送短息
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="mobile"></param>
        /// <param name="CenterNum"></param>
        /// <param name="msgTxt"></param>
        /// <returns></returns>
        string SendToCom(SerialPort sp, string mobile, string CenterNum, string msgTxt);
    }
}
