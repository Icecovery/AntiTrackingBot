using AntiTrackingBot.Core;
using AntiTrackingBot.Core.Models.FilterRule;
using AntiTrackingBot.Core.Models.FilterValue;
using System.Collections.Generic;

namespace AntiTrackingBot.Test;
public class UtilityTests
{
	#region GetQueryParameters

	[Fact]
	public void TestGetQueryParametersTwo()
	{
		const string query = "?param1=value1&param2=value2";
		List<string> expected =
		[
			"param1=value1",
			"param2=value2"
		];

		List<string> actual = Utilities.GetQueryParameters(query);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestGetQueryParametersOne()
	{
		const string query = "?param1=value1";
		List<string> expected =
		[
			"param1=value1"
		];

		List<string> actual = Utilities.GetQueryParameters(query);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestGetQueryParametersNone()
	{
		const string query = "";
		List<string> expected = [];

		List<string> actual = Utilities.GetQueryParameters(query);
		Assert.Equal(expected, actual);
	}

	#endregion

	#region IsRegex

	[Fact]
	public void TestIsRegexTrue()
	{
		const string pattern = "/isRegex/";
		bool actual = Utilities.IsRegex(pattern);
		Assert.True(actual);
	}

	[Fact]
	public void TestIsRegexFalse()
	{
		const string pattern = "notRegex";
		bool actual = Utilities.IsRegex(pattern);
		Assert.False(actual);
	}

	#endregion

	#region GetRegex

	[Fact]
	public void TestGetRegex()
	{
		const string pattern = "/getRegex/";
		const string expected = "getRegex";
		string actual = Utilities.GetRegex(pattern);
		Assert.Equal(expected, actual);
	}

	#endregion

	#region WildcardToRegex

	[Fact]
	public void TestWildcardToRegexNoChange()
	{
		const string domain = "example.com";
		const string expected = "example.com";
		string actual = Utilities.WildcardToRegex(domain);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestWildcardToRegexOneWildcard()
	{
		const string domain = "*.example.com";
		const string expected = @"/.*?\.example\.com/";
		string actual = Utilities.WildcardToRegex(domain);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestWildcardToRegexMultipleWildcards()
	{
		const string domain = "*.example.com/*/";
		const string expected = @"/.*?\.example\.com/.*?//";
		string actual = Utilities.WildcardToRegex(domain);
		Assert.Equal(expected, actual);
	}

	#endregion

	#region ProcessLine

	[Fact]
	public void TestProcessLineEmpty()
	{
		const string line = "";
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.Null(actual);
	}

	[Fact]
	public void TestProcessLineComment()
	{
		const string line = "! This is a comment";
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.Null(actual);
	}

	[Fact]
	public void TestProcessLineInvalid()
	{
		const string line = "invalid";
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.Null(actual);
	}

	[Fact]
	public void TestProcessLineValid()
	{
		const string line = "$removeparam=ref_src,domain=twitter.com|x.com";
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.NotNull(actual);
	}

	[Fact]
	public void TestProcessLinePattern()
	{
		const string line = "||amazon.com$removeparam=c";
		List<IFilterRule> expected =
		[
			new SpecificFilterRule(
				new ContainsFilterValue("amazon.com"),
				new QueryFilterValue("c"))
		];
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestProcessLinePatternRegexDomain()
	{
		const string line = "||amazon.*$removeparam=c";
		List<IFilterRule> expected =
		[
			new SpecificFilterRule(
				new RegexFilterValue(@"amazon\..*?"),
				new QueryFilterValue("c"))
		];
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestProcessLinePatternRegexFilter()
	{
		const string line = "||amazon.com$removeparam=/c[a-z]*/";
		List<IFilterRule> expected =
		[
			new SpecificFilterRule(
				new ContainsFilterValue("amazon.com"),
				new RegexFilterValue("c[a-z]*"))
		];
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TestProcessLinePatternMultipleDomainFilter()
	{
		const string line = "||amazon.com|ebay.com$removeparam=c|d";
		List<IFilterRule> expected =
		[
			new SpecificFilterRule(
				new ContainsFilterValue("amazon.com"),
				new QueryFilterValue("c")),
			new SpecificFilterRule(
				new ContainsFilterValue("amazon.com"),
				new QueryFilterValue("d")),
			new SpecificFilterRule(
				new ContainsFilterValue("ebay.com"),
				new QueryFilterValue("c")),
			new SpecificFilterRule(
				new ContainsFilterValue("ebay.com"),
				new QueryFilterValue("d"))
		];
		List<IFilterRule>? actual = Utilities.ProcessLine(line);
		Assert.Equal(expected, actual);
	}

	#endregion

	#region ExtractDomainAndParam

	[Theory]
	[InlineData("||amazon.com$removeparam=c", "amazon.com", "c")]
	[InlineData("||amazon.com^$subdocument,script,removeparam=c,document,image", "amazon.com", "c")]
	[InlineData("||amazon.com|ebay.com$removeparam=c|d", "amazon.com|ebay.com", "c|d")]
	[InlineData("$domain=amazon.com,removeparam=c", "amazon.com", "c")]
	[InlineData("$removeparam=c,domain=a.com|b.com|c.com", "a.com|b.com|c.com", "c")]
	[InlineData(".com/*/status/$removeparam=t,domain=twitter.com|x.com", "twitter.com|x.com", "t")]
	[InlineData("$removeparam=fb_source", "", "fb_source")]
	[InlineData("^z=*&var=$removeparam=var", "z=*&var=", "var")]
	[InlineData("rtkcid=*&clickid=$removeparam=clickid", "rtkcid=*&clickid=", "clickid")]
	public void TestExtractDomainAndParam(string line, string domain, string param)
	{
		bool actual = Utilities.ExtractDomainAndParam(line, out string actualDomain, out string actualParam);
		Assert.True(actual);
		Assert.Equal(domain, actualDomain);
		Assert.Equal(param, actualParam);
	}

	[Theory]
	[InlineData("")]
	[InlineData("something invalid")]
	[InlineData("$domain=just_domain")]
	[InlineData("$cookie=_wp_visitor")]
	public void TestExtractDomainAndParamInvalid(string line)
	{
		bool actual = Utilities.ExtractDomainAndParam(line, out _, out _);
		Assert.False(actual);
	}

	#endregion
}
