using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal.Filters;

namespace HomeExercises
{
	public class NumberValidatorTests
	{
		[TestCase(-1,2,true, TestName = "Negative precisiion")]
		[TestCase(1, -2, false, TestName = "Negative scale")]
		[TestCase(1, 2, false, TestName = "Scale greater than precision")]
		public void ValidatorResultExceptionThrow(int precision, int scale, bool onlyPositive)
		{
			Assert.That(() => new NumberValidator(precision, scale, onlyPositive), Throws.ArgumentException);
		}

		[TestCase(17, 6, true, ".", ExpectedResult = false, TestName = "Bad format: only a point")]
		[TestCase(17, 6, true, "1.23.4", ExpectedResult = false, TestName = "Bad format: two points")]
		[TestCase(17, 6, true, "1..234", ExpectedResult = false, TestName = "Bad format: two points together")]
		[TestCase(17, 6, true, "1.", ExpectedResult = false, TestName = "Bad format: point without fraction part")]
		[TestCase(17, 6, true, ".3", ExpectedResult = false, TestName = "Bad format: point without int part")]
		[TestCase(17, 2, true, "", ExpectedResult = false, TestName = "Empty input")]
		[TestCase(17, 2, true, "000000.00", ExpectedResult = true, TestName = "Zero number")]
		[TestCase(4, 2, true, "1,33", ExpectedResult = true, TestName = "Number with comma")]
		[TestCase(4, 2, true, "1.333", ExpectedResult = false, TestName = "Bigger scale than should be")]
		[TestCase(3, 2, true, "12.34", ExpectedResult = false, TestName = "Bigger precision than should be")]
		[TestCase(3, 2, false, "-8.88", ExpectedResult = false, TestName = "Bigger precision due to sign")]
		[TestCase(3, 2, true, "+1.11", ExpectedResult = false, TestName = "Bigger precision due to sign(minus)")]
		[TestCase(3, 2, true, "+1.98", ExpectedResult = false, TestName = "Bigger precision due to sign(plus)")]
		[TestCase(4, 2, true, "-2", ExpectedResult = false, TestName = "Negative number with onlyPositive parameter = true")]
		[TestCase(3, 2, true, "a.sd", ExpectedResult = false, TestName = "Not a number")]
		public bool ValidatorResult(int precision, int scale, bool onlyPositive, string value="0.0")
		{
			return new NumberValidator(precision, scale, onlyPositive).IsValidNumber(value);
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