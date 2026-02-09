using System;
using BaseLibrary.DTOs;

namespace ClientLibrary.Helpers;

public class GetHttpClient(IHttpClientFactory httpClientFactory,LocalStorageServices localStorageServices)
{
    private const string HeaderKey = "Authorization";
    public async Task<HttpClient> GetPrivatHttpClient()
    {

        var client = httpClientFactory.CreateClient();
        var stringToken = await localStorageServices.GetToken();
        if (string.IsNullOrEmpty(stringToken)) return client;

        var DeserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
        if (DeserializeToken == null) return client;

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", DeserializeToken.Token);
        return client;
    }
    public HttpClient GetPublicHttpClient()
    {
        var client = httpClientFactory.CreateClient("SystemApiClient");
        client.DefaultRequestHeaders.Remove(HeaderKey);
        return client;
    }
}
