using System.Security.Cryptography;
using System.Text;

namespace Shared.Tools
{
    public sealed class RandomGenerator
    {
        private const string _defaultCharset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public int NextInt(int min, int max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(min, max);

            uint scale = uint.MaxValue;
            while (scale == uint.MaxValue)
            {
                byte[] fourBytes = new byte[4];
                RandomNumberGenerator.Fill(fourBytes);
                uint v = BitConverter.ToUInt32(fourBytes, 0);
                scale = v;
            }

            return (int)(min + (max - min) * (scale / (double)uint.MaxValue));
        }
        public string NextString(int length, string? charset = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            charset ??= _defaultCharset;

            StringBuilder result = new(length);
            byte[] buffer = new byte[length];

            RandomNumberGenerator.Fill(buffer);
            for (int i = 0; i < length; i++)
            {
                result.Append(charset[buffer[i] % charset.Length]);
            }

            return result.ToString();
        }
    }
}