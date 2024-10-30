using Microsoft.Extensions.Caching.Memory;

namespace Base_BE.Services;

public class OTPService
{
    private readonly IMemoryCache _memoryCache;

    public OTPService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public string GenerateOTP()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    public void SaveOTP(string email, string otp)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(2))  // OTP hết hạn nếu không được truy cập trong vòng 2 phút
            .SetPriority(CacheItemPriority.High);            // Đặt ưu tiên cao để giữ trong bộ nhớ đệm

        _memoryCache.Set(email, otp, cacheEntryOptions);
    }

    public bool VerifyOTP(string email, string otp)
    {
        if (_memoryCache.TryGetValue(email, out string correctOtp))
        {
            return otp == correctOtp;
        }
        return false;
    }
}
