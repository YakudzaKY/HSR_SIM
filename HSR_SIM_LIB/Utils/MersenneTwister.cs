using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;

namespace HSR_SIM_LIB.Utils.Utils
{
    /// <summary>
    /// Генератор псевдослучайных чисел на основе алгоритма Вихрь Мерсенна
    /// Конфигурация: SFMT-19937:122-18-1-11-1:dfffffef-ddfecb7f-bffaffff-bffffff6
    /// </summary>
    public class MersenneTwister
    {
        private const int MersenneExponent = 19937;
        private const int Length128 = MersenneExponent / 128 + 1;
        private const int Length32 = Length128 * 4;

        private readonly Vector128<uint>[] _state = new Vector128<uint>[Length128];
        private int _index;

        [ThreadStatic]
        private static MersenneTwister _shared;

        /// <summary>
        /// Общий экземпляр генератора, создается отдельно для каждого потока
        /// </summary>
        public static MersenneTwister Shared => _shared ??= new(GetRandomSeed());

        /// <summary>
        /// Создает экземпляр генератора со случайным сидом
        /// </summary>
        public MersenneTwister() : this(Shared.NextUInt32()) { }

        /// <summary>
        /// Создает экземпляр генератора с указанным сидом
        /// </summary>
        /// <param name="seed">Сид для создания генератора</param>
        public MersenneTwister(int seed) : this((uint)seed) { }

        private MersenneTwister(uint seed)
        {
            InitGenerator(seed);
        }

        private static uint GetRandomSeed()
        {
            ReadOnlySpan<byte> bytes = Guid.NewGuid().ToByteArray();
            ReadOnlySpan<uint> hash = MemoryMarshal.Cast<byte, uint>(SHA256.HashData(bytes));
            uint result = 0;
            foreach (uint value in hash)
                result ^= value;
            return result;
        }

        private void InitGenerator(uint seed)
        {
            if (!Sse2.IsSupported)
                throw new InvalidOperationException("SSE2 не поддерживается на данном устройстве.");

            Span<uint> values = MemoryMarshal.Cast<Vector128<uint>, uint>(_state);
            values[0] = seed;
            for (int i = 1; i < Length32; i++)
            {
                values[i] = (uint)(1812433253ul * (values[i - 1] ^ values[i - 1] >> 30) + (uint)i);
            }
            _index = Length32;

            Vector128<uint> parity = Vector128.Create(0x00000001u, 0x00000000u, 0x00000000u, 0x13c9e684u);
            Vector128<uint> v = Sse2.And(_state[0], parity);

            uint inner = 0;
            for (int i = 0; i < 4; i++)
                inner ^= v.GetElement(i);
            for (int i = 16; i > 0; i >>= 1)
                inner ^= inner >> i;

            if ((inner & 1) == 0)
                values[0] ^= 1;
        }

        private void UpdateState()
        {
            const int offset = 122;
            Vector128<uint> r1 = _state[^2];
            Vector128<uint> r2 = _state[^1];
            int i = 0;
            for (; i < Length128 - offset; i++)
            {
                _state[i] = DoRecursion(_state[i], _state[i + offset], r1, r2);
                r1 = r2;
                r2 = _state[i];
            }
            for (; i < Length128; i++)
            {
                _state[i] = DoRecursion(_state[i], _state[i + offset - Length128], r1, r2);
                r1 = r2;
                r2 = _state[i];
            }
        }

        private Vector128<uint> DoRecursion(Vector128<uint> a, Vector128<uint> b, Vector128<uint> c, Vector128<uint> d)
        {
            Vector128<uint> z = Sse2.ShiftRightLogical128BitLane(c, 1);
            z = Sse2.Xor(z, a);
            Vector128<uint> v = Sse2.ShiftLeftLogical(d, 18);
            z = Sse2.Xor(z, v);
            Vector128<uint> x = Sse2.ShiftLeftLogical128BitLane(a, 1);
            z = Sse2.Xor(z, x);
            Vector128<uint> y = Sse2.ShiftRightLogical(b, 11);
            Vector128<uint> mask = Vector128.Create(0xdfffffefu, 0xddfecb7fu, 0xbffaffffu, 0xbffffff6u);
            y = Sse2.And(y, mask);
            return Sse2.Xor(z, y);
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,ulong.MaxValue]
        /// </summary>
        public ulong NextUInt64()
        {
            if ((_index & 1) == 1)
                _index++;
            if (_index >= Length32)
            {
                UpdateState();
                _index = 0;
            }
            Span<ulong> values = MemoryMarshal.Cast<Vector128<uint>, ulong>(_state);
            ulong r = values[_index >> 1];
            _index += 2;
            return r;
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,uint.MaxValue]
        /// </summary>
        public uint NextUInt32()
        {
            if (_index >= Length32)
            {
                UpdateState();
                _index = 0;
            }
            Span<uint> values = MemoryMarshal.Cast<Vector128<uint>, uint>(_state);
            return values[_index++];
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,int.MaxValue)
        /// </summary>
        public int Next()
        {
            return (int)((ulong)int.MaxValue * NextUInt32() >> 32);
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,maxValue)
        /// </summary>
        public int Next(int maxValue)
        {
            if (maxValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue), "Значение должно быть больше 0.");

            return (int)((ulong)maxValue * NextUInt32() >> 32);
        }

        /// <summary>
        /// Возвращает число в диапазоне [minValue,maxValue)
        /// </summary>
        public int Next(int minValue, int maxValue)
        {
            if (minValue < 0)
                throw new ArgumentOutOfRangeException(nameof(minValue), "Значение должно быть больше либо равно 0.");
            if (maxValue <= minValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), $"Значение должно быть больше, чем {nameof(minValue)}.");

            return (int)((ulong)(maxValue - minValue) * NextUInt32() >> 32) + minValue;
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,long.MaxValue)
        /// </summary>
        public long NextInt64()
        {
            return (long)((BigInteger)long.MaxValue * NextUInt64() >> 64);
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,maxValue)
        /// </summary>
        public long NextInt64(long maxValue)
        {
            if (maxValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue), "Значение должно быть больше 0.");

            return (long)((BigInteger)maxValue * NextUInt64() >> 64);
        }

        /// <summary>
        /// Возвращает число в диапазоне [minValue,maxValue)
        /// </summary>
        public long NextInt64(long minValue, long maxValue)
        {
            if (minValue < 0)
                throw new ArgumentOutOfRangeException(nameof(minValue), "Значение должно быть больше либо равно 0.");
            if (maxValue <= minValue)
                throw new ArgumentOutOfRangeException(nameof(maxValue), $"Значение должно быть больше, чем {nameof(minValue)}.");

            return (long)((BigInteger)(maxValue - minValue) * NextUInt64() >> 64) + minValue;
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,1)
        /// </summary>
        public double NextDouble()
        {
            return (NextUInt64() >> 11) / 9007199254740992.0; // 2^53
        }

        /// <summary>
        /// Возвращает число в диапазоне [0,1)
        /// </summary>
        public float NextSingle()
        {
            return (NextUInt32() >> 8) / 16777216.0f; // 2^24
        }
    }
}
