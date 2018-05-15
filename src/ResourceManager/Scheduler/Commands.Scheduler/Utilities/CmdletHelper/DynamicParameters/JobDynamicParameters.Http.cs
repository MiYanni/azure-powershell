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

namespace Microsoft.Azure.Commands.Scheduler.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation;

    public partial class JobDynamicParameters
    {
        /// <summary>
        /// Pfx path for client certificate authentication.
        /// </summary>
        private RuntimeDefinedParameter _clientCertificatePfx;

        /// <summary>
        /// Pfx password for client certificate authentication.
        /// </summary>
        private RuntimeDefinedParameter _clientCertificatePassword;

        /// <summary>
        /// Basic authentication username.
        /// </summary>
        private RuntimeDefinedParameter _basicUsername;

        /// <summary>
        /// Basic authentication password.
        /// </summary>
        private RuntimeDefinedParameter _basicPassword;

        /// <summary>
        /// OAuth authentication tenant.
        /// </summary>
        private RuntimeDefinedParameter _oAuthTenant;

        /// <summary>
        /// OAuth authentication audience.
        /// </summary>
        private RuntimeDefinedParameter _oAuthAudience;

        /// <summary>
        /// OAuth authentication client id.
        /// </summary>
        private RuntimeDefinedParameter _oAuthClientId;

        /// <summary>
        /// OAuth authentication secret.
        /// </summary>
        private RuntimeDefinedParameter _oAuthSecret;

        /// <summary>
        /// The error action method for Http and Https Action types (GET, PUT, POST, HEAD or DELETE).
        /// </summary>
        private RuntimeDefinedParameter _errorActionMethod;

        /// <summary>
        /// The Uri for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionUri;

        /// <summary>
        /// The request body for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionRequestBody;

        /// <summary>
        /// The headers for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionHeaders;

        /// <summary>
        /// Authententication type for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionHttpAuthenticationType;

        /// <summary>
        /// Client certificate pfx path for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionClientCertificatePfx;

        /// <summary>
        /// Client certificate pfx password for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionClientCertificatePassword;

        /// <summary>
        /// Basic authentication user name for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionBasicUsername;

        /// <summary>
        /// Basic authentication password for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionBasicPassword;

        /// <summary>
        /// OAuth authentication tenant for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionOAuthTenant;

        /// <summary>
        /// OAuth authentication audience for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionOAuthAudience;

        /// <summary>
        /// OAuth authentication client id for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionOAuthClientId;

        /// <summary>
        /// OAuth authentication secret for http error action.
        /// </summary>
        private RuntimeDefinedParameter _errorActionOAuthSecret;

        /// <summary>
        /// Gets client certificate pfx path.
        /// </summary>
        internal string ClientCertificatePfx
        {
            get
            {
                return _clientCertificatePfx == null ? null : (string)_clientCertificatePfx.Value;
            }
        }

        /// <summary>
        /// Gets client certificate password.
        /// </summary>
        internal string ClientCertificatePassword
        {
            get
            {
                return _clientCertificatePassword == null ? null : (string)_clientCertificatePassword.Value;
            }
        }

        /// <summary>
        /// Gets basic authencation user name.
        /// </summary>
        internal string BasicUsername
        {
            get
            {
                return _basicUsername == null ? null : (string)_basicUsername.Value;
            }
        }

        /// <summary>
        /// Gets basic authentication password.
        /// </summary>
        internal string BasicPassword
        {
            get
            {
                return _basicPassword == null ? null : (string)_basicPassword.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authentication tenant.
        /// </summary>
        internal string OAuthTenant
        {
            get
            {
                return _oAuthTenant == null ? null : (string)_oAuthTenant.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authentication audience.
        /// </summary>
        internal string OAuthAudience
        {
            get
            {
                return _oAuthAudience == null ? null : (string)_oAuthAudience.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authentication client id.
        /// </summary>
        internal string OAuthClientId
        {
            get
            {
                return _oAuthClientId == null ? null : (string)_oAuthClientId.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authentication secret.
        /// </summary>
        internal string OAuthSecret
        {
            get
            {
                return _oAuthSecret == null ? null : (string)_oAuthSecret.Value;
            }
        }

        /// <summary>
        /// Gets http method for http error action.
        /// </summary>
        internal string ErrorActionMethod
        {
            get
            {
                return _errorActionMethod == null ? null : (string)_errorActionMethod.Value;
            }
        }

        /// <summary>
        /// Gets uri for http error action.
        /// </summary>
        internal Uri ErrorActionUri
        {
            get
            {
                return _errorActionUri == null ? null : (Uri)_errorActionUri.Value;
            }
        }

        /// <summary>
        /// Gets request body for http error action.
        /// </summary>
        internal string ErrorActionRequestBody
        {
            get
            {
                return _errorActionRequestBody == null ? null : (string)_errorActionRequestBody.Value;
            }
        }

        /// <summary>
        /// Gets headers for http error action.
        /// </summary>
        internal Hashtable ErrorActionHeaders
        {
            get
            {
                return _errorActionHeaders == null ? null : (Hashtable)_errorActionHeaders.Value;
            }
        }

        /// <summary>
        /// Gets authentication type for http error action.
        /// </summary>
        internal string ErrorActionHttpAuthenticationType
        {
            get
            {
                return _errorActionHttpAuthenticationType == null ? null : (string)_errorActionHttpAuthenticationType.Value;
            }
        }

        /// <summary>
        /// Gets client certificate pfx path for http error action.
        /// </summary>
        internal string ErrorActionClientCertificatePfx
        {
            get
            {
                return _errorActionClientCertificatePfx == null ? null : (string)_errorActionClientCertificatePfx.Value;
            }
        }

        /// <summary>
        /// Gets client certificate password for http error action.
        /// </summary>
        internal string ErrorActionClientCertificatePassword
        {
            get
            {
                return _errorActionClientCertificatePassword == null ? null : (string)_errorActionClientCertificatePassword.Value;
            }
        }

        /// <summary>
        /// Gets basic authentication username for http error action.
        /// </summary>
        internal string ErrorActionBasicUsername
        {
            get
            {
                return _errorActionBasicUsername == null ? null : (string)_errorActionBasicUsername.Value;
            }
        }

        /// <summary>
        /// Gets basic authentication password for http error action.
        /// </summary>
        internal string ErrorActionBasicPassword
        {
            get
            {
                return _errorActionBasicPassword == null ? null : (string)_errorActionBasicPassword.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authencation tenant for http error action.
        /// </summary>
        internal string ErrorActionOAuthTenant
        {
            get
            {
                return _errorActionOAuthTenant == null ? null : (string)_errorActionOAuthTenant.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authentication audience for http error action.
        /// </summary>
        internal string ErrorActionOAuthAudience
        {
            get
            {
                return _errorActionOAuthAudience == null ? null : (string)_errorActionOAuthAudience.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authentication client id for http error action.
        /// </summary>
        internal string ErrorActionOAuthClientId
        {
            get
            {
                return _errorActionOAuthClientId == null ? null : (string)_errorActionOAuthClientId.Value;
            }
        }

        /// <summary>
        /// Gets OAuth authentication secret for http error action.
        /// </summary>
        internal string ErrorActionOAuthSecret
        {
            get
            {
                return _errorActionOAuthSecret == null ? null : (string)_errorActionOAuthSecret.Value;
            }
        }

        /// <summary>
        /// Adds client certificate authentication parameters to PowerShell.
        /// </summary>
        /// <param name="create">true if parameters added for create scenario and false for update scenario.</param>
        /// <returns>PowerShell parameters.</returns>
        internal RuntimeDefinedParameterDictionary AddHttpClientCertificateAuthenticationTypeParameters(bool create = true)
        {
            var clientCertificatePfxAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The pfx of client certificate.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var clientCertificatePasswordAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The password for the pfx.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            _clientCertificatePfx = new RuntimeDefinedParameter("ClientCertificatePfx", typeof(object), clientCertificatePfxAttributes);
            _clientCertificatePassword = new RuntimeDefinedParameter("ClientCertificatePassword", typeof(string), clientCertificatePasswordAttributes);

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();
            runtimeDefinedParameterDictionary.Add("ClientCertificatePfx", _clientCertificatePfx);
            runtimeDefinedParameterDictionary.Add("ClientCertificatePassword", _clientCertificatePassword);

            return runtimeDefinedParameterDictionary;
        }

        /// <summary>
        /// Adds basic authentication parameters to PowerShell.
        /// </summary>
        /// <param name="create">true if parameters added for create scenario and false for update scenario.</param>
        /// <returns>PowerShell parameters.</returns>
        internal RuntimeDefinedParameterDictionary AddHttpBasicAuthenticationTypeParameters(bool create = true)
        {
            var basicUsernameAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = true,
                    HelpMessage = "The user name.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var basicPasswordAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = true,
                    HelpMessage = "The password.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            _basicUsername = new RuntimeDefinedParameter("Username", typeof(string), basicUsernameAttributes);
            _basicPassword = new RuntimeDefinedParameter("Password", typeof(string), basicPasswordAttributes);

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();
            runtimeDefinedParameterDictionary.Add("Username", _basicUsername);
            runtimeDefinedParameterDictionary.Add("Password", _basicPassword);

            return runtimeDefinedParameterDictionary;
        }

        /// <summary>
        /// Adds OAuth authentication parameters to PowerShell.
        /// </summary>
        /// <param name="create">true if parameters added for create scenario and false for update scenario.</param>
        /// <returns>PowerShell parameters.</returns>
        internal RuntimeDefinedParameterDictionary AddHttpActiveDirectoryOAuthAuthenticationTypeParameters(bool create = true)
        {
            var oAuthTenantAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The tenant Id.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var oAuthAudienceAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The audience.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var oAuthClientIdAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The client id.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var oAuthSecretAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The secret.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            _oAuthTenant = new RuntimeDefinedParameter("Tenant", typeof(string), oAuthTenantAttributes);
            _oAuthAudience = new RuntimeDefinedParameter("Audience", typeof(string), oAuthAudienceAttributes);
            _oAuthClientId = new RuntimeDefinedParameter("ClientId", typeof(string), oAuthClientIdAttributes);
            _oAuthSecret = new RuntimeDefinedParameter("Secret", typeof(string), oAuthSecretAttributes);

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();
            runtimeDefinedParameterDictionary.Add("Tenant", _oAuthTenant);
            runtimeDefinedParameterDictionary.Add("Audience", _oAuthAudience);
            runtimeDefinedParameterDictionary.Add("ClientId", _oAuthClientId);
            runtimeDefinedParameterDictionary.Add("Secret", _oAuthSecret);

            return runtimeDefinedParameterDictionary;
        }

        /// <summary>
        /// Adds http error action parameters to PowerShell.
        /// </summary>
        /// <param name="create">true if parameters added for create scenario and false for update scenario.</param>
        /// <returns>PowerShell parameters.</returns>
        internal RuntimeDefinedParameterDictionary AddHttpErrorActionParameters(bool create = true)
        {
            var errorActionMethodAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The Method for Http and Https Action types (GET, PUT, POST, HEAD or DELETE).",
                },
                new ValidateSetAttribute(Constants.HttpMethodGET, Constants.HttpMethodPUT, Constants.HttpMethodPOST, Constants.HttpMethodDELETE)
                {
                    IgnoreCase = true,
                }
            };

            var errorActionUriAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = create ? true : false,
                    HelpMessage = "The Uri for error job action.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionRequestBodyAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The Body for PUT and POST job actions.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionHeadersAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The header collection."
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionHttpAuthenticationTypeAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The Http Authentication type."
                },
                new ValidateSetAttribute(Constants.HttpAuthenticationNone, Constants.HttpAuthenticationClientCertificate, Constants.HttpAuthenticationActiveDirectoryOAuth, Constants.HttpAuthenticationBasic)
                {
                    IgnoreCase = true
                }
            };

            _errorActionMethod = new RuntimeDefinedParameter("ErrorActionMethod", typeof(string), errorActionMethodAttributes);
            _errorActionUri = new RuntimeDefinedParameter("ErrorActionUri", typeof(Uri), errorActionUriAttributes);
            _errorActionRequestBody = new RuntimeDefinedParameter("ErrorActionRequestBody", typeof(string), errorActionRequestBodyAttributes);
            _errorActionHeaders = new RuntimeDefinedParameter("ErrorActionHeaders", typeof(Hashtable), errorActionHeadersAttributes);
            _errorActionHttpAuthenticationType = new RuntimeDefinedParameter("ErrorActionHttpAuthenticationType", typeof(string), errorActionHttpAuthenticationTypeAttributes);

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();
            runtimeDefinedParameterDictionary.Add("ErrorActionMethod", _errorActionMethod);
            runtimeDefinedParameterDictionary.Add("ErrorActionUri", _errorActionUri);
            runtimeDefinedParameterDictionary.Add("ErrorActionRequestBody", _errorActionRequestBody);
            runtimeDefinedParameterDictionary.Add("ErrorActionHeaders", _errorActionHeaders);
            runtimeDefinedParameterDictionary.Add("ErrorActionHttpAuthenticationType", _errorActionHttpAuthenticationType);

            runtimeDefinedParameterDictionary.AddRange(AddHttpErrorActionClientCertificateAuthenticationTypeParameters());
            runtimeDefinedParameterDictionary.AddRange(AddHttpErrorActionBasicAuthenticationTypeParameters());
            runtimeDefinedParameterDictionary.AddRange(AddHttpErrorActionActiveDirectoryOAuthAuthenticationTypeParameters());

            return runtimeDefinedParameterDictionary;
        }

        /// <summary>
        /// Adds client certificate authentication parameters for http error action.
        /// </summary>
        /// <returns>PowerShell parameters.</returns>
        internal RuntimeDefinedParameterDictionary AddHttpErrorActionClientCertificateAuthenticationTypeParameters()
        {
            var errorActionClientCertificatePfxAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The file name of client certificate.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionClientCertificatePasswordAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The password for the pfx.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            _errorActionClientCertificatePfx = new RuntimeDefinedParameter("ErrorActionClientCertificatePfx", typeof(object), errorActionClientCertificatePfxAttributes);
            _errorActionClientCertificatePassword = new RuntimeDefinedParameter("ErrorActionClientCertificatePassword", typeof(string), errorActionClientCertificatePasswordAttributes);

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();
            runtimeDefinedParameterDictionary.Add("ErrorActionClientCertificatePfx", _errorActionClientCertificatePfx);
            runtimeDefinedParameterDictionary.Add("ErrorActionClientCertificatePassword", _errorActionClientCertificatePassword);

            return runtimeDefinedParameterDictionary;
        }

        /// <summary>
        /// Adds basic authentication parameters for http error action.
        /// </summary>
        /// <returns>PowerShell parameters.</returns>
        internal RuntimeDefinedParameterDictionary AddHttpErrorActionBasicAuthenticationTypeParameters()
        {
            var errorActionBasicUsernameAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The user name.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionBasicPasswordAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The password.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            _errorActionBasicUsername = new RuntimeDefinedParameter("ErrorActionUsername", typeof(string), errorActionBasicUsernameAttributes);
            _errorActionBasicPassword = new RuntimeDefinedParameter("ErrorActionPassword", typeof(string), errorActionBasicPasswordAttributes);

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();
            runtimeDefinedParameterDictionary.Add("ErrorActionUsername", _errorActionBasicUsername);
            runtimeDefinedParameterDictionary.Add("ErrorActionPassword", _errorActionBasicPassword);

            return runtimeDefinedParameterDictionary;
        }

        /// <summary>
        /// Adds OAuth authentication parameters for http error action.
        /// </summary>
        /// <returns>PowerShell parameters.</returns>
        internal RuntimeDefinedParameterDictionary AddHttpErrorActionActiveDirectoryOAuthAuthenticationTypeParameters()
        {
            var errorActionOAuthTenantAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The tenant Id.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionOAuthAudienceAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The audience.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionOAuthClientIdAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The client id.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            var errorActionOAuthSecretAttributes = new Collection<Attribute>
            {
                new ParameterAttribute
                {
                    Mandatory = false,
                    HelpMessage = "The secret.",
                },
                new ValidateNotNullOrEmptyAttribute()
            };

            _errorActionOAuthTenant = new RuntimeDefinedParameter("ErrorActionTenant", typeof(string), errorActionOAuthTenantAttributes);
            _errorActionOAuthAudience = new RuntimeDefinedParameter("ErrorActionAudience", typeof(string), errorActionOAuthAudienceAttributes);
            _errorActionOAuthClientId = new RuntimeDefinedParameter("ErrorActionClientId", typeof(string), errorActionOAuthClientIdAttributes);
            _errorActionOAuthSecret = new RuntimeDefinedParameter("ErrorActionSecret", typeof(string), errorActionOAuthSecretAttributes);

            var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();
            runtimeDefinedParameterDictionary.Add("ErrorActionTenant", _errorActionOAuthTenant);
            runtimeDefinedParameterDictionary.Add("ErrorActionAudience", _errorActionOAuthAudience);
            runtimeDefinedParameterDictionary.Add("ErrorActionClientId", _errorActionOAuthClientId);
            runtimeDefinedParameterDictionary.Add("ErrorActionSecret", _errorActionOAuthSecret);

            return runtimeDefinedParameterDictionary;
        }
    }
}
