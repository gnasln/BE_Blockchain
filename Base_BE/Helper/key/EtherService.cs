using Nethereum.Web3.Accounts;
using Nethereum.Signer;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;
namespace Base_BE.Helper.key
{
    public class EtherService
    {
        public static Ether GenerateAddress(string privateKey)
        {
            var ether = new Ether();
            ether.PrivateKey = privateKey;

            // Tạo credentials từ private key
            var ecKeyPair = EthECKey.GenerateKey(privateKey.HexToByteArray());
            ether.PrivateKey = ecKeyPair.GetPrivateKeyAsBytes().ToHex();
            ether.PublicKey = ecKeyPair.GetPubKeyNoPrefix().ToHex();
            ether.Address = ecKeyPair.GetPublicAddress();

            return ether;
        }
    }
}
