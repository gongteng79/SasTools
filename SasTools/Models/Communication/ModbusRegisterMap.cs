
namespace SasTools.Models.Communication
{
    // Modbus寄存器地址映射表
    public static class ModbusRegisterMap
    {
        #region 测试地址
        //协议地址偏移检测
        public const string PROTOCOL_OFFSET = "4";
        //16位整数测试 (期望值: 258)
        public const string TEST_16BIT = "7";
        //32位整数测试 (期望值: 16909060)
        public const string TEST_32BIT = "8";
        //2位浮点测试 (期望值: 12.34)
        public const string TEST_FLOAT = "10";
        #endregion

        #region 锁螺丝控制地址
        //清零产品计数
        public const string CLEAR_PRODUCT_COUNT = "100";
        //清零当前规格螺丝锁付计数
        public const string CLEAR_SCREW_COUNT = "101";
        //放行螺丝+1
        public const string PASS_SCREW_PLUS = "102";
        //放行螺丝-1
        public const string PASS_SCREW_MINUS = "103";
        //扭力数据采样周期(ms)
        public const string TORQUE_SAMPLE_PERIOD = "104";   
        //启动锁付
        public const string START_LOCK = "105";
        //停止锁付
        public const string STOP_LOCK = "106";
        //清除缓存中螺丝锁付数据
        public const string CLEAR_LOCK_DATA = "107";
        //螺丝锁付数据读取模式
        public const string LOCK_DATA_MODE = "108";
        #endregion

        #region 产品和螺丝配置
        //当前产品编号 (0~7)
        public const string CURRENT_PRODUCT_NUMBER = "120";
        //当前产品中不同规格螺丝顺序编号 (0~7)
        public const string CURRENT_PRODUCT_SCREW_ORDER = "121";
        //当前规格螺丝编号 (0~7)
        public const string CURRENT_SCREW_SPEC = "122";
        //当前规格螺丝显示扭力单位
        public const string CURRENT_SCREW_TORQUE_UNIT = "123";
        //当前规格螺丝锁付计数显示模式
        public const string CURRENT_SCREW_COUNT_MODE = "124";
        #endregion

        #region 状态读取地址
        //当前电批转速(rpm)
        public const string CURRENT_SPEED = "176";
        //当前输入口状态
        public const string INPUT_STATUS = "178";
        //当前输出口状态
        public const string OUTPUT_STATUS = "179";
        //电批通电允许控制 (0:禁止, 1:允许)
        public const string POWER_ENABLE = "180";
        //切换缓存中下一颗螺丝锁付数据读取
        public const string SWITCH_LOCK_DATA = "181";
        #endregion

        #region 锁付结果读取地址
        //锁付编号
        public const string LOCK_ID = "182";
        //锁付所属产品编号
        public const string LOCK_PRODUCT_NUMBER = "184";
        //锁付所属产品中不同规格螺丝顺序编号
        public const string LOCK_PRODUCT_SCREW_ORDER = "185";
        //锁付所属螺丝编号
        public const string LOCK_SCREW_SPEC = "186";
        //锁付状态 (0:停止, 1:进行中)
        public const string LOCK_STATE = "187";
        //锁付结果 (0:OK, 非0:NG)
        public const string LOCK_RESULT = "188";
        //锁付峰值扭力(N.m)
        public const string LOCK_PEAK_TORQUE = "190";
        #endregion

        #region 松螺丝地址
        //启动松螺丝
        public const string START_LOOSE = "400";
        //松螺丝自动停止时间(ms)
        public const string LOOSE_AUTO_STOP_TIME = "401";
        //松螺丝自动停止旋转角度(°)
        public const string LOOSE_AUTO_STOP_ANGLE = "402";
        //停止松螺丝
        public const string STOP_LOOSE = "404";
        //松螺丝序号
        public const string LOOSE_SEQUENCE = "405";
        //松螺丝状态 (0:停止, 1:进行中)
        public const string LOOSE_STATE = "406";
        //松螺丝持续时间(ms)
        public const string LOOSE_DURATION = "407";
        //松螺丝旋转角度(°)
        public const string LOOSE_ANGLE = "408";
        //松螺丝速度(rpm)
        public const string LOOSE_SPEED = "902";
        #endregion

        #region 错误码定义
        //锁付结果错误码位定义
        public static class LockResultBits
        {
            public const int MOTOR_STALL = 0;           // bit0：电机堵转
            public const int USER_STOP = 1;            // bit1：用户停止
            public const int ANGLE_LIMIT = 2;          // bit2：角度超限
            public const int TIMEOUT = 3;              // bit3：超时
            public const int MOTOR_ERROR = 4;          // bit4：电机错误
            public const int TILT_LIMIT = 5;           // bit5：倾角超限
            public const int TORQUE_REACHED = 6;       // bit6：扭力到达

            public const int SPEED_UNDER_LIMIT = 17;   // bit17：速度超出设定下限
            public const int SPEED_OVER_LIMIT = 18;    // bit18：速度超出设定上限
            public const int TIME_UNDER_LIMIT = 19;    // bit19：时间超出设定下限
            public const int TIME_OVER_LIMIT = 20;     // bit20：时间超出设定上限
            public const int TORQUE_UNDER_LIMIT = 21;  // bit21：扭力超出设定下限
            public const int TORQUE_OVER_LIMIT = 22;   // bit22：扭力超出设定上限
            public const int ANGLE_UNDER_LIMIT = 23;   // bit23：角度超出设定下限
            public const int ANGLE_OVER_LIMIT = 24;    // bit24：角度超出设定上限
            public const int CLAMP_TORQUE_UNDER = 25;  // bit25：夹紧扭力超出设定下限
            public const int CLAMP_TORQUE_OVER = 26;   // bit26：夹紧扭力超出设定上限
            public const int CLAMP_ANGLE_UNDER = 27;   // bit27：夹紧角度超出设定下限
            public const int CLAMP_ANGLE_OVER = 28;    // bit28：夹紧角度超出设定上限
            public const int IDLE_RUNNING = 29;        // bit29：空转
        }
        #endregion
    }
}
    