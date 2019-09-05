using System;
using Resta.Domain;
using Xunit;

namespace TestResta
{
	public class TestProcessContext
	{
		[Fact]
		public void ContextStatus()
		{
			var context = new ProcessContext();
			Assert.False(context.HasErrors);
			var data = context.SetError(12.2, "Error MSG");
			Assert.True(context.HasErrors);
			Assert.IsType<double>(data);
			Assert.Equal(12.2, data);
			Assert.Equal("Error MSG", context.ErrorMessage);
		}
	}
}