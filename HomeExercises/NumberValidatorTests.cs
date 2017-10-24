﻿using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal.Filters;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[TestCase(-1,2, TestName = "NegativePrecisiion_isExceptionCase")]
		[TestCase(1, -2, TestName = "Negative scale_isExceptionCase")]
		[TestCase(1, 2, TestName = "ScaleGreaterThanPrecision_isExceptionCase")]
		public void ValidatorResultExceptionThrow(int precision, int scale)
		{
			Assert.That(() => new NumberValidator(precision, scale), Throws.ArgumentException);
		}

		// FormatTests
		[TestCase(".", ExpectedResult = false, TestName = "onlyAPoint_isBadFormat")]
		[TestCase("1.2.4", ExpectedResult = false, TestName = "twoPointsWithNumber_isBadFormat")]
		[TestCase("1..24", ExpectedResult = false, TestName = "twoPointsTogether_isBadFormat")]
		[TestCase("1.", ExpectedResult = false, TestName = "pointWithoutFractionPart_isBadFormat")]
		[TestCase(".3", ExpectedResult = false, TestName = "pointWithoutIntPart_isBadFormat")]
		[TestCase("", ExpectedResult = false, TestName = "emptyNumberString_isBadFormat")]
		[TestCase("a.sd", ExpectedResult = false, TestName = "notANumber_isBadFormat")]
		[TestCase(" 1.1", ExpectedResult = false, TestName = "numberWithSpace_isBadFormat")]
		[TestCase("1 .1", ExpectedResult = false, TestName = "numberWithSpaceInside_isBadFormat")]
		[TestCase(null, ExpectedResult = false, TestName = "nullInsteadNumber_isBadFormat")]
		//NumberRulesTests
		[TestCase("000.00", ExpectedResult = true, TestName = "zeroNumber_isGoodNumber")]
		[TestCase("1,33", ExpectedResult = true, TestName = "numberWithComma_isGoodNumber")]
		[TestCase("1.333", ExpectedResult = false, TestName = "biggerScale_thanShouldBe")]
		[TestCase("1342.34", ExpectedResult = false, TestName = "biggerPrecision_thanShouldBe")]
		[TestCase("+661.98", ExpectedResult = false, TestName = "biggerPrecision_dueToSign(plus)")]
		[TestCase("-2", ExpectedResult = false, TestName = "negativeNumber_WithOnlyPositiveParameter_ShouldBeFalse")]
		public bool ValidatorResult(string value)
		{
			return new NumberValidator(5, 2, true).IsValidNumber(value);
		}


		[TestCase("-1.11", ExpectedResult = false, TestName = "biggerPrecision_dueToSign(minus)")]
		[TestCase("-1.1", ExpectedResult = true, TestName = "negativeNumber_WithOnlyPositive=false_shouldBeTrue")]
		public bool ValidatorResultOnlyNegative(string value)
		{
			return new NumberValidator(3, 2).IsValidNumber(value);
		}

		[Test]
		public void FluentTests()
		{
			new NumberValidator(3, 2).IsValidNumber("-1.1").Should().BeTrue();
			new NumberValidator(3, 2).IsValidNumber("-1.11").Should().BeFalse();
			NumberValidator nv = new NumberValidator(5, 2, true);
			nv.IsValidNumber("1.1").Should().BeTrue();
			nv.IsValidNumber(" 1.1").Should().BeFalse();
			nv.IsValidNumber(".").Should().BeFalse();
			nv.IsValidNumber("1.2.4").Should().BeFalse();
			nv.IsValidNumber("1..2").Should().BeFalse();
			nv.IsValidNumber("1.").Should().BeFalse();
			nv.IsValidNumber("").Should().BeFalse();
			nv.IsValidNumber("null").Should().BeFalse();
			nv.IsValidNumber("a.sd").Should().BeFalse();
			nv.IsValidNumber("000.00").Should().BeTrue();
			nv.IsValidNumber("1,33").Should().BeTrue();
			nv.IsValidNumber("1.333").Should().BeFalse();
			nv.IsValidNumber("1111.11").Should().BeFalse();
			nv.IsValidNumber("+661.98").Should().BeFalse();
			nv.IsValidNumber("-2").Should().BeFalse();
		}
	}

public class NumberValidator
	{
		private readonly Regex numberRegex;
		private readonly bool onlyPositive;
		private readonly int precision;
		private readonly int scale;

		public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
		{
			this.precision = precision;
			this.scale = scale;
			this.onlyPositive = onlyPositive;
			if (precision <= 0)
				throw new ArgumentException("precision must be a positive number");
			if (scale < 0 || scale >= precision)
				throw new ArgumentException("scale must be a non-negative number less or equal than precision");
			numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
		}

		public bool IsValidNumber(string value)
		{
			// Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
			// описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
			// Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
			// целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
			// Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

			if (string.IsNullOrEmpty(value))
				return false;

			var match = numberRegex.Match(value);
			if (!match.Success)
				return false;

			// Знак и целая часть
			var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
			// Дробная часть
			var fracPart = match.Groups[4].Value.Length;

			if (intPart + fracPart > precision || fracPart > scale)
				return false;

			if (onlyPositive && match.Groups[1].Value == "-")
				return false;
			return true;
		}
	}
}