using System;
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
using CSC.Library;

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

        private string codeAux { get; set; }


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

        public bool getInfo()
        {
            GetInfoReceiveClass infoClass = Protocol.getInfo(serviceLink);
            return true;
        }


        public bool authLogin(bool rememberMe)
        {
            AuthLoginReceiveClass methodResponse = Protocol.authLogin(serviceLink, username, password, rememberMe);

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
            bool revokedRefresh = true;
            bool revokedAccess;

            if (refresh_token != null)
            {
                revokedRefresh = Protocol.authRevokeRefreshToken(refresh_token, serviceLink);
                refresh_token = null;
            }

            revokedAccess = Protocol.authRevokeAccessToken(accessToken,serviceLink);
            accessToken = null;

            if(!revokedRefresh || !revokedAccess)
            {
                //return false;
            }
            return true;
        }

        
        public void credentialsList()
        {
            if (accessToken == null)
            {
                return;
            }

            CredentialsListReceiveClass methodResponse = Protocol.credentialsList(accessToken,serviceLink);

            if (methodResponse.credentialIDs.Count == null)
            {
                return;
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
                return;
            }


            if (authModeSelected == "explicit")
            {
                CredentialInfoReceiveClassExplicit methodResponse = Protocol.credentialInfoExplicit(credentialName, accessToken, serviceLink);
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
                    return;
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
                CredentialsInfoReceiveClass methodResponse = Protocol.credentialInfo(credentialName, accessToken, serviceLink);

                if (methodResponse.key.status == null)
                {
                    return;
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

            if (keysInfo[index].SCAL == "1")
            {
                return true;
            }

            bool PIN_bool = false;
            bool OTP_bool = false;

            if (keysInfo[index].authMode == "explicit")
            {
                if (keysInfo[index].PIN.presence == "true")
                {
                    PIN_bool = true;
                }

                if (keysInfo[index].OTP.presence == "true")
                {
                    OTP_bool = true;
                }

                if (keysInfo[index].OTP.type == "online")
                {
                    OTP_bool = true;
                }
            }

            if(PIN_bool && OTP_bool)
                currentSAD = Protocol.credentialAuthorize(keysInfo[index].credentialName, accessToken, hash, PIN, OTP, serviceLink);
            else if(PIN_bool)
                currentSAD = Protocol.credentialAuthorize(keysInfo[index].credentialName, accessToken, hash, PIN, null, serviceLink);
            else
                currentSAD = Protocol.credentialAuthorize(keysInfo[index].credentialName, accessToken, hash, null, OTP, serviceLink);


            if (currentSAD.SAD == null)
            {
                return false;
            }
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

            if (keysInfo[index].SCAL == "1")
            {
                return true;
            }

            bool PIN_bool = false;
            bool OTP_bool = false;

            if (keysInfo[index].authMode == "explicit")
            {
                if (keysInfo[index].PIN.presence == "true")
                {
                    PIN_bool = true;
                }

                if (keysInfo[index].OTP.presence == "true")
                {
                    OTP_bool = true;
                }

                if (keysInfo[index].OTP.type == "online")
                {
                    OTP_bool = true;
                }
            }

            if (PIN_bool && OTP_bool)
                currentSAD = Protocol.credentialsAuthorizeMultipleHash(keysInfo[index].credentialName, numSign, accessToken, hashList, PIN, OTP, serviceLink);
            else if (PIN_bool)
                currentSAD = Protocol.credentialsAuthorizeMultipleHash(keysInfo[index].credentialName, numSign, accessToken, hashList, PIN, null, serviceLink);
            else
                currentSAD = Protocol.credentialsAuthorizeMultipleHash(keysInfo[index].credentialName, numSign, accessToken, hashList, null, OTP, serviceLink);


            if (currentSAD.SAD == null)
            {
                return false;
            }
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

            Protocol.sendOTP(credentialName, accessToken, serviceLink);
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

            SignHashReceiveClass methodResponse = Protocol.signSingleHash(credentialName, accessToken, serviceLink, currentSAD.SAD, hash, signOID, hashOID);

            if (methodResponse.signatures.Count == 0)
            {
                return false;
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

            SignHashReceiveClass methodResponse = Protocol.signMultipleHash(credentialName, accessToken, serviceLink, currentSAD.SAD, hash, signOID, hashOID);

            if (methodResponse.signatures.Count == 0)
            {
                return false;
            }

            foreach (string elem in methodResponse.signatures)
            {
                signatures.Add(elem);
            }

            return true;
        }

        public void oauth2Auth()
        {     
            //https://service.csctest.online/csc/v0/oauth2/authorize?response_type=code&client_id=bBdNs9Fa7kMx0qnFsPk66sklrDw&redirect_uri=http%3A%2F%2Flocalhost%3A8080%2Flogin.html&scope=service&state=12345678
        }

        

        public bool oauth2Token(string code, string grant_type)
        {
            OauthTokenReceiveClass methodResponse = Protocol.oauth2TokenAccess(grant_type,code,serviceLink);

            if (methodResponse.access_token == null)
            {
                accessToken = null;
                return false;
            }
            accessToken = methodResponse.access_token;

            return true;
        }

        public bool oauth2TokenCredential(string code, string grant_type)
        {
            OauthTokenReceiveClass methodResponse = Protocol.oauth2TokenCredential(grant_type, code, serviceLink);

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
            else
                methodResponse.expires_in = 3600;

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
        
    }
}
