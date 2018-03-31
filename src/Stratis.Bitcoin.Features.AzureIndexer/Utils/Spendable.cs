using System;
using NBitcoin;

namespace Stratis.Bitcoin.Features.AzureIndexer.Utils
{
    public class Spendable : IBitcoinSerializable
    {
        private OutPoint _outPoint;
        private TxOut _out;

        public Spendable()
        {

        }
        public Spendable(OutPoint output, TxOut txout)
        {
            _out = txout ?? throw new ArgumentNullException("txout");
            _outPoint = output ?? throw new ArgumentNullException("output");
        }

        public void ReadWrite(BitcoinStream stream)
        {
            stream.ReadWrite(ref _outPoint);
            if(stream.Serializing)
            {
                var compressor = new TxOutCompressor(_out);
                stream.ReadWrite(ref compressor);
            }
            else
            {
                var compressor = new TxOutCompressor();
                stream.ReadWrite(ref compressor);
                _out = compressor.TxOut;
            }
        }

        public override string ToString()
        {
            return TxOut != null && TxOut.Value != null ? TxOut.Value.ToString() : "?";
        }

        public OutPoint OutPoint => _outPoint;

        public TxOut TxOut => _out;
    }
}
