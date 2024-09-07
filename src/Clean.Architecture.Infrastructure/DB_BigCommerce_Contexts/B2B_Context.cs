﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Clean.Architecture.Infrastructure.HttpObjectMapping;
using Microsoft.Extensions.Configuration;

namespace Clean.Architecture.Infrastructure.BigCommerce;

public record extraFieldsPayload {
  public List<extraField>? payload;
  }
public record extraField {
  public string? fieldName;
  public string? fieldValue;
  };
public class B2B_Context
{
  private readonly HttpClient _httpClient;

  public B2B_Context(HttpClient httpClient, IConfiguration config)
  {
    _httpClient = httpClient;
    _httpClient.BaseAddress = new Uri(config["env:B2B_ENDPOINT"]!);
    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    _httpClient.DefaultRequestHeaders.Add("authToken", config["env:B2B_TOKEN"]);

  }

  public async Task<HttpResponseMessage> GetAllQuotes()
  {
    return await _httpClient.GetAsync(_httpClient.BaseAddress + "/rfq");
  }
  public async Task<HttpResponseMessage> GetQuote(int quoteId)
  {
    return await _httpClient.GetAsync(_httpClient.BaseAddress + "/rfq/" + quoteId);
  }

  public async Task<int> GetB2BCompanyIdUsingB2COrderId(int orderId)
  {
    var resp =  await _httpClient.GetAsync(_httpClient.BaseAddress + "/companies?bcOrderId=" + orderId);
    var respString = await resp.Content.ReadAsStringAsync();
    Http_B2B_Companies_Payload? data = JsonSerializer.Deserialize<Http_B2B_Companies_Payload>(respString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (data != null) return data.data[0].companyId;
    else throw new Exception("null resp, GetB2BCompanyIdUsingB2COrderId()");
  }
  public async Task<Http_B2B_CompanyUser> GetB2BCompanyUserUsingEmail(string email)
  {
    var resp = await _httpClient.GetAsync(_httpClient.BaseAddress + "/users?email=" + email);
    var respString = await resp.Content.ReadAsStringAsync();
    Http_B2B_CompanyUser_Payload? data = JsonSerializer.Deserialize<Http_B2B_CompanyUser_Payload>(respString,new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (data != null) return data.data[0];
    else throw new Exception("null resp, GetB2BCompanyUserUsingEmail()");
  }

  public async Task<Http_B2B_Company> GetCompany(int companyId)
  {
    var resp =  await _httpClient.GetAsync(_httpClient.BaseAddress + "/companies/" + companyId);
    var respString = await resp.Content.ReadAsStringAsync();
    Console.WriteLine(respString);
    Http_B2B_Company_Payload? data = JsonSerializer.Deserialize<Http_B2B_Company_Payload>(respString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    Console.WriteLine(respString);
    if (data != null) return data.data;
    else throw new Exception("null resp, GetCompanies()");
  }

  public async Task UpdateB2BCompanyCredits(int companyId, double credits)
  {
 
    var content = new extraFieldsPayload();
    var httpContent = new StringContent(
      JsonSerializer.Serialize($"{{\"extraFields\": [{{\"fieldName\":\"Remaining_Credits\"," +
                               $"\"fieldValue\":\"{credits}\"}}," +
                               $"{{\"fieldName\":\"Territory\"," +
                               $"\"fieldValue\":\"Steven's Territory\"}} ]}}"),
      Encoding.UTF8, "application/json"
      );
    Console.WriteLine(httpContent);
    Console.WriteLine(JsonSerializer.Serialize($"{{\"extraFields\": [{{\"fieldName\":\"Remaining_Credits\"," +
                                               $"\"fieldValue\":\"{credits}\"}}," +
                                               $"{{\"fieldName\":\"Territory\"," +
                                               $"\"fieldValue\":\"Steven's Territory\"}} ]}}"));
    var resp = await _httpClient.PutAsync(_httpClient.BaseAddress + "/companies/" + companyId, httpContent);
    Console.WriteLine(resp.StatusCode.ToString());
    
  }

  public async Task ChangeerB2BOrdStatus(int orderId)
  {
    var requestContent = new StringContent(JsonSerializer.Serialize("{status: \"Insufficient funds\"}"), Encoding.UTF8, "application/json");
    var resp = await _httpClient.PutAsync(_httpClient.BaseAddress + "/orders/" + orderId,  requestContent);
    var respString = resp.Content.ReadAsStringAsync();
    Console.WriteLine(respString.Status);
    Console.WriteLine(respString);
  }

}
