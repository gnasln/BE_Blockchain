using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

namespace Base_BE.Dtos
{
    [Function("submitVote", "bool")]
    public class SubmitBallotFunction : IEventDTO
    {
        [Parameter("uint256", "voterId", 1)]
        public BigInteger VoterId { get; set; }

        [Parameter("uint256[]", "candidateIds", 2)]
        public List<BigInteger> CandidateIds { get; set; }

        [Parameter("uint256", "votedTime", 3)]
        public BigInteger VotedTime { get; set; }

        [Parameter("string", "location", 4)]
        public string Location { get; set; }
    }


}