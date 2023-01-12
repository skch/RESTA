namespace Resta.Model;

public class WrapException
{

	public string exception = "";
	public WrapException(Exception ex)
	{
		exception = ex.Message;
	}
}