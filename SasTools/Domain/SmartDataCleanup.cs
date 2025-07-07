using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using log4net;
namespace SasTools.Domain
{
    public class SmartDataCleanup
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SmartDataCleanup));
        private readonly CleanupConfig _config;

        public class CleanupConfig
        {
            public int MaxRows { get; set; } = 50;        // 增加到50行触发清理
            public int TargetRows { get; set; } = 30;     // 清理后保留30行
            public int CleanupRowSize { get; set; } = 20; // 每次清理20行
            public string ErrorStatusText { get; set; } = "ERROR";
            public TimeSpan MaxRowAge { get; set; } = TimeSpan.FromMinutes(30); //数据最大保留时间
            public int MinErrorRows { get; set; } = 5;    //最少保留的错误记录数
        }

        public SmartDataCleanup(CleanupConfig config = null)
        {
            _config = config ?? new CleanupConfig();
        }

        public bool NeedsCleanup(DataTable dataTable)
        { 
            return dataTable.Rows.Count > _config.MaxRows;
        }

        //基于行数数量的清理策略
        public void PerformSmartCleanup(DataTable dataTable, string deviceId)
        {
            if (!NeedsCleanup(dataTable))
            {
                return;
            }

            try
            {
                lock (dataTable)
                {
                    // 重新检查是否需要清理
                    if (!NeedsCleanup(dataTable))
                    {
                        return;
                    }

                    var rowsToDelete = new List<DataRow>(); // 直接存储DataRow引用而不是索引
                    int deleteCount = 0;

                    // 从最旧数据开始检查，使用ToArray()创建快照避免并发修改
                    var rows = dataTable.Rows.Cast<DataRow>().ToArray();

                    for (int i = 0; i < rows.Length && deleteCount < _config.CleanupRowSize; i++)
                    {
                        var row = rows[i];

                        // 检查行是否仍然有效（没有被删除）
                        if (row.Table == dataTable && row.RowState != DataRowState.Deleted)
                        {
                            string statusType = row["类型"]?.ToString() ?? "";

                            // 如果不是错误状态
                            if (statusType != _config.ErrorStatusText)
                            {
                                rowsToDelete.Add(row);
                                deleteCount++;
                            }
                        }
                    }

                    // 删除标记的行
                    foreach (var row in rowsToDelete)
                    {
                        if (row.Table == dataTable && row.RowState != DataRowState.Deleted)
                        {
                            dataTable.Rows.Remove(row);
                        }
                    }

                    _logger.InfoFormat("设备{0}智能清理完成,删除了{1}条非错误记录，保留了所有错误信息", deviceId, deleteCount);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"设备{deviceId}智能清理失败: {ex.Message}", ex);
            }
        }
        //基于时间的清理策略
        public void PerformTimeBasedCleanup(DataTable dataTable, string deviceId)
        {
            if (dataTable.Rows.Count == 0) return;

            try
            {
                lock (dataTable)
                {
                    var cutoffTime = DateTime.Now - _config.MaxRowAge;
                    var rowsToDelete = new List<DataRow>();
                    int errorRowsCount = 0;

                    // 统计错误记录数量
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["类型"]?.ToString() == _config.ErrorStatusText)
                        {
                            errorRowsCount++;
                        }
                    }

                    // 从最旧数据开始检查
                    var rows = dataTable.Rows.Cast<DataRow>().ToArray();

                    for (int i = 0; i < rows.Length; i++)
                    {
                        var row = rows[i];

                        if (row.Table == dataTable && row.RowState != DataRowState.Deleted)
                        {
                            // 尝试解析时间
                            if (DateTime.TryParse(row["时间"]?.ToString(), out DateTime rowTime))
                            {
                                bool isError = row["类型"]?.ToString() == _config.ErrorStatusText;

                                // 清理条件：超过时间限制 且 (非错误记录 或 错误记录超过最小保留数)
                                if (rowTime < cutoffTime &&
                                    (!isError || errorRowsCount > _config.MinErrorRows))
                                {
                                    rowsToDelete.Add(row);
                                    if (isError) errorRowsCount--;
                                }
                            }
                        }
                    }

                    // 删除过期记录
                    foreach (var row in rowsToDelete)
                    {
                        if (row.Table == dataTable && row.RowState != DataRowState.Deleted)
                        {
                            dataTable.Rows.Remove(row);
                        }
                    }

                    if (rowsToDelete.Count > 0)
                    {
                        _logger.InfoFormat("设备{0}基于时间清理完成，删除了{1}条过期记录", deviceId, rowsToDelete.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"设备{deviceId}基于时间清理失败: {ex.Message}", ex);
            }
        }
    }
}
