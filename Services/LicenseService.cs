using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Newtonsoft.Json;
using Nop.Plugin.Widgets.DPDShipToShop.Models;
using Nop.Plugin.Widgets.DPDShipToShop.Utility;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.DPDShipToShop.Services
{
    public class LicenseService
    {
        #region Fields
        private readonly DPDShipToShopSettings _DPDShipToShopSettings;
        private readonly ILogger _logger;
        #endregion

        #region Ctor
        public LicenseService(DPDShipToShopSettings DPDShipToShopSettings,
            ILogger logger)
        {
            _DPDShipToShopSettings = DPDShipToShopSettings;
            _logger = logger;

        }
        #endregion


        //Salt
        private const string saltKey = "Datahash";
        //Product Key
        private const string productKey = "%t9nHg5jf7W+vJHh";

        /// <summary>
        /// Validates the plugin serial number against the current store URL.
        /// </summary>
        /// <param name="serialNumber">The serial number to validate.</param>
        /// <returns>True when the serial number is valid for the current store; otherwise false.</returns>
        public async Task<bool> VerifyLicense(string serialNumber)
        {
            string decryptLicenseString = "";
            string storeUrl = _DPDShipToShopSettings.StoreUrl;

            //retun null if the licenseKey or store url is null
            if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(storeUrl))
            {
                return false;
            }

            //VerifyLicense
            try
            {

                DeriveBytes rgb = new Rfc2898DeriveBytes(productKey, Encoding.Unicode.GetBytes(saltKey));
                SymmetricAlgorithm algorithm = new TripleDESCryptoServiceProvider();
                byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
                byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);
                ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIV);
                using (MemoryStream buffer = new MemoryStream(Convert.FromBase64String(serialNumber)))
                {
                    using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                        {
                            decryptLicenseString = reader.ReadToEnd();
                        }
                    }
                }

            } 
            catch (Exception ex)
            {
                await _logger.ErrorAsync("An error occurred validating the DPD Ship to Shop plugin License: "+ ex.Message);
                return false;
            }

            if (IsValidJson(decryptLicenseString))
            {
                LicenseModel license = JsonConvert.DeserializeObject<LicenseModel>(decryptLicenseString.ToString());

                string licenseType = license.D;
                string licenseUrl = license.U;
                DateTime licenseIssueDate = license.T;

                switch (licenseType)
                {
                    case "Domain":
                        if (licenseUrl.ToLower() == storeUrl.ToLower())
                        {
                            return true;
                        }
                    break;
                    case "Url":
                        if (licenseUrl.ToLower() == storeUrl.ToLower())
                        {
                            return true;
                        }
                    break;
                    case "Localhost":
                        if (licenseUrl.ToLower() == storeUrl.ToLower() || storeUrl.ToLower().Contains("local"))
                        {
                            if (DateTime.Compare(licenseIssueDate.AddDays(30), DateTime.UtcNow) > 0)
                            {
                                return true;
                            } else
                            {
                                return false;
                            }
                            
                        }
                    break;
                    case "Staging":
                        if (licenseUrl.ToLower() == storeUrl.ToLower())
                        {
                            return true;
                        }
                    break;
                    case "IP":
                        if(NetworkUtil.HasIPAddress(licenseUrl.ToLower()))
                        {
                            return true;
                        } 
                        else
                        {
                            return false;
                        }
                }
            }
            

            return false;
        }

        /// <summary>
        /// Determines whether the supplied string contains valid license JSON.
        /// </summary>
        /// <param name="strInput">The string to validate.</param>
        /// <returns>True when the string contains valid JSON; otherwise false.</returns>
        public bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) return false;

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<LicenseModel>(strInput);
                    return true;
                }
                catch // not valid
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
