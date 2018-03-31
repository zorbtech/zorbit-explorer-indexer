using System;

namespace Stratis.Bitcoin.Features.AzureIndexer.Indexing
{
	public class IndexerConfigurationErrorsException : Exception
	{
		public IndexerConfigurationErrorsException(string message) : base(message)
		{

		}
	}
}
