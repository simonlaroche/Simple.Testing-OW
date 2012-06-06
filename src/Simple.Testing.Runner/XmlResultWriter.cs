namespace Simple.Testing.Runner
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Xml;
	using Framework;

	/// <summary>
	/// Summary description for XmlResultWriter.
	/// </summary>
	public class XmlResultWriter
	{
		private MemoryStream memoryStream;
		private TextWriter writer;
		private XmlTextWriter xmlWriter;

		public XmlResultWriter(string fileName)
		{
			xmlWriter = new XmlTextWriter(new StreamWriter(fileName, false, Encoding.UTF8));
		}

		public XmlResultWriter(TextWriter writer)
		{
			memoryStream = new MemoryStream();
			this.writer = writer;
			xmlWriter = new XmlTextWriter(new StreamWriter(memoryStream, Encoding.UTF8));
		}

		

		private void InitializeXmlFile(IEnumerable<RunResult> results)
		{
			var passed = results.Count(x => x.Passed);
			var failed = results.Count(x => !x.Passed);
			var name = results.First().FoundOnMemberInfo.Module.Assembly.Location;

			xmlWriter.Formatting = Formatting.Indented;
			xmlWriter.WriteStartDocument(false);
			xmlWriter.WriteComment("This file represents the results of running a test suite");

			xmlWriter.WriteStartElement("test-results");

			xmlWriter.WriteAttributeString("name", name);
			xmlWriter.WriteAttributeString("total", (passed + failed).ToString());
			xmlWriter.WriteAttributeString("errors", "0");
			xmlWriter.WriteAttributeString("failures", failed.ToString());
			xmlWriter.WriteAttributeString("not-run", "0");
			xmlWriter.WriteAttributeString("inconclusive", "0");
			xmlWriter.WriteAttributeString("ignored", "0");
			xmlWriter.WriteAttributeString("skipped", "0");
			xmlWriter.WriteAttributeString("invalid", "0");

			var now = DateTime.Now;
			xmlWriter.WriteAttributeString("date", XmlConvert.ToString(now, "yyyy-MM-dd"));
			xmlWriter.WriteAttributeString("time", XmlConvert.ToString(now, "HH:mm:ss"));
			WriteEnvironment();
			WriteCultureInfo();
		}

		private void WriteCultureInfo()
		{
			xmlWriter.WriteStartElement("culture-info");
			xmlWriter.WriteAttributeString("current-culture",
			                               CultureInfo.CurrentCulture.ToString());
			xmlWriter.WriteAttributeString("current-uiculture",
			                               CultureInfo.CurrentUICulture.ToString());
			xmlWriter.WriteEndElement();
		}

		private void WriteEnvironment()
		{
			xmlWriter.WriteStartElement("environment");
			xmlWriter.WriteAttributeString("nunit-version",
			                               Assembly.GetExecutingAssembly().GetName().Version.ToString());
			xmlWriter.WriteAttributeString("clr-version",
			                               Environment.Version.ToString());
			xmlWriter.WriteAttributeString("os-version",
			                               Environment.OSVersion.ToString());
			xmlWriter.WriteAttributeString("platform",
			                               Environment.OSVersion.Platform.ToString());
			xmlWriter.WriteAttributeString("cwd",
			                               Environment.CurrentDirectory);
			xmlWriter.WriteAttributeString("machine-name",
			                               Environment.MachineName);
			xmlWriter.WriteAttributeString("user",
			                               Environment.UserName);
			xmlWriter.WriteAttributeString("user-domain",
			                               Environment.UserDomainName);
			xmlWriter.WriteEndElement();
		}

		public void SaveTestResult(IEnumerable<RunResult> results)
		{
			InitializeXmlFile(results);
			WriteResultElement(null,results);
			TerminateXmlFile();
		}


		private void WriteResultElement(string @namespace, IEnumerable<RunResult> results)
		{
			IEnumerable<string> rootNamespaces;
			if (@namespace == null)
			{
				rootNamespaces = results.Select(x => x.FoundOnMemberInfo.DeclaringType.Namespace.Split('.').First()).Distinct();
			}
			else
			{
				rootNamespaces = results.Where(x => x.FoundOnMemberInfo.DeclaringType.Namespace.StartsWith(@namespace) && x.FoundOnMemberInfo.DeclaringType.Namespace != @namespace)
					.Select(x =>
					        	{
					        		var ns =
					        			x.FoundOnMemberInfo.DeclaringType.Namespace.Split('.').Skip(@namespace.Split('.').Length).FirstOrDefault
					        				();
									if (ns == null)
									{
										return null;
									}
					        		return @namespace + "." + ns;
					        	}).Where(x=> x!= null).Distinct();
			}
			
			foreach (var rootNamespace in rootNamespaces)
			{
				StartTestSuite(rootNamespace.Split('.').Last(),
				               !results.Any(
				               	x => x.Passed == false && x.FoundOnMemberInfo.DeclaringType.Namespace.StartsWith(rootNamespace)));

				WriteChildResultElement(rootNamespace, results);	
					
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			}

//			StartTestElement(results);
//
//			WriteCategoriesElement(results);
//			WritePropertiesElement(results);
//
//			switch (results.ResultState)
//			{
//				case ResultState.Ignored:
//				case ResultState.NotRunnable:
//				case ResultState.Skipped:
//					WriteReasonElement(results);
//					break;
//
//				case ResultState.Failure:
//				case ResultState.Error:
//				case ResultState.Cancelled:
//					if (!results.Test.IsSuite || results.FailureSite == FailureSite.SetUp)
//						WriteFailureElement(results);
//					break;
//				case ResultState.Success:
//				case ResultState.Inconclusive:
//					if (results.Message != null)
//						WriteReasonElement(results);
//					break;
//			}
//
//			if (results.HasResults)
//				WriteChildResults(results);
//
//			xmlWriter.WriteEndElement(); // test element
		}

		private void WriteChildResultElement(string rootNamespace, IEnumerable<RunResult> results)
		{
			//Select child namespace
			WriteResultElement(rootNamespace, results);

			//Select test-cases
			foreach (var group in results.Where(x=> x.FoundOnMemberInfo.DeclaringType.Namespace == rootNamespace)
				.GroupBy(x=>x.FoundOnMemberInfo.DeclaringType))
			{

				StartTestSuite(group.First().FoundOnMemberInfo.DeclaringType.Name, group.All(x=>x.Passed));

				foreach (var runResult in group)
				{
					StartTestSuite(runResult.Name.Replace('_', ' '), runResult.Passed);

					if (runResult.Thrown != null)
					{
						xmlWriter.WriteStartElement("test-case");
						xmlWriter.WriteAttributeString("name", runResult.Name);
						xmlWriter.WriteAttributeString("success", runResult.Passed.ToString());
						xmlWriter.WriteAttributeString("executed", true.ToString());
						xmlWriter.WriteAttributeString("asserts", 0.ToString());
						xmlWriter.WriteStartElement("failure");
						xmlWriter.WriteStartElement("message");
						xmlWriter.WriteCData(runResult.Message + runResult.Thrown.Message);
						xmlWriter.WriteEndElement();

						xmlWriter.WriteStartElement("stack-trace");
						xmlWriter.WriteCData(runResult.Thrown.StackTrace);

						xmlWriter.WriteEndElement();
						xmlWriter.WriteEndElement();
						xmlWriter.WriteEndElement();
					}

					foreach (var ex in runResult.Expectations)
					{
						xmlWriter.WriteStartElement("test-case");
						xmlWriter.WriteAttributeString("name", ex.Text);
						xmlWriter.WriteAttributeString("success", ex.Passed.ToString());
						xmlWriter.WriteAttributeString("executed", true.ToString());
						xmlWriter.WriteAttributeString("asserts", 1.ToString());
						xmlWriter.WriteEndElement();
					}

					xmlWriter.WriteEndElement();
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			}


		}

		private void TerminateXmlFile()
		{
			try
			{
				xmlWriter.WriteEndElement(); // test-results
				xmlWriter.WriteEndDocument();
				xmlWriter.Flush();

				if (memoryStream != null && writer != null)
				{
					memoryStream.Position = 0;
					using (var rdr = new StreamReader(memoryStream))
					{
						writer.Write(rdr.ReadToEnd());
					}
				}

				xmlWriter.Close();
			}
			finally
			{
				//writer.Close();
			}
		}

	
		private void StartTestSuite(string name, bool success)
		{
			xmlWriter.WriteStartElement("test-suite");
			xmlWriter.WriteAttributeString("name", name);
			xmlWriter.WriteAttributeString("success", success.ToString());
			xmlWriter.WriteStartElement("results");

			
		}


//		private void StartTestElement(RunResult result)
//		{
//			if (result.Test.IsSuite)
//			{
//				xmlWriter.WriteStartElement("test-suite");
//				xmlWriter.WriteAttributeString("type", result.Test.TestType);
//				xmlWriter.WriteAttributeString("name", result.Name);
//			}
//			else
//			{
//				xmlWriter.WriteStartElement("test-case");
//				xmlWriter.WriteAttributeString("name", result.SpecificationName);
//			}
//
//			if (result.Message != null)
//				xmlWriter.WriteAttributeString("description", result.Message);
//
//			xmlWriter.WriteAttributeString("executed", result.Executed.ToString());
//			xmlWriter.WriteAttributeString("result", result.Passed.ToString());
//
//			if (result.Executed)
//			{
//				xmlWriter.WriteAttributeString("success", result.IsSuccess.ToString());
//				xmlWriter.WriteAttributeString("time", result.Time.ToString("#####0.000", NumberFormatInfo.InvariantInfo));
//				xmlWriter.WriteAttributeString("asserts", result.AssertCount.ToString());
//			}
//		}

//		private void WriteCategoriesElement(TestResult result)
//		{
//			if (result.Test.Categories != null && result.Test.Categories.Count > 0)
//			{
//				xmlWriter.WriteStartElement("categories");
//				foreach (string category in result.Test.Categories)
//				{
//					xmlWriter.WriteStartElement("category");
//					xmlWriter.WriteAttributeString("name", category);
//					xmlWriter.WriteEndElement();
//				}
//				xmlWriter.WriteEndElement();
//			}
//		}

//		private void WritePropertiesElement(TestResult result)
//		{
//			IDictionary props = result.Test.Properties;
//
//			if (result.Test.Properties != null && props.Count > 0)
//			{
//				int nprops = 0;
//
//				foreach (string key in result.Test.Properties.Keys)
//				{
//					if (!key.StartsWith("_"))
//					{
//						object val = result.Test.Properties[key];
//						if (val != null)
//						{
//							if (nprops == 0)
//								xmlWriter.WriteStartElement("properties");
//
//							xmlWriter.WriteStartElement("property");
//							xmlWriter.WriteAttributeString("name", key);
//							xmlWriter.WriteAttributeString("value", val.ToString());
//							xmlWriter.WriteEndElement();
//
//							++nprops;
//						}
//					}
//				}
//
//				if (nprops > 0)
//					xmlWriter.WriteEndElement();
//			}
//		}

//		private void WriteReasonElement(TestResult result)
//		{
//			xmlWriter.WriteStartElement("reason");
//			xmlWriter.WriteStartElement("message");
//			xmlWriter.WriteCData(result.Message);
//			xmlWriter.WriteEndElement();
//			xmlWriter.WriteEndElement();
//		}
//
//		private void WriteFailureElement(TestResult result)
//		{
//			xmlWriter.WriteStartElement("failure");
//
//			xmlWriter.WriteStartElement("message");
//			WriteCData(result.Message);
//			xmlWriter.WriteEndElement();
//
//			xmlWriter.WriteStartElement("stack-trace");
//			if (result.StackTrace != null)
//				WriteCData(StackTraceFilter.Filter(result.StackTrace));
//			xmlWriter.WriteEndElement();
//
//			xmlWriter.WriteEndElement();
//		}
//
//		private void WriteChildResults(TestResult result)
//		{
//			xmlWriter.WriteStartElement("results");
//
//			if (result.HasResults)
//				foreach (TestResult childResult in result.Results)
//					WriteResultElement(childResult);
//
//			xmlWriter.WriteEndElement();
//		}

	

		/// <summary>
		/// 	Makes string safe for xml parsing, replacing control chars with '?'
		/// </summary>
		/// <param name = "encodedString">string to make safe</param>
		/// <returns>xml safe string</returns>
		private static string CharacterSafeString(string encodedString)
		{
			/*The default code page for the system will be used.
			Since all code pages use the same lower 128 bytes, this should be sufficient
			for finding uprintable control characters that make the xslt processor error.
			We use characters encoded by the default code page to avoid mistaking bytes as
			individual characters on non-latin code pages.*/
			var encodedChars = Encoding.Default.GetChars(Encoding.Default.GetBytes(encodedString));

			var pos = new ArrayList();
			for (var x = 0; x < encodedChars.Length; x++)
			{
				var currentChar = encodedChars[x];
				//unprintable characters are below 0x20 in Unicode tables
				//some control characters are acceptable. (carriage return 0x0D, line feed 0x0A, horizontal tab 0x09)
				if (currentChar < 32 && (currentChar != 9 && currentChar != 10 && currentChar != 13))
				{
					//save the array index for later replacement.
					pos.Add(x);
				}
			}
			foreach (int index in pos)
			{
				encodedChars[index] = '?'; //replace unprintable control characters with ?(3F)
			}
			return Encoding.Default.GetString(Encoding.Default.GetBytes(encodedChars));
		}

		private void WriteCData(string text)
		{
			var start = 0;
			while (true)
			{
				var illegal = text.IndexOf("]]>", start);
				if (illegal < 0)
					break;
				xmlWriter.WriteCData(text.Substring(start, illegal - start + 2));
				start = illegal + 2;
				if (start >= text.Length)
					return;
			}

			if (start > 0)
				xmlWriter.WriteCData(text.Substring(start));
			else
				xmlWriter.WriteCData(text);
		}

		
	}
}