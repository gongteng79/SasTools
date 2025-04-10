using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SasTools.UI
{
    public partial class AbnormalAlarmView : UserControl
    {
        DataTable dataTable;
        public AbnormalAlarmView()
        {
            InitializeComponent();

            // 定义列
            var columns = new ColumnCollection
            {
                new Column("Time", "时间"),
                new Column("Info", "消息"),
            };
            tableAlarmInfo.Columns = columns;

            // 绑定数据源
            dataTable = new DataTable();
            dataTable.Columns.Add("Time", typeof(string));
            dataTable.Columns.Add("Info", typeof(string));
            tableAlarmInfo.DataSource = dataTable;
            tableAlarmInfo.ColumnDragSort = true;

            DataRow row = dataTable.NewRow();
            row["Time"] = DateTime.Now.ToString();
            row["Info"] = "测试消息！";
            dataTable.Rows.Add(row);

            tableAlarmInfo.DataSource = dataTable;
        }
    }
}
