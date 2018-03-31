using System;

namespace Stratis.Bitcoin.Features.AzureIndexer
{
	public class IndexerConfigurationErrorsException : Exception
	{
		public IndexerConfigurationErrorsException(string message) : base(message)
		{

		}
	}
}
