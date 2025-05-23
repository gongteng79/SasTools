using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SasTools.Common
{
    // JSON 消息编解码工具类
    public static class JsonMessageCodec
    {
        // 默认 JSON 序列化选项
        private static readonly JsonSerializerOptions _defaultOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // 将对象序列化为 JSON 字符串
        public static string Serialize<T>(T obj, JsonSerializerOptions options = null)
        {
            try
            {
                return JsonSerializer.Serialize(obj, options ?? _defaultOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"序列化对象到 JSON 失败: {ex.Message}", ex);
            }
        }

        // 将对象序列化为 JSON 字节数组
        public static byte[] SerializeToBytes<T>(T obj, JsonSerializerOptions options = null)
        {
            try
            {
                //调用 Serialize方法得到JSON字符串
                string json = Serialize(obj, options);
                //使用 UTF-8 编码转换为字节数组
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"序列化对象到 JSON 字节数组失败: {ex.Message}", ex);
            }
        }

        // 从 JSON 字符串反序列化对象
        public static T Deserialize<T>(string json, JsonSerializerOptions options = null)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json, options ?? _defaultOptions);
            }
            catch (Exception ex)
            {
                throw new Exception($"从 JSON 反序列化对象失败: {ex.Message}", ex);
            }
        }

        // 从 JSON 字节数组反序列化对象
        public static T Deserialize<T>(byte[] jsonBytes, JsonSerializerOptions options = null)
        {
            try
            {
                if (jsonBytes == null || jsonBytes.Length == 0)
                    return default;

                string json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                return Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"从 JSON 字节数组反序列化对象失败: {ex.Message}", ex);
            }
        }
    }
}
