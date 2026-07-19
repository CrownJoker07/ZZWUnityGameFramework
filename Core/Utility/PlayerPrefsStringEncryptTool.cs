using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// PlayerPrefs 字符串加密工具。
    /// </summary>
    /// <remarks>
    /// 这个工具只服务于 PlayerPrefsSettingHelper 中的 string value：
    /// 1. 不加密 PlayerPrefs 的 key，保证 HasSetting、RemoveSetting 等接口仍按原 key 工作。
    /// 2. 不在存储内容里写明文版本号或格式前缀，避免本地数据一眼看出加密方案。
    /// 3. 读取时通过 Base64 解析、长度检查、HMAC 校验共同判断数据是否由本工具写入。
    /// 4. 校验或解密失败时返回 false，由调用方返回默认值，避免本地数据异常直接打断游戏流程。
    /// </remarks>
    internal static class PlayerPrefsStringEncryptTool
    {
        // AES-CBC 的 IV 固定为 16 字节；HMACSHA256 的输出固定为 32 字节。
        // 最终写入 PlayerPrefs 的内容是 Base64(payload)，payload 的二进制布局如下：
        // [0, 16)                         : iv
        // [16, payload.Length - 32)        : cipherText
        // [payload.Length - 32, end)       : hmac(iv + cipherText)
        // 不写入类似 "UGF_AES_HMAC_V1:" 的明文前缀，是为了减少本地存档里的明显特征。
        private const int IvLength = 16;
        private const int HmacLength = 32;

        // AES key 固定在客户端代码里，只能提供本地混淆级保护：
        // 目标是避免 PlayerPrefs 直接暴露明文，并提高普通手动篡改的成本。
        // 它不能替代服务端校验，也不能防止拥有客户端代码和逆向能力的人提取密钥。
        private static readonly byte[] AesKey =
        {
            0x25, 0x8F, 0x42, 0xC7, 0x11, 0x6D, 0xE3, 0x90,
            0xAB, 0x54, 0x7C, 0x19, 0xD6, 0xF0, 0x32, 0x8A,
            0x71, 0x0E, 0xBD, 0x49, 0xC3, 0x5A, 0x96, 0x27,
            0xE8, 0x14, 0x6B, 0xD2, 0x3F, 0xA5, 0x80, 0xCC
        };

        // HMAC key 与 AES key 分开，避免同一个密钥同时承担加密和完整性校验两个用途。
        // HMAC 用来确认 iv + cipherText 没有被改过；校验失败时不会进入 AES 解密。
        private static readonly byte[] HmacKey =
        {
            0xD4, 0x63, 0x1A, 0xB8, 0x5F, 0x02, 0xE7, 0x94,
            0x30, 0xCB, 0x7D, 0x26, 0xA1, 0x58, 0xF9, 0x0C,
            0x86, 0x3E, 0xB5, 0x41, 0xDA, 0x70, 0x12, 0xEF,
            0x9B, 0x24, 0xC8, 0x55, 0x6A, 0xD1, 0x03, 0xBE
        };

        /// <summary>
        /// 加密字符串，并追加 HMAC 校验码。
        /// </summary>
        /// <param name="value">原始字符串。</param>
        /// <returns>Base64(iv + cipherText + hmac)。</returns>
        /// <remarks>
        /// 每次加密都会生成新的 IV，所以相同明文多次保存也会得到不同密文。
        /// 这里使用 Encrypt-then-MAC：先加密得到 cipherText，再对 iv + cipherText 计算 HMAC。
        /// 解密侧会先验证 HMAC，只有完整性通过后才执行 AES 解密。
        /// </remarks>
        public static string EncryptString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            byte[] iv;
            byte[] cipherText;
            using (AesManaged aesManaged = new AesManaged())
            {
                aesManaged.Mode = CipherMode.CBC;
                aesManaged.Padding = PaddingMode.PKCS7;
                aesManaged.Key = AesKey;
                aesManaged.GenerateIV();
                iv = aesManaged.IV;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(value);
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        cipherText = memoryStream.ToArray();
                    }
                }
            }

            // HMAC 覆盖 IV 和密文。IV 虽然不需要保密，但必须防篡改；
            // 如果 IV 被改动，解密出的第一块明文会被影响，所以也要纳入校验。
            byte[] hmacData = Combine(iv, cipherText);
            byte[] hmac;
            using (HMACSHA256 hmacSha256 = new HMACSHA256(HmacKey))
            {
                hmac = hmacSha256.ComputeHash(hmacData);
            }

            return Convert.ToBase64String(Combine(hmacData, hmac));
        }

        /// <summary>
        /// 尝试解密字符串。
        /// </summary>
        /// <param name="encryptedValue">Base64(iv + cipherText + hmac)。</param>
        /// <param name="value">解密成功后的原始字符串。</param>
        /// <returns>是否解密成功。</returns>
        /// <remarks>
        /// 返回 false 的情况包括：没有值、不是 Base64、长度不合法、HMAC 不匹配、AES 解密失败。
        /// 这些情况都会被视为“不是当前工具写入的有效字符串”，调用方会按默认值处理。
        /// </remarks>
        public static bool TryDecryptString(string encryptedValue, out string value)
        {
            value = null;

            if (encryptedValue == null)
            {
                return false;
            }

            // 旧版明文值通常不是当前 payload 的 Base64 结构，会在 Base64 解析、
            // 长度检查或 HMAC 校验中的某一步失败；这里不做旧数据兼容。
            byte[] payload;
            try
            {
                payload = Convert.FromBase64String(encryptedValue);
            }
            catch
            {
                return false;
            }

            // payload 至少要包含 IV、非空密文和 HMAC。
            // AES-CBC + PKCS7 对空字符串也会产生一个完整密文块，因此 cipherTextLength 必须大于 0。
            if (payload.Length <= IvLength + HmacLength)
            {
                return false;
            }

            int cipherTextLength = payload.Length - IvLength - HmacLength;
            byte[] hmacData = new byte[IvLength + cipherTextLength];
            byte[] expectedHmac = new byte[HmacLength];
            Buffer.BlockCopy(payload, 0, hmacData, 0, hmacData.Length);
            Buffer.BlockCopy(payload, hmacData.Length, expectedHmac, 0, expectedHmac.Length);

            byte[] actualHmac;
            using (HMACSHA256 hmacSha256 = new HMACSHA256(HmacKey))
            {
                actualHmac = hmacSha256.ComputeHash(hmacData);
            }

            // 先验签再解密。HMAC 不一致说明数据被篡改、密钥不匹配，或内容不是本工具写入的；
            // 继续解密既没有意义，也可能把异常数据带进后续 JSON/业务逻辑。
            if (!FixedTimeEquals(actualHmac, expectedHmac))
            {
                return false;
            }

            byte[] iv = new byte[IvLength];
            byte[] cipherText = new byte[cipherTextLength];
            Buffer.BlockCopy(hmacData, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(hmacData, iv.Length, cipherText, 0, cipherText.Length);

            try
            {
                using (AesManaged aesManaged = new AesManaged())
                {
                    aesManaged.Mode = CipherMode.CBC;
                    aesManaged.Padding = PaddingMode.PKCS7;
                    aesManaged.Key = AesKey;
                    aesManaged.IV = iv;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(cipherText, 0, cipherText.Length);
                            cryptoStream.FlushFinalBlock();
                            value = Encoding.UTF8.GetString(memoryStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                value = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 拼接两个 byte 数组。
        /// </summary>
        /// <remarks>
        /// 用显式数组拼接保持数据布局简单可控，避免字符串分隔符带来的额外可见特征。
        /// </remarks>
        private static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }

        /// <summary>
        /// 固定时间比较，避免校验过程过早返回。
        /// </summary>
        /// <remarks>
        /// 普通逐字节比较通常会在发现第一个不同字节时立刻返回。
        /// 这里始终扫描完整数组，让比较耗时尽量只和数组长度有关，而不是和第几个字节不同有关。
        /// </remarks>
        private static bool FixedTimeEquals(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            int difference = 0;
            for (int i = 0; i < first.Length; i++)
            {
                difference |= first[i] ^ second[i];
            }

            return difference == 0;
        }
    }
}
