﻿using System;
using NUnit.Framework;

namespace Blob.Tests
{
    public class TestUtilities
    {
        private static object[] _alignCases = new object[]
        {
            new int[] { 0, 0, 1 },
            new int[] { -1, -1, 1 },
            new int[] { 100, 100, 1 },
            new int[] { int.MaxValue, int.MaxValue, 1 },

            new int[] { 0, 0, 2 },
            new int[] { 0, -1, 2 },
            new int[] { -2, -2, 2 },
            new int[] { 2, 1, 2 },
            new int[] { 10, 10, 2 },
            new int[] { 1000, 999, 2 },

            new int[] { 0, 0, 4 },
            new int[] { 0, -1, 4 },
            new int[] { 0, -2, 4 },
            new int[] { -4, -4, 4 },
            new int[] { -1000, -1000, 4 },
            new int[] { -1000, -1001, 4 },
            new int[] { 4, 1, 4 },
            new int[] { 12, 10, 4 },
            new int[] { 1000, 999, 4 },

            new int[] { 0, 0, 8 },
            new int[] { 0, -1, 8 },
            new int[] { 0, -2, 8 },
            new int[] { -8, -8, 8 },
            new int[] { -1000, -1000, 8 },
            new int[] { -1000, -1001, 8 },
            new int[] { 8, 1, 8 },
            new int[] { 16, 10, 8 },
            new int[] { 1000, 999, 8 },

            new int[] { 0, 0, 16 },
            new int[] { 0, -1, 16 },
            new int[] { 0, -2, 16 },
            new int[] { -16, -16, 16 },
            new int[] { -992, -1000, 16 },
            new int[] { -992, -1001, 16 },
            new int[] { -1008, -1008, 16 },
            new int[] { 16, 1, 16 },
            new int[] { 16, 10, 16 },
            new int[] { 1008, 999, 16 },
            new int[] { 1008, 1008, 16 },
            new int[] { 1024, 1009, 16 },
        };

        [TestCaseSource(nameof(_alignCases))]
        public void should_align_numbers(int expected, int address, int alignment)
        {
            Assert.AreEqual(expected, Utilities.Align(address, alignment));
        }

        [Test]
        public void should_throw_on_invalid_alignment()
        {
            Assert.Catch<ArgumentOutOfRangeException>(() => Utilities.Align(0, 0));
            Assert.Catch<ArgumentOutOfRangeException>(() => Utilities.Align(0, -1));
            Assert.Catch<ArgumentOutOfRangeException>(() => Utilities.Align(0, int.MinValue));
            Assert.Catch<ArgumentOutOfRangeException>(() => Utilities.Align(0, 3));
            Assert.Catch<ArgumentOutOfRangeException>(() => Utilities.Align(0, 6));
            Assert.Catch<ArgumentOutOfRangeException>(() => Utilities.Align(0, 1000));
        }


        struct Tuple<T1, T2>
        {
            private T1 _;
            private T2 __;
        }

        [Test]
        public void should_align_by_type()
        {
            Assert.AreEqual(1, Utilities.AlignOf<byte>());
            Assert.AreEqual(2, Utilities.AlignOf<short>());
            Assert.AreEqual(4, Utilities.AlignOf<int>());
            Assert.AreEqual(8, Utilities.AlignOf<long>());
            Assert.AreEqual(4, Utilities.AlignOf<Tuple<short, int>>());
            Assert.AreEqual(8, Utilities.AlignOf<Tuple<long, short>>());
            Assert.AreEqual(4, Utilities.AlignOf<Tuple<byte, Tuple<byte, int>>>());
            Assert.AreEqual(8, Utilities.AlignOf<Tuple<int, Tuple<long, byte>>>());
        }
    }
}