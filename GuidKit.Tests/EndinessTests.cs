using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GuidPhantom.Tests
{
	[TestClass]
	public class GuidKitEndianTests
	{
		static readonly Guid Known = new Guid("00112233-4455-6677-8899-aabbccddeeff");

		static readonly byte[] KnownBigEndian =
		{
		0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
		0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
	};

		static readonly byte[] KnownDotNetLayout =
		{
		0x33, 0x22, 0x11, 0x00, 0x55, 0x44, 0x77, 0x66,
		0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
	};

		[TestMethod]
		public void ToByteArray_BigEndian_GivesRfcOrder()
		{
			var bytes = Known.ToByteArray(bigEndian: true);
			CollectionAssert.AreEqual(KnownBigEndian, bytes);
		}

		[TestMethod]
		public void ToByteArray_LittleEndian_GivesDotNetLayout()
		{
			var bytes = Known.ToByteArray(bigEndian: false);
			CollectionAssert.AreEqual(KnownDotNetLayout, bytes);
		}

		[TestMethod]
		public void FromByteArray_BigEndian_ParsesRfcOrder()
		{
			var guid = GuidKit.FromByteArray(KnownBigEndian, bigEndian: true);
			Assert.AreEqual(Known, guid);
		}

		[TestMethod]
		public void FromByteArray_LittleEndian_ParsesDotNetLayout()
		{
			var guid = GuidKit.FromByteArray(KnownDotNetLayout, bigEndian: false);
			Assert.AreEqual(Known, guid);
		}

		[TestMethod]
		[DataRow(true)]
		[DataRow(false)]
		public void ByteArray_RoundTrip(bool bigEndian)
		{
			var original = Guid.NewGuid();
			var bytes = original.ToByteArray(bigEndian: bigEndian);
			var roundTripped = GuidKit.FromByteArray(bytes, bigEndian: bigEndian);
			Assert.AreEqual(original, roundTripped);
		}

#if NET6_0_OR_GREATER
		[TestMethod]
		public void BigEndianBytes_MatchGuidStringOrder()
		{
			var bytes = Known.ToByteArray(bigEndian: true);
			var hex = Convert.ToHexString(bytes);
			Assert.AreEqual(Known.ToString("N").ToUpperInvariant(), hex);
		}
#endif


		[TestMethod]
		public void ToHexString_Known()
		{
			var hex = Known.ToHexString(bigEndian: true);
			Assert.AreEqual("0x00112233445566778899AABBCCDDEEFF", hex);
		}

		[TestMethod]
		[DataRow(true)]
		[DataRow(false)]
		public void HexString_RoundTrip(bool bigEndian)
		{
			var original = Guid.NewGuid();
			var hex = original.ToHexString(bigEndian: bigEndian);
			var roundTripped = GuidKit.FromHexString(hex, bigEndian: bigEndian);
			Assert.AreEqual(original, roundTripped);
		}

		// Goes through the private ToBigEndianSpan indirectly.
		[TestMethod]
		public void GetVariantAndVersion_Version7()
		{
			var v7 = GuidKit.CreateVersion7();
			var info = v7.GetVariantAndVersion();
			Assert.AreEqual(GuidVariant.IETF, info.Variant);
			Assert.AreEqual((byte)7, info.Version);
		}

		// Test vector from RFC 9562 appendix B.1.
		[TestMethod]
		public void ConvertVersion1To6_Rfc9562Vector()
		{
			var v1 = new Guid("c232ab00-9414-11ec-b3c8-9e6bdeced846");
			var expectedV6 = new Guid("1ec9414c-232a-6b00-b3c8-9e6bdeced846");

			var v6 = v1.ConvertVersion1To6();
			var backToV1 = v6.ConvertVersion6To1();

			Assert.AreEqual(expectedV6, v6);
			Assert.AreEqual(v1, backToV1);
		}
			
		[TestMethod]
		public void ConvertVersion7To8MsSql_RoundTrip()
		{
			var v7 = GuidKit.CreateVersion7();
			var v8 = v7.ConvertVersion7To8MsSql();
			var backToV7 = v8.ConvertVersion8MsSqlTo7();

			Assert.AreEqual(v7, backToV7);
			Assert.AreEqual((byte)8, v8.GetVariantAndVersion().Version);
		}

		[TestMethod]
		public void IncrementedGuid_RoundTrip()
		{
			var baseGuid = Guid.NewGuid();
			var incremented = baseGuid.CreateIncrementedGuid(12345);
			var increment = incremented.ReverseIncrementedGuid(baseGuid);
			Assert.AreEqual(12345, increment);
		}

		[TestMethod]
		public void XorGuid_RoundTrip()
		{
			var a = Guid.NewGuid();
			var b = Guid.NewGuid();
			var xor = a.CreateXorGuid(b);
			Assert.AreEqual(b, xor.ReverseXorGuid(a));
			Assert.AreEqual(a, xor.ReverseXorGuid(b));
		}

		[TestMethod]
		public void NumericGuid_RoundTrip()
		{
			var guid = GuidKit.CreateNumericGuid(42);
			Assert.AreEqual(42, guid.ReverseNumericGuid());
		}






		[TestMethod]
		public void CopyGuidToSpan_GivesBothLayouts()
		{
			Span<byte> bigEndianBytes = stackalloc byte[16];
			Span<byte> dotNetBytes = stackalloc byte[16];

			GuidKit.CopyGuidToSpan(Known, bigEndianBytes, bigEndian: true);
			GuidKit.CopyGuidToSpan(Known, dotNetBytes, bigEndian: false);

			CollectionAssert.AreEqual(KnownBigEndian, bigEndianBytes.ToArray());
			CollectionAssert.AreEqual(KnownDotNetLayout, dotNetBytes.ToArray());
		}

		// Regresjonstest for seeding av teller i CreateVersion7Or8MsSql.
		// Guid.NewGuid() er v4, så versjonsnibblen 0100 ligger i øvre nibble av
		// bytes[7] i .NET-layout. Med bigEndian: false lekker den inn i telleren
		// og nibblen blir alltid 4. Med bigEndian: true er den tilfeldig.
		[TestMethod]
		public void CreateVersion7_CounterSeed_IsRandom()
		{
			AssertByte7UpperNibbleVaries(() => GuidKit.CreateVersion7());
		}

		[TestMethod]
		public void CreateVersion8MsSql_CounterSeed_IsRandom()
		{
			AssertByte7UpperNibbleVaries(() => GuidKit.CreateVersion8MsSql());
		}

		static void AssertByte7UpperNibbleVaries(Func<Guid> create)
		{
			var nibbles = new HashSet<int>();

			for (int i = 0; i < 40; i++)
			{
				var guid = create();
				var bytes = guid.ToByteArray(bigEndian: true);
				var nibble = bytes[7] >> 4;
				nibbles.Add(nibble);

				// ny ms-slot gir ny seeding av telleren
				Thread.Sleep(2);
			}

			Assert.IsTrue(nibbles.Count > 1, "bytes[7] upper nibble is constant, counter seed is not random");
		}
	}

}
