﻿using System.Net.Mime;
using System.Text;
using System.Text.Json;
using NftApi.Http.Models;

namespace NftApi.Http.Services;

public class CnftIoClient
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;

    public CnftIoClient(HttpClient httpClient) 
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.cnft.io");
    }

    public async Task<List<CnftIoListing>> FetchAllListings(CnftIoPayload payload)
    {
        var responseMessage = await _httpClient.PostAsync("/market/listings",
            new StringContent(
                JsonSerializer.Serialize(payload, DefaultSerializerOptions),
                Encoding.UTF8,
                MediaTypeNames.Application.Json));

        var response = await JsonSerializer.DeserializeAsync<CnftIoResponse>(responseMessage.Content.ReadAsStream(), DefaultSerializerOptions);
        var listings = new List<CnftIoListing>();

        while (true)
        {
            if (response.Results.Count == 0)
            {
                break;
            }

            listings.AddRange(response.Results);
            payload.Page++;

            responseMessage = await _httpClient.PostAsync("/market/listings",
                new StringContent(
                    JsonSerializer.Serialize(payload, DefaultSerializerOptions),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json));

            response = await JsonSerializer.DeserializeAsync<CnftIoResponse>(responseMessage.Content.ReadAsStream(), DefaultSerializerOptions);
        }

        return listings;
    }
}
