using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameBase.Algorithm
{
    /// <summary>
    /// 加密解密工具
    /// </summary>
    public static class EncryptHelper
    {
        /// <summary>
        /// 计算文件MD5值
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hashMD5"></param>
        /// <returns></returns>
        public static bool CalcMD5(string filePath, out string hashMD5)
        {
            hashMD5 = string.Empty;
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var calculator = MD5.Create();
                    var hashBytes = calculator.ComputeHash(fs);
                    StringBuilder sb = new StringBuilder(hashBytes.Length);
                    for (int i = 0; i < hashBytes.Length; ++i)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }
                    hashMD5 = sb.ToString();
                }
            }
            return !string.IsNullOrEmpty(hashMD5);
        }

        //public static bool CalcCRC32(string filePath,out string crc32)
        //{
        //    crc32 = string.Empty;
        //    if (File.Exists(filePath))
        //    {
        //        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //        {
        //            var calculator = MD5.Create();
        //            var hashBytes = calculator.ComputeHash(fs);
        //            StringBuilder sb = new StringBuilder(hashBytes.Length);
        //            for (int i = 0; i < hashBytes.Length; ++i)
        //            {
        //                sb.Append(hashBytes[i].ToString("x2"));
        //            }
        //            hashMD5 = sb.ToString();
        //        }
        //    }
        //    return !string.IsNullOrEmpty(crc32);
        //}
    }

}
