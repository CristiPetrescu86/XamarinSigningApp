using LicentaApp.JsonClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CSC.Library
{
    public static class Protocol
    {
        public static GetInfoReceiveClass getInfo(string serviceLink)
        {
            string language = "en-US";
            // choose variant
            var method = new GetInfoSendClass
            {
                lang = language
            };

            string address = serviceLink + "info";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(address);

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = client.PostAsync("", byteContent).Result;

            return JsonConvert.DeserializeObject<GetInfoReceiveClass>(result.Content.ReadAsStringAsync().Result);
        }

        public static AuthLoginReceiveClass authLogin(string serviceLink, string username, string password, bool rememberMe)
        {
            bool okRememberMe = false;
            if (rememberMe)
            {
                okRememberMe = true;
            }

            var method = new AuthLoginSendClass
            {
                rememberMe = okRememberMe
            };

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            string userInfo = username + ":" + password;
            string userCredEncoded = Base64Encode(userInfo);

            string address = serviceLink + "auth/login";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(address);

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Basic " + userCredEncoded);
            //byteContent.Headers.Add("Authorization", "Basic" + userCredEncoded);
            var result = client.PostAsync("", byteContent).Result;

            return JsonConvert.DeserializeObject<AuthLoginReceiveClass>(result.Content.ReadAsStringAsync().Result);
        }

        public static bool authRevokeRefreshToken(string refresh_token, string serviceLink)
        {
            var method = new AuthRevokeSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.token = refresh_token;
            method.token_type_hint = "refresh_token";

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string address = serviceLink + "auth/revoke";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(address);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + refresh_token);
            //byteContent.Headers.Add("Authorization", "Basic" + userCredEncoded);
            var response = client.PostAsync("", byteContent).Result;

            if (response.StatusCode.ToString() != "NoContent")
            {
                return false;
            }
            return true;
            
        }

        public static bool authRevokeAccessToken(string accessToken, string serviceLink)
        {
            var method = new AuthRevokeSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.token = accessToken;
            method.token_type_hint = "access_token";

            string adress = serviceLink + "auth/revoke";

            var client2 = new System.Net.Http.HttpClient();
            client2.BaseAddress = new Uri(adress);

            string jsonString2 = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);
            var buffer2 = System.Text.Encoding.UTF8.GetBytes(jsonString2);
            var byteContent2 = new ByteArrayContent(buffer2);
            byteContent2.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client2.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);
            var response2 = client2.PostAsync("", byteContent2).Result;

            if (response2.Content.ReadAsStringAsync().Result != string.Empty)
            {
                return false;
            }
            return true;
        }

        public static CredentialsListReceiveClass credentialsList(string accessToken, string serviceLink)
        {
            var method = new CredentialsListSendClass();
            int maxResults = 10;

            if (maxResults != 0)
            {
                method.maxResults = maxResults;
            }

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "credentials/list";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;

            return System.Text.Json.JsonSerializer.Deserialize<CredentialsListReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
        }

        public static CredentialInfoReceiveClassExplicit credentialInfoExplicit(string credentialName, string accessToken, string serviceLink)
        {
            string credentialsCertificatesSelect = "chain";
            bool credentialsCertInfoBool = true;
            bool credentialsAuthInfoBool = true;

            var method = new CredentialsInfoSendClass
            {
                credentialID = credentialName
            };

            if (credentialsCertificatesSelect != null)
            {
                method.certificates = credentialsCertificatesSelect;
            }
            if (credentialsCertInfoBool)
            {
                method.certInfo = true;
            }
            if (credentialsAuthInfoBool)
            {
                method.authInfo = true;
            }

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "credentials/info";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;
            return System.Text.Json.JsonSerializer.Deserialize<CredentialInfoReceiveClassExplicit>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
        }

        public static CredentialsInfoReceiveClass credentialInfo(string credentialName, string accessToken, string serviceLink)
        {
            string credentialsCertificatesSelect = "chain";
            bool credentialsCertInfoBool = true;
            bool credentialsAuthInfoBool = true;

            var method = new CredentialsInfoSendClass
            {
                credentialID = credentialName
            };

            if (credentialsCertificatesSelect != null)
            {
                method.certificates = credentialsCertificatesSelect;
            }
            if (credentialsCertInfoBool)
            {
                method.certInfo = true;
            }
            if (credentialsAuthInfoBool)
            {
                method.authInfo = true;
            }

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "credentials/info";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;
            return System.Text.Json.JsonSerializer.Deserialize<CredentialsInfoReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
        }

        public static CredentialsAuthorizeReceiveClass credentialAuthorize(string credID, string accessToken, string hash, string PIN, string OTP, string serviceLink)
        {
            var method = new CredentialsAuthorizeSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = credID;

            method.numSignatures = 1;

            method.hash.Add(hash);

            if (PIN != null)
            {
                method.PIN = PIN;
            }

            if (OTP != null)
            {
                method.OTP = OTP;
            }

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "credentials/authorize";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;

            return System.Text.Json.JsonSerializer.Deserialize<CredentialsAuthorizeReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
        }

        public static CredentialsAuthorizeReceiveClass credentialsAuthorizeMultipleHash(string credID, int numSign, string accessToken, List<string> hashList, string PIN, string OTP, string serviceLink)
        {
            var method = new CredentialsAuthorizeSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = credID;

            method.numSignatures = numSign;

            foreach (var hash in hashList)
                method.hash.Add(hash);

            if (PIN != null)
            {
                method.PIN = PIN;
            }

            if (OTP != null)
            {
                method.OTP = OTP;
            }

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "credentials/authorize";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;

            return System.Text.Json.JsonSerializer.Deserialize<CredentialsAuthorizeReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
        }

        public static bool sendOTP(string credentialID, string accessToken, string serviceLink)
        {
            var method = new SendOTPClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = credentialID;

            //if(clientDataAuthBool)
            //{
            //    method.clientData = clientDataAuthString;
            //}

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "credentials/sendOTP";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;

            if (response.StatusCode.ToString() != "No Content")
            {
                return false;
            }

            return true;
        }

        public static SignHashReceiveClass signSingleHash(string credID, string accessToken, string serviceLink, string SAD, string hash, string signOID, string hashOID)
        {
            var method = new SignHashSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = credID;
            method.SAD = SAD;

            method.hash.Add(hash);

            method.signAlgo = signOID;

            if (hashOID != string.Empty)
            {
                method.hashAlgo = hashOID;
            }

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "signatures/signHash";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;

            return System.Text.Json.JsonSerializer.Deserialize<SignHashReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
        }

        public static SignHashReceiveClass signMultipleHash(string credID, string accessToken, string serviceLink, string SAD, List<string> hash, string signOID, string hashOID)
        {
            var method = new SignHashSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = credID;
            method.SAD = SAD;

            foreach (string docToHash in hash)
            {
                method.hash.Add(docToHash);
            }

            method.signAlgo = signOID;

            if (hashOID != string.Empty)
            {
                method.hashAlgo = hashOID;
            }

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "signatures/signHash";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;

            return System.Text.Json.JsonSerializer.Deserialize<SignHashReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
        }


        public static OauthTokenReceiveClass oauth2TokenAccess(string grant_type, string code, string serviceLink)
        {
            var method = new OauthTokenSendClass
            {
                client_id = "ts_csc",
                client_secret = "h767ujHG654GHhgI",
                redirect_uri = "http://localhost:8080/login.html"
            };

            method.grant_type = grant_type;
            method.code = code;

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            string adress = serviceLink + "oauth2/token";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = client.PostAsync("", byteContent).Result;

            return JsonConvert.DeserializeObject<OauthTokenReceiveClass>(result.Content.ReadAsStringAsync().Result);
        }

        public static OauthTokenReceiveClass oauth2TokenCredential(string grant_type, string code, string serviceLink)
        {
            var method = new OauthTokenSendClass
            {
                client_id = "ts_csc",
                client_secret = "h767ujHG654GHhgI",
                redirect_uri = "http://localhost:8080/login.html"
            };

            method.grant_type = grant_type;
            method.code = code;

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            string adress = serviceLink + "oauth2/token";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);
            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = client.PostAsync("", byteContent).Result;

            return JsonConvert.DeserializeObject<OauthTokenReceiveClass>(result.Content.ReadAsStringAsync().Result);
        }





        private static string Base64Encode(string plaintext)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
            string userEncoded = Convert.ToBase64String(plainTextBytes);
            return userEncoded;
        }
    }
}
