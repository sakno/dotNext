using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using Xunit;

namespace DotNext.Runtime
{
    [ExcludeFromCodeCoverage]
    public class IntrinsicsTests : Assert
    {
        private Guid field;
        private string str;

        [Fact]
        public void FieldTypedReferenceValueType()
        {
            TypedReference reference = __makeref(field);
            ref Guid g = ref reference.AsRef<Guid>();
            Equal(Guid.Empty, g);
            g = Guid.NewGuid();
            Equal(field, g);
            True(Intrinsics.AreSame(in field, in g));
        }

        [Fact]
        public void FieldTypedReferenceClass()
        {
            TypedReference reference = __makeref(str);
            ref string f = ref reference.AsRef<string>();
            Null(f);
            f = "Hello, world!";
            Equal(str, f);
            True(Intrinsics.AreSame(in str, in f));
        }

        [Fact]
        public static void SwapValues()
        {
            var x = 10;
            var y = 20;
            Intrinsics.Swap(ref x, ref y);
            Equal(20, x);
            Equal(10, y);
        }

        [Fact]
        public static unsafe void SwapValuesByPointer()
        {
            var x = 10;
            var y = 20;
            Intrinsics.Swap(&x, &y);
            Equal(20, x);
            Equal(10, y);
        }

        [Fact]
        public static void AddressOfLocal()
        {
            var i = 20;
            True(Intrinsics.AddressOf(i) != IntPtr.Zero);
        }

        [Fact]
        public static unsafe void BitwiseEqualityForByte()
        {
            byte value1 = 10;
            byte value2 = 20;
            False(Intrinsics.Equals(&value1, &value2, sizeof(byte)));
            value2 = 10;
            True(Intrinsics.Equals(&value1, &value2, sizeof(byte)));
        }

        [Fact]
        public static unsafe void BitwiseEqualityForLong()
        {
            var value1 = 10L;
            var value2 = 20L;
            False(Intrinsics.Equals(&value1, &value2, sizeof(long)));
            value2 = 10;
            True(Intrinsics.Equals(&value1, &value2, sizeof(long)));
        }

        [Fact]
        public static unsafe void BitwiseHashCode()
        {
            var i = 42L;
            NotEqual(0, Intrinsics.GetHashCode32(&i, sizeof(long)));
            NotEqual(0L, Intrinsics.GetHashCode64(&i, sizeof(long)));
        }

        [Fact]
        public static void NullCheck()
        {
            static void NullRefCheck()
            {
                ref readonly var ch = ref default(string).GetRawData();
                Intrinsics.ThrowIfNull(in ch);
            }
            var i = 0L;
            False(Intrinsics.IsNull(in i));
            ref readonly var ch = ref default(string).GetRawData();
            True(Intrinsics.IsNull(in ch));
            Throws<NullReferenceException>(NullRefCheck);
        }

        [Fact]
        public static void CopyBlock()
        {
            char[] chars1 = new[] { 'a', 'b', 'c' };
            var chars2 = new char[2];
            Intrinsics.Copy(ref chars1[1], ref chars2[0], 2);
            Equal('b', chars2[0]);
            Equal('c', chars2[1]);
        }

        [Fact]
        public static unsafe void CopyValue()
        {
            int a = 42, b = 0;
            Intrinsics.Copy(&a, &b);
            Equal(a, b);
            Equal(42, b);
        }

        [Fact]
        public static unsafe void ZeroMem()
        {
            var g = Guid.NewGuid();
            Intrinsics.ClearBits(&g, sizeof(Guid));
            Equal(Guid.Empty, g);
        }

        [Fact]
        public static void ReadonlyRef()
        {
            var array = new[] { "a", "b", "c" };
            ref readonly var element = ref array.GetReadonlyRef(2);
            Equal("c", element);
        }

        [Fact]
        public static void IsNullable()
        {
            True(Intrinsics.IsNullable<string>());
            True(Intrinsics.IsNullable<ValueType>());
            True(Intrinsics.IsNullable<int?>());
            False(Intrinsics.IsNullable<int>());
            False(Intrinsics.IsNullable<IntPtr>());
        }

        [Fact]
        public static void RefTypeDefaultTest()
        {
            True(Intrinsics.IsDefault<string>(default));
            False(Intrinsics.IsDefault(""));
        }

        [Fact]
        public static void StructTypeDefaultTest()
        {
            var value = default(Guid);
            True(Intrinsics.IsDefault(value));
            value = Guid.NewGuid();
            False(Intrinsics.IsDefault(value));
        }

        [Fact]
        public static void SmallStructDefault()
        {
            True(Intrinsics.IsDefault(default(long)));
            False(Intrinsics.IsDefault(42L));
            True(Intrinsics.IsDefault(default(int)));
            False(Intrinsics.IsDefault(42));
            True(Intrinsics.IsDefault(default(byte)));
            False(Intrinsics.IsDefault((byte)42));
            True(Intrinsics.IsDefault(default(short)));
            False(Intrinsics.IsDefault((short)42));
        }

        [Fact]
        public static void Bitcast()
        {
            var point = new Point { X = 40, Y = 100 };
            Intrinsics.Bitcast(point, out decimal dec);
            Intrinsics.Bitcast(dec, out point);
            Equal(40, point.X);
            Equal(100, point.Y);
            Intrinsics.Bitcast<uint, int>(2U, out var i);
            Equal(2, i);
        }

        [Fact]
        public static void BitcastToLargerValueType()
        {
            var point = new Point { X = 40, Y = 100 };
            Intrinsics.Bitcast(point, out Guid g);
            False(g == Guid.Empty);
        }

        [Fact]
        public static void LightweightTypeOf()
        {
            var handle = Intrinsics.TypeOf<string>();
            Equal(typeof(string).TypeHandle, handle);
            NotEqual(typeof(StringComparer).TypeHandle, handle);
        }

        [Flags]
        private enum ByteEnum : byte
        {
            None = 0,
            One = 1,
            Two = 2,
        }

        [Flags]
        private enum ShortEnum : short
        {
            None = 0,
            One = 1,
            Two = 2,
        }

        [Flags]
        private enum LongEnum : long
        {
            None = 0L,
            One = 1L,
            Two = 2L,
        }

        [Fact]
        public static void HasFlag()
        {
            static void HasFlag<T>(T value, T validFlag, T invalidFlag)
                where T : struct, Enum
                {
                    True(Intrinsics.HasFlag(value, validFlag));
                    False(Intrinsics.HasFlag(value, invalidFlag));
                }

            HasFlag(BindingFlags.Public | BindingFlags.Instance, BindingFlags.Public, BindingFlags.Static);
            HasFlag(ByteEnum.Two, ByteEnum.Two, ByteEnum.One);
            HasFlag(ShortEnum.Two, ShortEnum.Two, ShortEnum.One);
            HasFlag(LongEnum.Two, LongEnum.Two, LongEnum.One);
        }
    }
}