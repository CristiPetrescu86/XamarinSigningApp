﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using LicentaApp.JsonClass;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;
using iText.Signatures;
using System.Security.Cryptography;
using System.Diagnostics;

namespace LicentaApp
{
    public class User
    {
        // Credentiale
        private string username { get; set; }
        private string password { get; set; }

        public string authModeSelected { get; set; }
        public string serviceLink { get; set; }
        private string accessToken { get; set; }

        
        public void setAccess(string a)
        {
            accessToken = a;
        }

        private string refresh_token { get; set; }

        public List<string> credentialsIDs = new List<string>();

        public List<CredentialsInfoReceiveClass> keysInfo = new List<CredentialsInfoReceiveClass>();

        CredentialsAuthorizeReceiveClass currentSAD { get; set; }

        public List<string> signatures = new List<string>();


        public User(string Username, string Password)
        {
            username = Username;
            password = Password;
        }

        public User() { }


        public string getInfo()
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

            var result2 = JsonConvert.DeserializeObject<GetInfoReceiveClass>(result.Content.ReadAsStringAsync().Result);

            return result2.name;
        }
        
        public bool authLogin(bool rememberMe)
        {
            // CHOOSE OPTIONS
            bool okRememberMe = false;
            string clientDataAux = null;

            //if (refreshTokenAuthBool)
            //{
            //    okRefresh = true;
            //}
            if (rememberMe)
            {
                okRememberMe = true;
            }


            var method = new AuthLoginSendClass
            {
                rememberMe = okRememberMe
            };
            if(clientDataAux != null)
            {
                method.clientData = clientDataAux;
            }

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

            var methodResponse = JsonConvert.DeserializeObject<AuthLoginReceiveClass>(result.Content.ReadAsStringAsync().Result);        

            if (methodResponse.access_token == null)
            {
                return false;
            }
            else
            {
                accessToken = methodResponse.access_token;
                if (rememberMe)
                {
                    refresh_token = methodResponse.refresh_token;
                }
            }

            return true;
        }
        
        public bool authRevoke()
        {
            var method = new AuthRevokeSendClass();
            
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };     

            if (refresh_token != null)
            {
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
                refresh_token = null;
            }

            var method2 = new AuthRevokeSendClass();

            var jsonSerializerOptions2 = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method2.token = accessToken;
            method2.token_type_hint = "access_token";

            string adress = serviceLink + "auth/revoke";

            var client2 = new System.Net.Http.HttpClient();
            client2.BaseAddress = new Uri(adress);

            string jsonString2 = System.Text.Json.JsonSerializer.Serialize(method2, jsonSerializerOptions2);
            var buffer2 = System.Text.Encoding.UTF8.GetBytes(jsonString2);
            var byteContent2 = new ByteArrayContent(buffer2);
            byteContent2.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client2.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);
            //byteContent.Headers.Add("Authorization", "Basic" + userCredEncoded);
            var response2 = client2.PostAsync("", byteContent2).Result;

            if (response2.Content.ReadAsStringAsync().Result != "NoContent")
            {
                return false;
            }
            accessToken = null;

            return true;
        }

        
        public void credentialsList()
        {
            if (accessToken == null)
            {
                return;
            }

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

            CredentialsListReceiveClass methodResponse = System.Text.Json.JsonSerializer.Deserialize<CredentialsListReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);

            if (methodResponse.credentialIDs.Count == null)
            {
                dynamic inform = JObject.Parse(response.Content.ToString());
                if (inform.error_description != null)
                {
                    Console.WriteLine(inform.error_description);
                    return;
                }
            }
            else
            {
                foreach (string elem in methodResponse.credentialIDs)
                {
                    bool ok = true;
                    foreach (string name in credentialsIDs)
                    {
                        if(elem == name)
                        {
                            ok = false;
                        }
                    }
                    if(ok)
                    {
                        credentialsIDs.Add(elem);
                    }
                }
            }
        }

        
        public void credentialsInfo(string credentialName)
        {
            string credentialsCertificatesSelect = "chain";
            bool credentialsCertInfoBool = true;
            bool credentialsAuthInfoBool = true;

            bool ok = true;
            foreach (var credential in credentialsIDs)
            {
                if (credentialName == credential)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                Console.WriteLine("Nu exista credentialele");
                return;
            }

            var method = new CredentialsInfoSendClass
            {
                credentialID = credentialName
            };

            if (credentialsCertificatesSelect != null)
            {
                method.certificates = credentialsCertificatesSelect;
            }
            if(credentialsCertInfoBool)
            {
                method.certInfo = true;
            }
            if(credentialsAuthInfoBool)
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

            if (authModeSelected == "explicit")
            {
                CredentialInfoReceiveClassExplicit methodResponse = System.Text.Json.JsonSerializer.Deserialize<CredentialInfoReceiveClassExplicit>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);
                CredentialsInfoReceiveClass aux = new CredentialsInfoReceiveClass();
                aux.authMode = methodResponse.authMode;
                aux.cert = methodResponse.cert;
                aux.description = methodResponse.description;
                aux.key = methodResponse.key;
                if (methodResponse.multisign > 0)
                    aux.multisign = true;
                aux.lang = methodResponse.lang;
                aux.OTP = methodResponse.OTP;
                aux.PIN = methodResponse.PIN;
                aux.SCAL = methodResponse.SCAL;

                if (methodResponse.key.status == null)
                {
                    dynamic inform = JObject.Parse(response.Content.ToString());
                    if (inform.error_description != null)
                    {
                        Console.WriteLine(inform.error_description);
                        return;
                    }
                }
                else
                {
                    aux.credentialName = credentialName;

                    bool exists = false;
                    foreach (var elem in keysInfo)
                    {
                        if (ObjectComparerUtility.ObjectsAreEqual(elem, aux))
                        {
                            exists = true;
                        }
                    }
                    if (!exists)
                    {
                        keysInfo.Add(aux);
                    }

                }
            }
            else
            {
                CredentialsInfoReceiveClass methodResponse = System.Text.Json.JsonSerializer.Deserialize<CredentialsInfoReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);

                if (methodResponse.key.status == null)
                {
                    dynamic inform = JObject.Parse(response.Content.ToString());
                    if (inform.error_description != null)
                    {
                        Console.WriteLine(inform.error_description);
                        return;
                    }
                }
                else
                {
                    methodResponse.credentialName = credentialName;

                    bool exists = false;
                    foreach (var elem in keysInfo)
                    {
                        if (ObjectComparerUtility.ObjectsAreEqual(elem, methodResponse))
                        {
                            exists = true;
                        }
                    }
                    if (!exists)
                    {
                        keysInfo.Add(methodResponse);
                    }

                }
            }
        }

        public static class ObjectComparerUtility
        {
            public static bool ObjectsAreEqual<T>(T obj1, T obj2)
            {
                var obj1Serialized = JsonConvert.SerializeObject(obj1);
                var obj2Serialized = JsonConvert.SerializeObject(obj2);

                return obj1Serialized == obj2Serialized;
            }
        }


        public bool credentialsAuthorize(string credentialName, string hash, string PIN, string OTP)
        {
            int index = -1;
            for (int i = 0; i < keysInfo.Count; i++)
            {
                if(keysInfo[i].credentialName == credentialName)
                {
                    index = i;
                    break;
                }
            }

            var method = new CredentialsAuthorizeSendClass();
            
            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            if (keysInfo[index].SCAL == "1")
            {
                return true;
            }


            //if isset PIN
            //if isset OTP
            //if isset credID

            method.credentialID = keysInfo[index].credentialName;

            method.numSignatures = 1;

            method.hash.Add(hash);


            if (keysInfo[index].authMode == "explicit")
            {
                if (keysInfo[index].PIN.presence == "true")
                {
                    method.PIN = PIN;
                }

                if (keysInfo[index].OTP.presence == "true")
                {
                    method.OTP = OTP;
                }

                if(keysInfo[index].OTP.type == "online")
                {
                    method.OTP = OTP;
                }
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

            CredentialsAuthorizeReceiveClass methodResponse = System.Text.Json.JsonSerializer.Deserialize<CredentialsAuthorizeReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);

            if(methodResponse.SAD == null)
            {
                dynamic inform = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (inform.error_description != null)
                {
                    //Debug.WriteLine(inform.error_description);
                    return false;
                }
            }

            currentSAD = methodResponse;

            return true;
        }


        public bool credentialsAuthorizeMultipleHash(string credentialName, List<string> hashList, string PIN, string OTP, int numSign)
        {
            int index = -1;
            for (int i = 0; i < keysInfo.Count; i++)
            {
                if (keysInfo[i].credentialName == credentialName)
                {
                    index = i;
                    break;
                }
            }

            var method = new CredentialsAuthorizeSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            if (keysInfo[index].SCAL == "1")
            {
                return true;
            }


            //if isset PIN
            //if isset OTP
            //if isset credID

            method.credentialID = keysInfo[index].credentialName;

            method.numSignatures = numSign;

            foreach(var hash in hashList)
                method.hash.Add(hash);


            if (keysInfo[index].authMode == "explicit")
            {
                if (keysInfo[index].PIN.presence == "true")
                {
                    method.PIN = PIN;
                }

                if (keysInfo[index].OTP.presence == "true")
                {
                    method.OTP = OTP;
                }

                if (keysInfo[index].OTP.type == "online")
                {
                    method.OTP = OTP;
                }
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

            CredentialsAuthorizeReceiveClass methodResponse = System.Text.Json.JsonSerializer.Deserialize<CredentialsAuthorizeReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);

            if (methodResponse.SAD == null)
            {
                dynamic inform = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (inform.error_description != null)
                {
                    //Debug.WriteLine(inform.error_description);
                    return false;
                }
            }

            currentSAD = methodResponse;

            return true;
        }


        public bool credentialsExtendTransaction(byte[] hash, string credentialName, string docType)
        {
            byte[] hashDocument;
            string hashedDocumentB64;

            int index = -1;
            for (int i = 0; i < keysInfo.Count; i++)
            {
                if (keysInfo[i].credentialName == credentialName)
                {
                    index = i;
                    break;
                }
            }

            var method = new ExtendTransactionSendClass();


            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = keysInfo[index].credentialName;
            method.SAD = currentSAD.SAD;

            //if (docType == "PDF")
            //{
            //    //  PDF
            //    SHA256 shaM = new SHA256Managed();
            //    var result = shaM.ComputeHash(hash);

            //    hashedDocumentB64 = Convert.ToBase64String(result);
            //    method.hash.Add(hashedDocumentB64);
            //}
            //else if (docType == "XML")
            //{
            //    // XML
            //    hashedDocumentB64 = Convert.ToBase64String(hash);
            //    method.hash.Add(hashedDocumentB64);
            //}
            //else
            //{
            //    return false;
            //}

            string jsonString = System.Text.Json.JsonSerializer.Serialize(method, jsonSerializerOptions);

            string adress = serviceLink + "credentials/extendTransaction";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(adress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            var response = client.PostAsync("", byteContent).Result;

            ExtendTransactionReceiveClass methodResponse = System.Text.Json.JsonSerializer.Deserialize<ExtendTransactionReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);

            if(methodResponse.SAD == null)
            {
                dynamic inform = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (inform.error_description != null)
                {
                    //Debug.WriteLine(inform.error_description);
                    return false;
                }
            }


            currentSAD.SAD = methodResponse.SAD;
            if(methodResponse.expiresIn != 0)
            {
                currentSAD.expiresIn = methodResponse.expiresIn;
            }

            return true;
        }

        
        public void credentialsSendOTP(string credentialName)
        {
            int index = -1;
            for (int i = 0; i < keysInfo.Count; i++)
            {
                if (keysInfo[i].credentialName == credentialName)
                {
                    index = i;
                    break;
                }
            }

            if(index == -1)
            {
                return;
            }

            var method = new SendOTPClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = keysInfo[index].credentialName;

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

            if(response.StatusCode.ToString() != "No Content")
            {
                //dynamic inform = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                //Console.WriteLine(inform.error_description);
            }

            //Console.WriteLine("OTP send");
        }

        
        public bool signSingleHash(string credentialName, string hash, string signOID , string hashOID)
        {
            int index = -1;
            for (int i = 0; i < keysInfo.Count; i++)
            {
                if (keysInfo[i].credentialName == credentialName)
                {
                    index = i;
                    break;
                }
            }

            if(index == -1)
            {
                return false;
            }

            var method = new SignHashSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = keysInfo[index].credentialName;
            method.SAD = currentSAD.SAD;

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

            SignHashReceiveClass methodResponse = System.Text.Json.JsonSerializer.Deserialize<SignHashReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);

            if (methodResponse.signatures.Count == 0)
            {
                dynamic inform = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (inform.error_description != null)
                {
                    //Debug.WriteLine(inform.error_description);
                    return false;
                }
            }

            foreach (string elem in methodResponse.signatures)
            {
                signatures.Add(elem);
            }

            return true;
        }

        
        public bool signMultipleHash(string credentialName, List<string> hash, string signOID, string hashOID)
        {
            int index = -1;
            for (int i = 0; i < keysInfo.Count; i++)
            {
                if (keysInfo[i].credentialName == credentialName)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return false;
            }

            var method = new SignHashSendClass();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };

            method.credentialID = keysInfo[index].credentialName;
            method.SAD = currentSAD.SAD;

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

            SignHashReceiveClass methodResponse = System.Text.Json.JsonSerializer.Deserialize<SignHashReceiveClass>(response.Content.ReadAsStringAsync().Result, jsonSerializerOptions);

            if (methodResponse.signatures.Count == 0)
            {
                dynamic inform = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (inform.error_description != null)
                {
                    return false;
                }
            }

            foreach (string elem in methodResponse.signatures)
            {
                signatures.Add(elem);
            }

            return true;
        }

        public void oauth2Auth()
        {     
            // https://service.csctest.online/csc/v0/oauth2/authorize?response_type=code&client_id=bBdNs9Fa7kMx0qnFsPk66sklrDw&redirect_uri=http%3A%2F%2Flocalhost%3A8080%2Flogin.html&scope=service&state=12345678
        }

        public void oauth2Revoke()
        {

        }


        private string codeAux { get; set; }

        public bool oauth2Token(string code, string grant_type)
        {

            var method = new OauthTokenSendClass
            {
                client_id = "ts_csc",
                client_secret = "h767ujHG654GHhgI",
                redirect_uri = "http://localhost:8080/login.html"
            };

            method.grant_type = grant_type;
            method.code = code;

            codeAux = code;

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

            var methodResponse = JsonConvert.DeserializeObject<OauthTokenReceiveClass>(result.Content.ReadAsStringAsync().Result);

            if (methodResponse.access_token == null)
            {
                return false;
            }

            accessToken = methodResponse.access_token;
            //if (rememberMe)
            //{
            //    refresh_token = methodResponse.refresh_token;
            //}

            return true;
        }

        public bool oauth2TokenCredential(string code, string grant_type)
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

            var methodResponse = JsonConvert.DeserializeObject<OauthTokenReceiveClass>(result.Content.ReadAsStringAsync().Result);

            dynamic inform = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            if (inform.error_description != null)
            {
                //Debug.WriteLine(inform.error_description);
                return false;
            }

            if (methodResponse.access_token == null)
            {
                return false;
            }

            currentSAD = new CredentialsAuthorizeReceiveClass();
            currentSAD.SAD = methodResponse.access_token;

            if (methodResponse.expires_in != 3600)
            {
                currentSAD.expiresIn = methodResponse.expires_in;
            }


            return true;
        }

        public bool oauth2TokenCredentialRenew(string grant_type)
        {
            var method = new OauthTokenSendClass
            {
                client_id = "ts_csc",
                client_secret = "h767ujHG654GHhgI",
                redirect_uri = "http://localhost:8080/login.html"
            };

            method.code = codeAux;
            method.grant_type = grant_type;

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

            var methodResponse = JsonConvert.DeserializeObject<OauthTokenReceiveClass>(result.Content.ReadAsStringAsync().Result);

            dynamic inform = JObject.Parse(result.Content.ReadAsStringAsync().Result);
            if (inform.error_description != null)
            {
                //Debug.WriteLine(inform.error_description);
                return false;
            }

            if (methodResponse.access_token == null)
            {
                return false;
            }

            currentSAD = new CredentialsAuthorizeReceiveClass();
            currentSAD.SAD = methodResponse.access_token;

            if (methodResponse.expires_in != 3600)
            {
                currentSAD.expiresIn = methodResponse.expires_in;
            }


            return true;
        }




        private string Base64Encode(string plaintext)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
            string userEncoded = Convert.ToBase64String(plainTextBytes);
            return userEncoded;
        }

        
    }
}
