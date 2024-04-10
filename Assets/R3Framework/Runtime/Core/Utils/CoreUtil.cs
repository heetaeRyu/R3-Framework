using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

namespace Netmarble.Core
{
	public static class CoreUtil
	{
		public static async UniTaskVoid MainThreadCall(Action listener)
		{
			await UniTask.SwitchToMainThread();
			try
			{
				listener?.Invoke();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
			}
		}

		public static string ConvertHtmlToTextSimple(string result)
		{
			if (result != null)
			{
				// replace special characters:
				result = Regex.Replace(result,
					@" ", " ",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&bull;", " * ",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&lsaquo;", "<",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&rsaquo;", ">",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&trade;", "(tm)",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&copy;", "(c)",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&reg;", "(r)",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&frasl;", "/",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&lt;", "<",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"&#39;", "'",
					RegexOptions.IgnoreCase);
				return Regex.Replace(result,
					@"&gt;", ">",
					RegexOptions.IgnoreCase);
			}

			return null;
		}

		public static string ConvertHtmlToText(string source)
		{
			try
			{
				string result;

				// Remove HTML Development formatting
				// Replace line breaks with space
				// because browsers inserts space
				result = source.Replace("\r", " ");
				// Replace line breaks with space
				// because browsers inserts space
				result = result.Replace("\n", " ");
				// Remove step-formatting
				result = result.Replace("\t", string.Empty);
				// Remove repeating spaces because browsers ignore them
				result = Regex.Replace(result,
					@"( )+", " ");

				// Remove the header (prepare first by clearing attributes)
				result = Regex.Replace(result,
					@"<( )*head([^>])*>", "<head>",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"(<( )*(/)( )*head( )*>)", "</head>",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					"(<head>).*(</head>)", string.Empty,
					RegexOptions.IgnoreCase);

				// remove all scripts (prepare first by clearing attributes)
				result = Regex.Replace(result,
					@"<( )*script([^>])*>", "<script>",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"(<( )*(/)( )*script( )*>)", "</script>",
					RegexOptions.IgnoreCase);
				//result = Regex.Replace(result,
				//         @"(<script>)([^(<script>\.</script>)])*(</script>)",
				//         string.Empty,
				//         RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"(<script>).*(</script>)", string.Empty,
					RegexOptions.IgnoreCase);

				// remove all styles (prepare first by clearing attributes)
				result = Regex.Replace(result,
					@"<( )*style([^>])*>", "<style>",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"(<( )*(/)( )*style( )*>)", "</style>",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					"(<style>).*(</style>)", string.Empty,
					RegexOptions.IgnoreCase);

				// insert tabs in spaces of <td> tags
				result = Regex.Replace(result,
					@"<( )*td([^>])*>", "\t",
					RegexOptions.IgnoreCase);

				// insert line breaks in places of <BR> and <LI> tags
				result = Regex.Replace(result,
					@"<( )*br( )*>", "\r",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"<( )*li( )*>", "\r",
					RegexOptions.IgnoreCase);

				// insert line paragraphs (double line breaks) in place
				// if <P>, <DIV> and <TR> tags
				result = Regex.Replace(result,
					@"<( )*div([^>])*>", "\r\r",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"<( )*tr([^>])*>", "\r\r",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					@"<( )*p([^>])*>", "\r\r",
					RegexOptions.IgnoreCase);

				// Remove remaining tags like <a>, links, images,
				// comments etc - anything that's enclosed inside < >
				result = Regex.Replace(result,
					@"<[^>]*>", string.Empty,
					RegexOptions.IgnoreCase);

				result = ConvertHtmlToTextSimple(result);
				// Remove all others. More can be added, see
				// http://hotwired.lycos.com/webmonkey/reference/special_characters/
				result = Regex.Replace(result,
					@"&(.{2,6});", string.Empty,
					RegexOptions.IgnoreCase);

				// for testing
				//Regex.Replace(result,
				//       this.txtRegex.Text,string.Empty,
				//       RegexOptions.IgnoreCase);

				// make line breaking consistent
				result = result.Replace("\n", "\r");

				// Remove extra line breaks and tabs:
				// replace over 2 breaks with 2 and over 4 tabs with 4.
				// Prepare first to remove any whitespaces in between
				// the escaped characters and remove redundant tabs in between line breaks
				result = Regex.Replace(result,
					"(\r)( )+(\r)", "\r\r",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					"(\t)( )+(\t)", "\t\t",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					"(\t)( )+(\r)", "\t\r",
					RegexOptions.IgnoreCase);
				result = Regex.Replace(result,
					"(\r)( )+(\t)", "\r\t",
					RegexOptions.IgnoreCase);
				// Remove redundant tabs
				result = Regex.Replace(result,
					"(\r)(\t)+(\r)", "\r\r",
					RegexOptions.IgnoreCase);
				// Remove multiple tabs following a line break with just one tab
				result = Regex.Replace(result,
					"(\r)(\t)+", "\r\t",
					RegexOptions.IgnoreCase);
				// Initial replacement target string for line breaks
				string breaks = "\r\r\r";
				// Initial replacement target string for tabs
				string tabs = "\t\t\t\t\t";
				for (int index = 0; index < result.Length; index++)
				{
					result = result.Replace(breaks, "\r\r");
					result = result.Replace(tabs, "\t\t\t\t");
					breaks = breaks + "\r";
					tabs = tabs + "\t";
				}

				// That's it.
				return result;
			}
			catch
			{
				return source;
			}
		}
	}
}