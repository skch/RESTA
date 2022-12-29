using System;
using System.IO;
using Resta.Domain;
using Resta.Model;
using Xunit;

namespace TestResta;

public class TestPublicCalls
{

	private void failParamsValidation(RestaParams cparams, string emsg)
	{
		var context = new ProcessContext();
		string dir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
		Directory.SetCurrentDirectory(dir+"../../../scripts");
		var dataValidator = new RestaScriptValidator();
		dataValidator.LoadRunbook(context, cparams);
		var runbook = dataValidator.ValidateScripts(context);
		Assert.Equal(emsg, context.ErrorMessage);
		Assert.True(context.HasErrors);
	}
	
	[Fact]
	public void FailRunbookIsMissing()
	{
		var rp = new RestaParams();
		failParamsValidation(rp, "Runbook name is missing");
	}
	
	[Fact]
	public void FailFindFile()
	{
		var rp = new RestaParams
		{
			bookName = "wrong-book"
		};
		failParamsValidation(rp, "Cannot find runbook file");
	}
	
	[Fact]
	public void FailRunbookIsWrong()
	{
		var rp = new RestaParams
		{
			bookName = "test-book"
		};
		failParamsValidation(rp, "Cannot find runbook file");
	}
	
	[Fact]
	public void FailEnvironmentIsMissing()
	{
		var rp = new RestaParams
		{
			isScript = true,
			bookName = "test-valid",
		};
		failParamsValidation(rp, "Environment is missing");
	}
	
	[Fact]
	public void FailRunScript()
	{
		var context = new ProcessContext();
		var rp = new RestaParams
		{
			isScript = true,
			bookName = "test-bad",
			environmentName = "global", 
			keepSuccess = true
		};
		var dataValidator = new RestaScriptValidator();
		dataValidator.LoadRunbook(context, rp);
		var runbook = dataValidator.ValidateScripts(context);
		
		var book = new RestApiBook();
		book.Execute(context, rp, runbook);
		
		
		Assert.Null(context.ErrorMessage);
		Assert.False(context.HasErrors);
		
	}
	
	[Fact]
	public void SuccessRunScript()
	{
		var context = new ProcessContext();
		var rp = new RestaParams
		{
			isScript = true,
			bookName = "test-valid",
			environmentName = "global",
			keepSuccess = true
		};
		var dataValidator = new RestaScriptValidator();
		dataValidator.LoadRunbook(context, rp);
		var runbook = dataValidator.ValidateScripts(context);
		
		var book = new RestApiBook();
		book.Execute(context, rp, runbook);
		
		Assert.Null(context.ErrorMessage);
		Assert.False(context.HasErrors);
		
	}
}