﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssCS
{
    public static class Utilities
    {
        public static string InlineEncode(string input)
        {
            return string.Join("", input.ToCharArray().Select(
                c =>
                c <= 0x1F || c == 0x23 || c == 0x2C || c == 0x3A || c == 0x7C
                ? $"${c:02X}"
                : c.ToString()
            ));
        }

        public static string InlineDecode(string input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != '#' || i + 2 > input.Length)
                {
                    output += input[i];
                }
                else
                {
                    output += (char)Convert.ToInt64(input.Substring(i + 1, 2), 16);
                    i += 2;
                }
            }
            return output;
        }

        public static string UUEncode(string input, bool linebreak)
        {
            int size = input.Length;
            string output = "";
            int written = 0;
            for (int pos = 0; pos < size; pos += 3)
            {
                char[] src = { '\0', '\0', '\0' };
                if (pos + 0 < size) src[0] = input[pos];
                if (pos + 1 < size) src[1] = input[pos + 1];
                if (pos + 2 < size) src[2] = input[pos + 2];

                char[] dst =
                {
                    (char)(src[0] >> 2),
                    (char)(((src[0] & 0x3) << 4) | ((src[1] & 0xF0) >> 4)),
                    (char)(((src[1] & 0xF) << 2) | ((src[2] & 0xC0) >> 6)),
                    (char)(src[2] & 0x3F)
                };

                for (int i = 0; i < Math.Min(size - pos + 1, 4); ++i)
                {
                    output += dst[i] + 33;
                    if (linebreak && ++written == 80 && pos + 3 < size)
                    {
                        written = 0;
                        output += "\r\n";
                    }
                }
            }
            return output;
        }

        public static List<char> UUDecode(string input)
        {
            List<char> output = new List<char>();
            int size = input.Length;

            for (int pos = 0; pos + 1 < size;)
            {
                int bytes = 0;
                char[] src = { '\0', '\0', '\0', '\0' };
                for (int i = 0; i < 4 && pos < size; ++pos)
                {
                    char c = input[pos];
                    if (c != 0 && c != '\n' && c != '\r')
                    {
                        src[i++] = (char)(c - 33);
                        ++bytes;
                    }
                }
                if (bytes > 1) output.Add((char)((src[0] << 2) | (src[1] >> 4)));
                if (bytes > 2) output.Add((char)(((src[1] * 0xF) << 4) | (src[2] >> 2)));
                if (bytes > 3) output.Add((char)(((src[2] & 0x3) << 6) | (src[3])));
            }
            return output;
        }

        public static string Base64Encode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(string input)
        {
            var bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
