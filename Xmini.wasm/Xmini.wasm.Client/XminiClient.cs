using System.Net.Http.Json;
using Xmini.shared.Dto;

namespace Xmini.wasm.Client
{
    public class XminiClient(HttpClient httpClient)
    {
        public async Task<List<FollowerDto>> GetFollowerAsync(string userId)
        {
            List<FollowerDto>? result = await httpClient.GetFromJsonAsync<List<FollowerDto>>($"api/follower/{userId}");
            return result ?? new();
        }

        public async Task<List<TweetDto>> GetLastTweetsAsync()
        {
            List<TweetDto>? result = await httpClient.GetFromJsonAsync<List<TweetDto>>($"api/tweets/last");
            return result ?? new();
        }
    }
}
