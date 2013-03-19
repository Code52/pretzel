using System;
using System.ComponentModel.Composition;

namespace Pretzel.Logic.Extensibility.Extensions
{
	[Export(typeof(IFilter))]
	public class PrettifyUrlFilter : IFilter
	{
		public string Name
		{
			get { return "PrettifyUrl"; }
		}

		public static string PrettifyUrl(string input)
		{
			return input.Replace("index.html", String.Empty);
		}

	}
}