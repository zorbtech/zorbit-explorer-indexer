using System;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Features.BlockStore;
using Stratis.Bitcoin.Features.Consensus;
using Stratis.Bitcoin.Utilities;
using Zorbit.Features.Observatory.TableStorage;

namespace Zorbit.Observatory.IndexerD
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public static async Task MainAsync(string[] args)
        {
            try
            {
                var network = args.Contains("-testnet") ? Network.StratisTest : Network.StratisMain;
                var nodeSettings = new NodeSettings(network, ProtocolVersion.ALT_PROTOCOL_VERSION, args: args, loadConfiguration: false);

                // NOTES: running BTC and STRAT side by side is not possible yet as the flags for serialization are static
                var node = new FullNodeBuilder()
                    .UseNodeSettings(nodeSettings)
                    .UsePosConsensus()
                    .UseBlockStore(settings => settings.TxIndex = true)
                    .UseAzureIndexer()
                    .Build();

                // Run node.
                if (node != null)
                {
                    await node.RunAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was a problem initializing the node. Details: '{ex.Message}'");
                Console.ReadLine();
            }
        }
    }
}
