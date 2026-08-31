<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
</Query>

void Main()
{
	// ===== AES 解密設定 =====
	var aesKey        = "lLRWKw4XA9k4hA4bbO1nqgY+XmDWY1p4/u0cteY1Nqo=";
	var aesCiphertext = "z01l+4yFKdJCaTqfy0TNPIviIrGT87OzO8PmVNF0GwW1bh4UQAKrednWlSfhQsCPw3UUjBH/PqHTIr22pDpvKEGy2v2pb8INY9Whm04C2J2c7dr+vVa2Azn4Q54HpEHBvOE+Unl3J4TVXnS+E4dJqAgsbbWL8kt0+9jBH7miBHbUgMwu5mTKUlFrLmN5s9aWJ6wGCAFRujGfqpSlat7xK7GzvM1ihyiGnxEnyHFtm+Uva9+v0UWFmvoybT57jaaiI2eFBHRAk765PEqi0YxNqqKP0U370QBcSEnSAAhnYeMkAx5jLJU//mIm77AKhrBl";

	// ===== Page 設定 =====
	var pageId          = "354612031548736";
	var pageAccessToken = DecryptAes(aesKey, aesCiphertext);

	pageAccessToken.Dump("解密後 Page Access Token");

	var requiredFields = new[] { "feed", "live_videos", "message_reactions", "messages", "messaging_postbacks" };

	// ===== 切換 Action =====
	var action = WebhookAction.Query;

	switch (action)
	{
		case WebhookAction.Query:       QuerySubscription(pageId, pageAccessToken, requiredFields); break;
		case WebhookAction.Subscribe:   Subscribe(pageId, pageAccessToken, requiredFields); break;
		case WebhookAction.Unsubscribe: Unsubscribe(pageId, pageAccessToken); break;
	}
}

enum WebhookAction
{
	Query,
	Subscribe,
	Unsubscribe
}

string DecryptAes(string base64Key, string base64Ciphertext)
{
	var key        = Convert.FromBase64String(base64Key);
	var ciphertext = Convert.FromBase64String(base64Ciphertext);
	var iv         = ciphertext.Take(16).ToArray();
	var data       = ciphertext.Skip(16).ToArray();

	using var aes = System.Security.Cryptography.Aes.Create();
	aes.Key     = key;
	aes.IV      = iv;
	aes.Mode    = System.Security.Cryptography.CipherMode.CBC;
	aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

	using var decryptor = aes.CreateDecryptor();
	var decrypted = decryptor.TransformFinalBlock(data, 0, data.Length);
	return Encoding.UTF8.GetString(decrypted);
}

void QuerySubscription(string pageId, string token, string[] requiredFields)
{
	var url = $"https://graph.facebook.com/v24.0/{pageId}/subscribed_apps?access_token={token}";

	using var client = new HttpClient();
	var response = client.GetAsync(url).Result;
	var json = response.Content.ReadAsStringAsync().Result;

	// 先 Dump 原始 response 確認結構
	json.Dump("原始 API Response");

	var result = System.Text.Json.JsonDocument.Parse(json);

	// 防呆：確認有 data key
	if (!result.RootElement.TryGetProperty("data", out var data))
	{
		result.RootElement.ToString().Dump("❌ 回應格式異常，完整內容");
		return;
	}

	if (data.GetArrayLength() == 0)
	{
		"❌ 尚未綁定任何 App".Dump("查詢結果");
		return;
	}

	var subscribedFields = data[0].GetProperty("subscribed_fields")
		.EnumerateArray()
		.Select(x => x.GetString())
		.ToHashSet();

	var checkResult = requiredFields.Select(f => new
	{
		Field  = f,
		Status = subscribedFields.Contains(f) ? "✅ 已綁定" : "❌ 未綁定"
	}).ToList();

	checkResult.Dump("Webhook 綁定狀態");

	var allBound = requiredFields.All(f => subscribedFields.Contains(f));
	(allBound ? "✅ 五個 Webhook 全部已綁定" : "⚠️ 部分 Webhook 尚未綁定").Dump("總結");
}

void Subscribe(string pageId, string token, string[] requiredFields)
{
	var url = $"https://graph.facebook.com/v24.0/{pageId}/subscribed_apps";

	var body = new Dictionary<string, string>
	{
		["subscribed_fields"] = string.Join(",", requiredFields),
		["access_token"] = token
	};

	using var client = new HttpClient();
	using var content = new FormUrlEncodedContent(body);
	var response = client.PostAsync(url, content).Result;
	var json = response.Content.ReadAsStringAsync().Result;

	new { StatusCode = response.StatusCode, Body = json }.Dump("綁定結果");

	if (json.Contains("\"success\":true"))
		"✅ 綁定成功".Dump();
	else
		"❌ 綁定失敗，請確認 Page Access Token 是否正確".Dump();
}

void Unsubscribe(string pageId, string token)
{
	var url = $"https://graph.facebook.com/v24.0/{pageId}/subscribed_apps?access_token={token}";

	using var client = new HttpClient();
	var response = client.DeleteAsync(url).Result;
	var json = response.Content.ReadAsStringAsync().Result;

	new { StatusCode = response.StatusCode, Body = json }.Dump("移除結果");

	if (json.Contains("\"success\":true"))
		"✅ 已移除所有 Webhook 綁定".Dump();
	else
		"❌ 移除失敗，請確認 Page Access Token 是否正確".Dump();
}