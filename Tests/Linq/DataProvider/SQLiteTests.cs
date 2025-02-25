﻿using System;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
using LinqToDB.Mapping;

using NUnit.Framework;

namespace Tests.DataProvider
{
	using Model;

	[TestFixture]
	public class SQLiteTests : TestBase
	{
		[Test]
		public void TestParameters([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<string>("SELECT @p",        new { p =  1  }), Is.EqualTo("1"));
				Assert.That(conn.Execute<string>("SELECT @p",        new { p = "1" }), Is.EqualTo("1"));
				Assert.That(conn.Execute<int>   ("SELECT @p",        new { p =  new DataParameter { Value = 1   } }), Is.EqualTo(1));
				Assert.That(conn.Execute<string>("SELECT @p1",       new { p1 = new DataParameter { Value = "1" } }), Is.EqualTo("1"));
				Assert.That(conn.Execute<int>   ("SELECT @p1 + @p2", new { p1 = 2, p2 = 3 }), Is.EqualTo(5));
				Assert.That(conn.Execute<int>   ("SELECT @p2 + @p1", new { p2 = 2, p1 = 3 }), Is.EqualTo(5));
			}
		}

		static void TestType<T>(DataConnection connection, string dataTypeName, T value, string tableName = "AllTypes", bool convertToString = false)
			where T : notnull
		{
			Assert.That(connection.Execute<T>(string.Format("SELECT {0} FROM {1} WHERE ID = 1", dataTypeName, tableName)),
				Is.EqualTo(connection.MappingSchema.GetDefaultValue(typeof(T))));

			object actualValue   = connection.Execute<T>(string.Format("SELECT {0} FROM {1} WHERE ID = 2", dataTypeName, tableName))!;
			object expectedValue = value;

			if (convertToString)
			{
				actualValue   = actualValue.  ToString()!;
				expectedValue = expectedValue.ToString()!;
			}

			Assert.That(actualValue, Is.EqualTo(expectedValue));
		}

		[Test]
		public void TestDataTypes([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				TestType(conn, "bigintDataType",           1000000L);
				TestType(conn, "numericDataType",          9999999m);
				TestType(conn, "bitDataType",              true);
				TestType(conn, "smallintDataType",         (short)25555);
				TestType(conn, "decimalDataType",          2222222m);
				TestType(conn, "intDataType",              7777777);
				TestType(conn, "tinyintDataType",          (sbyte)100);
				TestType(conn, "moneyDataType",            100000m);
				TestType(conn, "floatDataType",            20.31d);
				TestType(conn, "realDataType",             16.2f);

				TestType(conn, "datetimeDataType",         new DateTime(2012, 12, 12, 12, 12, 12));

				TestType(conn, "charDataType",             '1');
				TestType(conn, "varcharDataType",          "234");
				TestType(conn, "textDataType",             "567");
				TestType(conn, "ncharDataType",            "23233");
				TestType(conn, "nvarcharDataType",         "3323");
				TestType(conn, "ntextDataType",            "111");

				TestType(conn, "binaryDataType",           new byte[] { 1 });
				TestType(conn, "varbinaryDataType",        new byte[] { 2 });
				TestType(conn, "imageDataType",            new byte[] { 0, 0, 0, 3 });

				TestType(conn, "uniqueidentifierDataType", new Guid("{6F9619FF-8B86-D011-B42D-00C04FC964FF}"));
				TestType(conn, "objectDataType",           (object)10);
			}
		}

		// this adds type as string using single quotes, but sqlite understands such syntax
		[Sql.Expression("CAST({0} as {1})", ServerSideOnly = true)]
		static TValue Cast<TValue>(TValue value, string type)
		{
			throw new InvalidOperationException();
		}

		static void TestNumeric<T>(DataConnection conn, T expectedValue, DataType dataType, string skip = "")
		{
			var skipTypes = skip.Split(' ');

			conn.InlineParameters = true;

			foreach (var sqlType in new[]
				{
					"bigint",
					"bit",
					"decimal",
					"int",
					"money",
					"numeric",
					"smallint",
					"tinyint",

					"float",
					"real"
				}.Except(skipTypes))
			{
				var result = conn.Select(() => Cast(expectedValue, sqlType))!;

				// sqlite floating point parser doesn't restore roundtrip values properly
				// and also deviation could differ for different versions of engine
				// see https://system.data.sqlite.org/index.html/tktview/fb9e4b30874d83042e09c2f791d6065fc5e73a4b
				// and TestDoubleRoundTrip test
				if (expectedValue is double doubleValue)
					Assert.AreEqual(doubleValue, (double)(object)result, Math.Abs(doubleValue) * 1e-15);
				else if (expectedValue is float floatValue)
					Assert.AreEqual(floatValue, (float)(object)result, Math.Abs(floatValue) * 1e-9);
				else
					Assert.That(result, Is.EqualTo(expectedValue));
			}

			conn.InlineParameters = false;

			Debug.WriteLine("{0} -> DataType.{1}",  typeof(T), dataType);
			Assert.That(conn.Execute<T>("SELECT @p", new DataParameter { Name = "p", DataType = dataType, Value = expectedValue }), Is.EqualTo(expectedValue));
			Debug.WriteLine("{0} -> auto", typeof(T));
			Assert.That(conn.Execute<T>("SELECT @p", new DataParameter { Name = "p", Value = expectedValue }), Is.EqualTo(expectedValue));
			Debug.WriteLine("{0} -> new",  typeof(T));
			Assert.That(conn.Execute<T>("SELECT @p", new { p = expectedValue }), Is.EqualTo(expectedValue));
		}

		static void TestSimple<T>(DataConnection conn, T expectedValue, DataType dataType, string skip = "")
			where T : struct
		{
			TestNumeric<T> (conn, expectedValue, dataType, skip);
			TestNumeric<T?>(conn, expectedValue, dataType, skip);
			TestNumeric<T?>(conn, (T?)null,      dataType, skip);
		}

		[Test]
		public void TestNumerics([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			// culture region needed if tests run on system with non-dot decimal separator, e.g. nl-NL
			using (new InvariantCultureRegion())
			using (var conn = new DataConnection(context))
			{
				TestSimple<bool>   (conn, true, DataType.Boolean);
				TestSimple<sbyte>  (conn, 1,    DataType.SByte);
				TestSimple<short>  (conn, 1,    DataType.Int16);
				TestSimple<int>    (conn, 1,    DataType.Int32);
				TestSimple<long>   (conn, 1L,   DataType.Int64);
				TestSimple<byte>   (conn, 1,    DataType.Byte);
				TestSimple<ushort> (conn, 1,    DataType.UInt16);
				TestSimple<uint>   (conn, 1u,   DataType.UInt32);
				TestSimple<ulong>  (conn, 1ul,  DataType.UInt64);
				TestSimple<float>  (conn, 1f,   DataType.Single);
				TestSimple<float>  (conn, 1.1f, DataType.Single,      "bigint int smallint tinyint");
				TestSimple<double> (conn, 1d,   DataType.Double);
				TestSimple<double> (conn, 1.1d, DataType.Double,      "bigint int smallint tinyint");
				TestSimple<decimal>(conn, 1m,   DataType.Decimal);
				//TestSimple<decimal>(conn, 1.1m, DataType.Decimal,     "bigint int smallint tinyint");
				TestSimple<decimal>(conn, 1m,   DataType.VarNumeric);
				//TestSimple<decimal>(conn, 1.1m, DataType.VarNumeric,  "bigint int smallint tinyint");
				TestSimple<decimal>(conn, 1m,   DataType.Money);
				//TestSimple<decimal>(conn, 1.1m, DataType.Money,       "bigint int smallint tinyint");
				TestSimple<decimal>(conn, 1m,   DataType.SmallMoney);
				//TestSimple<decimal>(conn, 1.1m, DataType.SmallMoney,  "bigint int smallint tinyint");

				TestNumeric(conn, sbyte.MinValue,    DataType.SByte);
				TestNumeric(conn, sbyte.MaxValue,    DataType.SByte);
				TestNumeric(conn, short.MinValue,    DataType.Int16);
				TestNumeric(conn, short.MaxValue,    DataType.Int16);
				TestNumeric(conn, int.  MinValue,    DataType.Int32);
				TestNumeric(conn, int.  MaxValue,    DataType.Int32);
				TestNumeric(conn, long. MinValue,    DataType.Int64);
				TestNumeric(conn, long. MaxValue,    DataType.Int64,      "float real");

				TestNumeric(conn, byte.MaxValue,     DataType.Byte);
				TestNumeric(conn, ushort.MaxValue,   DataType.UInt16);
				TestNumeric(conn, uint.MaxValue,     DataType.UInt32);

				if (context != ProviderName.SQLiteMS)
					TestNumeric(conn, ulong.MaxValue,    DataType.UInt64,     "bigint bit decimal int money numeric smallint tinyint float real");

				TestNumeric(conn, -3.40282306E+38f,  DataType.Single,     "bigint int smallint tinyint");
				TestNumeric(conn,  3.40282306E+38f,  DataType.Single,     "bigint int smallint tinyint");
				TestNumeric(conn, decimal.MinValue,  DataType.Decimal,    "bigint bit decimal int money numeric smallint tinyint float real");
				TestNumeric(conn, decimal.MaxValue,  DataType.Decimal,    "bigint bit decimal int money numeric smallint tinyint float real");
				TestNumeric(conn, decimal.MinValue,  DataType.VarNumeric, "bigint bit decimal int money numeric smallint tinyint float real");
				TestNumeric(conn, decimal.MaxValue,  DataType.VarNumeric, "bigint bit decimal int money numeric smallint tinyint float real");
				TestNumeric(conn, -922337203685477m, DataType.Money);
				TestNumeric(conn, +922337203685477m, DataType.Money);
				TestNumeric(conn, -214748m,          DataType.SmallMoney);
				TestNumeric(conn, +214748m,          DataType.SmallMoney);
			}
		}

		[ActiveIssue("https://system.data.sqlite.org/index.html/tktview/fb9e4b30874d83042e09c2f791d6065fc5e73a4b")]
		[Test]
		public void TestDoubleRoundTrip([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				var cmd = conn.CreateCommand();
				var value = -1.7900000000000002E+308;

				// SELECT CAST(-1.7900000000000002E+308 as real)
				cmd.CommandText = FormattableString.Invariant($"SELECT CAST({value:G17} as real)");
				using (var rd = cmd.ExecuteReader())
				{
					rd.Read();
					var valueFromDB = rd.GetDouble(0);

					// -1.790000000000001E+308d != -1.7900000000000002E+308
					Assert.AreEqual(value, valueFromDB);
				}
			}
		}

		[Test]
		public void TestNumericsDouble([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				TestNumeric(conn, -1.7900000000000002E+308d, DataType.Double, "bigint int smallint tinyint");
				TestNumeric(conn, -1.7900000000000008E+308d, DataType.Double, "bigint int smallint tinyint");
				TestNumeric(conn,  1.7900000000000002E+308d, DataType.Double, "bigint int smallint tinyint");
				TestNumeric(conn,  1.7900000000000008E+308d, DataType.Double, "bigint int smallint tinyint");
			}
		}

		[Test]
		public void TestDateTime([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				var dateTime = new DateTime(2012, 12, 12, 12, 12, 12);

				Assert.That(conn.Execute<DateTime> ("SELECT '2012-12-12 12:12:12'"), Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT '2012-12-12 12:12:12'"), Is.EqualTo(dateTime));

				Assert.That(conn.Execute<DateTime> ("SELECT @p", DataParameter.DateTime("p", dateTime)),               Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT @p", new DataParameter("p", dateTime)),                    Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT @p", new DataParameter("p", dateTime, DataType.DateTime)), Is.EqualTo(dateTime));
			}
		}

		[Test]
		public void TestChar([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<char> ("SELECT Cast('1' as char)"),         Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as char)"),         Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT Cast('1' as char(1))"),      Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as char(1))"),      Is.EqualTo('1'));

				Assert.That(conn.Execute<char> ("SELECT Cast('1' as varchar)"),      Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as varchar)"),      Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT Cast('1' as varchar(20))"),  Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as varchar(20))"),  Is.EqualTo('1'));

				Assert.That(conn.Execute<char> ("SELECT Cast('1' as nchar)"),        Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as nchar)"),        Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT Cast('1' as nchar(20))"),    Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as nchar(20))"),    Is.EqualTo('1'));

				Assert.That(conn.Execute<char> ("SELECT Cast('1' as nvarchar)"),     Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as nvarchar)"),     Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT Cast('1' as nvarchar(20))"), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as nvarchar(20))"), Is.EqualTo('1'));

				if (context != ProviderName.SQLiteMS)
				{
					Assert.That(conn.Execute<char> ("SELECT @p",                  DataParameter.Char    ("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>("SELECT @p",                  DataParameter.Char    ("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char> ("SELECT Cast(@p as char)",    DataParameter.Char    ("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>("SELECT Cast(@p as char)",    DataParameter.Char    ("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char> ("SELECT Cast(@p as char(1))", DataParameter.Char    ("p", '1')), Is.EqualTo('1'));
					Assert.That(conn.Execute<char?>("SELECT Cast(@p as char(1))", DataParameter.Char    ("p", '1')), Is.EqualTo('1'));
				}

				Assert.That(conn.Execute<char> ("SELECT @p",                  DataParameter.VarChar ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p",                  DataParameter.VarChar ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT @p",                  DataParameter.NChar   ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p",                  DataParameter.NChar   ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT @p",                  DataParameter.NVarChar("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p",                  DataParameter.NVarChar("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT @p",                  DataParameter.Create  ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p",                  DataParameter.Create  ("p", '1')), Is.EqualTo('1'));

				Assert.That(conn.Execute<char> ("SELECT @p", new DataParameter { Name = "p", Value = '1' }), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p", new DataParameter { Name = "p", Value = '1' }), Is.EqualTo('1'));
			}
		}

		[Test]
		public void TestString([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<string>("SELECT Cast('12345' as char)"),         Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast('12345' as char(20))"),     Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast(NULL    as char(20))"),     Is.Null);

				Assert.That(conn.Execute<string>("SELECT Cast('12345' as varchar)"),      Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast('12345' as varchar(20))"),  Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast(NULL    as varchar(20))"),  Is.Null);

				Assert.That(conn.Execute<string>("SELECT Cast('12345' as text)"),         Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast(NULL    as text)"),         Is.Null);

				Assert.That(conn.Execute<string>("SELECT Cast('12345' as nchar)"),        Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast('12345' as nchar(20))"),    Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast(NULL    as nchar(20))"),    Is.Null);

				Assert.That(conn.Execute<string>("SELECT Cast('12345' as nvarchar)"),     Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast('12345' as nvarchar(20))"), Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast(NULL    as nvarchar(20))"), Is.Null);

				Assert.That(conn.Execute<string>("SELECT Cast('12345' as ntext)"),        Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast(NULL    as ntext)"),        Is.Null);

				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Char    ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.VarChar ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Text    ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.NChar   ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.NVarChar("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.NText   ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Create  ("p", "123")), Is.EqualTo("123"));

				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Create("p", (string?)null)), Is.EqualTo(null));
				Assert.That(conn.Execute<string>("SELECT @p", new DataParameter { Name = "p", Value = "1" }), Is.EqualTo("1"));
			}
		}

		[Test]
		public void TestBinary([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			var arr1 = new byte[] { 1 };
			var arr2 = new byte[] { 2 };

			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<byte[]>("SELECT    binaryDataType FROM AllTypes WHERE ID = 2"), Is.EqualTo(           arr1));
				Assert.That(conn.Execute<Binary>("SELECT varbinaryDataType FROM AllTypes WHERE ID = 2"), Is.EqualTo(new Binary(arr2)));

				Assert.That(conn.Execute<byte[]>("SELECT    binaryDataType FROM AllTypes WHERE ID = 2"), Is.EqualTo(           arr1));
				Assert.That(conn.Execute<Binary>("SELECT varbinaryDataType FROM AllTypes WHERE ID = 2"), Is.EqualTo(new Binary(arr2)));

				Assert.That(conn.Execute<byte[]>("SELECT Cast(NULL as image)"),           Is.EqualTo(null));

				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Binary   ("p", arr1)), Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.VarBinary("p", arr1)), Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Create   ("p", arr1)), Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.VarBinary("p", null)), Is.EqualTo(null));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Binary   ("p", Array<byte>.Empty)), Is.EqualTo(Array<byte>.Empty));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.VarBinary("p", Array<byte>.Empty)), Is.EqualTo(Array<byte>.Empty));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Image    ("p", Array<byte>.Empty)), Is.EqualTo(Array<byte>.Empty));
				Assert.That(conn.Execute<byte[]>("SELECT @p", new DataParameter { Name = "p", Value = arr1 }), Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Create   ("p", new Binary(arr1))), Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", new DataParameter("p", new Binary(arr1))), Is.EqualTo(arr1));
			}
		}

		[Test]
		public void TestGuid([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(
					conn.Execute<Guid>("SELECT uniqueidentifierDataType FROM AllTypes WHERE ID = 2"),
					Is.EqualTo(new Guid("6F9619FF-8B86-D011-B42D-00C04FC964FF")));

				Assert.That(
					conn.Execute<Guid?>("SELECT '6F9619FF-8B86-D011-B42D-00C04FC964FF'"),
					Is.EqualTo(new Guid("6F9619FF-8B86-D011-B42D-00C04FC964FF")));

				var guid = TestData.Guid1;

				Assert.That(conn.Execute<Guid>("SELECT @p", DataParameter.Create("p", guid)),                Is.EqualTo(guid));
				Assert.That(conn.Execute<Guid>("SELECT @p", new DataParameter { Name = "p", Value = guid }), Is.EqualTo(guid));
			}
		}

		[Test]
		public void TestObject([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<object>("SELECT Cast(1 as Object)"), Is.EqualTo(1));
				Assert.That(conn.Execute<int>   ("SELECT Cast(1 as Object)"), Is.EqualTo(1));
				Assert.That(conn.Execute<int?>  ("SELECT Cast(1 as Object)"), Is.EqualTo(1));
				Assert.That(conn.Execute<string>("SELECT Cast(1 as Object)"), Is.EqualTo("1"));

				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Variant("p", 1)), Is.EqualTo("1"));
			}
		}

		[Test]
		public void TestXml([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<XDocument>  ("SELECT '<xml/>'").ToString(), Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XmlDocument>("SELECT '<xml/>'").InnerXml,   Is.EqualTo("<xml />"));

				var xdoc = XDocument.Parse("<xml/>");
				var xml  = Convert<string,XmlDocument>.Lambda("<xml/>");

				Assert.That(conn.Execute<string>     ("SELECT  @p", DataParameter.Xml("p", "<xml/>")),        Is.EqualTo("<xml/>"));
				Assert.That(conn.Execute<XDocument>  ("SELECT  @p", DataParameter.Xml("p", xdoc)).ToString(), Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XmlDocument>("SELECT  @p", DataParameter.Xml("p", xml)). InnerXml,   Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XDocument>  ("SELECT  @p", new DataParameter("p", xdoc)).ToString(), Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XDocument>  ("SELECT  @p", new DataParameter("p", xml)). ToString(), Is.EqualTo("<xml />"));
			}
		}

		/// <summary>
		/// Ensure we can pass data as Json parameter type and get
		/// same value back out equivalent in value
		/// </summary>
		[Test]
		public void TestJson([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				var testJson = "{\"name\":\"bob\", \"age\":10}";

				Assert.That(conn.Execute<string>("SELECT @p", new DataParameter("p", testJson, DataType.Json)), Is.EqualTo(testJson));
			}
		}

		enum TestEnum
		{
			[MapValue("A")] AA,
			[MapValue("B")] BB,
		}

		[Test]
		public void TestEnum1([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<TestEnum> ("SELECT 'A'"), Is.EqualTo(TestEnum.AA));
				Assert.That(conn.Execute<TestEnum?>("SELECT 'A'"), Is.EqualTo(TestEnum.AA));
				Assert.That(conn.Execute<TestEnum> ("SELECT 'B'"), Is.EqualTo(TestEnum.BB));
				Assert.That(conn.Execute<TestEnum?>("SELECT 'B'"), Is.EqualTo(TestEnum.BB));
			}
		}

		[Test]
		public void TestEnum2([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<string>("SELECT @p", new { p = TestEnum.AA }),            Is.EqualTo("A"));
				Assert.That(conn.Execute<string>("SELECT @p", new { p = (TestEnum?)TestEnum.BB }), Is.EqualTo("B"));

				Assert.That(conn.Execute<string>("SELECT @p", new { p = ConvertTo<string>.From((TestEnum?)TestEnum.AA) }), Is.EqualTo("A"));
				Assert.That(conn.Execute<string>("SELECT @p", new { p = ConvertTo<string>.From(TestEnum.AA) }), Is.EqualTo("A"));
				Assert.That(conn.Execute<string>("SELECT @p", new { p = conn.MappingSchema.GetConverter<TestEnum?,string>()!(TestEnum.AA) }), Is.EqualTo("A"));
			}
		}

		[Table(Name = "CreateTableTest", Schema = "IgnoreSchema")]
		public class CreateTableTest
		{
			[PrimaryKey, Identity]
			public int Id;
		}

		[Test]
		public void CreateDatabase([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			try
			{
				SQLiteTools.DropDatabase("TestDatabase");
			}
			catch
			{
			}

			SQLiteTools.CreateDatabase("TestDatabase");
			Assert.IsTrue(File.Exists ("TestDatabase.sqlite"));

			using (var db = new DataConnection(SQLiteTools.GetDataProvider(context), "Data Source=TestDatabase.sqlite"))
			{
				db.CreateTable<CreateTableTest>();
				db.DropTable  <CreateTableTest>();
			}

			SQLiteTools.ClearAllPools  ();
			SQLiteTools.DropDatabase   ("TestDatabase");
			Assert.IsFalse(File.Exists ("TestDatabase.sqlite"));
		}

		[Test]
		public void BulkCopyLinqTypes([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			foreach (var bulkCopyType in new[] { BulkCopyType.MultipleRows, BulkCopyType.ProviderSpecific })
			{
				using (var db = new DataConnection(context))
				{
					try
					{
						db.BulkCopy(
							new BulkCopyOptions { BulkCopyType = bulkCopyType },
							Enumerable.Range(0, 10).Select(n =>
								new LinqDataTypes
								{
									ID            = 4000 + n,
									MoneyValue    = 1000m + n,
									DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
									BoolValue     = true,
									GuidValue     = TestData.SequentialGuid(n),
									SmallIntValue = (short)n
								}
							));
					}
					finally
					{
						db.GetTable<LinqDataTypes>().Delete(p => p.ID >= 4000);
					}
				}
			}
		}

		[Test]
		public async Task BulkCopyLinqTypesAsync([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			foreach (var bulkCopyType in new[] { BulkCopyType.MultipleRows, BulkCopyType.ProviderSpecific })
			{
				using (var db = new DataConnection(context))
				{
					try
					{
						await db.BulkCopyAsync(
							new BulkCopyOptions { BulkCopyType = bulkCopyType },
							Enumerable.Range(0, 10).Select(n =>
								new LinqDataTypes
								{
									ID            = 4000 + n,
									MoneyValue    = 1000m + n,
									DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
									BoolValue     = true,
									GuidValue     = TestData.SequentialGuid(n),
									SmallIntValue = (short)n
								}
							));
					}
					finally
					{
						await db.GetTable<LinqDataTypes>().DeleteAsync(p => p.ID >= 4000);
					}
				}
			}
		}

		[Test]
		public void Issue784Test([IncludeDataSources(TestProvName.AllSQLiteClassic)] string context)
		{
			using (var db = new TestDataConnection(context))
			{
				var sp = db.DataProvider.GetSchemaProvider();
				var s  = sp.GetSchema(db);

				var table = s.Tables.FirstOrDefault(_ => _.TableName!.Equals("ForeignKeyTable", StringComparison.OrdinalIgnoreCase))!;
				Assert.IsNotNull(table);

				Assert.AreEqual(1,                   table.ForeignKeys                   .Count);
				Assert.AreEqual("PrimaryKeyTable",   table.ForeignKeys[0].OtherTable     .TableName);
				Assert.AreEqual("ID",                table.ForeignKeys[0].OtherColumns[0].ColumnName);
				Assert.AreEqual("PrimaryKeyTableID", table.ForeignKeys[0].ThisColumns[0] .ColumnName);

			}
		}

		// test to make sure our tests work with expected version of sqlite
		// should be updated when we bump dependency
		// also test matrix document should be updated too in that case (Build/Azure/README.md)
		[Test]
		public void TestDbVersion([IncludeDataSources(TestProvName.AllSQLite)] string context)
		{
			string expectedVersion;
			switch (context)
			{
				case ProviderName.SQLiteClassic:
				case TestProvName.SQLiteClassicMiniProfilerMapped:
				case TestProvName.SQLiteClassicMiniProfilerUnmapped:
					expectedVersion = "3.37.0";
					break;
				case ProviderName.SQLiteMS:
#if NET472
					expectedVersion = "3.13.0";
#elif NET5_0 || NET6_0
					expectedVersion = "3.35.5";
#else
					expectedVersion = "3.33.0";
#endif
					break;
				default:
					throw new InvalidOperationException();
			}

			using (var db  = new TestDataConnection(context))
			using (var cmd = db.CreateCommand())
			{
				cmd.CommandText = "select sqlite_version();";
				var version     = (string)cmd.ExecuteScalar()!;

				Assert.AreEqual(expectedVersion, version);
			}
		}
	}
}
