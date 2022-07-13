using System.Collections.Generic;
using Resta.Domain;
using Resta.Model;
using Xunit;

namespace TestResta
{
	public class TestValidations
	{
		[Fact]
		public void ContextStatus()
		{
			var cparams = new RestaParams();
			var bookData = new RunBook();
			var context = executeBook("", cparams, bookData);
			Assert.True(context.HasErrors);
			Assert.Equal("Book scripts is missing", context.ErrorMessage);
			
			bookData.scripts = createEmptyArray();
			context = executeBook("", cparams, bookData);
			Assert.True(context.HasErrors);
			Assert.Equal("Book environment is missing", context.ErrorMessage);

			context = executeBook("dev", cparams, bookData);
			Assert.True(context.HasErrors);
			Assert.Equal("Cannot find environment file", context.ErrorMessage);
		}

		private ProcessContext executeBook(string env, RestaParams cparams, RunBook bookData)
		{
			var context = new ProcessContext();
			var book = new RestApiBook();
			book.Execute(context, env, cparams, bookData);
			return context;
		}

		private string[] createEmptyArray()
		{
			var list = new List<string>();
			return list.ToArray();
		}
	}
}