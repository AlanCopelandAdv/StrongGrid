using Shouldly;
using StrongGrid.Models.Legacy;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using static StrongGrid.Internal;

namespace StrongGrid.UnitTests.Utilities
{
	public class Extensions
	{
		public class FromUnixTime
		{
			// Note to self:
			// I'm using TheoryData because can't use DateTime with InlineData: 
			// Error CS0182  An attribute argument must be a constant expression, typeof expression or array creation expression of an attribute parameter type
			public static TheoryData<long, DateTime> FromMilliseconds = new TheoryData<long, DateTime>()
			{
				{ 0, new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
				{ 1000, new DateTime(1970, 1, 1, 0, 0, 1, 0, DateTimeKind.Utc) },
				{ 16040, new DateTime(1970, 1, 1, 0, 0, 16, 40, DateTimeKind.Utc) },
			};

			public static TheoryData<long, DateTime> FromSeconds = new TheoryData<long, DateTime>()
			{
				{ 0, new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
				{ 1000, new DateTime(1970, 1, 1, 0, 16, 40, 0, DateTimeKind.Utc) },
			};

			[Theory, MemberData(nameof(FromMilliseconds))]
			public void Converts_from_milliseconds(long numberOfMilliseconds, DateTime expected)
			{
				// Act
				var result = numberOfMilliseconds.FromUnixTime(UnixTimePrecision.Milliseconds);

				// Assert
				result.ShouldBe(expected);
			}

			[Theory, MemberData(nameof(FromSeconds))]
			public void Converts_from_seconds(long numberOfSeconds, DateTime expected)
			{
				// Act
				var result = numberOfSeconds.FromUnixTime(UnixTimePrecision.Seconds);

				// Assert
				result.ShouldBe(expected);
			}

			[Fact]
			public void Throws_when_unknown_precision()
			{
				// Arrange
				var unknownPrecision = (UnixTimePrecision)3;

				// Act
				Should.Throw<ArgumentException>(() => 123L.FromUnixTime(unknownPrecision));
			}
		}

		public class ToUnixTime
		{
			// Note to self:
			// I'm using TheoryData because can't use DateTime with InlineData: 
			// Error CS0182  An attribute argument must be a constant expression, typeof expression or array creation expression of an attribute parameter type
			public static TheoryData<DateTime, long> ToMilliseconds = new TheoryData<DateTime, long>()
			{
				{ new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
				{ new DateTime(1970, 1, 1, 0, 0, 1, 0, DateTimeKind.Utc), 1000 },
				{ new DateTime(1970, 1, 1, 0, 0, 16, 40, DateTimeKind.Utc), 16040 },
			};

			public static TheoryData<DateTime, long> ToSeconds = new TheoryData<DateTime, long>()
			{
				{ new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
				{ new DateTime(1970, 1, 1, 0, 0, 1, 0, DateTimeKind.Utc), 1 },
				{ new DateTime(1970, 1, 1, 0, 0, 16, 40, DateTimeKind.Utc), 16 },
			};

			[Theory, MemberData(nameof(ToMilliseconds))]
			public void Converts_to_milliseconds(DateTime date, long expected)
			{
				// Act
				var result = date.ToUnixTime(UnixTimePrecision.Milliseconds);

				// Assert
				result.ShouldBe(expected);
			}

			[Theory, MemberData(nameof(ToSeconds))]
			public void Converts_to_seconds(DateTime date, long expected)
			{
				// Act
				var result = date.ToUnixTime(UnixTimePrecision.Seconds);

				// Assert
				result.ShouldBe(expected);
			}

			[Fact]
			public void Throws_when_unknown_precision()
			{
				// Arrange
				var unknownPrecision = (UnixTimePrecision)3;

				// Act
				Should.Throw<ArgumentException>(() => DateTime.UtcNow.ToUnixTime(unknownPrecision));
			}
		}

		public class ToEnum
		{
			[Fact]
			public void Throws_when_invalid_value()
			{
				Should.Throw<Exception>(() => "This is not a valid value".ToEnum<CampaignStatus>());
			}

			[Theory]
			[InlineData("in progress", CampaignStatus.InProgress)]
			[InlineData("IN PROGRESS", CampaignStatus.InProgress)]
			[InlineData("In Progress", CampaignStatus.InProgress)]
			[InlineData("In progress", CampaignStatus.InProgress)]
			public void Is_case_insensitive(string descripion, CampaignStatus expectedStatus)
			{
				// Act
				var result = descripion.ToEnum<CampaignStatus>();

				// Assert
				result.ShouldBe(expectedStatus);
			}
		}

		public class GetEncoding
		{
			[Fact]
			public void Returns_actual_encoding()
			{
				// Arrange
				var defaultEncoding = Encoding.UTF32;
				var desiredEncoding = Encoding.ASCII;
				var content = new StringContent("This is a test", desiredEncoding);

				// Act
				var result = content.GetEncoding(defaultEncoding);

				// Assert
				result.ShouldBe(desiredEncoding);
			}

			[Fact]
			public void Returns_default_when_charset_is_empty()
			{
				// Arrange
				var defaultEncoding = Encoding.UTF32;
				var desiredEncoding = Encoding.ASCII;
				var content = new StringContent("This is a test");
				content.Headers.ContentType = new MediaTypeHeaderValue("text/plain")
				{
					CharSet = string.Empty
				};

				// Act
				var result = content.GetEncoding(defaultEncoding);

				// Assert
				result.ShouldBe(defaultEncoding);
			}

			[Fact]
			public void Returns_default_when_charset_is_invalid()
			{
				// Arrange
				var defaultEncoding = Encoding.UTF32;
				var desiredEncoding = Encoding.ASCII;
				var content = new StringContent("This is a test");
				content.Headers.ContentType = new MediaTypeHeaderValue("text/plain")
				{
					CharSet = "for_testing_purposes_setting_charset_property_to_an_invalid_value"
				};

				// Act
				var result = content.GetEncoding(defaultEncoding);

				// Assert
				result.ShouldBe(defaultEncoding);
			}
		}

		public class ToDurationString
		{
			[Fact]
			public void Less_than_one_millisecond()
			{
				// Arrange
				var days = 0;
				var hours = 0;
				var minutes = 0;
				var seconds = 0;
				var milliseconds = 0;
				var timespan = new TimeSpan(days, hours, minutes, seconds, milliseconds);

				// Act
				var result = timespan.ToDurationString();

				// Assert
				result.ShouldBe("1 millisecond");
			}

			[Fact]
			public void Normal()
			{
				// Arrange
				var days = 1;
				var hours = 2;
				var minutes = 3;
				var seconds = 4;
				var milliseconds = 5;
				var timespan = new TimeSpan(days, hours, minutes, seconds, milliseconds);

				// Act
				var result = timespan.ToDurationString();

				// Assert
				result.ShouldBe("1 day 2 hours 3 minutes 4 seconds 5 milliseconds");
			}
		}

		public class EnsureStartsWith
		{
			[Fact]
			public void When_string_is_null()
			{
				// Arrange
				var input = (string)null;
				var prefix = "Hello";
				var desired = "Hello";

				// Act
				var result = input.EnsureStartsWith(prefix);

				// Assert
				result.ShouldBe(desired);
			}

			[Fact]
			public void When_string_starts_with_prefix()
			{
				// Arrange
				var input = "Hello world";
				var prefix = "Hello";
				var desired = "Hello world";

				// Act
				var result = input.EnsureStartsWith(prefix);

				// Assert
				result.ShouldBe(desired);
			}

			[Fact]
			public void When_string_does_not_start_with_prefix()
			{
				// Arrange
				var input = "world";
				var prefix = "Hello";
				var desired = "Helloworld";

				// Act
				var result = input.EnsureStartsWith(prefix);

				// Assert
				result.ShouldBe(desired);
			}
		}

		public class EnsureEndsWith
		{
			[Fact]
			public void When_string_is_null()
			{
				// Arrange
				var input = (string)null;
				var prefix = "Hello";
				var desired = "Hello";

				// Act
				var result = input.EnsureEndsWith(prefix);

				// Assert
				result.ShouldBe(desired);
			}

			[Fact]
			public void When_string_ends_with_suffix()
			{
				// Arrange
				var input = "Hello world";
				var suffix = "world";
				var desired = "Hello world";

				// Act
				var result = input.EnsureEndsWith(suffix);

				// Assert
				result.ShouldBe(desired);
			}

			[Fact]
			public void When_string_does_not_end_with_suffix()
			{
				// Arrange
				var input = "Hello";
				var suffix = "world";
				var desired = "Helloworld";

				// Act
				var result = input.EnsureEndsWith(suffix);

				// Assert
				result.ShouldBe(desired);
			}
		}

		public class GetProperty
		{
			[Fact]
			public void When_property_exists()
			{
				// Arrange
				var jsonString = @"{""Name"":""John""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetProperty("Name", true);

				// Assert
				result.ShouldNotBeNull();
				result.Value.ValueEquals("John");
			}

			[Fact]
			public void When_property_does_not_exist_and_throwIfMissing_is_false()
			{
				// Arrange
				var jsonString = @"{""Name"":""John""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetProperty("xxxyyyzzz", false);

				// Assert
				result.ShouldBeNull();
			}

			[Fact]
			public void When_property_does_not_exist_and_throwIfMissing_is_true()
			{
				// Arrange
				var jsonString = @"{""Name"":""John""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				Should.Throw<Exception>(() => jsonObj.GetProperty("xxxyyyzzz", true));
			}

			[Fact]
			public void When_multilevel_property_exists()
			{
				// Arrange
				var jsonString = @"{""Name"":""John"", ""Child"":{""Name"":""Bob""}}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetProperty("Child/Name", true);

				// Assert
				result.ShouldNotBeNull();
				result.Value.ValueEquals("Bob");
			}

			[Fact]
			public void When_multilevel_property_does_not_exist_and_throwIfMissing_is_false()
			{
				// Arrange
				var jsonString = @"{""Name"":""John"", ""Child"":{""Name"":""Bob""}}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetProperty("Child/xxxyyyzzz", false);

				// Assert
				result.ShouldBeNull();
			}

			[Fact]
			public void When_multilevel_property_does_not_exist_and_throwIfMissing_is_true()
			{
				// Arrange
				var jsonString = @"{""Name"":""John"", ""Child"":{""Name"":""Bob""}}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				Should.Throw<Exception>(() => jsonObj.GetProperty("Child/xxxyyyzzz", true));
			}
		}

		public class GetPropertyValue
		{
			[Fact]
			public void When_property_exists()
			{
				// Arrange
				var jsonString = @"{""Name"":""John""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<string>("Name");

				// Assert
				result.ShouldBe("John");
			}

			[Fact]
			public void When_property_does_not_exist()
			{
				// Arrange
				var jsonString = @"{""Name"":""John""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				Should.Throw(() => jsonObj.GetPropertyValue<string>("xxxyyyzzz"), typeof(Exception));
			}

			[Fact]
			public void Default_value_when_property_does_not_exist()
			{
				// Arrange
				var jsonString = @"{""Name"":""John""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<string>("xxxyyyzzz", "default value");

				// Assert
				result.ShouldBe("default value");
			}

			[Fact]
			public void Multiple_properties_exist()
			{
				// Arrange
				var jsonString = @"{""Name"":""John"",""City"":""Atlanta""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<string>(new[] { "Name", "City" });

				// Assert
				result.ShouldBe("John");
			}

			[Fact]
			public void Multiple_properties_only_one_exists()
			{
				// Arrange
				var jsonString = @"{""Name"":""John"",""City"":""Atlanta""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<string>(new[] { "xxxyyyzzz", "City" });

				// Assert
				result.ShouldBe("Atlanta");
			}

			[Fact]
			public void Value_is_int()
			{
				// Arrange
				var jsonString = @"{""NumberOfChildren"":2}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<int>("NumberOfChildren");

				// Assert
				result.ShouldBe(2);
			}

			private enum MyEnum
			{
				Value1 = 1,
				Value2 = 2
			}

			[Fact]
			public void Value_is_enum_from_int()
			{
				// Arrange
				var jsonString = @"{""MyProperty"":2}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<MyEnum>("MyProperty");

				// Assert
				result.ShouldBe(MyEnum.Value2);
			}

			[Fact]
			public void Value_is_enum_from_string()
			{
				// Arrange
				var jsonString = @"{""MyProperty"":""Value1""}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<MyEnum>("MyProperty");

				// Assert
				result.ShouldBe(MyEnum.Value1);
			}

			[Theory]
			[InlineData("null", null)]
			[InlineData("2", 2)]
			public void Value_is_nullable_int(string jsonValue, int? expected)
			{
				// Arrange
				var jsonString = @"{""MyProperty"":" + jsonValue + "}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<int?>("MyProperty");

				// Assert
				result.ShouldBe(expected);
			}

			[Theory]
			[InlineData("null", (int[])null)]
			[InlineData("[1,2,3]", new int[] { 1, 2, 3 })]
			public void Value_is_array(string jsonValue, int[] expected)
			{
				// Arrange
				var jsonString = @"{""MyProperty"":" + jsonValue + "}";

				var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
				var jsonObj = JsonElement.ParseValue(ref reader);

				// Act
				var result = jsonObj.GetPropertyValue<int[]>("MyProperty");

				// Assert
				result.ShouldBe(expected);
			}
		}
	}
}
