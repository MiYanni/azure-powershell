// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Microsoft.Azure.Commands.Common.Authentication.Abstractions;
using Microsoft.Azure.Commands.KeyVault.Models.ManagedStorageAccounts;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.Rest.Azure;
using KeyVaultProperties = Microsoft.Azure.Commands.KeyVault.Properties;

namespace Microsoft.Azure.Commands.KeyVault.Models
{
    internal class KeyVaultDataServiceClient : IKeyVaultDataServiceClient
    {
        public KeyVaultDataServiceClient(IAuthenticationFactory authFactory, IAzureContext context)
        {
            if (authFactory == null)
                throw new ArgumentNullException(nameof(authFactory));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.Environment == null)
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidAzureEnvironment);

            var credential = new DataServiceCredential(authFactory, context, AzureEnvironment.Endpoint.AzureKeyVaultServiceEndpointResourceId);
            keyVaultClient = new KeyVaultClient(credential.OnAuthentication);


            vaultUriHelper = new VaultUriHelper(
                context.Environment.GetEndpoint(AzureEnvironment.Endpoint.AzureKeyVaultDnsSuffix));
        }

        /// <summary>
        /// Parameterless constructor for Mocking.
        /// </summary>
        public KeyVaultDataServiceClient()
        {
        }

        public PSKeyVaultKey CreateKey(string vaultName, string keyName, PSKeyVaultKeyAttributes keyAttributes, int? size)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException(nameof(keyName));
            if (keyAttributes == null)
                throw new ArgumentNullException(nameof(keyAttributes));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);
            var attributes = (KeyAttributes)keyAttributes;

            KeyBundle keyBundle;
            try
            {
                keyBundle = keyVaultClient.CreateKeyAsync(
                    vaultAddress,
                    keyName,
                    keyAttributes.KeyType,
                    size,
                    keyAttributes.KeyOps == null ? null : new List<string> (keyAttributes.KeyOps),
                    attributes,
                    keyAttributes.TagsDirectionary).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultKey(keyBundle, vaultUriHelper);
        }        

        public PSKeyVaultCertificate MergeCertificate(string vaultName, string certName, X509Certificate2Collection certs, IDictionary<string, string> tags)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certName))
                throw new ArgumentNullException(nameof(certName));
            if (null == certs)
                throw new ArgumentNullException(nameof(certs));

            CertificateBundle certBundle;

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            try
            {
                certBundle = keyVaultClient.MergeCertificateAsync(vaultAddress, certName, certs, null, tags).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultCertificate(certBundle);

        }

        public PSKeyVaultCertificate ImportCertificate(string vaultName, string certName, string base64CertColl, SecureString certPassword, IDictionary<string, string> tags)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certName))
                throw new ArgumentNullException(nameof(certName));
            if (string.IsNullOrEmpty(base64CertColl))
                throw new ArgumentNullException(nameof(base64CertColl));

            CertificateBundle certBundle;

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            var password = certPassword == null ? string.Empty : certPassword.ConvertToString();


            try
            {
                certBundle = keyVaultClient.ImportCertificateAsync(vaultAddress, certName, base64CertColl, password, new CertificatePolicy
                {
                    SecretProperties = new SecretProperties
                    {
                        ContentType = "application/x-pkcs12"
                    }
                }, null, tags).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultCertificate(certBundle);
        }

        public PSKeyVaultCertificate ImportCertificate(string vaultName, string certName, X509Certificate2Collection certificateCollection, IDictionary<string, string> tags)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certName))
                throw new ArgumentNullException(nameof(certName));
            if (null == certificateCollection)
                throw new ArgumentNullException(nameof(certificateCollection));

            CertificateBundle certBundle;
            var vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            try
            {
                certBundle = keyVaultClient.ImportCertificateAsync(vaultAddress, certName, certificateCollection, new CertificatePolicy
                {
                    SecretProperties = new SecretProperties
                    {
                        ContentType = "application/x-pkcs12"
                    }
                }, null, tags).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultCertificate(certBundle);
        }

        public PSKeyVaultKey ImportKey(string vaultName, string keyName, PSKeyVaultKeyAttributes keyAttributes, JsonWebKey webKey, bool? importToHsm)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException(nameof(keyName));
            if (keyAttributes == null)
                throw new ArgumentNullException(nameof(keyAttributes));
            if (webKey == null)
                throw new ArgumentNullException(nameof(webKey));
            if (webKey.Kty == JsonWebKeyType.RsaHsm && importToHsm.HasValue && !importToHsm.Value)
                throw new ArgumentException(KeyVaultProperties.Resources.ImportByokAsSoftkeyError);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);
            
            webKey.KeyOps = keyAttributes.KeyOps;            
            var keyBundle = new KeyBundle
            {
                Attributes = (KeyAttributes)keyAttributes,
                Key = webKey,
                Tags = keyAttributes.TagsDirectionary
            };

            try
            {
                keyBundle = keyVaultClient.ImportKeyAsync(vaultAddress, keyName, keyBundle, importToHsm).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultKey(keyBundle, vaultUriHelper);
        }

        public PSKeyVaultKey UpdateKey(string vaultName, string keyName, string keyVersion, PSKeyVaultKeyAttributes keyAttributes)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException(nameof(keyName));
            if (keyAttributes == null)
                throw new ArgumentNullException(nameof(keyAttributes));
            
            var attributes = (KeyAttributes)keyAttributes;
            var keyIdentifier = new KeyIdentifier(vaultUriHelper.CreateVaultAddress(vaultName), keyName, keyVersion);

            KeyBundle keyBundle;
            try
            {
                keyBundle = keyVaultClient.UpdateKeyAsync(
                    keyIdentifier.Identifier, keyAttributes.KeyOps, attributes, keyAttributes.TagsDirectionary).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultKey(keyBundle, vaultUriHelper);
        }

        public IEnumerable<PSKeyVaultCertificateContact> GetCertificateContacts(string vaultName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            Contacts contacts;

            try
            {
                contacts = keyVaultClient.GetCertificateContactsAsync(vaultAddress).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            if (contacts == null ||
                contacts.ContactList == null)
            {
                return null;
            }

            var contactsModel = new List<PSKeyVaultCertificateContact>();

            foreach (var contact in contacts.ContactList)
            {
                contactsModel.Add(PSKeyVaultCertificateContact.FromKVCertificateContact(contact, vaultName));
            }

            return contactsModel;
        }

        public PSKeyVaultCertificate GetCertificate(string vaultName, string certName, string certificateVersion)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certName))
                throw new ArgumentNullException(nameof(certName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificateBundle certBundle;

            try
            {
                certBundle = keyVaultClient.GetCertificateAsync(vaultAddress, certName, certificateVersion).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultCertificate(certBundle);
        }

        public PSKeyVaultKey GetKey(string vaultName, string keyName, string keyVersion)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException(nameof(keyName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            KeyBundle keyBundle;
            try
            {
                keyBundle = keyVaultClient.GetKeyAsync(vaultAddress, keyName, keyVersion).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultKey(keyBundle, vaultUriHelper);
        }

        public IEnumerable<PSKeyVaultCertificateIdentityItem> GetCertificates(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<CertificateItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetCertificatesAsync(vaultAddress).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetCertificatesNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSKeyVaultCertificateIdentityItem>() :
                    result.Select(certItem => { return new PSKeyVaultCertificateIdentityItem(certItem, vaultUriHelper); });
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public IEnumerable<PSKeyVaultCertificateIdentityItem> GetCertificateVersions(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            if (string.IsNullOrEmpty(options.Name))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidKeyName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<CertificateItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetCertificateVersionsAsync(vaultAddress, options.Name).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetCertificateVersionsNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result.Select(certificateItem => new PSKeyVaultCertificateIdentityItem(certificateItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public IEnumerable<PSKeyVaultKeyIdentityItem> GetKeys(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<KeyItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetKeysAsync(vaultAddress).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetKeysNextAsync(options.NextLink).GetAwaiter().GetResult();
                
                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSKeyVaultKeyIdentityItem>() :
                    result.Select(keyItem => new PSKeyVaultKeyIdentityItem(keyItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public IEnumerable<PSKeyVaultKeyIdentityItem> GetKeyVersions(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            if (string.IsNullOrEmpty(options.Name))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidKeyName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<KeyItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetKeyVersionsAsync(vaultAddress, options.Name).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetKeyVersionsNextAsync(options.NextLink).GetAwaiter().GetResult();
               
                options.NextLink = result.NextPageLink;
                return result.Select(keyItem => new PSKeyVaultKeyIdentityItem(keyItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public PSDeletedKeyVaultKey DeleteKey(string vaultName, string keyName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException(nameof(keyName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedKeyBundle keyBundle;
            try
            {
                keyBundle = keyVaultClient.DeleteKeyAsync(vaultAddress, keyName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSDeletedKeyVaultKey(keyBundle, vaultUriHelper);
        }

        public IEnumerable<PSKeyVaultCertificateContact> SetCertificateContacts(string vaultName, IEnumerable<PSKeyVaultCertificateContact> contacts)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            List<Contact> contactList = null;
            if (contacts != null)
            {
                contactList = new List<Contact>();
                foreach (var psContact in contacts)
                {
                    contactList.Add(new Contact { EmailAddress = psContact.Email });
                }
            }
            Contacts inputContacts = new Contacts { ContactList = contactList };

            Contacts outputContacts;

            try
            {
                outputContacts = keyVaultClient.SetCertificateContactsAsync(vaultAddress, inputContacts).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            if (outputContacts == null ||
                outputContacts.ContactList == null)
            {
                return null;
            }

            var contactsModel = new List<PSKeyVaultCertificateContact>();

            foreach (var contact in outputContacts.ContactList)
            {
                contactsModel.Add(PSKeyVaultCertificateContact.FromKVCertificateContact(contact, vaultName));
            }

            return contactsModel;
        }

        public PSKeyVaultSecret SetSecret(string vaultName, string secretName, SecureString secretValue, PSKeyVaultSecretAttributes secretAttributes)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException(nameof(secretName));
            if (secretValue == null)
                throw new ArgumentNullException(nameof(secretValue));
            if (secretAttributes == null)
                throw new ArgumentNullException(nameof(secretAttributes));

            string value = secretValue.ConvertToString();
            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);
            var attributes = (SecretAttributes)secretAttributes;

            SecretBundle secret;
            try
            {
                secret = keyVaultClient.SetSecretAsync(vaultAddress, secretName, value,
                    secretAttributes.TagsDictionary, secretAttributes.ContentType, attributes).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultSecret(secret, vaultUriHelper);
        }

        public PSKeyVaultSecret UpdateSecret(string vaultName, string secretName, string secretVersion, PSKeyVaultSecretAttributes secretAttributes)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException(nameof(secretName));
            if (secretAttributes == null)
                throw new ArgumentNullException(nameof(secretAttributes));

            var secretIdentifier = new SecretIdentifier(vaultUriHelper.CreateVaultAddress(vaultName), secretName, secretVersion);

            SecretAttributes attributes = (SecretAttributes)secretAttributes;

            SecretBundle secret;
            try
            {
                secret = keyVaultClient.UpdateSecretAsync(secretIdentifier.Identifier,
                    secretAttributes.ContentType, attributes, secretAttributes.TagsDictionary).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultSecret(secret, vaultUriHelper);
        }

        public PSKeyVaultSecret GetSecret(string vaultName, string secretName, string secretVersion)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException(nameof(secretName));

            var secretIdentifier = new SecretIdentifier(vaultUriHelper.CreateVaultAddress(vaultName), secretName, secretVersion);
            SecretBundle secret;
            try
            {
                secret = keyVaultClient.GetSecretAsync(secretIdentifier.Identifier).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultSecret(secret, vaultUriHelper);
        }

        public IEnumerable<PSKeyVaultSecretIdentityItem> GetSecrets(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<SecretItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetSecretsAsync(vaultAddress).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetSecretsNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSKeyVaultSecretIdentityItem>() :
                    result.Select(secretItem => new PSKeyVaultSecretIdentityItem(secretItem, vaultUriHelper));            
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public IEnumerable<PSKeyVaultSecretIdentityItem> GetSecretVersions(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);
            if (string.IsNullOrEmpty(options.Name))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidSecretName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<SecretItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetSecretVersionsAsync(vaultAddress, options.Name).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetSecretVersionsNextAsync(options.NextLink).GetAwaiter().GetResult();
                
                options.NextLink = result.NextPageLink;
                return result.Select(secretItem => new PSKeyVaultSecretIdentityItem(secretItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public PSKeyVaultCertificateOperation EnrollCertificate(string vaultName, string certificateName, CertificatePolicy certificatePolicy, IDictionary<string, string> tags)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificateOperation certificateOperation;

            try
            {
                certificateOperation = keyVaultClient.CreateCertificateAsync(vaultAddress, certificateName, certificatePolicy, null, tags).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificateOperation.FromCertificateOperation(certificateOperation);
        }

        public PSKeyVaultCertificate UpdateCertificate(string vaultName, string certificateName, string certificateVersion, CertificateAttributes certificateAttributes, IDictionary<string, string> tags)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));

            var certificateIdentifier = new CertificateIdentifier(vaultUriHelper.CreateVaultAddress(vaultName), certificateName, certificateVersion);

            CertificateBundle certificateBundle;
            try
            {
                certificateBundle = keyVaultClient.UpdateCertificateAsync(
                    certificateIdentifier.Identifier, null, certificateAttributes, tags).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultCertificate(certificateBundle);
        }

        public PSDeletedKeyVaultCertificate DeleteCertificate(string vaultName, string certName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certName))
                throw new ArgumentNullException(nameof(certName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedCertificateBundle certBundle;

            try
            {
                certBundle = keyVaultClient.DeleteCertificateAsync(vaultAddress, certName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSDeletedKeyVaultCertificate(certBundle);
        }

        public void PurgeCertificate(string vaultName, string certName)
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrEmpty( certName ) )
                throw new ArgumentNullException( "certName" );

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            try
            {
                keyVaultClient.PurgeDeletedCertificateAsync( vaultAddress, certName ).GetAwaiter( ).GetResult( );
            }
            catch (Exception ex)
            {
                throw GetInnerException( ex );
            }
        }

        public PSKeyVaultCertificateOperation GetCertificateOperation(string vaultName, string certificateName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificateOperation certificateOperation;

            try
            {
                certificateOperation = keyVaultClient.GetCertificateOperationAsync(vaultAddress, certificateName).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificateOperation.FromCertificateOperation(certificateOperation);
        }

        public PSKeyVaultCertificateOperation CancelCertificateOperation(string vaultName, string certificateName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificateOperation certificateOperation;

            try
            {
                certificateOperation = keyVaultClient.UpdateCertificateOperationAsync(vaultAddress, certificateName, true).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificateOperation.FromCertificateOperation(certificateOperation);
        }

        public PSKeyVaultCertificateOperation DeleteCertificateOperation(string vaultName, string certificateName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificateOperation certificateOperation;

            try
            {
                certificateOperation = keyVaultClient.DeleteCertificateOperationAsync(vaultAddress, certificateName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificateOperation.FromCertificateOperation(certificateOperation);
        }

        public PSDeletedKeyVaultSecret DeleteSecret(string vaultName, string secretName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException(nameof(secretName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedSecretBundle secret;
            try
            {
                secret = keyVaultClient.DeleteSecretAsync(vaultAddress, secretName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSDeletedKeyVaultSecret(secret, vaultUriHelper);
        }

        public string BackupKey(string vaultName, string keyName, string outputBlobPath)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException(nameof(keyName));
            if (string.IsNullOrEmpty(outputBlobPath))
                throw new ArgumentNullException(nameof(outputBlobPath));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            BackupKeyResult backupKeyResult;
            try
            {
                backupKeyResult = keyVaultClient.BackupKeyAsync(vaultAddress, keyName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            File.WriteAllBytes(outputBlobPath, backupKeyResult.Value);

            return outputBlobPath;
        }

        public PSKeyVaultKey RestoreKey(string vaultName, string inputBlobPath)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(inputBlobPath))
                throw new ArgumentNullException(nameof(inputBlobPath));

            var backupBlob = File.ReadAllBytes(inputBlobPath);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            KeyBundle keyBundle;
            try
            {
                keyBundle = keyVaultClient.RestoreKeyAsync(vaultAddress, backupBlob).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultKey(keyBundle, vaultUriHelper);
        }

        public string BackupSecret( string vaultName, string secretName, string outputBlobPath )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException(nameof(vaultName));
            if ( string.IsNullOrEmpty( secretName ) )
                throw new ArgumentNullException(nameof(secretName));
            if ( string.IsNullOrEmpty( outputBlobPath ) )
                throw new ArgumentNullException(nameof(outputBlobPath));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            BackupSecretResult backupSecretResult;
            try
            {
                backupSecretResult = keyVaultClient.BackupSecretAsync( vaultAddress, secretName ).GetAwaiter( ).GetResult( );
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            File.WriteAllBytes( outputBlobPath, backupSecretResult.Value );

            return outputBlobPath;
        }

        public PSKeyVaultSecret RestoreSecret( string vaultName, string inputBlobPath )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException(nameof(vaultName));
            if ( string.IsNullOrEmpty( inputBlobPath ) )
                throw new ArgumentNullException(nameof(inputBlobPath));

            var backupBlob = File.ReadAllBytes(inputBlobPath);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            SecretBundle secretBundle;
            try
            {
                secretBundle = keyVaultClient.RestoreSecretAsync( vaultAddress, backupBlob ).GetAwaiter( ).GetResult( );
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultSecret( secretBundle, vaultUriHelper );
        }

        public PSKeyVaultCertificatePolicy GetCertificatePolicy(string vaultName, string certificateName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificatePolicy certificatePolicy;
            try
            {
                certificatePolicy = keyVaultClient.GetCertificatePolicyAsync(vaultAddress, certificateName).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificatePolicy.FromCertificatePolicy(certificatePolicy);
        }

        public PSKeyVaultCertificatePolicy UpdateCertificatePolicy(string vaultName, string certificateName, CertificatePolicy certificatePolicy)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));
            if (certificatePolicy == null)
                throw new ArgumentNullException(nameof(certificatePolicy));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);
            CertificatePolicy resultantCertificatePolicy;

            try
            {
                resultantCertificatePolicy = keyVaultClient.UpdateCertificatePolicyAsync(vaultAddress, certificateName, certificatePolicy).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificatePolicy.FromCertificatePolicy(certificatePolicy);
        }

        public PSKeyVaultCertificateIssuer GetCertificateIssuer(string vaultName, string issuerName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(issuerName))
                throw new ArgumentNullException(nameof(issuerName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            IssuerBundle certificateIssuer;
            try
            {
                certificateIssuer = keyVaultClient.GetCertificateIssuerAsync(vaultAddress, issuerName).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificateIssuer.FromIssuer(certificateIssuer);
        }

        public IEnumerable<PSKeyVaultCertificateIssuerIdentityItem> GetCertificateIssuers(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<CertificateIssuerItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetCertificateIssuersAsync(vaultAddress).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetCertificateIssuersNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSKeyVaultCertificateIssuerIdentityItem>() :
                    result.Select(issuerItem => new PSKeyVaultCertificateIssuerIdentityItem(issuerItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public PSKeyVaultCertificateIssuer SetCertificateIssuer(
            string vaultName,
            string issuerName,
            string issuerProvider,
            string accountId,
            SecureString apiKey,
            PSKeyVaultCertificateOrganizationDetails organizationDetails)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(issuerName))
                throw new ArgumentNullException(nameof(issuerName));
            if (string.IsNullOrEmpty(issuerProvider))
                throw new ArgumentNullException(nameof(issuerProvider));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);
            var issuer = new IssuerBundle
            {
                Provider = issuerProvider,
                OrganizationDetails = organizationDetails == null ? null : organizationDetails.ToOrganizationDetails(),
            };

            if (!string.IsNullOrEmpty(accountId) || apiKey != null)
            {
                issuer.Credentials = new IssuerCredentials
                {
                    AccountId = accountId,
                    Password = apiKey == null ? null : apiKey.ConvertToString(),
                };
            }

            IssuerBundle resultantIssuer;
            try
            {
                resultantIssuer = keyVaultClient.SetCertificateIssuerAsync(
                    vaultAddress,
                    issuerName,
                    issuer.Provider,
                    issuer.Credentials,
                    issuer.OrganizationDetails,
                    issuer.Attributes).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificateIssuer.FromIssuer(resultantIssuer);
        }        

        public PSKeyVaultCertificateIssuer DeleteCertificateIssuer(string vaultName, string issuerName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(issuerName))
                throw new ArgumentNullException(nameof(issuerName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            IssuerBundle issuer;

            try
            {
                issuer = keyVaultClient.DeleteCertificateIssuerAsync(vaultAddress, issuerName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return PSKeyVaultCertificateIssuer.FromIssuer(issuer);
        }

        #region Managed Storage Accounts
        public IEnumerable<PSKeyVaultManagedStorageAccountIdentityItem> GetManagedStorageAccounts( KeyVaultObjectFilterOptions options )
        {
            if ( options == null )
                throw new ArgumentNullException( "options" );
            if ( string.IsNullOrEmpty( options.VaultName ) )
                throw new ArgumentException( KeyVaultProperties.Resources.InvalidVaultName );

            string vaultAddress = vaultUriHelper.CreateVaultAddress( options.VaultName );

            try
            {
                IPage<StorageAccountItem> result;

                if ( string.IsNullOrEmpty( options.NextLink ) )
                    result = keyVaultClient.GetStorageAccountsAsync( vaultAddress ).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetStorageAccountsNextAsync( options.NextLink ).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result.Select(storageAccountItem => new PSKeyVaultManagedStorageAccountIdentityItem( storageAccountItem, vaultUriHelper ) );
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }
        }

        public PSKeyVaultManagedStorageAccount GetManagedStorageAccount( string vaultName, string managedStorageAccountName )
        {
            if ( string.IsNullOrWhiteSpace( vaultName ) ) throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrWhiteSpace( managedStorageAccountName ) ) throw new ArgumentNullException( "managedStorageAccountName" );

            StorageBundle storageBundle;

            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );

            try
            {
                storageBundle = keyVaultClient.GetStorageAccountAsync( vaultAddress, managedStorageAccountName ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultManagedStorageAccount( storageBundle, vaultUriHelper );
        }

        public PSKeyVaultManagedStorageAccount SetManagedStorageAccount( string vaultName, string managedStorageAccountName, string storageResourceId,
            string activeKeyName, bool? autoRegenerateKey, TimeSpan? regenerationPeriod,
            PSKeyVaultManagedStorageAccountAttributes managedStorageAccountAttributes, Hashtable tags )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrEmpty( managedStorageAccountName ) )
                throw new ArgumentNullException( "managedStorageAccountName" );
            if ( string.IsNullOrEmpty( storageResourceId ) )
                throw new ArgumentNullException( "storageResourceId" );
            if ( string.IsNullOrEmpty( activeKeyName ) )
                throw new ArgumentNullException( "activeKeyName" );

            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );
            var attributes = managedStorageAccountAttributes == null ? null : new StorageAccountAttributes
            {
                Enabled = managedStorageAccountAttributes.Enabled,
            };

            StorageBundle storageBundle;
            try
            {
                storageBundle =
                    keyVaultClient.SetStorageAccountAsync( vaultAddress, managedStorageAccountName,
                        storageResourceId, activeKeyName,
                        autoRegenerateKey ?? true,
                        regenerationPeriod == null ? null : XmlConvert.ToString( regenerationPeriod.Value ), attributes,
                        tags == null ? null : tags.ConvertToDictionary() ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultManagedStorageAccount( storageBundle, vaultUriHelper );
        }

        public PSKeyVaultManagedStorageAccount UpdateManagedStorageAccount( string vaultName, string managedStorageAccountName, string activeKeyName,
            bool? autoRegenerateKey, TimeSpan? regenerationPeriod, PSKeyVaultManagedStorageAccountAttributes managedStorageAccountAttributes,
            Hashtable tags )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrEmpty( managedStorageAccountName ) )
                throw new ArgumentNullException( "managedStorageAccountName" );

            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );
            var attributes = managedStorageAccountAttributes == null ? null : new StorageAccountAttributes
            {
                Enabled = managedStorageAccountAttributes.Enabled,
            };

            StorageBundle storageBundle;
            try
            {
                storageBundle =
                    keyVaultClient.UpdateStorageAccountAsync( vaultAddress, managedStorageAccountName,
                        activeKeyName,
                        autoRegenerateKey,
                        regenerationPeriod == null ? null : XmlConvert.ToString( regenerationPeriod.Value ), attributes,
                        tags == null ? null : tags.ConvertToDictionary() ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultManagedStorageAccount( storageBundle, vaultUriHelper );
        }

        public PSDeletedKeyVaultManagedStorageAccount DeleteManagedStorageAccount( string vaultName, string managedStorageAccountName )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrEmpty( managedStorageAccountName ) )
                throw new ArgumentNullException( "managedStorageAccountName" );

            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );

            DeletedStorageBundle storageBundle;
            try
            {
                storageBundle = keyVaultClient.DeleteStorageAccountAsync( vaultAddress, managedStorageAccountName ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSDeletedKeyVaultManagedStorageAccount( storageBundle, vaultUriHelper );
        }

        public PSKeyVaultManagedStorageAccount RegenerateManagedStorageAccountKey( string vaultName, string managedStorageAccountName, string keyName )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrEmpty( managedStorageAccountName ) )
                throw new ArgumentNullException( "managedStorageAccountName" );
            if ( string.IsNullOrEmpty( keyName ) )
                throw new ArgumentNullException( "keyName" );

            StorageBundle storageBundle;
            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );

            try
            {
                storageBundle = keyVaultClient.RegenerateStorageAccountKeyAsync( vaultAddress, managedStorageAccountName, keyName ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultManagedStorageAccount( storageBundle, vaultUriHelper );
        }

        public PSKeyVaultManagedStorageSasDefinition GetManagedStorageSasDefinition( string vaultName, string managedStorageAccountName, string sasDefinitionName )
        {
            if ( string.IsNullOrWhiteSpace( vaultName ) ) throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrWhiteSpace( managedStorageAccountName ) ) throw new ArgumentNullException( "managedStorageAccountName" );
            if ( string.IsNullOrWhiteSpace( sasDefinitionName ) ) throw new ArgumentNullException( "sasDefinitionName" );

            SasDefinitionBundle storagesasDefinitionBundle;

            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );

            try
            {
                storagesasDefinitionBundle = keyVaultClient.GetSasDefinitionAsync( vaultAddress, managedStorageAccountName, sasDefinitionName ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultManagedStorageSasDefinition( storagesasDefinitionBundle, vaultUriHelper );
        }

        public IEnumerable<PSKeyVaultManagedStorageSasDefinitionIdentityItem> GetManagedStorageSasDefinitions( KeyVaultStorageSasDefinitiontFilterOptions options )
        {
            if ( options == null )
                throw new ArgumentNullException( "options" );
            if ( string.IsNullOrEmpty( options.VaultName ) )
                throw new ArgumentException( KeyVaultProperties.Resources.InvalidVaultName );
            if ( string.IsNullOrEmpty( options.AccountName ) )
                throw new ArgumentException( KeyVaultProperties.Resources.InvalidManagedStorageAccountName );

            string vaultAddress = vaultUriHelper.CreateVaultAddress( options.VaultName );

            try
            {
                IPage<SasDefinitionItem> result;

                if ( string.IsNullOrEmpty( options.NextLink ) )
                    result = keyVaultClient.GetSasDefinitionsAsync( vaultAddress, options.AccountName ).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetSasDefinitionsNextAsync( options.NextLink ).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result.Select(storageAccountItem => new PSKeyVaultManagedStorageSasDefinitionIdentityItem( storageAccountItem, vaultUriHelper ) );
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }
        }

        public PSKeyVaultManagedStorageSasDefinition SetManagedStorageSasDefinition( 
            string vaultName, 
            string managedStorageAccountName, 
            string sasDefinitionName,
            string templateUri,
            string sasType, 
            string validityPeriod,
            PSKeyVaultManagedStorageSasDefinitionAttributes sasDefinitionAttributes, 
            Hashtable tags )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException(nameof(vaultName));
            if ( string.IsNullOrEmpty( managedStorageAccountName ) )
                throw new ArgumentNullException(nameof(managedStorageAccountName));
            if (string.IsNullOrEmpty(templateUri))
                throw new ArgumentNullException(nameof(templateUri));
            if (string.IsNullOrEmpty(sasType))
                throw new ArgumentNullException(nameof(sasType));
            if (string.IsNullOrEmpty(validityPeriod))
                throw new ArgumentNullException(nameof(validityPeriod));
            if ( string.IsNullOrEmpty( sasDefinitionName ) )
                throw new ArgumentNullException(nameof(sasDefinitionName));

            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );
            var attributes = sasDefinitionAttributes == null ? null : new SasDefinitionAttributes
            {
                Enabled = sasDefinitionAttributes.Enabled,
            };

            SasDefinitionBundle sasDefinitionBundle;
            try
            {
                sasDefinitionBundle =
                    keyVaultClient.SetSasDefinitionAsync( 
                        vaultAddress, 
                        managedStorageAccountName,
                        sasDefinitionName,
                        templateUri,
                        sasType,
                        validityPeriod,
                        attributes,
                        tags == null ? null : tags.ConvertToDictionary() ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultManagedStorageSasDefinition( sasDefinitionBundle, vaultUriHelper );
        }

        public PSDeletedKeyVaultManagedStorageSasDefinition DeleteManagedStorageSasDefinition( string vaultName, string managedStorageAccountName, string sasDefinitionName )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( "vaultName" );
            if ( string.IsNullOrEmpty( managedStorageAccountName ) )
                throw new ArgumentNullException( "managedStorageAccountName" );
            if ( string.IsNullOrEmpty( sasDefinitionName ) )
                throw new ArgumentNullException( "sasDefinitionName" );

            var vaultAddress = vaultUriHelper.CreateVaultAddress( vaultName );

            DeletedSasDefinitionBundle sasDefinitionBundle;
            try
            {
                sasDefinitionBundle =
                    keyVaultClient.DeleteSasDefinitionAsync( vaultAddress,
                        managedStorageAccountName,
                        sasDefinitionName ).GetAwaiter().GetResult();
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSDeletedKeyVaultManagedStorageSasDefinition( sasDefinitionBundle, vaultUriHelper );
        }

        #endregion


        private Exception GetInnerException(Exception exception)
        {
            while (exception.InnerException != null) exception = exception.InnerException;
            return exception;
        }

        public PSDeletedKeyVaultKey GetDeletedKey(string vaultName, string keyName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException("vaultName");
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException("keyName");

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedKeyBundle deletedKeyBundle;
            try
            {
                deletedKeyBundle = keyVaultClient.GetDeletedKeyAsync(vaultAddress, keyName).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if(ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSDeletedKeyVaultKey(deletedKeyBundle, vaultUriHelper);
        }

        public IEnumerable<PSDeletedKeyVaultKeyIdentityItem> GetDeletedKeys(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<DeletedKeyItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetDeletedKeysAsync(vaultAddress).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetDeletedKeysNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSDeletedKeyVaultKeyIdentityItem>() :
                    result.Select(deletedKeyItem => new PSDeletedKeyVaultKeyIdentityItem(deletedKeyItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public PSDeletedKeyVaultSecret GetDeletedSecret(string vaultName, string secretName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException("vaultName");
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException("secretName");

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedSecretBundle deletedSecret;
            try
            {
                deletedSecret = keyVaultClient.GetDeletedSecretAsync(vaultAddress, secretName).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSDeletedKeyVaultSecret(deletedSecret, vaultUriHelper);
        }

        public IEnumerable<PSDeletedKeyVaultSecretIdentityItem> GetDeletedSecrets(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<DeletedSecretItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetDeletedSecretsAsync(vaultAddress).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetDeletedSecretsNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSDeletedKeyVaultSecretIdentityItem>() :
                    result.Select(deletedSecretItem => new PSDeletedKeyVaultSecretIdentityItem(deletedSecretItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public void PurgeKey(string vaultName, string keyName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException("vaultName");
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException("keyName");

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            try
            {
                keyVaultClient.PurgeDeletedKeyAsync(vaultAddress, keyName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public void PurgeSecret(string vaultName, string secretName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException("vaultName");
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException("secretName");

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            try
            {
                keyVaultClient.PurgeDeletedSecretAsync(vaultAddress, secretName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public PSKeyVaultKey RecoverKey(string vaultName, string keyName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException("vaultName");
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException("keyName");

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            KeyBundle recoveredKey;
            try
            {
                recoveredKey = keyVaultClient.RecoverDeletedKeyAsync(vaultAddress, keyName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultKey(recoveredKey, vaultUriHelper);
        }

        public PSKeyVaultSecret RecoverSecret(string vaultName, string secretName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException("vaultName");
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentNullException("secretName");

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            SecretBundle recoveredSecret;
            try
            {
                recoveredSecret = keyVaultClient.RecoverDeletedSecretAsync(vaultAddress, secretName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultSecret(recoveredSecret, vaultUriHelper);
        }

        public PSDeletedKeyVaultCertificate GetDeletedCertificate( string vaultName, string certName )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( nameof(vaultName) );
            if ( string.IsNullOrEmpty( certName ) )
                throw new ArgumentNullException( nameof(certName) );

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedCertificateBundle deletedCertificate;
            try
            {
                deletedCertificate = keyVaultClient.GetDeletedCertificateAsync( vaultAddress, certName ).GetAwaiter( ).GetResult( );
            }
            catch ( KeyVaultErrorException ex )
            {
                if ( ex.Response.StatusCode == HttpStatusCode.NotFound )
                    return null;
                throw;
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSDeletedKeyVaultCertificate(deletedCertificate);
        }

        public IEnumerable<PSDeletedKeyVaultCertificateIdentityItem> GetDeletedCertificates( KeyVaultObjectFilterOptions options )
        {
            if ( options == null )
                throw new ArgumentNullException( nameof( options ) );
            if ( string.IsNullOrEmpty( options.VaultName ) )
                throw new ArgumentException( KeyVaultProperties.Resources.InvalidVaultName );

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<DeletedCertificateItem> result;

                if ( string.IsNullOrEmpty( options.NextLink ) )
                    result = keyVaultClient.GetDeletedCertificatesAsync( vaultAddress ).GetAwaiter( ).GetResult( );
                else
                    result = keyVaultClient.GetDeletedCertificatesNextAsync( options.NextLink ).GetAwaiter( ).GetResult( );

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSDeletedKeyVaultCertificateIdentityItem>( ) :
                    result.Select(deletedItem => new PSDeletedKeyVaultCertificateIdentityItem( deletedItem, vaultUriHelper ) );
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }
        }

        public PSKeyVaultCertificate RecoverCertificate( string vaultName, string certName )
        {
            if ( string.IsNullOrEmpty( vaultName ) )
                throw new ArgumentNullException( nameof( vaultName ) );
            if ( string.IsNullOrEmpty( certName ) )
                throw new ArgumentNullException( nameof( certName ) );

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificateBundle recoveredCertificate;
            try
            {
                recoveredCertificate = keyVaultClient.RecoverDeletedCertificateAsync( vaultAddress, certName ).GetAwaiter( ).GetResult( );
            }
            catch ( Exception ex )
            {
                throw GetInnerException( ex );
            }

            return new PSKeyVaultCertificate(recoveredCertificate);
        }

        public string BackupCertificate(string vaultName, string certificateName, string outputBlobPath)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentNullException(nameof(certificateName));
            if (string.IsNullOrEmpty(outputBlobPath))
                throw new ArgumentNullException(nameof(outputBlobPath));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            BackupCertificateResult backupCertificateResult;
            try
            {
                backupCertificateResult = keyVaultClient.BackupCertificateAsync(vaultAddress, certificateName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            File.WriteAllBytes(outputBlobPath, backupCertificateResult.Value);

            return outputBlobPath;
        }

        public PSKeyVaultCertificate RestoreCertificate(string vaultName, string inputBlobPath)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(inputBlobPath))
                throw new ArgumentNullException(nameof(inputBlobPath));

            var backupBlob = File.ReadAllBytes(inputBlobPath);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            CertificateBundle certificateBundle;
            try
            {
                certificateBundle = keyVaultClient.RestoreCertificateAsync(vaultAddress, backupBlob).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultCertificate(certificateBundle, vaultUriHelper);
        }

        public PSDeletedKeyVaultManagedStorageAccount GetDeletedManagedStorageAccount(string vaultName, string managedStorageAccountName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(managedStorageAccountName))
                throw new ArgumentNullException(nameof(managedStorageAccountName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedStorageBundle deletedStorageBundle;
            try
            {
                deletedStorageBundle = keyVaultClient.GetDeletedStorageAccountAsync(vaultAddress, managedStorageAccountName).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSDeletedKeyVaultManagedStorageAccount(deletedStorageBundle, vaultUriHelper);
        }

        public PSDeletedKeyVaultManagedStorageSasDefinition GetDeletedManagedStorageSasDefinition(string vaultName, string managedStorageAccountName, string sasDefinitionName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(managedStorageAccountName))
                throw new ArgumentNullException(nameof(managedStorageAccountName));
            if (string.IsNullOrWhiteSpace(sasDefinitionName))
                throw new ArgumentNullException(nameof(sasDefinitionName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            DeletedSasDefinitionBundle deletedSasDefinitionBundle;
            try
            {
                deletedSasDefinitionBundle = keyVaultClient.GetDeletedSasDefinitionAsync(vaultAddress, managedStorageAccountName, sasDefinitionName).GetAwaiter().GetResult();
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSDeletedKeyVaultManagedStorageSasDefinition(deletedSasDefinitionBundle, vaultUriHelper);
        }

        public IEnumerable<PSDeletedKeyVaultManagedStorageAccountIdentityItem> GetDeletedManagedStorageAccounts(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<DeletedStorageAccountItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetDeletedStorageAccountsAsync(vaultAddress).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetDeletedStorageAccountsNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSDeletedKeyVaultManagedStorageAccountIdentityItem>() :
                    result.Select(deletedItem => new PSDeletedKeyVaultManagedStorageAccountIdentityItem(deletedItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public IEnumerable<PSDeletedKeyVaultManagedStorageSasDefinitionIdentityItem> GetDeletedManagedStorageSasDefinitions(KeyVaultObjectFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.VaultName))
                throw new ArgumentException(KeyVaultProperties.Resources.InvalidVaultName);

            if (String.IsNullOrWhiteSpace(options.Name))
                throw new ArgumentNullException(KeyVaultProperties.Resources.InvalidManagedStorageAccountName);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(options.VaultName);

            try
            {
                IPage<DeletedSasDefinitionItem> result;

                if (string.IsNullOrEmpty(options.NextLink))
                    result = keyVaultClient.GetDeletedSasDefinitionsAsync(vaultAddress, options.Name).GetAwaiter().GetResult();
                else
                    result = keyVaultClient.GetDeletedSasDefinitionsNextAsync(options.NextLink).GetAwaiter().GetResult();

                options.NextLink = result.NextPageLink;
                return result == null ? new List<PSDeletedKeyVaultManagedStorageSasDefinitionIdentityItem>() :
                    result.Select(deletedItem => new PSDeletedKeyVaultManagedStorageSasDefinitionIdentityItem(deletedItem, vaultUriHelper));
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public PSKeyVaultManagedStorageAccount RecoverManagedStorageAccount(string vaultName, string deletedManagedStorageAccountName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(deletedManagedStorageAccountName))
                throw new ArgumentNullException(nameof(deletedManagedStorageAccountName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            StorageBundle recoveredStorageBundle;
            try
            {
                recoveredStorageBundle = keyVaultClient.RecoverDeletedStorageAccountAsync(vaultAddress, deletedManagedStorageAccountName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultManagedStorageAccount(recoveredStorageBundle, vaultUriHelper);
        }

        public PSKeyVaultManagedStorageSasDefinition RecoverManagedStorageSasDefinition(string vaultName, string managedStorageAccountName, string sasDefinitionName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(managedStorageAccountName))
                throw new ArgumentNullException(nameof(managedStorageAccountName));
            if (string.IsNullOrWhiteSpace(sasDefinitionName))
                throw new ArgumentNullException(nameof(sasDefinitionName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            SasDefinitionBundle recoveredSasDefinitionBundle;
            try
            {
                recoveredSasDefinitionBundle = keyVaultClient.RecoverDeletedSasDefinitionAsync(vaultAddress, managedStorageAccountName, sasDefinitionName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultManagedStorageSasDefinition(recoveredSasDefinitionBundle, vaultUriHelper);
        }

        public void PurgeManagedStorageAccount(string vaultName, string managedStorageAccountName)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(managedStorageAccountName))
                throw new ArgumentNullException(nameof(managedStorageAccountName));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            try
            {
                keyVaultClient.PurgeDeletedStorageAccountAsync(vaultAddress, managedStorageAccountName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }
        }

        public string BackupManagedStorageAccount(string vaultName, string managedStorageAccountName, string outputBlobPath)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(managedStorageAccountName))
                throw new ArgumentNullException(nameof(managedStorageAccountName));
            if (string.IsNullOrEmpty(outputBlobPath))
                throw new ArgumentNullException(nameof(outputBlobPath));

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            BackupStorageResult backupStorageAccountResult;
            try
            {
                backupStorageAccountResult = keyVaultClient.BackupStorageAccountAsync(vaultAddress, managedStorageAccountName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            File.WriteAllBytes(outputBlobPath, backupStorageAccountResult.Value);

            return outputBlobPath;
        }

        public PSKeyVaultManagedStorageAccount RestoreManagedStorageAccount(string vaultName, string inputBlobPath)
        {
            if (string.IsNullOrEmpty(vaultName))
                throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrEmpty(inputBlobPath))
                throw new ArgumentNullException(nameof(inputBlobPath));

            var backupBlob = File.ReadAllBytes(inputBlobPath);

            string vaultAddress = vaultUriHelper.CreateVaultAddress(vaultName);

            StorageBundle storageAccountBundle;
            try
            {
                storageAccountBundle = keyVaultClient.RestoreStorageAccountAsync(vaultAddress, backupBlob).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw GetInnerException(ex);
            }

            return new PSKeyVaultManagedStorageAccount(storageAccountBundle, vaultUriHelper);
        }

        private VaultUriHelper vaultUriHelper;
        private KeyVaultClient keyVaultClient;
    }
}
