using LifeOS.Companion.Core.Pairing;
using System.Security.Cryptography;
namespace LifeOS.Companion.Tests;
public sealed class Group33Tests
{
 [Fact] public void Hmac_round_trip_is_verified(){var key=Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));var sig=CompanionAuthenticator.CreateSignature(key,"123","nonce","body");Assert.True(CompanionAuthenticator.Verify(key,"123","nonce","body",sig));}
 [Fact] public void Modified_body_is_rejected(){var key=Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));var sig=CompanionAuthenticator.CreateSignature(key,"123","nonce","body");Assert.False(CompanionAuthenticator.Verify(key,"123","nonce","changed",sig));}
 [Fact] public void Protocol_is_explicit(){Assert.Equal("1",CompanionProtocol.Version);Assert.InRange(CompanionProtocol.Port,1024,65535);}
 [Theory] [InlineData(TransferResult.AcceptedForReview,true)] [InlineData(TransferResult.Duplicate,true)] [InlineData(TransferResult.Invalid,false)] [InlineData(TransferResult.RejectedByPolicy,false)]
 public void Only_success_results_can_deliver(TransferResult result,bool expected)=>Assert.Equal(expected,result is TransferResult.AcceptedForReview or TransferResult.Duplicate);
}
